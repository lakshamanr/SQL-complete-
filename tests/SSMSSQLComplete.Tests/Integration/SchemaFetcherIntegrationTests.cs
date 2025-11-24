using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Schema;

namespace SSMSSQLComplete.Tests.Integration
{
    /// <summary>
    /// Integration tests for SchemaFetcher
    /// NOTE: These tests require a SQL Server instance and should be run manually or in CI with proper setup
    /// </summary>
    public class SchemaFetcherIntegrationTests
    {
        private const string TestConnectionString = "Server=(localdb)\\mssqllocaldb;Integrated Security=true;";

        [Fact(Skip = "Requires SQL Server instance")]
        public async Task FetchMetadataAsync_WithValidConnection_ReturnsMetadata()
        {
            // Arrange
            var fetcher = new SchemaFetcher();

            // Act
            var metadata = await fetcher.FetchMetadataAsync(TestConnectionString, "master");

            // Assert
            metadata.Should().NotBeNull();
            metadata.DatabaseName.Should().Be("master");
            metadata.Tables.Should().NotBeEmpty();
        }

        [Fact(Skip = "Requires SQL Server instance")]
        public async Task TestConnectionAsync_WithValidConnection_ReturnsTrue()
        {
            // Arrange
            var fetcher = new SchemaFetcher();

            // Act
            var result = await fetcher.TestConnectionAsync(TestConnectionString);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public async Task TestConnectionAsync_WithInvalidConnection_ReturnsFalse()
        {
            // Arrange
            var fetcher = new SchemaFetcher();
            var invalidConnectionString = "Server=invalid;Database=invalid;";

            // Act
            var result = await fetcher.TestConnectionAsync(invalidConnectionString);

            // Assert
            result.Should().BeFalse();
        }
    }
}
