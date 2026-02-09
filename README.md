# DAO Manager Web

A web-based solution for scanning and managing .NET repository dependencies, tracking project structures, assemblies, and packages across multiple scans.

## Overview

DAO Manager (Dependency & Operations) provides temporal analysis of .NET repositories through snapshot-based scanning. Each scan creates an immutable point-in-time view of a repository's structure, enabling dependency archaeology and drift detection over time.

### Key Features

- **Repository Scanning**: Scan .NET solutions and projects to capture dependency information
- **Snapshot Architecture**: Each scan is independent, allowing historical comparison
- **Dependency Tracking**: Track NuGet packages, assembly references, and project relationships
- **Temporal Analysis**: Compare scans over time to detect dependency changes
- **Web Interface**: Built with Blazor for modern, interactive UI

## Project Structure

```
dao-manager-web/
├── src/                          # Source code
│   ├── DAO.Manager.Web/          # Blazor web application
│   ├── DAO.Manager.Scanner/      # Repository scanning logic
│   └── DAO.Manager.Data/         # Data access layer (EF Core)
├── tests/                        # Test projects
│   ├── DAO.Manager.Scanner.Tests/
│   └── DAO.Manager.Data.Tests/
├── docs/                         # Documentation
├── .editorconfig                 # Code style configuration
├── Directory.Build.props         # Shared MSBuild properties
├── global.json                   # .NET SDK version
└── DAO.Manager.Web.sln          # Solution file
```

## Prerequisites

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- SQL Server (LocalDB, Express, or full edition)
- Git

## Getting Started

### 1. Clone the Repository

```bash
git clone https://github.com/gtpdev-net/dao-manager-web.git
cd dao-manager-web
```

### 2. Restore Dependencies

```bash
dotnet restore
```

### 3. Configure Database

Update the connection string in `src/DAO.Manager.Web/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DAOManager;Trusted_Connection=True;MultipleActiveResultSets=true"
  }
}
```

### 4. Apply Migrations

```bash
dotnet ef database update --project src/DAO.Manager.Data --startup-project src/DAO.Manager.Web
```

### 5. Run the Application

```bash
dotnet run --project src/DAO.Manager.Web
```

Navigate to `https://localhost:5001` (or the URL shown in the console).

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests for a specific project
dotnet test tests/DAO.Manager.Scanner.Tests
dotnet test tests/DAO.Manager.Data.Tests

# Run tests with coverage
dotnet test --collect:"XPlat Code Coverage"
```

## Building

```bash
# Debug build
dotnet build

# Release build
dotnet build --configuration Release
```

## Architecture

### Projects

- **DAO.Manager.Web**: ASP.NET Core Blazor Server application providing the web interface
- **DAO.Manager.Scanner**: Business logic for scanning repositories and extracting dependency information
- **DAO.Manager.Data**: Entity Framework Core data layer with models, DbContext, and migrations

### Data Model

The application uses a scan-owned snapshot architecture:

- **Scan**: Root entity representing a point-in-time repository scan
- **Solution**: Visual Studio solutions found in the scan
- **Project**: .NET projects within solutions
- **Package**: NuGet package references
- **Assembly**: Assembly (DLL) references
- **References**: Relationships between projects and their dependencies

See [docs/entity-model-specification.md](docs/entity-model-specification.md) for detailed data model documentation.

## Development Guidelines

### Code Style

This project uses EditorConfig for consistent code formatting. Most IDEs will automatically apply these settings. Key conventions:

- **Indentation**: 4 spaces for C# files, 2 spaces for XML/JSON
- **Nullable**: Enabled across all projects
- **Implicit usings**: Enabled for cleaner code

### Naming Conventions

- **Classes/Interfaces**: PascalCase (e.g., `ScanService`, `IRepository`)
- **Methods/Properties**: PascalCase
- **Parameters/Variables**: camelCase
- **Interfaces**: Prefix with `I` (e.g., `IProgressReporter`)

### Testing

- Write unit tests for business logic in Scanner project
- Write integration tests for Data layer operations
- Use xUnit as the test framework
- Follow AAA pattern: Arrange, Act, Assert

## Contributing

1. Create a feature branch from `main`
2. Make your changes following the code style guidelines
3. Ensure all tests pass
4. Submit a pull request

## Technology Stack

- **Framework**: .NET 8.0
- **Web**: ASP.NET Core Blazor Server
- **Database**: Entity Framework Core with SQL Server
- **Testing**: xUnit, Moq (if needed)
- **Version Control**: Git

## License

See [LICENSE](LICENSE) file for details.

## Documentation

- [Entity Model Specification](docs/entity-model-specification.md) - Detailed database schema and entity relationships
- Additional documentation available in the `docs/` directory

## Support

For issues, questions, or contributions, please use the GitHub issue tracker.
