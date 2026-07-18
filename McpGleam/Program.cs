using McpGleam.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Logging.ClearProviders();
builder.Logging.AddConsole(options =>
{
	options.LogToStandardErrorThreshold = LogLevel.Trace;
});

builder.Services.AddHttpClient(HttpConstants.GleamDocsEndpointName, options => {
	options.BaseAddress = new Uri("https://tour.gleam.run/");
});

builder.Services
	.AddMcpServer(options => {
		options.ServerInfo = new()
		{
			Name = "gleamProgrammingDocumentation",
			Version = "1.2.1",
			Description = "MCP server for collecting documentation for the Gleam Programming language."
		};
	})
	.WithToolsFromAssembly()
	.WithStdioServerTransport();


await builder.Build().RunAsync();
