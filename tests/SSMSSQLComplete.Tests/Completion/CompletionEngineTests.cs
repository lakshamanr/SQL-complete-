using System.Linq;
using System.Threading.Tasks;
using Xunit;
using FluentAssertions;
using SSMSSQLComplete.Core.Completion;

namespace SSMSSQLComplete.Tests.Completion
{
    public class CompletionEngineTests
    {
        private readonly CompletionEngine _engine;

        public CompletionEngineTests()
        {
            _engine = new CompletionEngine();
        }

        [Fact]
        public async Task GetCompletionsAsync_SimpleQuery_ReturnsCompletions()
        {
            // Arrange
            var text = "SELECT ";
            var position = 7;
            var trigger = CompletionTrigger.Invoked;

            // Act
            var completions = await _engine.GetCompletionsAsync(text, position, trigger);

            // Assert
            completions.Should().NotBeEmpty();
        }

        [Fact]
        public async Task GetCompletionsAsync_EmptyText_ReturnsEmpty()
        {
            // Arrange
            var text = "";
            var position = 0;
            var trigger = CompletionTrigger.Invoked;

            // Act
            var completions = await _engine.GetCompletionsAsync(text, position, trigger);

            // Assert
            completions.Should().BeEmpty();
        }
    }
}
