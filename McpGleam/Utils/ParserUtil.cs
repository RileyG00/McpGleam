using Models;
using System.Net;
using System.Text.RegularExpressions;
using ModelContextProtocol;

namespace McpGleam.Utils
{
	internal static class ParserUtil
	{
		internal static List<DocumentationItem> ParseTableOfContents(string html)
		{
			List<DocumentationItem> items = new();
			if (string.IsNullOrEmpty(html))
			{
				return items;
			}

			// Match category headers: <h3 class="mb-0">Category Name</h3>
			Regex categoryRegex = new(@"<h3[^>]*>(?<category>[^<]+)</h3>", RegexOptions.Compiled);
			// Match list items: <li><a href="/path">Name</a></li>
			Regex linkRegex = new(@"<li><a\s+href=""(?<path>[^""\s>]+)""[^>]*>(?<name>[^<]+)</a></li>", RegexOptions.Compiled);

			MatchCollection matches = categoryRegex.Matches(html);

			for (int i = 0; i < matches.Count; i++)
			{
				Match currentMatch = matches[i];
				string category = WebUtility.HtmlDecode(currentMatch.Groups["category"].Value.Trim());

				int startIndex = currentMatch.Index + currentMatch.Length;
				int endIndex = (i + 1 < matches.Count) ? matches[i + 1].Index : html.Length;
				string section = html.Substring(startIndex, endIndex - startIndex);

				MatchCollection linkMatches = linkRegex.Matches(section);
				foreach (Match linkMatch in linkMatches)
				{
					string path = linkMatch.Groups["path"].Value.Trim();
					string name = WebUtility.HtmlDecode(linkMatch.Groups["name"].Value.Trim());

					items.Add(new DocumentationItem
					{
						Category = category,
						Name = name,
						Path = path
					});
				}
			}

			return items;
		}

		internal static string ParserDocumentationItem(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				throw new McpException("The HTML document was null or empty.");
			}

			// Find <section id="left"...><div>...</div>
			string rawContent = string.Empty;
			var match = Regex.Match(html, @"<section\s+[^>]*id=""left""[^>]*>\s*<div>(?<content>.*?)</div>", RegexOptions.Singleline);
			if (match.Success)
			{
				rawContent = match.Groups["content"].Value.Trim();
			}
			else
			{
				// Fallback: search for <h2>...</h2> and match until the next </div> or <nav>
				var fallbackMatch = Regex.Match(html, @"<h2>.*?</h2>.*?(?=</div>|<nav)", RegexOptions.Singleline);
				if (fallbackMatch.Success)
				{
					rawContent = fallbackMatch.Value.Trim();
				}
			}

			if (string.IsNullOrEmpty(rawContent))
			{
				throw new McpException("Could not find documentation content in the fetched page.");
			}

			string markdown = ConvertHtmlToMarkdown(rawContent);

			// Extract playground code from <script type="gleam" id="code">...</script>
			var codeMatch = Regex.Match(html, @"<script\s+[^>]*id=""code""[^>]*>(?<code>.*?)</script>", RegexOptions.Singleline);
			if (codeMatch.Success)
			{
				string code = codeMatch.Groups["code"].Value.Trim();
				markdown += $"\n\n### Example Code\n```gleam\n{code}\n```";
			}

			return markdown;
		}

		internal static string ConvertHtmlToMarkdown(string html)
		{
			if (string.IsNullOrEmpty(html))
			{
				return string.Empty;
			}

			// Decode HTML entities first
			string text = WebUtility.HtmlDecode(html);

			// 1. Convert headers
			text = Regex.Replace(text, @"<h2>\s*(.*?)\s*</h2>", "\n## $1\n\n", RegexOptions.Singleline);
			text = Regex.Replace(text, @"<h3>\s*(.*?)\s*</h3>", "\n### $1\n\n", RegexOptions.Singleline);

			// 2. Convert pre/code blocks
			text = Regex.Replace(text, @"<pre>\s*<code[^>]*>\s*(.*?)\s*</code>\s*</pre>", "\n```\n$1\n```\n", RegexOptions.Singleline);

			// 3. Convert inline code
			text = Regex.Replace(text, @"<code>\s*(.*?)\s*</code>", "`$1`", RegexOptions.Singleline);

			// 4. Convert strong/em
			text = Regex.Replace(text, @"<strong>\s*(.*?)\s*</strong>", "**$1**", RegexOptions.Singleline);
			text = Regex.Replace(text, @"<b>\s*(.*?)\s*</b>", "**$1**", RegexOptions.Singleline);
			text = Regex.Replace(text, @"<em>\s*(.*?)\s*</em>", "*$1*", RegexOptions.Singleline);
			text = Regex.Replace(text, @"<i>\s*(.*?)\s*</i>", "*$1*", RegexOptions.Singleline);

			// 5. Convert links (convert relative to absolute)
			text = Regex.Replace(text, @"<a\s+[^>]*href=""(?<href>[^""]+)""[^>]*>(?<inner>.*?)</a>", m =>
			{
				string href = m.Groups["href"].Value.Trim();
				string inner = m.Groups["inner"].Value.Trim();
				if (href.StartsWith("/"))
				{
					href = "https://tour.gleam.run" + href;
				}
				return $"[{inner}]({href})";
			}, RegexOptions.Singleline);

			// 6. Convert list items
			text = Regex.Replace(text, @"<li>\s*(.*?)\s*</li>", "* $1\n", RegexOptions.Singleline);

			// 7. Convert paragraphs
			text = Regex.Replace(text, @"<p>\s*(.*?)\s*</p>", "$1\n\n", RegexOptions.Singleline);

			// 8. Convert line breaks
			text = Regex.Replace(text, @"<br\s*/?>", "\n", RegexOptions.IgnoreCase);

			// 9. Strip all remaining HTML tags
			text = Regex.Replace(text, @"<[^>]+>", "");

			// 10. Clean up multiple newlines (3 or more down to exactly 2)
			text = Regex.Replace(text, @"(\r?\n){3,}", "\n\n");

			return text.Trim();
		}
	}
}
