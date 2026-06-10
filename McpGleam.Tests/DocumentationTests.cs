using McpGleam.Utils;
using Models;
using Xunit;

namespace McpGleam.Tests
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
        public void ParserDocumentationItem_WithNullOrEmpty_ReturnsEmptyString()
        {
            // Act & Assert
            Assert.Equal(string.Empty, ParserUtil.ParserDocumentationItem(null!));
            Assert.Equal(string.Empty, ParserUtil.ParserDocumentationItem(string.Empty));
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
            Assert.Contains("<h2>Assignments</h2>", result);
            Assert.Contains("A value can be assigned to a variable using <code>let</code>.", result);
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
            Assert.Contains("<h2>Assignments</h2>", result);
            Assert.Contains("A value can be assigned to a variable using <code>let</code>.", result);
        }
    }
}
