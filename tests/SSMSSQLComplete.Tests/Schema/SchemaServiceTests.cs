using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Schema;

namespace SSMSSQLComplete.Tests.Schema
{
    public class SchemaServiceTests
    {
        [Fact]
        public void Instance_ReturnsSingletonInstance()
        {
            // Act
            var instance1 = SchemaService.Instance;
            var instance2 = SchemaService.Instance;

            // Assert
            instance1.Should().BeSameAs(instance2);
        }

        [Fact]
        public void IsSchemaLoaded_Initially_ReturnsFalse()
        {
            // Arrange
            var service = SchemaService.Instance;

            // Act & Assert
            // Note: This may vary depending on when tests run
            service.IsSchemaLoaded.Should().BeFalse();
        }
    }
}
