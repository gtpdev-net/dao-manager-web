# Test Coverage Examples

## Quick Reference

### Run Tests with Coverage (Standard)
```bash
# Using VS Code test runner - coverage is automatic with runsettings
dotnet test --settings coverageSettings.runsettings
```

### Generate HTML Reports
```bash
# Linux/Mac
./test-coverage.sh

# With auto-open in browser
./test-coverage.sh --open

# Windows PowerShell
./test-coverage.ps1

# With auto-open
./test-coverage.ps1 -o
```

### Manual Coverage with MSBuild Integration
```bash
# For individual test projects with coverlet.msbuild
cd tests/DAO.Manager.Data.Tests
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura

# With threshold enforcement (fails if coverage < 80%)
dotnet test /p:CollectCoverage=true /p:Threshold=80 /p:ThresholdType=line
```

### CI/CD One-Liner
```bash
# Simple coverage collection for CI
dotnet test --settings coverageSettings.runsettings --logger trx --results-directory ./TestResults
```

## VS Code Integration

### Using Coverage Gutters Extension

1. Install: `ext install ryanluker.vscode-coverage-gutters`
2. Run tests: `dotnet test --settings coverageSettings.runsettings`
3. Add LCOV format to runsettings (see below)
4. Click "Watch" in status bar
5. See coverage highlights in editor

**Note**: To use Coverage Gutters, add `lcov` back to the Format in `coverageSettings.runsettings`:
```xml
<Format>cobertura,json,opencover,lcov</Format>
```

And comment out or remove the `<DeterministicReport>` setting if present.

### Using Fine Code Coverage Extension

1. Install: `ext install FortuneN.perfect-coverage`
2. Run tests normally
3. Coverage panel automatically updates
4. Works with opencover format (already configured)

## Coverage Output Locations

After running tests with coverage:
- **Raw Coverage**: `tests/*/TestResults/{guid}/coverage.*.xml`
- **HTML Reports**: `coveragereport/index.html` (after running script)
- **Summary Text**: `coveragereport/Summary.txt`
- **Badges**: `coveragereport/badge_*.svg`

## Viewing Reports

### Option 1: Simple Browser (VS Code)
Press `Ctrl+Shift+P` (or `Cmd+Shift+P` on Mac), type "Simple Browser" and open:
```
file:///workspace/coveragereport/index.html
```

### Option 2: Command Line
```bash
# Linux/Mac
xdg-open coveragereport/index.html

# Using environment variable
$BROWSER coveragereport/index.html

# Windows
start coveragereport/index.html
```

### Option 3: Live Server
If you have Live Server extension:
1. Right-click `coveragereport/index.html`
2. Select "Open with Live Server"

## Advanced Scenarios

### Coverage for Specific Test
```bash
dotnet test --filter "FullyQualifiedName~MySpecificTest" --settings coverageSettings.runsettings
```

### Coverage with Detailed Logging
```bash
dotnet test --settings coverageSettings.runsettings --verbosity detailed
```

### Generate Only Specific Report Types
```bash
reportgenerator \
    -reports:"./tests/**/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"./coveragereport" \
    -reporttypes:"Html;Badges"
```

### Coverage Diff Between Branches
```bash
# Generate coverage for current branch
dotnet test --settings coverageSettings.runsettings
cp coveragereport/Summary.txt current-coverage.txt

# Switch branch and compare
git checkout main
dotnet test --settings coverageSettings.runsettings
diff current-coverage.txt coveragereport/Summary.txt
```

## Troubleshooting

### No Coverage Files Generated
**Solution**: Ensure you're using `--settings coverageSettings.runsettings` flag.

### LCOV Error with DeterministicReport
**Solution**: Remove `lcov` from Format or remove `DeterministicReport` setting.

### Low or Zero Coverage
**Solution**: Ensure tests actually execute the code in your source projects. Check test references.

### Coverage Files in Wrong Location
**Solution**: VSTest places files in `TestResults/{guid}/` subdirectories. Our scripts handle this automatically.

## Integration Examples

### GitHub Actions
```yaml
- name: Test with Coverage
  run: dotnet test --settings coverageSettings.runsettings --logger trx
  
- name: Generate Report
  run: |
    dotnet tool install -g dotnet-reportgenerator-globaltool
    reportgenerator -reports:"**/coverage.cobertura.xml" -targetdir:"coveragereport" -reporttypes:"Html"
    
- name: Upload Coverage
  uses: actions/upload-artifact@v3
  with:
    name: coverage-report
    path: coveragereport/
```

### Azure DevOps
```yaml
- task: DotNetCoreCLI@2
  displayName: Test with Coverage
  inputs:
    command: test
    arguments: '--settings coverageSettings.runsettings'
    
- task: PublishCodeCoverageResults@1
  inputs:
    codeCoverageTool: 'Cobertura'
    summaryFileLocation: '**/coverage.cobertura.xml'
```

### GitLab CI
```yaml
test:
  script:
    - dotnet test --settings coverageSettings.runsettings
  artifacts:
    reports:
      coverage_report:
        coverage_format: cobertura
        path: tests/**/TestResults/**/coverage.cobertura.xml
```

## Performance Tips

- Use `--no-restore` and `--no-build` when running coverage multiple times
- Exclude test assemblies from coverage with `<Exclude>[*.Tests]*</Exclude>`
- Use `<SingleHit>true</SingleHit>` for faster collection if you don't need detailed hit counts
- Consider using only Cobertura format for CI (faster than multiple formats)

## Next Steps

1. Write actual tests that exercise your code
2. Set up CI/CD integration
3. Add coverage badges to README
4. Set minimum coverage thresholds
5. Consider integrating with SonarQube or Codecov
