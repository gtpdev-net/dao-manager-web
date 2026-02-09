# Plan: Implement EF Core Entity Model from Specification

This plan implements the complete scan-owned snapshot entity model specified in the entity-model-specification.md document. All entities will be created in the DAO.Manager.Data/Models/Entities folder, and all relationships will be configured using Fluent API.

## Steps

### Project Setup

1. **Create DAO.Manager.Data project**:
   - Run: `dotnet new classlib -n DAO.Manager.Data -f net8.0`
   - Add to solution: `dotnet sln add DAO.Manager.Data/DAO.Manager.Data.csproj`
   - Delete default Class1.cs file

2. **Add required NuGet packages**:
   - `dotnet add DAO.Manager.Data package Microsoft.EntityFrameworkCore`
   - `dotnet add DAO.Manager.Data package Microsoft.EntityFrameworkCore.SqlServer`
   - `dotnet add DAO.Manager.Data package Microsoft.EntityFrameworkCore.Design`

3. **Create project folder structure**:
   - Create `DAO.Manager.Data/Models/Entities/` to contain all entity classes
   - Create `DAO.Manager.Data/Data/` folder to contain DbContext and configuration
   - Create `DAO.Manager.Data/Migrations/` folder to store EF Core migration files

### Entity Model Implementation

4. **Create entity classes** in DAO.Manager.Data/Models/Entities:
   - `ScanEvent.cs` - verbose logging for scan execution
   - `Scan.cs` - root entity for each scan operation
   - `Solution.cs` - solution snapshots with UniqueIdentifier
   - `Project.cs` - project snapshots with UniqueIdentifier and TargetFramework
   - `Package.cs` - NuGet packages discovered across all projects
   - `Assembly.cs` - compiled assemblies with Type property and nullable Version
   - Junction tables will NOT have entity classes per spec (configured via Fluent API only)

5. **Create DAO.Manager.Data/Data/ApplicationDbContext.cs**:
   - Add DbSet properties: `Scans`, `ScanEvents`, `Solutions`, `Projects`, `Packages`, `Assemblies`
   - Implement comprehensive `OnModelCreating` method with Fluent API for:
     - All entity configurations (keys, properties, constraints, indexes)
     - All 5 junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences, AssemblyDependencies)
     - Cascade delete behaviors (CASCADE for most, NO ACTION for ProjectReferences.ReferencedProjectId per spec)
     - Unique indexes per spec (e.g., `ScanId + UniqueIdentifier` for Solutions/Projects)
   - Set default values (CreatedAt, ScanDate, OccurredAt to UTC now)
   - Configure string max lengths per spec

6. **Create initial migration**:
   - Run: `dotnet ef migrations add InitialNormalizedSchema --project DAO.Manager.Data`
   - This generates migration with all tables, indexes, constraints matching the spec

## Verification

- Project builds successfully: `dotnet build DAO.Manager.Data/DAO.Manager.Data.csproj`
- Solution builds successfully: `dotnet build DAO.Manager.Web.sln`
- Migration scaffolds without errors
- Review generated migration to confirm:
  - All 6 main tables (Scans, ScanEvents, Solutions, Projects, Packages, Assemblies)
  - All 5 junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences, AssemblyDependencies)
  - Correct cascade behaviors (especially NO ACTION on ProjectReferences.ReferencedProjectId)
  - All unique indexes per spec
- Apply to test database: `dotnet ef database update --project DAO.Manager.Data`
- Inspect database schema to verify table/column names, types, and constraints

## Decisions
Fresh project**: DAO.Manager.Data is a new class library project targeting .NET 8.0
- **Entities in Entities folder**: All entity classes go to DAO.Manager.Data/Models/Entities
- **Data context in Data folder**: ApplicationDbContext in DAO.Manager.Data/Data
- **Entities in Entities folder**: All entity classes go to DAO.Manager/Models/Entities
- **Fluent API only**: All configuration via OnModelCreating for consistency and visibility
- **No junction entity classes**: Per spec, junction tables configured directly in DbContext
- **Cascade strategy**: CASCADE everywhere except ProjectReferences.ReferencedProjectId (NO ACTION to avoid circular conflicts)
