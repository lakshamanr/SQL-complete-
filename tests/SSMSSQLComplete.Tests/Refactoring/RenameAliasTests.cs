using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Refactoring;

namespace SSMSSQLComplete.Tests.Refactoring
{
    public class RenameAliasTests
    {
        [Fact]
        public async Task ApplyAsync_RenamesAllOccurrences()
        {
            // Arrange
            var refactoring = new RenameAliasRefactoring("u", "usr");
            var sql = "SELECT u.Id FROM Users u WHERE u.Name = 'John'";

            // Act
            var result = await refactoring.ApplyAsync(sql, 0);

            // Assert
            result.Success.Should().BeTrue();
            result.ModifiedSql.Should().Contain("usr");
            result.ModifiedSql.Should().NotContain(" u.");
        }

        [Fact]
        public void CanApply_WithValidAliases_ReturnsTrue()
        {
            // Arrange
            var refactoring = new RenameAliasRefactoring("u", "usr");
            var sql = "SELECT u.Id FROM Users u";

            // Act
            var canApply = refactoring.CanApply(sql, 0);

            // Assert
            canApply.Should().BeTrue();
        }
    }
}
