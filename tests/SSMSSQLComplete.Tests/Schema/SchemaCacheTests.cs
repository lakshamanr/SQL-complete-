using System;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Schema;

namespace SSMSSQLComplete.Tests.Schema
{
    public class SchemaCacheTests
    {
        [Fact]
        public void Set_And_TryGet_StoresAndRetrievesMetadata()
        {
            // Arrange
            var cache = new SchemaCache(TimeSpan.FromMinutes(5));
            var metadata = new DatabaseMetadata { DatabaseName = "TestDB" };

            // Act
            cache.Set("testkey", metadata);
            var retrieved = cache.TryGet("testkey", out var result);

            // Assert
            retrieved.Should().BeTrue();
            result.Should().NotBeNull();
            result.DatabaseName.Should().Be("TestDB");
        }

        [Fact]
        public void TryGet_NonExistentKey_ReturnsFalse()
        {
            // Arrange
            var cache = new SchemaCache();

            // Act
            var retrieved = cache.TryGet("nonexistent", out var result);

            // Assert
            retrieved.Should().BeFalse();
            result.Should().BeNull();
        }

        [Fact]
        public void Remove_RemovesFromCache()
        {
            // Arrange
            var cache = new SchemaCache();
            var metadata = new DatabaseMetadata { DatabaseName = "TestDB" };
            cache.Set("testkey", metadata);

            // Act
            cache.Remove("testkey");
            var retrieved = cache.TryGet("testkey", out var result);

            // Assert
            retrieved.Should().BeFalse();
        }
    }
}
