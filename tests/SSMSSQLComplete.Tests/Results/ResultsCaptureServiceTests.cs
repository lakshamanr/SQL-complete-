using Xunit;
using FluentAssertions;
using System.Data;
using SSMSSQLComplete.Core.Results;

namespace SSMSSQLComplete.Tests.Results
{
    public class ResultsCaptureServiceTests
    {
        private readonly ResultsCaptureService _service;

        public ResultsCaptureServiceTests()
        {
            _service = ResultsCaptureService.Instance;
        }

        [Fact]
        public void IsJsonColumn_WithJsonData_ReturnsTrue()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("JsonData", typeof(string));
            table.Rows.Add("{\"key\": \"value\"}");
            table.Rows.Add("{\"another\": \"object\"}");

            var column = table.Columns["JsonData"];

            // Act
            var result = _service.IsJsonColumn(column, table);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void IsJsonColumn_WithNonJsonData_ReturnsFalse()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("Name", typeof(string));
            table.Rows.Add("John Doe");
            table.Rows.Add("Jane Smith");

            var column = table.Columns["Name"];

            // Act
            var result = _service.IsJsonColumn(column, table);

            // Assert
            result.Should().BeFalse();
        }

        [Fact]
        public void IsJsonColumn_WithJsonInColumnName_ChecksData()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("JsonData", typeof(string));
            table.Rows.Add("{\"test\": true}");

            var column = table.Columns["JsonData"];

            // Act
            var result = _service.IsJsonColumn(column, table);

            // Assert
            result.Should().BeTrue();
        }

        [Fact]
        public void GetColumnTypeSummary_ReturnsFormattedSummary()
        {
            // Arrange
            var table = new DataTable();
            table.Columns.Add("Id", typeof(int));
            table.Columns.Add("Name", typeof(string));
            table.Columns.Add("JsonData", typeof(string));
            table.Rows.Add(1, "Test", "{\"key\": \"value\"}");

            // Act
            var summary = _service.GetColumnTypeSummary(table);

            // Assert
            summary.Should().Contain("Id: Int32");
            summary.Should().Contain("Name: String");
            summary.Should().Contain("JsonData");
        }
    }
}
