using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Refactoring;

namespace SSMSSQLComplete.Tests.Refactoring
{
    public class ExpandSelectStarTests
    {
        private readonly ExpandSelectStarRefactoring _refactoring;

        public ExpandSelectStarTests()
        {
            _refactoring = new ExpandSelectStarRefactoring();
        }

        [Fact]
        public void CanApply_SelectStarQuery_ReturnsTrue()
        {
            // Arrange
            var sql = "SELECT * FROM Users";

            // Act
            var canApply = _refactoring.CanApply(sql, 0);

            // Assert
            canApply.Should().BeTrue();
        }

        [Fact]
        public void CanApply_NoSelectStar_ReturnsFalse()
        {
            // Arrange
            var sql = "SELECT Id, Name FROM Users";

            // Act
            var canApply = _refactoring.CanApply(sql, 0);

            // Assert
            canApply.Should().BeFalse();
        }
    }
}
