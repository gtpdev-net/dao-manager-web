# ORIGINAL_DAO.Manager_REFERENCE_ONLY

A simplified C# .NET 8 web application that scans .NET repositories to discover and analyze solutions, projects, and assemblies. This application stores basic scan data in a SQL Server database using Entity Framework Core, focusing on fundamental repository analysis.

## Features

- **Solution Discovery**: Finds all `.sln` files and extracts metadata including GUIDs, project counts, and structure
- **Project Analysis**: Analyzes all `.csproj` files and identifies:
  - Target frameworks
  - Project style (SDK-style vs Legacy)
  - Visual Studio GUIDs
- **Assembly Identification**: Determines output assemblies (DLLs, EXEs) from project configurations
- **Database Storage**: Stores all scan data in SQL Server using Entity Framework Core
- **Git Integration**: Captures git commit hash for each scan to track repository state
- **Web UI**: Provides intuitive web interface to create scans and view scan list
- **Real-time Progress**: SignalR-based live progress updates during repository scanning

## Technology Stack

- **.NET 8**: Latest LTS version of .NET
- **ASP.NET Core MVC**: Web framework for user interface
- **Entity Framework Core 8**: ORM for database access
- **SQL Server**: Database for persistent storage
- **SignalR**: WebSocket-based real-time communication for progress updates
- **Bootstrap 5**: UI framework for responsive design

## Database Schema

The application uses the following main entities:

- **Scan**: Represents a repository scan with timestamp and git commit
- **Solution**: Solution file metadata (.sln)
- **Project**: Project file metadata (.csproj)
- **Assembly**: Output assembly information (DLL/EXE)

## Getting Started

### Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB, Express, or full SQL Server)
- Git (optional, for commit tracking)

### Setup

1. **Clone or navigate to the repository**:
   ```bash
   cd /path/to/dao-manager/ORIGINAL_DAO.Manager_REFERENCE_ONLY
   ```

2. **Update the connection string** in `appsettings.json`:
   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=DAOManager;Trusted_Connection=true;TrustServerCertificate=true;"
     }
   }
   ```

3. **Create the database**:
   ```bash
   dotnet ef database update
   ```

4. **Run the application**:
   ```bash
   dotnet run
   ```

5. **Open your browser** and navigate to `https://localhost:7014/dm` or `http://localhost:5161/dm`

## Usage

### Creating a New Scan

1. Click "Start New Scan" on the home page
2. Enter the full path to the repository you want to scan
3. Click "Start Scan"
4. Watch real-time progress updates as the scan runs in the background
   - See the current phase (Solutions, Projects, Assemblies)
   - Monitor percentage completion
   - View detailed progress log
5. When complete, click "View Scan Results" to see the scan list

### Viewing Scan Results

The scan list displays:

- **Scan Date**: When the scan was performed
- **Repository Path**: The path that was scanned
- **Git Commit**: The commit hash at the time of scan

### Example Repository Path

- **Windows**: `C:\source\repos\MyProject`
- **Linux/Mac**: `/home/user/source/repos/MyProject`

## Architecture

### Services

- **RepositoryScannerService**: Core scanning logic that:
  - Discovers solution and project files
  - Parses XML project files
  - Extracts metadata using regex and XDocument
  - Persists data to database

### Data Models

- Entities mirror the structure discovered in repositories
- Relationships are properly configured with navigation properties
- GUIDs are generated deterministically from file paths for consistent identification

### Controllers

- **HomeController**: Landing page
- **ScansController**: Create scans and view scan list

## Comparison to PowerShell Scripts

This application provides a simplified version of the PowerShell scripts functionality:

| PowerShell Script | C# Equivalent |
|-------------------|---------------|
| `01_find-solutions.ps1` | `FindSolutionsAsync()` |
| `02_find-projects.ps1` | `FindProjectsAsync()` |
| `03_extract-solution-info.ps1` | `ExtractSolutionInfoAsync()` |
| `04_extract-project-info.ps1` | `ExtractProjectInfoAsync()` |
| `05_extract-assembly-info.ps1` | `ExtractAssembliesAsync()` |

### Key Differences

- **Database Storage**: Uses SQL Server instead of CSV files
- **Web UI**: Provides interactive web interface instead of text files
- **Persistent Storage**: Data is stored permanently and can be queried
- **Cross-Platform**: Works on Windows, Linux, and macOS
- **Async Operations**: Uses async/await for better performance
- **Type Safety**: Strong typing with C# vs dynamic PowerShell

## Database Migrations

To create a new migration after model changes:

```bash
dotnet ef migrations add MigrationName
```

To update the database:

```bash
dotnet ef database update
```

To remove the last migration:

```bash
dotnet ef migrations remove
```

## Configuration

### Connection Strings

For different environments, update `appsettings.json` or `appsettings.Development.json`:

- **LocalDB**: `Server=(localdb)\\mssqllocaldb;Database=DAOManager;Trusted_Connection=true;`
- **SQL Server Express**: `Server=.\\SQLEXPRESS;Database=DAOManager;Trusted_Connection=true;`
- **Full SQL Server**: `Server=servername;Database=DAOManager;User Id=username;Password=password;`

## Future Enhancements

Possible improvements:

- Add detailed scan views (solutions, projects, assemblies)
- Add dependency graph building and visualization
- Export scan results to CSV/Excel
- Compare scans to track changes over time
- Support for additional project types (F#, VB.NET)
- REST API for programmatic access
- Docker containerization

## License

This project uses the same license as the project-analysis PowerShell scripts.

## Contributing

Contributions are welcome! Please ensure:

- Code follows .NET conventions
- Entity Framework migrations are included
- Views are properly formatted
- Tests are updated (when test project is added)

## Support

For issues or questions, please refer to the main repository documentation.
