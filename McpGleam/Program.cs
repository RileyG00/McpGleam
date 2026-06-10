using McpGleam.Constants;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;


HostApplicationBuilder builder = Host.CreateApplicationBuilder(args);

builder.Services.AddHttpClient(HttpConstants.GleamDocsEndpointName, options => {
	options.BaseAddress = new Uri("https://tour.gleam.run/");
});

builder.Services
	.AddMcpServer(options => {
		options.ServerInfo = new()
		{
			Name = "gleamProgrammingDocumentation",
			Version = "0.0.1-beta",
			Description = "MCP server for collecting documentation for the Gleam Programming language."
		};
	})
	.WithToolsFromAssembly()
	.WithStdioServerTransport();


await builder.Build().RunAsync();
