#!/bin/bash
set -e

echo "============================================"
echo "  DAO Manager - Test Coverage Report"
echo "============================================"
echo ""

# Colors for output
GREEN='\033[0;32m'
BLUE='\033[0;34m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Clean previous results
echo -e "${BLUE}Cleaning previous test results...${NC}"
find . -type d -name "TestResults" -exec rm -rf {} + 2>/dev/null || true
rm -rf ./coveragereport 2>/dev/null || true

# Restore packages
echo -e "${BLUE}Restoring packages...${NC}"
dotnet restore

# Run tests with coverage
echo -e "${BLUE}Running tests with coverage collection...${NC}"
dotnet test --no-restore --settings coverageSettings.runsettings

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "${YELLOW}ReportGenerator not found. Installing as global tool...${NC}"
    dotnet tool install -g dotnet-reportgenerator-globaltool

    # Add to PATH if not already there
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

# Find all coverage files
COVERAGE_FILES=$(find ./tests -name "coverage.cobertura.xml" 2>/dev/null | head -1)

if [ -z "$COVERAGE_FILES" ]; then
    echo -e "${YELLOW}Warning: No coverage files found. Make sure tests ran successfully.${NC}"
    exit 1
fi

# Generate HTML report
echo -e "${BLUE}Generating HTML coverage report...${NC}"
reportgenerator \
    -reports:"./tests/**/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"./coveragereport" \
    -reporttypes:"HtmlInline_AzurePipelines;Badges;TextSummary;JsonSummary" \
    -title:"DAO Manager Coverage Report" \
    -verbosity:"Warning"

# Display summary
echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  Coverage Report Generated Successfully!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""

# Show text summary if available
if [ -f "./coveragereport/Summary.txt" ]; then
    cat "./coveragereport/Summary.txt"
    echo ""
fi

echo -e "${BLUE}Report Locations:${NC}"
echo "  - HTML Report: ./coveragereport/index.html"
echo "  - Text Summary: ./coveragereport/Summary.txt"
echo "  - JSON Details: ./coveragereport/Summary.json (full method coverage)"
echo "  - Badges: ./coveragereport/badge_*.svg"
echo ""
echo -e "${YELLOW}Tip:${NC} Install VS Code 'Coverage Gutters' extension for inline coverage!"
echo "     Run: code --install-extension ryanluker.vscode-coverage-gutters"
echo ""

# Optionally open in browser
if [ "$1" == "--open" ] || [ "$1" == "-o" ]; then
    if [ -n "$BROWSER" ]; then
        echo -e "${BLUE}Opening report in browser...${NC}"
        "$BROWSER" "./coveragereport/index.html" &
    else
        echo -e "${YELLOW}BROWSER environment variable not set. Cannot auto-open.${NC}"
    fi
fi
