# Plan: Implement EF Core Entity Model from Specification

This plan implements the complete scan-owned snapshot entity model specified in the entity-model-specification.md document. All entities will be created fresh in the DAO.Manager/Models/Entities folder, the existing migration will be deleted and replaced with a clean initial migration, and all relationships will be configured using Fluent API.

## Steps

1. **Create new entity classes** in DAO.Manager/Models/Entities:
   - `ScanEvent.cs` (new) - verbose logging for scan execution
   - Update `Scan.cs` to match spec exactly (remove any non-spec properties/relationships)
   - Update `Solution.cs` to match spec (add UniqueIdentifier, adjust properties)
   - Update `Project.cs` to match spec (add UniqueIdentifier, TargetFramework)
   - Create `Package.cs` (new) - replaces old PackageReference entity approach
   - Update `Assembly.cs` to match spec (ensure Type property, Version nullable)
   - Junction tables will NOT have entity classes per spec (configured via Fluent API only)

2. **Update DAO.Manager/Data/ApplicationDbContext.cs**:
   - Add/update DbSet properties: `ScanEvents`, `Solutions`, `Projects`, `Packages`, `Assemblies`
   - Remove DbSets for old junction entity classes if they exist (`AssemblyReferences`, `PackageReferences`)
   - Implement comprehensive `OnModelCreating` method with Fluent API for:
     - All entity configurations (keys, properties, constraints, indexes)
     - All 5 junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences, AssemblyDependencies)
     - Cascade delete behaviors (CASCADE for most, NO ACTION for ProjectReferences.ReferencedProjectId per spec)
     - Unique indexes per spec (e.g., `ScanId + UniqueIdentifier` for Solutions/Projects)
   - Set default values (CreatedAt, ScanDate, OccurredAt to UTC now)
   - Configure string max lengths per spec

3. **Delete existing migration** in DAO.Manager/Migrations:
   - Remove `20260205230306_InitialNormalizedSchema.cs`
   - Remove `20260205230306_InitialNormalizedSchema.Designer.cs`
   - Update `ApplicationDbContextModelSnapshot.cs` will be regenerated automatically

4. **Remove obsolete entity files** if present:
   - Delete DAO.Manager/Models/AssemblyReference.cs (replaced by junction table)
   - Delete DAO.Manager/Models/PackageReference.cs (replaced by junction table)
   - Keep DAO.Manager/Models/ErrorViewModel.cs (not part of domain model, used for views)

5. **Create fresh initial migration**:
   - Run: `dotnet ef migrations add InitialNormalizedSchema --project DAO.Manager`
   - This generates migration with all tables, indexes, constraints matching the spec

## Verification

- Build succeeds: `dotnet build DAO.Manager/DAO.Manager.csproj`
- Migration scaffolds without errors
- Review generated migration to confirm:
  - All 6 main tables (Scans, ScanEvents, Solutions, Projects, Packages, Assemblies)
  - All 5 junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences, AssemblyDependencies)
  - Correct cascade behaviors (especially NO ACTION on ProjectReferences.ReferencedProjectId)
  - All unique indexes per spec
- Apply to test database: `dotnet ef database update --project DAO.Manager`
- Inspect database schema to verify table/column names, types, and constraints

## Decisions

- **New entities in Entities folder**: All entity classes go to DAO.Manager/Models/Entities as you specified
- **Clean slate migration**: Deleted existing migration for fresh start matching spec exactly
- **Fluent API only**: All configuration via OnModelCreating for consistency and visibility
- **No junction entity classes**: Per spec, junction tables configured directly in DbContext
- **Cascade strategy**: CASCADE everywhere except ProjectReferences.ReferencedProjectId (NO ACTION to avoid circular conflicts)
