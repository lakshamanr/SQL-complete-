# Contributing to SSMS SQL Complete

Thank you for your interest in contributing! This document provides guidelines for contributing to the project.

## Development Setup

### Prerequisites
- Visual Studio 2019 or later
- Visual Studio SDK
- .NET Framework 4.7.2 or later
- SQL Server (for integration tests)

### Getting Started
1. Fork the repository
2. Clone your fork: `git clone <your-fork-url>`
3. Open `SSMSSQLComplete.sln` in Visual Studio
4. Restore NuGet packages
5. Build the solution

### Running Tests
```bash
dotnet test
```

### Debugging
1. Set `SSMSSQLComplete` as the startup project
2. Press F5 to launch the experimental instance of Visual Studio/SSMS
3. Your breakpoints will be hit when using the extension

## Code Style

- Follow C# coding conventions
- Use meaningful variable and method names
- Add XML documentation comments for public APIs
- Keep methods focused and concise

## Project Structure

```
SSMSSQLComplete/
├── src/
│   ├── SSMSSQLComplete/        # VSIX project
│   └── SSMSSQLComplete.Core/   # Core library
└── tests/
    └── SSMSSQLComplete.Tests/  # Test project
```

## Submitting Changes

### Pull Request Process
1. Create a feature branch: `git checkout -b feature/your-feature-name`
2. Make your changes
3. Add tests for new functionality
4. Ensure all tests pass
5. Commit with clear messages
6. Push to your fork
7. Create a pull request

### PR Guidelines
- Describe what your PR does
- Reference related issues
- Include test results
- Keep PRs focused on a single feature/fix

## Reporting Issues

When reporting bugs, include:
- SSMS version
- Extension version
- Steps to reproduce
- Expected vs actual behavior
- Error logs (from `%AppData%\SSMSSQLComplete\Logs`)

## Feature Requests

Feature requests are welcome! Please:
- Check existing issues first
- Describe the use case
- Explain how it fits with existing features

## Code Review Process

All PRs require:
- Passing CI builds
- Code review approval
- No merge conflicts
- Updated documentation (if needed)

## Development Workflow

### Adding a New Feature
1. Create an issue describing the feature
2. Discuss approach with maintainers
3. Implement in a feature branch
4. Add unit and integration tests
5. Update documentation
6. Submit PR

### Fixing a Bug
1. Create an issue (if one doesn't exist)
2. Write a failing test that reproduces the bug
3. Fix the bug
4. Verify the test passes
5. Submit PR

## Architecture Guidelines

### Core Principles
- **Separation of Concerns**: Keep UI, business logic, and data access separate
- **Performance**: All operations must be async and non-blocking
- **Testability**: Write testable code with dependency injection
- **Error Handling**: Log errors, never crash SSMS

### Performance Targets
- Completion: < 120ms
- Schema fetch: < 2s (databases ≤ 2000 tables)
- Formatting: < 500ms (files ≤ 5000 lines)

## Testing Guidelines

### Unit Tests
- Test one thing at a time
- Use descriptive test names
- Follow Arrange-Act-Assert pattern
- Use FluentAssertions for readable assertions

### Integration Tests
- Mark with `[Fact(Skip = "Requires SQL Server")]` if needs database
- Clean up test data
- Use transactions when possible

## Documentation

Update documentation when:
- Adding new features
- Changing existing behavior
- Adding configuration options
- Fixing significant bugs

## License

By contributing, you agree that your contributions will be licensed under the MIT License.

## Questions?

Feel free to:
- Open an issue for discussion
- Ask in pull request comments
- Contact maintainers

Thank you for contributing!
