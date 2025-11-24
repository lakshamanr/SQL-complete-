# Changelog

All notable changes to SSMS SQL Complete will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Added
- Initial project structure
- Schema-aware SQL autocompletion
- IntelliSense enhancements with context-aware suggestions
- SQL formatting engine with multiple profiles
- Code snippet manager with parameter substitution
- Quick info tooltips for database objects
- Refactoring tools (expand SELECT *, qualify identifiers, rename alias, extract to CTE)
- Tools → Options integration for all settings
- Telemetry and logging infrastructure
- Comprehensive unit and integration tests
- VSIX packaging

### Features
- Keyword suggestions based on SQL context
- Table and column suggestions from schema
- JOIN predicate suggestions from foreign key relationships
- Fuzzy matching for completions
- Performance optimization (< 120ms completion, < 2s schema fetch)
- Windows Forms UI for snippet management
- Support for SSMS 17, 18.x, and 19.x

## [1.0.0] - TBD

### Initial Release
- Complete implementation of all MVP features
- Full documentation
- Tested on SSMS 18.x and 19.x
- Ready for production use

---

## Version History

### Version Numbering
- **Major**: Breaking changes or major feature releases
- **Minor**: New features, backwards compatible
- **Patch**: Bug fixes and small improvements

### Categories
- **Added**: New features
- **Changed**: Changes in existing functionality
- **Deprecated**: Soon-to-be removed features
- **Removed**: Removed features
- **Fixed**: Bug fixes
- **Security**: Security fixes
