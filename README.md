# McpGleamDocs: Gleam Programming Language Documentation MCP Server (v1.2.2)

`McpGleamDocs` is a Model Context Protocol (MCP) server written in C# (.NET 10.0) that fetches and parses documentation for the Gleam programming language from [The Gleam Tour](https://tour.gleam.run/). It allows LLMs and MCP clients to query available documentation categories and retrieve up-to-date documentation content.

---

## Features

The server exposes the following MCP tools:

1. **`list_available_docs`**
   - **Description**: Returns a JSON array containing all available documentation items, their category, and the path/URL to fetch additional information.
   - **Usage**: Use this to discover what Gleam concepts, syntax, and libraries are documented in the tour.
   - **Response Format**:
     ```json
     [
       {
         "Category": "Basics",
         "Name": "Hello world",
         "Path": "/basics/hello-world"
       }
     ]
     ```

2. **`get_item_documentation`**
   - **Description**: Fetches the up-to-date documentation details for one or many items by the URL/path fragment (e.g. `basics/hello-world`).
   - **Parameters**:
     - `documentationUrl` (string[]): The URL path fragments at which the documentation items can be retrieved. Supports relative paths (e.g., `basics/hello-world` or `/basics/hello-world`) as well as absolute URLs (e.g., `https://tour.gleam.run/basics/hello-world`).
   - **Response Format**:
     ```json
     {
       "basics/hello-world": "## Hello world\n\nHere is a tiny program that prints out text...\n\n### Example Code\n```gleam\nimport gleam/io\npub fn main() {\n  io.println(\"Hello, Joe!\")\n}\n```"
     }
     ```

---

## Project Structure

- **[Program.cs](file:///c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/Program.cs)**: Main entry point that sets up the dependency injection, HTTP clients, registers MCP tools, and starts the Stdio server transport.
- **[Tools/Documentation.cs](file:///c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/Tools/Documentation.cs)**: Contains the MCP tool implementations and descriptions.
- **[Utils/ParserUtil.cs](file:///c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/Utils/ParserUtil.cs)**: Helper class using regular expressions to parse the Table of Contents and clean up html fragments into raw text documentation content.
- **[Models/DocumentationItem.cs](file:///c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/Models/DocumentationItem.cs)**: Holds structured data for parsed items (Category, Name, Path).
- **[McpGleamDocs.Tests/](file:///c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs.Tests/)**: Unit tests verifying parser behaviors.

---

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or higher.

---

## Building and Running

### Build the Project
To compile the project, run:
```bash
dotnet build
```

### Run Tests
To run unit tests:
```bash
dotnet test
```

### Run the Server
The server runs over Standard Input/Output (Stdio) transport:
```bash
dotnet run --project McpGleamDocs/McpGleamDocs.csproj
```

---

## Configuration for MCP Clients

To integrate `McpGleamDocs` with MCP clients (such as Claude Desktop or Cursor), add the following server configuration to your `claude_desktop_config.json` or equivalent client configuration file:

### Option 1: Running via `dotnet run` (Development)
```json
{
  "mcpServers": {
    "gleam-docs": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/McpGleamDocs.csproj"
      ]
    }
  }
}
```

### Option 2: Running the Published Executable (Production)
First, publish the application:
```bash
dotnet publish McpGleamDocs/McpGleamDocs.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Then configure the client to run the executable:
```json
{
  "mcpServers": {
    "gleam-docs": {
      "command": "c:/Users/Riley/source/repos/mcp-gleam-docs/McpGleamDocs/bin/Release/net10.0/win-x64/publish/McpGleamDocs.exe"
    }
  }
}
```

---

## License

This project is licensed under the terms of the license file included in the root directory.
