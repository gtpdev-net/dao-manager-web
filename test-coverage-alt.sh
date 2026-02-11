#!/bin/bash
set -e

echo "============================================"
echo "  DAO Manager - Alternative Coverage Report"
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

# Find coverage files
COVERAGE_FILES=$(find ./tests -name "coverage.cobertura.xml" 2>/dev/null | head -1)

if [ -z "$COVERAGE_FILES" ]; then
    echo -e "${YELLOW}Warning: No coverage files found.${NC}"
    exit 1
fi

# Check if reportgenerator is installed
if ! command -v reportgenerator &> /dev/null; then
    echo -e "${YELLOW}ReportGenerator not found. Installing as global tool...${NC}"
    dotnet tool install -g dotnet-reportgenerator-globaltool
    export PATH="$PATH:$HOME/.dotnet/tools"
fi

# Generate reports with free-tier friendly formats
echo -e "${BLUE}Generating coverage reports (method coverage visible)...${NC}"
reportgenerator \
    -reports:"./tests/**/TestResults/**/coverage.cobertura.xml" \
    -targetdir:"./coveragereport" \
    -reporttypes:"HtmlInline_AzurePipelines;HtmlSummary;Badges;TextSummary;JsonSummary;Cobertura" \
    -title:"DAO Manager Coverage Report" \
    -verbosity:"Warning"

# Display summary
echo ""
echo -e "${GREEN}============================================${NC}"
echo -e "${GREEN}  Coverage Report Generated Successfully!${NC}"
echo -e "${GREEN}============================================${NC}"
echo ""

# Show text summary
if [ -f "./coveragereport/Summary.txt" ]; then
    cat "./coveragereport/Summary.txt"
    echo ""
fi

# Show JSON summary with method details
if [ -f "./coveragereport/Summary.json" ]; then
    echo -e "${BLUE}Method Coverage Details:${NC}"
    if command -v jq &> /dev/null; then
        jq -r '.summary.methodcoverage' "./coveragereport/Summary.json" 2>/dev/null || echo "Method coverage: See Summary.json"
    else
        echo "Install 'jq' to see formatted method coverage, or check Summary.json"
    fi
    echo ""
fi

echo -e "${BLUE}Report Locations:${NC}"
echo "  - HTML Report: ./coveragereport/index.html"
echo "  - Text Summary: ./coveragereport/Summary.txt"
echo "  - JSON Details: ./coveragereport/Summary.json (includes method coverage)"
echo "  - Badges: ./coveragereport/badge_*.svg"
echo ""
echo -e "${YELLOW}Note:${NC} This uses HtmlInline_AzurePipelines format which shows"
echo "      method coverage without Pro features."
echo ""

# Optionally open in browser
if [ "$1" == "--open" ] || [ "$1" == "-o" ]; then
    if [ -n "$BROWSER" ]; then
        echo -e "${BLUE}Opening report in browser...${NC}"
        "$BROWSER" "./coveragereport/index.html" &
    fi
fi
