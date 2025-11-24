# Building SSMS SQL Complete

This document describes how to build and package the SSMS SQL Complete extension.

## Prerequisites

### Required Software
- **Visual Studio 2019 or later** (Community, Professional, or Enterprise)
- **Visual Studio SDK** (install via Visual Studio Installer → Individual Components)
- **.NET Framework 4.7.2 or later**
- **NuGet Package Manager**

### Optional (for development)
- **SQL Server** (LocalDB, Express, or full version) for integration tests
- **Git** for version control

## Building from Source

### 1. Clone the Repository
```bash
git clone https://github.com/yourusername/ssms-sql-complete.git
cd ssms-sql-complete
```

### 2. Restore NuGet Packages
```bash
nuget restore SSMSSQLComplete.sln
```

Or in Visual Studio: Right-click solution → Restore NuGet Packages

### 3. Build the Solution

#### Using Visual Studio
1. Open `SSMSSQLComplete.sln`
2. Select configuration (Debug or Release)
3. Build → Build Solution (Ctrl+Shift+B)

#### Using MSBuild
```bash
msbuild SSMSSQLComplete.sln /p:Configuration=Release
```

### 4. Output Location
- **VSIX Package**: `src/SSMSSQLComplete/bin/Release/SSMSSQLComplete.vsix`
- **Core DLL**: `src/SSMSSQLComplete.Core/bin/Release/SSMSSQLComplete.Core.dll`

## Running Tests

### Unit Tests
```bash
dotnet test tests/SSMSSQLComplete.Tests/SSMSSQLComplete.Tests.csproj
```

### Integration Tests
Integration tests require a SQL Server instance:
```bash
dotnet test --filter "Category!=Integration"
```

To run integration tests:
1. Ensure SQL Server is running
2. Update connection string in test files
3. Run: `dotnet test --filter "Category=Integration"`

## Debugging

### Debug in Experimental Instance
1. Set `SSMSSQLComplete` as startup project
2. Press F5 (or Debug → Start Debugging)
3. Visual Studio experimental instance will launch
4. Open a SQL file to test the extension

### Attach to Running SSMS
1. Start SSMS
2. In Visual Studio: Debug → Attach to Process
3. Select `Ssms.exe`
4. Set breakpoints in your code

## Packaging for Release

### Creating a VSIX Package

1. **Build in Release Mode**
   ```bash
   msbuild SSMSSQLComplete.sln /p:Configuration=Release /p:DeployExtension=false
   ```

2. **VSIX is Generated**
   - Location: `src/SSMSSQLComplete/bin/Release/SSMSSQLComplete.vsix`

3. **Test the VSIX**
   - Close all Visual Studio/SSMS instances
   - Double-click the VSIX file
   - Follow installation wizard
   - Launch SSMS and test functionality

### Signing the VSIX (Optional)

For official releases, sign the VSIX:

```bash
# Generate a certificate (once)
makecert -r -pe -n "CN=YourCompany" -sky signature -sv cert.pvk cert.cer
pvk2pfx -pvk cert.pvk -spc cert.cer -pfx cert.pfx

# Sign the VSIX
signtool sign /f cert.pfx /p password /t http://timestamp.digicert.com SSMSSQLComplete.vsix
```

## Build Configurations

### Debug
- Symbols included
- No optimizations
- Logging enabled
- Quick rebuild

### Release
- Optimized code
- Symbols in separate PDB files
- Minimal logging
- Smaller VSIX size

## Common Build Issues

### Issue: "Cannot find Visual Studio SDK"
**Solution**: Install Visual Studio SDK via Visual Studio Installer

### Issue: "NuGet packages missing"
**Solution**:
```bash
nuget restore SSMSSQLComplete.sln
```

### Issue: "VSIX build fails"
**Solution**:
- Ensure VSSDK is installed
- Clean and rebuild: `Clean Solution` then `Rebuild Solution`

### Issue: "Cannot find Microsoft.VisualStudio.*"
**Solution**: Install required VS SDK components:
- Visual Studio extension development workload
- .NET Framework 4.7.2 targeting pack

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Build

on: [push, pull_request]

jobs:
  build:
    runs-on: windows-latest
    steps:
      - uses: actions/checkout@v2

      - name: Setup MSBuild
        uses: microsoft/setup-msbuild@v1

      - name: Restore NuGet
        run: nuget restore SSMSSQLComplete.sln

      - name: Build
        run: msbuild SSMSSQLComplete.sln /p:Configuration=Release

      - name: Test
        run: dotnet test --no-build --configuration Release

      - name: Upload VSIX
        uses: actions/upload-artifact@v2
        with:
          name: VSIX Package
          path: src/SSMSSQLComplete/bin/Release/*.vsix
```

## Version Management

Version is defined in:
- `src/SSMSSQLComplete/source.extension.vsixmanifest`
- `src/SSMSSQLComplete/Properties/AssemblyInfo.cs`
- `src/SSMSSQLComplete.Core/Properties/AssemblyInfo.cs`

Update all three for new releases.

## Distribution

### Manual Distribution
1. Build VSIX in Release mode
2. Test thoroughly
3. Upload to GitHub Releases
4. Provide installation instructions

### Visual Studio Marketplace
1. Create publisher account
2. Upload VSIX via marketplace portal
3. Fill in metadata (description, screenshots, etc.)
4. Publish

## Support

For build issues:
- Check [CONTRIBUTING.md](CONTRIBUTING.md)
- Open an issue on GitHub
- Include build logs and error messages

## Additional Resources

- [Visual Studio SDK Documentation](https://docs.microsoft.com/en-us/visualstudio/extensibility/)
- [VSIX Project Templates](https://docs.microsoft.com/en-us/visualstudio/extensibility/creating-an-extension-with-a-menu-command)
- [MSBuild Reference](https://docs.microsoft.com/en-us/visualstudio/msbuild/msbuild-reference)
