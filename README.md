# McpGleam: Gleam Programming Language Documentation MCP Server

`McpGleam` is a Model Context Protocol (MCP) server written in C# (.NET 10.0) that fetches and parses documentation for the Gleam programming language from [The Gleam Tour](https://tour.gleam.run/). It allows LLMs and MCP clients to query available documentation categories and retrieve up-to-date documentation content.

---

## Features

The server exposes the following MCP tools:

1. **`list_available_docs`**
   - **Description**: Returns a JSON array containing all available documentation items, their category, and the path/URL to fetch additional information.
   - **Usage**: Use this to discover what Gleam concepts, syntax, and libraries are documented in the tour.

2. **`get_item_documentation`**
   - **Description**: Fetches the up-to-date documentation details for a single item by its URL/path fragment (e.g. `basics/hello-world`).
   - **Parameters**:
     - `documentationUrl` (string): The URL path fragment at which the documentation can be retrieved.

3. **`get_multiple_item_documentation`**
   - **Description**: Fetches up-to-date documentation details for one or more URLs. Useful for batch requests.
   - **Parameters**:
     - `documentationUrl` (string[]): Array of URL path fragments.

---

## Project Structure

- **[Program.cs](file:///c:/Users/rg102529/source/repos/McpGleam/McpGleam/Program.cs)**: Main entry point that sets up the dependency injection, HTTP clients, registers MCP tools, and starts the Stdio server transport.
- **[Tools/Documentation.cs](file:///c:/Users/rg102529/source/repos/McpGleam/McpGleam/Tools/Documentation.cs)**: Contains the MCP tool implementations and descriptions.
- **[Utils/ParserUtil.cs](file:///c:/Users/rg102529/source/repos/McpGleam/McpGleam/Utils/ParserUtil.cs)**: Helper class using regular expressions to parse the Table of Contents and clean up html fragments into raw text documentation content.
- **[Models/DocumentationItem.cs](file:///c:/Users/rg102529/source/repos/McpGleam/McpGleam/Models/DocumentationItem.cs)**: Holds structured data for parsed items (Category, Name, Path).
- **[McpGleam.Tests/](file:///c:/Users/rg102529/source/repos/McpGleam/McpGleam.Tests/)**: Unit tests verifying parser behaviors.

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
dotnet run --project McpGleam/McpGleam.csproj
```

---

## Configuration for MCP Clients

To integrate `McpGleam` with MCP clients (such as Claude Desktop or Cursor), add the following server configuration to your `claude_desktop_config.json` or equivalent client configuration file:

### Option 1: Running via `dotnet run` (Development)
```json
{
  "mcpServers": {
    "gleam-docs": {
      "command": "dotnet",
      "args": [
        "run",
        "--project",
        "c:/Users/rg102529/source/repos/McpGleam/McpGleam/McpGleam.csproj"
      ]
    }
  }
}
```

### Option 2: Running the Published Executable (Production)
First, publish the application:
```bash
dotnet publish McpGleam/McpGleam.csproj -c Release -r win-x64 --self-contained true -p:PublishSingleFile=true
```

Then configure the client to run the executable:
```json
{
  "mcpServers": {
    "gleam-docs": {
      "command": "c:/Users/rg102529/source/repos/McpGleam/McpGleam/bin/Release/net10.0/win-x64/publish/McpGleam.exe"
    }
  }
}
```

---

## License

This project is licensed under the terms of the license file included in the root directory.
