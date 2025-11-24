using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Formatting;

namespace SSMSSQLComplete.Tests.Formatting
{
    public class SqlFormatterTests
    {
        private readonly SqlFormatter _formatter;

        public SqlFormatterTests()
        {
            _formatter = new SqlFormatter();
        }

        [Fact]
        public void Format_SimpleSelect_FormatsCorrectly()
        {
            // Arrange
            var sql = "select * from users where id=1";

            // Act
            var formatted = _formatter.Format(sql);

            // Assert
            formatted.Should().Contain("SELECT");
            formatted.Should().Contain("FROM");
            formatted.Should().Contain("WHERE");
        }

        [Fact]
        public void Format_WithKeywords_ConvertsToUppercase()
        {
            // Arrange
            var sql = "select id, name from users";

            // Act
            var formatted = _formatter.Format(sql);

            // Assert
            formatted.Should().Contain("SELECT");
            formatted.Should().Contain("FROM");
        }

        [Fact]
        public void Format_EmptyString_ReturnsEmpty()
        {
            // Arrange
            var sql = "";

            // Act
            var formatted = _formatter.Format(sql);

            // Assert
            formatted.Should().BeEmpty();
        }
    }
}
