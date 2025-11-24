using System.Collections.Generic;
using System.Linq;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Completion;

namespace SSMSSQLComplete.Tests.Completion
{
    public class CompletionRankerTests
    {
        private readonly CompletionRanker _ranker;

        public CompletionRankerTests()
        {
            _ranker = new CompletionRanker();
        }

        [Fact]
        public void RankCompletions_WithMatchingPrefix_RanksCorrectly()
        {
            // Arrange
            var completions = new List<CompletionItem>
            {
                new CompletionItem("Users", CompletionItemKind.Table),
                new CompletionItem("UserRoles", CompletionItemKind.Table),
                new CompletionItem("Products", CompletionItemKind.Table)
            };

            var context = new CompletionContext
            {
                Text = "SELECT * FROM U",
                Position = 15
            };

            // Act
            var ranked = _ranker.RankCompletions(completions, context).ToList();

            // Assert
            ranked.Should().NotBeEmpty();
            ranked.First().Label.Should().StartWith("U");
        }

        [Fact]
        public void RankCompletions_EmptyList_ReturnsEmpty()
        {
            // Arrange
            var completions = new List<CompletionItem>();
            var context = new CompletionContext { Text = "SELECT", Position = 6 };

            // Act
            var ranked = _ranker.RankCompletions(completions, context);

            // Assert
            ranked.Should().BeEmpty();
        }
    }
}
