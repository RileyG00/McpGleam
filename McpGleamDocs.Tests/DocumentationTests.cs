using McpGleamDocs.Utils;
using Models;
using Xunit;

namespace McpGleamDocs.Tests
{
    public class DocumentationTests
    {
        [Fact]
        public void ParseTableOfContents_WithNullOrEmpty_ReturnsEmptyList()
        {
            // Act
            var resultNull = ParserUtil.ParseTableOfContents(null!);
            var resultEmpty = ParserUtil.ParseTableOfContents(string.Empty);

            // Assert
            Assert.Empty(resultNull);
            Assert.Empty(resultEmpty);
        }

        [Fact]
        public void ParseTableOfContents_WithValidHtml_ParsesCorrectly()
        {
            // Arrange
            string html = @"
                <div class='toc'>
                    <h3 class=""mb-0"">Basics</h3>
                    <ul>
                        <li><a href=""/basics/hello-world"">Hello world</a></li>
                        <li><a href=""/basics/modules"">Modules</a></li>
                    </ul>
                    <h3 class=""mb-0"">Functions &amp; Methods</h3>
                    <ul>
                        <li><a href=""/functions/functions"">Functions</a></li>
                    </ul>
                </div>";

            // Act
            var items = ParserUtil.ParseTableOfContents(html);

            // Assert
            Assert.NotNull(items);
            Assert.Equal(3, items.Count);

            // Item 1
            Assert.Equal("Basics", items[0].Category);
            Assert.Equal("Hello world", items[0].Name);
            Assert.Equal("/basics/hello-world", items[0].Path);

            // Item 2
            Assert.Equal("Basics", items[1].Category);
            Assert.Equal("Modules", items[1].Name);
            Assert.Equal("/basics/modules", items[1].Path);

            // Item 3 (decoding verification)
            Assert.Equal("Functions & Methods", items[2].Category);
            Assert.Equal("Functions", items[2].Name);
            Assert.Equal("/functions/functions", items[2].Path);
        }

        [Fact]
        public void ParseTableOfContents_WithNoMatches_ReturnsEmptyList()
        {
            // Arrange
            string html = "<html><body><h1>Hello World</h1><p>Not a TOC</p></body></html>";

            // Act
            var items = ParserUtil.ParseTableOfContents(html);

            // Assert
            Assert.Empty(items);
        }

        [Fact]
        public void ParserDocumentationItem_WithNullOrEmpty_ThrowsException()
        {
            // Act & Assert
            Assert.Throws<ModelContextProtocol.McpException>(() => ParserUtil.ParserDocumentationItem(null!));
            Assert.Throws<ModelContextProtocol.McpException>(() => ParserUtil.ParserDocumentationItem(string.Empty));
        }

        [Fact]
        public void ParserDocumentationItem_WithValidHtml_ExtractsContentCorrectly()
        {
            // Arrange
            string html = @"
                <section id=""left"" class=""content-nav"">
                    <div>
                        <h2>Assignments</h2>
                        <p>A value can be assigned to a variable using <code>let</code>.</p>
                    </div>
                    <nav class=""prev-next"">
                        <a href=""/basics/bools"">Back</a>
                    </nav>
                </section>";

            // Act
            string result = ParserUtil.ParserDocumentationItem(html);

            // Assert
            Assert.Contains("## Assignments", result);
            Assert.Contains("A value can be assigned to a variable using `let`.", result);
        }

        [Fact]
        public void ParserDocumentationItem_Fallback_ExtractsContentCorrectly()
        {
            // Arrange (without section id="left")
            string html = @"
                <div>
                    <h2>Assignments</h2>
                    <p>A value can be assigned to a variable using <code>let</code>.</p>
                </div>";

            // Act
            string result = ParserUtil.ParserDocumentationItem(html);

            // Assert
            Assert.Contains("## Assignments", result);
            Assert.Contains("A value can be assigned to a variable using `let`.", result);
        }

        [Fact]
        public void ParserDocumentationItem_WithPlaygroundCode_ExtractsMarkdownAndCode()
        {
            // Arrange
            string html = @"
                <section id=""left"" class=""content-nav"">
                    <div>
                        <h2>Hello world</h2>
                        <p>Here is a tiny program that prints out text.</p>
                    </div>
                </section>
                <script type=""gleam"" id=""code"">
                import gleam/io
                pub fn main() {
                  io.println(""Hello, Joe!"")
                }
                </script>";

            // Act
            string result = ParserUtil.ParserDocumentationItem(html);

            // Assert
            Assert.Contains("## Hello world", result);
            Assert.Contains("Here is a tiny program that prints out text.", result);
            Assert.Contains("### Example Code", result);
            Assert.Contains("```gleam", result);
            Assert.Contains("import gleam/io", result);
            Assert.Contains("io.println(\"Hello, Joe!\")", result);
        }

        [Fact]
        public void ConvertHtmlToMarkdown_ConvertsTagsCorrectly()
        {
            // Arrange
            string html = "<h2>Title</h2><p>Click <a href=\"/relative\">link</a>.</p><ul><li>One</li><li>Two</li></ul>";

            // Act
            string result = ParserUtil.ConvertHtmlToMarkdown(html);

            // Assert
            Assert.Contains("## Title", result);
            Assert.Contains("[link](https://tour.gleam.run/relative)", result);
            Assert.Contains("* One", result);
            Assert.Contains("* Two", result);
        }

    }
}
