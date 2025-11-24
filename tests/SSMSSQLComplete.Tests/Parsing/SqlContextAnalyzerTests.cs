using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Parsing;

namespace SSMSSQLComplete.Tests.Parsing
{
    public class SqlContextAnalyzerTests
    {
        private readonly SqlContextAnalyzer _analyzer;

        public SqlContextAnalyzerTests()
        {
            _analyzer = new SqlContextAnalyzer();
        }

        [Fact]
        public void AnalyzeContext_SimpleSelect_ReturnsSelectClause()
        {
            // Arrange
            var sql = "SELECT * FROM Users";
            var position = 7; // After "SELECT "

            // Act
            var context = _analyzer.AnalyzeContext(sql, position);

            // Assert
            context.CurrentClause.Should().Be("SELECT");
        }

        [Fact]
        public void AnalyzeContext_FromClause_ReturnsFromClause()
        {
            // Arrange
            var sql = "SELECT * FROM Users";
            var position = 15; // After "FROM "

            // Act
            var context = _analyzer.AnalyzeContext(sql, position);

            // Assert
            context.CurrentClause.Should().Be("FROM");
        }
    }
}
