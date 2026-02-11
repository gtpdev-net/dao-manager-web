# Alternative Code Coverage Viewing Options

ReportGenerator's HTML reports have a paywall for some advanced features. Here are free alternatives to view complete coverage including method-level details.

## 🎯 Quick Solutions

### Option 1: Use HtmlInline_AzurePipelines Format (Recommended)

Run the alternative script that uses a more permissive HTML format:

```bash
./test-coverage-alt.sh        # Generate with no paywall format
./test-coverage-alt.sh --open # Auto-open in browser
```

This generates reports using `HtmlInline_AzurePipelines` format which shows method coverage without Pro restrictions.

### Option 2: View XML/JSON Reports Directly

After running tests, view the detailed coverage files directly:

```bash
# Run tests
dotnet test --settings coverageSettings.runsettings

# View Cobertura XML (human-readable)
cat tests/*/TestResults/*/coverage.cobertura.xml | less

# View JSON (more structured)
cat tests/*/TestResults/*/coverage.json | jq '.'
```

### Option 3: Use VS Code Extensions (Best Developer Experience)

Install one of these free VS Code extensions for inline coverage:

#### A. Coverage Gutters (Most Popular)
```bash
code --install-extension ryanluker.vscode-coverage-gutters
```

**Usage:**
1. Run tests: `dotnet test --settings coverageSettings.runsettings`
2. In VS Code, open any source file
3. Click "Watch" in the status bar (or press `Ctrl+Shift+7`)
4. Coverage shows as colored gutters in your code:
   - 🟢 Green = Covered
   - 🔴 Red = Not covered
   - 🟡 Yellow = Partial coverage

**Note:** Requires LCOV format. Add to coverageSettings.runsettings:
```xml
<Format>cobertura,json,opencover,lcov</Format>
```

#### B. Fine Code Coverage
```bash
code --install-extension FortuneNgwenya.vscode-fine-coverage
```

**Features:**
- Full coverage panel with percentages
- Class and method-level details
- Line-by-line visualization
- No paywall, completely free

### Option 4: Use dotCover (JetBrains - Free for Open Source)

If you have Rider or ReSharper:
1. Right-click test project → "Cover Tests"
2. View detailed coverage in Coverage window
3. See method-level, line-level, branch-level details

Free for open-source projects with appropriate license.

### Option 5: Generate Text/CSV Reports

Modify the report script to output text-based formats:

```bash
reportgenerator \
    -reports:"tests/**/coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:"TextSummary;Html;CsvSummary;MarkdownSummary" \
    -title:"Coverage Report"
```

Then view:
- `coveragereport/Summary.txt` - Full text summary with method coverage
- `coveragereport/Summary.csv` - Import into spreadsheet
- `coveragereport/Summary.md` - Markdown formatted summary

### Option 6: Use OpenCover/Cobertura Viewers Online

Upload your coverage files to free online viewers:
- **Codecov.io** - Free for open source
- **Coveralls.io** - Free for open source  
- **Code Climate** - Free for open source

Example for Codecov:
```bash
# Install codecov uploader
curl -Os https://uploader.codecov.io/latest/linux/codecov
chmod +x codecov

# Upload coverage
./codecov -t YOUR_TOKEN -f tests/**/coverage.cobertura.xml
```

### Option 7: Custom HTML Report Script

Create a simple HTML viewer from JSON coverage:

```bash
# Generate JSON summary
reportgenerator \
    -reports:"tests/**/coverage.cobertura.xml" \
    -targetdir:"coveragereport" \
    -reporttypes:"JsonSummary"

# View with Python's built-in JSON viewer
python3 -m json.tool coveragereport/Summary.json | less
```

Or create a custom viewer:
```bash
# Create simple HTML from JSON
cat coveragereport/Summary.json | jq -r '
  "<!DOCTYPE html><html><head><title>Coverage</title></head><body>" +
  "<h1>Coverage: \(.summary.linecoverage)%</h1>" +
  "<h2>Method Coverage: \(.summary.methodcoverage)%</h2>" +
  "<pre>" + (. | tostring) + "</pre>" +
  "</body></html>"
' > coveragereport/simple.html
```

## 📊 Comparison

| Solution | Method Coverage | Inline Code View | Setup Effort | Free |
|----------|----------------|------------------|--------------|------|
| HtmlInline_AzurePipelines | ✅ Yes | ❌ No | Low | ✅ Yes |
| Coverage Gutters | ✅ Yes | ✅ Yes | Low | ✅ Yes |
| Fine Code Coverage | ✅ Yes | ✅ Yes | Low | ✅ Yes |
| Text/CSV Reports | ✅ Yes | ❌ No | Low | ✅ Yes |
| Codecov/Coveralls | ✅ Yes | ✅ Yes | Medium | ✅ Yes (OSS) |
| dotCover | ✅ Yes | ✅ Yes | Low | ✅ Yes (OSS) |
| JSON Direct View | ✅ Yes | ❌ No | Low | ✅ Yes |

## 🎯 Recommended Workflow

**For Development:**
1. Install **Coverage Gutters** VS Code extension
2. Run tests with LCOV format enabled
3. View coverage inline while coding

**For Reports:**
1. Use `./test-coverage-alt.sh` for HTML reports
2. Check `Summary.txt` for quick stats
3. Use `Summary.json` for detailed method-level data

**For CI/CD:**
1. Upload to Codecov or Coveralls (free for public repos)
2. Get PR comments with coverage changes
3. Badges for README

## 🔧 Implementation

### Enable LCOV for VS Code Extensions

Update [coverageSettings.runsettings](../coverageSettings.runsettings):

```xml
<Format>cobertura,json,opencover,lcov</Format>
```

Or run with lcov explicitly:
```bash
dotnet test --settings coverageSettings.runsettings --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=lcov
```

### Install Coverage Gutters

```bash
code --install-extension ryanluker.vscode-coverage-gutters
```

Then in VS Code:
1. Press `Ctrl+Shift+P`
2. Type "Coverage Gutters: Watch"
3. Open any source file to see coverage

### Use Alternative Report Format Permanently

Update [test-coverage.sh](../test-coverage.sh) to use:
```bash
-reporttypes:"HtmlInline_AzurePipelines;Badges;TextSummary;JsonSummary"
```

Instead of:
```bash
-reporttypes:"Html;HtmlSummary;Badges;TextSummary"
```

## 💡 Tips

1. **Coverage Gutters** is the most popular choice among .NET developers
2. **Fine Code Coverage** provides the most comprehensive VS Code experience
3. **HtmlInline_AzurePipelines** format is designed for CI/CD but works great locally
4. JSON reports contain ALL coverage data - nothing is hidden
5. Text summaries show method coverage percentages without any restrictions

## 🚀 Next Steps

Choose your preferred method and update your workflow:

1. For inline coverage while coding: **Coverage Gutters**
2. For detailed HTML reports: **test-coverage-alt.sh**
3. For CI/CD integration: **Codecov** or **Coveralls**
4. For comprehensive IDE integration: **Fine Code Coverage**

All options are completely free and show full method, line, and branch coverage!
