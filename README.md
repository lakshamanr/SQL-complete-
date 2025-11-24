# SSMS SQL Complete

Advanced SQL code completion, formatting, and refactoring add-in for Microsoft SQL Server Management Studio (SSMS).

## Features

### MVP Features
- **Schema-aware SQL Autocomplete**: Intelligent suggestions for tables, columns, views, stored procedures, and functions
- **IntelliSense Enhancements**: Smart JOIN suggestions, context-aware completions, snippet expansions
- **SQL Formatting**: Customizable formatting engine with multiple profiles
- **Code Snippets**: Manage and insert reusable SQL code templates
- **Quick Info Tooltips**: Display column types, constraints, nullability, and relationships
- **Basic Refactorings**: Expand SELECT *, rename aliases, qualify identifiers
- **Enhanced Results Viewer**: View query results with automatic JSON/XML detection and formatting

### Supported SSMS Versions
- SSMS 2017
- SSMS 18.x
- SSMS 19.x

## Architecture

The add-in consists of several core components:

1. **Completion Provider**: Integrates with SSMS editor to provide context-aware suggestions
2. **Schema Service**: Caches database metadata for fast lookups
3. **Formatting Engine**: Parses and formats T-SQL code
4. **Snippet Manager**: Manages code templates and snippets
5. **Results Viewer**: Enhanced results display with JSON/XML formatting
6. **Configuration System**: User settings and preferences
7. **Telemetry & Logging**: Optional usage analytics and error logging

## Project Structure

```
SSMSSQLComplete/
├── src/
│   ├── SSMSSQLComplete/           # Main VSIX project
│   │   ├── Package/               # VSIX package files
│   │   ├── Commands/              # SSMS commands
│   │   ├── Editor/                # Editor integration
│   │   ├── UI/                    # Options pages and dialogs
│   │   └── SSMSSQLCompletePackage.cs
│   └── SSMSSQLComplete.Core/      # Core library
│       ├── Completion/            # Completion engine
│       ├── Schema/                # Schema metadata service
│       ├── Formatting/            # SQL formatter
│       ├── Snippets/              # Snippet management
│       ├── Refactoring/           # Refactoring tools
│       ├── Results/               # Results viewer service
│       └── Config/                # Configuration
└── tests/
    └── SSMSSQLComplete.Tests/     # Unit and integration tests
```

## Building

### Prerequisites
- Visual Studio 2019 or later
- Visual Studio SDK
- .NET Framework 4.7.2 or later

### Build Steps
```bash
# Clone the repository
git clone <repository-url>
cd SQL-complete-

# Restore NuGet packages
nuget restore SSMSSQLComplete.sln

# Build the solution
msbuild SSMSSQLComplete.sln /p:Configuration=Release
```

## Installation

1. Close all instances of SSMS
2. Run the generated `SSMSSQLComplete.vsix` file
3. Follow the installation wizard
4. Restart SSMS

## Configuration

Access settings via: **Tools → Options → SSMS SQL Complete**

### Available Settings
- **Completion**: Enable/disable features, suggestion behavior
- **Schema Cache**: Cache size, refresh intervals
- **Formatting**: Formatting profiles, keyword casing, indentation
- **Snippets**: Manage custom snippets
- **Telemetry**: Enable/disable anonymous usage data

## Usage

### Code Completion
- Type to trigger suggestions automatically
- Press `Ctrl+Space` to manually invoke completion
- Use arrow keys to navigate, `Enter` to accept

### Formatting
- **Format Document**: `Ctrl+K, Ctrl+D`
- **Format Selection**: `Ctrl+K, Ctrl+F`

### Snippets
- Type snippet shortcut and press `Tab`
- Manage snippets via **Tools → SSMS SQL Complete → Snippets**

### Refactoring
- Right-click in editor → **SSMS SQL Complete** menu
- Available refactorings:
  - Expand SELECT *
  - Qualify/Unqualify identifiers
  - Rename alias
  - Extract to CTE

### Enhanced Results Viewer
- Access via **Tools → SSMS SQL Complete → Show Enhanced Results Viewer**
- **Features**:
  - **Automatic JSON Detection**: Columns containing JSON are automatically detected and highlighted
  - **JSON Formatting**: Click on any JSON cell to see beautifully formatted JSON in the detail panel
  - **XML Support**: XML data is also detected and formatted
  - **Split View**: Top panel shows the grid, bottom panel shows formatted details
  - **Visual Indicators**: JSON columns are marked with 📄 icon and blue text
  - **Color Coding**:
    - Yellow background = JSON data
    - Green background = XML data
    - Gray text = NULL values
  - **Smart Truncation**: Long values are truncated in the grid but fully visible in detail view

## Development

### Running in Debug Mode
1. Set `SSMSSQLComplete` as startup project
2. Press F5 to launch experimental instance of SSMS
3. Debug breakpoints will be hit in Visual Studio

### Adding Features
See [CONTRIBUTING.md](CONTRIBUTING.md) for development guidelines.

## Performance

- Completion popup: < 120ms
- Schema fetch: < 2 seconds (databases ≤ 2000 tables)
- Formatting: < 500ms (files ≤ 5000 lines)

## Testing

```bash
# Run all tests
dotnet test

# Run specific test category
dotnet test --filter Category=Integration
```

## Roadmap

### Version 1.0 (MVP)
- [x] Basic project structure
- [ ] Schema-aware completion
- [ ] SQL formatting
- [ ] Snippet management
- [ ] Basic refactorings

### Version 1.1
- [ ] Natural language → SQL assistant
- [ ] Execution plan hints
- [ ] Static code analysis (linting)
- [ ] Custom themes

### Version 2.0
- [ ] AI-powered code optimization
- [ ] Advanced refactorings
- [ ] Team snippet sharing
- [ ] Live templates

## License

[Add your license here]

## Support

For issues and feature requests, please use the [GitHub issue tracker](https://github.com/yourusername/ssms-sql-complete/issues).

## Credits

Built with:
- Visual Studio SDK
- ANTLR4 for SQL parsing
- [Other dependencies]
