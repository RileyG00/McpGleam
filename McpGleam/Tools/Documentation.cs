using System.Text.Json;
using McpGleam.Utils;
using McpGleam.Constants;
using ModelContextProtocol;
using ModelContextProtocol.Server;
using Models;
using System.ComponentModel;
using Microsoft.Extensions.Hosting;
using System.Text;

namespace McpGleam.Tools
{
	
	[McpServerToolType]
	public static class Documentation
	{
		[McpServerTool(Name = "list_available_docs")]
		[Description("Returns a list of all available documentation items, their category, and the URL to fetch additional information.")]
		public static async Task<string> GetAvailableDocsAsync(IHttpClientFactory httpClientFactory)
		{
			HttpClient httpClient = httpClientFactory.CreateClient(HttpConstants.GleamDocsEndpointName);

			HttpResponseMessage httpResponseMessage = await httpClient.GetAsync("table-of-contents");
			string html = await httpResponseMessage.Content.ReadAsStringAsync();

			List<DocumentationItem> items = ParserUtil.ParseTableOfContents(html);

			return JsonSerializer.Serialize(items, new JsonSerializerOptions { WriteIndented = true });
		}


		[McpServerTool(Name = "get_item_documentation")]
		[Description("When provided with a URL, this method will fetch the up-to-date documentation for the requested item. Prefer this tool when only requesting information on one documentation item.")]
		public static async Task<string> GetItemDocumentationAsync(
			IHttpClientFactory httpClientFactory,
			[Description("The URL at which the documentation can be retrieved from Gleam.")] string documentationUrl
		)
		{
			HttpClient httpClient = httpClientFactory.CreateClient(HttpConstants.GleamDocsEndpointName);

			string urlFragment = documentationUrl.TrimStart('/');
			string html = string.Empty;

			try
			{
				HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(urlFragment);

				httpResponseMessage.EnsureSuccessStatusCode();

				html = await httpResponseMessage.Content.ReadAsStringAsync();
			} catch(Exception ex)
			{
				throw new McpException("The API call to fetch the documentation failed.", ex);
			}

			string documentation = ParserUtil.ParserDocumentationItem(html);

			return documentation;
		}


		[McpServerTool(Name = "get_multiple_item_documentation")]
		[Description("When provided with one or more URLs, this method will fetch the up-to-date documentation for the requested items.")]
		public static async Task<string> GetMultipleItemDocumentationAsync(
			IHttpClientFactory httpClientFactory,
			[Description("The URLs at which the documentation can be retrieved from Gleam.")] string[] documentationUrl
		)
		{
			HttpClient httpClient = httpClientFactory.CreateClient(HttpConstants.GleamDocsEndpointName);

			var results = new Dictionary<string, string>();

			foreach (string url in documentationUrl)
			{
				string urlFragment = url.TrimStart('/');
				string html = string.Empty;

				try
				{
					HttpResponseMessage httpResponseMessage = await httpClient.GetAsync(urlFragment);

					httpResponseMessage.EnsureSuccessStatusCode();

					html = await httpResponseMessage.Content.ReadAsStringAsync();
				}
				catch (Exception ex)
				{
					throw new McpException($"The API call to fetch documentation for '{url}' failed.", ex);
				}

				string markdown = ParserUtil.ParserDocumentationItem(html);
				results[url] = markdown;
			}

			return JsonSerializer.Serialize(results, new JsonSerializerOptions { WriteIndented = true });
		}
	}
}
