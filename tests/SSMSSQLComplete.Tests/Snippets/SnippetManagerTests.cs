using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Snippets;

namespace SSMSSQLComplete.Tests.Snippets
{
    public class SnippetManagerTests
    {
        [Fact]
        public void GetAllSnippets_ReturnsSnippets()
        {
            // Arrange
            var manager = SnippetManager.Instance;

            // Act
            var snippets = manager.GetAllSnippets();

            // Assert
            snippets.Should().NotBeNull();
        }

        [Fact]
        public void GetSnippetByShortcut_ValidShortcut_ReturnsSnippet()
        {
            // Arrange
            var manager = SnippetManager.Instance;

            // Act
            var snippet = manager.GetSnippetByShortcut("sel");

            // Assert
            snippet.Should().NotBeNull();
        }
    }
}
