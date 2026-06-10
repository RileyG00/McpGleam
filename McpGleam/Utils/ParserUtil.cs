using Models;
using System.Net;
using System.Text.RegularExpressions;

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
			Regex linkRegex = new(@"<li><a\s+href=""(?<path>[^""]+)"">(?<name>[^<]+)</a></li>", RegexOptions.Compiled);

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
				return string.Empty;
			}

			// Find <section id="left"...><div>...</div>
			var match = Regex.Match(html, @"<section\s+[^>]*id=""left""[^>]*>\s*<div>(?<content>.*?)</div>", RegexOptions.Singleline);
			if (match.Success)
			{
				return match.Groups["content"].Value.Trim();
			}

			// Fallback: search for <h2>...</h2> and match until the next </div> or <nav>
			var fallbackMatch = Regex.Match(html, @"<h2>.*?</h2>.*?(?=</div>|<nav)", RegexOptions.Singleline);
			if (fallbackMatch.Success)
			{
				return fallbackMatch.Value.Trim();
			}

			return string.Empty;
		}
	}
}
