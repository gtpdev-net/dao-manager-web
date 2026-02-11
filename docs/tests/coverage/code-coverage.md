# Code Coverage Setup

This repository is configured with automatic code coverage collection using Microsoft's standard tooling.

## 🎯 What's Configured

- **`coverlet.collector`** - Collection during test runs
- **`coverlet.msbuild`** - MSBuild integration for automatic coverage
- **Central Configuration** - Coverage settings in `Directory.Build.props`
- **Multiple Formats** - Cobertura, JSON, LCOV, OpenCover
- **HTML Reports** - via ReportGenerator tool

## 📊 Coverage Formats Generated

- **Cobertura XML** - Standard format, compatible with most CI/CD tools
- **OpenCover XML** - Detailed format
- **JSON** - Machine-readable
- **LCOV** - For VS Code Coverage Gutters extension

## 🚀 Quick Start

### Option 1: Automatic Coverage (Every Test Run)

Simply run tests normally - coverage is collected automatically:

```bash
dotnet test
```

Coverage files are generated in each test project's `TestResults/` folder.

### Option 2: Generate HTML Reports

Use the provided scripts to run tests and generate beautiful HTML reports:

**Linux/Mac:**
```bash
./test-coverage.sh
```

**Windows PowerShell:**
```powershell
./test-coverage.ps1
```

**With auto-open:**
```bash
./test-coverage.sh --open
# or
./test-coverage.ps1 -o
```

## 📁 Output Locations

- **Raw Coverage Data**: `tests/*/TestResults/coverage.*.xml`
- **HTML Reports**: `./coveragereport/index.html`
- **Summary**: `./coveragereport/Summary.txt`
- **Badges**: `./coveragereport/badge_*.svg`

## ⚙️ Configuration

Coverage settings are centrally managed in [`Directory.Build.props`](Directory.Build.props):

```xml
<PropertyGroup Condition="'$(IsTestProject)' == 'true'">
  <CollectCoverage>true</CollectCoverage>
  <CoverletOutputFormat>cobertura,json,lcov,opencover</CoverletOutputFormat>
  <Exclude>[xunit.*]*,[*.Tests]*</Exclude>
  <Include>[DAO.Manager.*]*</Include>
</PropertyGroup>
```

### Customization Options

To modify coverage behavior, edit the `PropertyGroup` in `Directory.Build.props`:

- **`Exclude`** - Assemblies/namespaces to exclude from coverage
- **`ExcludeByAttribute`** - Exclude code with specific attributes
- **`Include`** - Explicitly include assemblies (overrides excludes)
- **`Threshold`** - Set minimum coverage percentage (build fails if not met)
- **`ThresholdType`** - Apply threshold to: line, branch, method

Example with thresholds:
```xml
<Threshold>80</Threshold>
<ThresholdType>line,branch</ThresholdType>
```

## 🔧 VS Code Integration

Install these extensions for in-editor coverage visualization:

1. **Coverage Gutters** (`ryanluker.vscode-coverage-gutters`)
   - Shows line-by-line coverage in editor gutters
   - Uses the generated `coverage.lcov` files

2. **Fine Code Coverage** (`FortuneN.perfect-coverage`)
   - Full coverage panel with detailed reports
   - Real-time coverage updates

After installing, run tests and click "Watch" in Coverage Gutters to see coverage highlights.

## 🎯 CI/CD Integration

### GitHub Actions Example

```yaml
- name: Test with Coverage
  run: dotnet test --no-restore --verbosity normal
  
- name: Upload Coverage to Codecov
  uses: codecov/codecov-action@v3
  with:
    files: ./tests/**/TestResults/coverage.cobertura.xml
```

### Azure DevOps Example

```yaml
- task: DotNetCoreCLI@2
  displayName: 'Run Tests'
  inputs:
    command: test
    
- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '$(Build.SourcesDirectory)/tests/**/TestResults/coverage.cobertura.xml'
```

## 📈 Coverage Thresholds

To enforce minimum coverage, add to `Directory.Build.props`:

```xml
<PropertyGroup Condition="'$(IsTestProject)' == 'true'">
  <Threshold>80</Threshold>
  <ThresholdType>line,branch</ThresholdType>
  <ThresholdStat>total</ThresholdStat>
</PropertyGroup>
```

This will fail the build if coverage drops below 80%.

## 🐛 Troubleshooting

### No coverage files generated
- Ensure `IsTestProject` is set to `true` in test project files
- Run `dotnet restore` to ensure packages are restored
- Check that `coverlet.msbuild` package is installed

### ReportGenerator not found
- Install globally: `dotnet tool install -g dotnet-reportgenerator-globaltool`
- Or use the coverage scripts which auto-install it

### Coverage includes test code
- Verify the `Exclude` pattern in `Directory.Build.props`
- Default excludes `[*.Tests]*` assemblies

## 📚 Additional Resources

- [Coverlet Documentation](https://github.com/coverlet-coverage/coverlet)
- [ReportGenerator Documentation](https://github.com/danielpalme/ReportGenerator)
- [Microsoft Code Coverage](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-code-coverage)
