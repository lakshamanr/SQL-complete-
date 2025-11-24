using System.Collections.Generic;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Snippets;

namespace SSMSSQLComplete.Tests.Snippets
{
    public class SnippetExpanderTests
    {
        private readonly SnippetExpander _expander;

        public SnippetExpanderTests()
        {
            _expander = new SnippetExpander();
        }

        [Fact]
        public void Expand_WithDefaultValues_ReplacesPlaceholders()
        {
            // Arrange
            var snippet = new Snippet
            {
                Code = "SELECT * FROM ${TableName}",
                Parameters = new List<SnippetParameter>
                {
                    new SnippetParameter("TableName", "Users")
                }
            };

            // Act
            var expanded = _expander.Expand(snippet);

            // Assert
            expanded.Should().Be("SELECT * FROM Users");
        }

        [Fact]
        public void Expand_WithCustomValues_ReplacesWithCustom()
        {
            // Arrange
            var snippet = new Snippet
            {
                Code = "SELECT * FROM ${TableName}",
                Parameters = new List<SnippetParameter>
                {
                    new SnippetParameter("TableName", "Users")
                }
            };

            var values = new Dictionary<string, string>
            {
                { "TableName", "Customers" }
            };

            // Act
            var expanded = _expander.Expand(snippet, values);

            // Assert
            expanded.Should().Be("SELECT * FROM Customers");
        }

        [Fact]
        public void ExtractFields_FromSnippet_ReturnsAllFields()
        {
            // Arrange
            var snippet = new Snippet
            {
                Code = "INSERT INTO ${TableName} (${Columns}) VALUES (${Values})",
                Parameters = new List<SnippetParameter>
                {
                    new SnippetParameter("TableName", "Users"),
                    new SnippetParameter("Columns", "Id, Name"),
                    new SnippetParameter("Values", "1, 'John'")
                }
            };

            // Act
            var fields = _expander.ExtractFields(snippet);

            // Assert
            fields.Should().HaveCount(3);
            fields.Should().Contain(f => f.Name == "TableName");
            fields.Should().Contain(f => f.Name == "Columns");
            fields.Should().Contain(f => f.Name == "Values");
        }
    }
}
