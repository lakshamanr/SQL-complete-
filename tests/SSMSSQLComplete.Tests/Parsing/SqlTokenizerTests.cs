using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Tests.Parsing
{
    public class SqlTokenizerTests
    {
        private readonly SqlTokenizer _tokenizer;

        public SqlTokenizerTests()
        {
            _tokenizer = new SqlTokenizer();
        }

        [Fact]
        public void Tokenize_SimpleSelect_ReturnsCorrectTokens()
        {
            // Arrange
            var sql = "SELECT * FROM Users";

            // Act
            var tokens = _tokenizer.Tokenize(sql);

            // Assert
            tokens.Should().NotBeEmpty();
            tokens.Should().Contain(t => t.Type == SqlTokenType.Keyword && t.Text == "SELECT");
            tokens.Should().Contain(t => t.Type == SqlTokenType.Operator && t.Text == "*");
            tokens.Should().Contain(t => t.Type == SqlTokenType.Keyword && t.Text == "FROM");
            tokens.Should().Contain(t => t.Type == SqlTokenType.Identifier && t.Text == "Users");
        }

        [Fact]
        public void Tokenize_StringLiteral_PreservesQuotes()
        {
            // Arrange
            var sql = "SELECT 'Hello World'";

            // Act
            var tokens = _tokenizer.Tokenize(sql);

            // Assert
            tokens.Should().Contain(t => t.Type == SqlTokenType.StringLiteral && t.Text == "'Hello World'");
        }

        [Fact]
        public void Tokenize_Comment_IdentifiesAsComment()
        {
            // Arrange
            var sql = "SELECT * -- This is a comment\nFROM Users";

            // Act
            var tokens = _tokenizer.Tokenize(sql);

            // Assert
            tokens.Should().Contain(t => t.Type == SqlTokenType.Comment);
        }

        [Fact]
        public void Tokenize_MultilineComment_IdentifiesCorrectly()
        {
            // Arrange
            var sql = "SELECT /* multiline\ncomment */ * FROM Users";

            // Act
            var tokens = _tokenizer.Tokenize(sql);

            // Assert
            tokens.Should().Contain(t => t.Type == SqlTokenType.Comment && t.Text.Contains("multiline"));
        }
    }
}
