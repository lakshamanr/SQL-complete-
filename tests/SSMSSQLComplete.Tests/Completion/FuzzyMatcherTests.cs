using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Completion;

namespace SSMSSQLComplete.Tests.Completion
{
    public class FuzzyMatcherTests
    {
        [Theory]
        [InlineData("sel", "SELECT", true)]
        [InlineData("usr", "Users", true)]
        [InlineData("cst", "Customers", true)]
        [InlineData("xyz", "SELECT", false)]
        public void IsMatch_VariousPatterns_ReturnsExpectedResult(string pattern, string text, bool expectedMatch)
        {
            // Act
            var result = FuzzyMatcher.IsMatch(pattern, text);

            // Assert
            result.Should().Be(expectedMatch);
        }

        [Fact]
        public void CalculateScore_ExactMatch_ReturnsHighScore()
        {
            // Arrange
            var pattern = "Users";
            var text = "Users";

            // Act
            var score = FuzzyMatcher.CalculateScore(pattern, text);

            // Assert
            score.Should().Be(100.0);
        }

        [Fact]
        public void CalculateScore_StartsWithPattern_ReturnsHighScore()
        {
            // Arrange
            var pattern = "sel";
            var text = "SELECT";

            // Act
            var score = FuzzyMatcher.CalculateScore(pattern, text);

            // Assert
            score.Should().BeGreaterThan(90.0);
        }

        [Fact]
        public void CalculateScore_NoMatch_ReturnsZero()
        {
            // Arrange
            var pattern = "xyz";
            var text = "SELECT";

            // Act
            var score = FuzzyMatcher.CalculateScore(pattern, text);

            // Assert
            score.Should().Be(0.0);
        }
    }
}
