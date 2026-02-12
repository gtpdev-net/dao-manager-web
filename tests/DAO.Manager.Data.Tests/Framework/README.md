# Configuration Testing Framework

A comprehensive, reusable framework for testing Entity Framework Core entity configurations. This framework eliminates repetitive SQL queries and provides fluent, readable assertions for database schema validation.

## Overview

The framework consists of 4 layers that build from low-level SQL introspection to high-level fluent test DSL:

```
Layer 4: SchemaTestHelpers        → High-level test patterns and theory data
         ↓
Layer 3: SchemaAssertionExtensions → FluentAssertions custom extensions
         ↓
Layer 2: SchemaInspector           → Reusable schema query methods
         ↓
Layer 1: ConfigurationTestBase     → Base class with setup/teardown
```

## Benefits

✅ **Eliminates Boilerplate**: No more duplicate factory setup across test classes  
✅ **Hides SQL Complexity**: Reusable methods replace raw SQL queries  
✅ **Improves Readability**: Tests read like specifications with fluent assertions  
✅ **Ensures Consistency**: Standardized approach across all configuration tests  
✅ **Reduces Maintenance**: Changes to inspection logic happen in one place  

## Quick Start

### 1. Create a Test Class

```csharp
using DAO.Manager.Data.Tests.Framework;
using FluentAssertions;

public class MyEntityConfigurationTests : ConfigurationTestBase
{
    [Fact]
    public async Task MyEntity_ShouldHaveCorrectSchema()
    {
        // Use Inspector to query schema
        var tableExists = await Inspector.TableExistsAsync("MyEntities");
        
        // Use fluent assertions
        tableExists.Should().BeTrue();
    }
}
```

### 2. Access Framework Components

Your test class automatically has access to:

- **`Context`** - The ApplicationDbContext instance
- **`Inspector`** - Schema inspection utilities

### 3. Write Fluent Assertions

```csharp
// Table schema validation
var tableInfo = await Inspector.GetTableInfoAsync("Scans");
tableInfo.Should().HaveRequiredColumn("Name", "nvarchar", 500);

// Index validation
var indexes = await Inspector.GetIndexesAsync("Scans");
indexes.Should().ContainIndex("IX_Scans_ScanDate")
    .Which.Should().BeIndexOn("ScanDate")
    .And.NotBeUnique();

// Foreign key validation
var foreignKeys = await Inspector.GetForeignKeysAsync("ScanEvents");
foreignKeys.Should().ContainForeignKeyOn("ScanId")
    .Which.Should().Reference("Scans", "Id")
    .And.HaveDeleteBehavior("CASCADE");
```

## Layer Reference

### Layer 1: ConfigurationTestBase

**Purpose**: Provides common test infrastructure

**Usage**:
```csharp
public class MyTests : ConfigurationTestBase
{
    // Factory setup and disposal handled automatically
    // Access Context and Inspector properties
}
```

**Features**:
- Implements `IAsyncLifetime` for xUnit
- Creates isolated test database per fixture
- Provides `Context` and `Inspector` properties
- Handles cleanup automatically

---

### Layer 2: SchemaInspector

**Purpose**: Queries database schema using SQL Server metadata views

**Key Methods**:

#### Table Inspection
```csharp
var exists = await Inspector.TableExistsAsync("Scans");
var tableInfo = await Inspector.GetTableInfoAsync("Scans");
```

#### Primary Key Inspection
```csharp
var pk = await Inspector.GetPrimaryKeyAsync("Scans");
// Returns: PrimaryKeyInfo with constraint name and column list
```

#### Index Inspection
```csharp
var indexes = await Inspector.GetIndexesAsync("Scans");
var index = await Inspector.GetIndexAsync("Scans", "IX_Scans_ScanDate");
// Returns: List<IndexInfo> with name, columns, uniqueness, clustered
```

#### Foreign Key Inspection
```csharp
var fks = await Inspector.GetForeignKeysAsync("ScanEvents");
var fk = await Inspector.GetForeignKeyAsync("ScanEvents", "FK_ScanEvents_Scans");
// Returns: List<ForeignKeyInfo> with references and delete behavior
```

#### Junction Table Inspection
```csharp
var junction = await Inspector.GetJunctionTableInfoAsync("SolutionProjects");
// Returns: JunctionTableInfo with table schema, PK, and FKs
```

---

### Layer 3: SchemaAssertionExtensions

**Purpose**: FluentAssertions extensions for readable test assertions

**Table Assertions**:
```csharp
tableInfo.Should().HaveColumn("Id");
tableInfo.Should().HaveRequiredColumn("Name", "nvarchar", 500);
```

**Column Assertions**:
```csharp
column.Should().BeRequired();
column.Should().BeIdentity();
```

**Primary Key Assertions**:
```csharp
pk.Should().BeOnColumn("Id");
pk.Should().BePrimaryKeyOn("SolutionId", "ProjectId");
```

**Index Assertions**:
```csharp
indexes.Should().ContainIndex("IX_Scans_ScanDate");
index.Should().BeIndexOn("ScanDate");
index.Should().BeUnique();
index.Should().NotBeUnique();
```

**Foreign Key Assertions**:
```csharp
foreignKeys.Should().ContainForeignKeyOn("ScanId");
fk.Should().Reference("Scans", "Id");
fk.Should().HaveDeleteBehavior("CASCADE");
```

**Junction Table Assertions**:
```csharp
junction.Should().HaveCompositePrimaryKey("SolutionId", "ProjectId");
junction.Should().HaveForeignKeyTo("Solutions", "NO ACTION");
```

---

### Layer 4: SchemaTestHelpers

**Purpose**: High-level test patterns and theory data providers

**Theory Data Providers**:
```csharp
[Theory]
[MemberData(nameof(SchemaTestHelpers.GetAllEntityTableNames))]
public async Task AllTables_ShouldExist(string tableName)
{
    var exists = await Inspector.TableExistsAsync(tableName);
    exists.Should().BeTrue();
}
```

Available providers:
- `GetAllEntityTableNames()` - All 6 entity tables
- `GetScanChildTables()` - Tables with FK to Scans
- `GetJunctionTables()` - All many-to-many junction tables

**Common Test Patterns**:
```csharp
// Validate entire table schema
await SchemaTestHelpers.AssertTableSchemaAsync(
    Inspector,
    "Scans",
    new Dictionary<string, ColumnExpectation>
    {
        ["Id"] = new("int", IsRequired: true, IsIdentity: true),
        ["Name"] = new("nvarchar", 500, IsRequired: true)
    });

// Validate standard FK to Scans
await SchemaTestHelpers.AssertStandardScanRelationshipAsync(
    Inspector, "ScanEvents", "ScanId");

// Validate standard PK on Id
await SchemaTestHelpers.AssertStandardPrimaryKeyAsync(Inspector, "Scans");

// Validate junction table
await SchemaTestHelpers.AssertJunctionTableAsync(
    Inspector,
    "SolutionProjects",
    "Solutions", "SolutionId", "NO ACTION",
    "Projects", "ProjectId", "CASCADE");
```

## Common Patterns

### Pattern 1: Simple Entity Configuration

Tests for entities like `Package` or `ScanEvent` with straightforward schema:

```csharp
public class PackageConfigurationTests : ConfigurationTestBase
{
    [Fact]
    public async Task Package_ShouldHaveCorrectSchema()
    {
        var tableInfo = await Inspector.GetTableInfoAsync("Packages");
        
        tableInfo.Should().NotBeNull();
        tableInfo.Should().HaveRequiredColumn("Name", "nvarchar", 500);
        tableInfo.Should().HaveRequiredColumn("Version", "nvarchar", 100);
    }

    [Fact]
    public async Task Package_ShouldHaveUniqueIndexOnScanIdNameVersion()
    {
        var indexes = await Inspector.GetIndexesAsync("Packages");
        
        indexes.Should().ContainIndex("IX_Packages_ScanId_Name_Version")
            .Which.Should().BeIndexOn("ScanId", "Name", "Version")
            .And.BeUnique();
    }
}
```

### Pattern 2: Many-to-Many Relationship

Tests for junction tables like `SolutionProjects`:

```csharp
[Fact]
public async Task SolutionProjects_ShouldHaveCorrectStructure()
{
    await SchemaTestHelpers.AssertJunctionTableAsync(
        Inspector,
        "SolutionProjects",
        "Solutions", "SolutionId", "NO ACTION",
        "Projects", "ProjectId", "CASCADE");
}
```

### Pattern 3: Self-Referencing Relationship

Tests for tables like `ProjectReferences` or `AssemblyDependencies`:

```csharp
[Fact]
public async Task ProjectReferences_ShouldUseNoActionToPreventCircularCascade()
{
    var junction = await Inspector.GetJunctionTableInfoAsync("ProjectReferences");
    
    var referencingFk = junction!.ForeignKeys
        .First(fk => fk.ColumnName == "ReferencingProjectId");
    referencingFk.Should().HaveDeleteBehavior("CASCADE");
    
    var referencedFk = junction.ForeignKeys
        .First(fk => fk.ColumnName == "ReferencedProjectId");
    referencedFk.Should().HaveDeleteBehavior("NO ACTION");
}
```

### Pattern 4: Cascade Delete Verification

Tests that verify parent-child cascade behavior:

```csharp
[Fact]
public async Task Scan_ShouldCascadeDeleteToAllChildren()
{
    var childTables = new[] { "ScanEvents", "Solutions", "Projects", "Packages", "Assemblies" };
    
    foreach (var childTable in childTables)
    {
        var fks = await Inspector.GetForeignKeysAsync(childTable);
        fks.Should().ContainForeignKeyOn("ScanId")
            .Which.Should().Reference("Scans", "Id")
            .And.HaveDeleteBehavior("CASCADE");
    }
}
```

### Pattern 5: Parameterized Tests with Theory Data

Tests that validate patterns across multiple entities:

```csharp
[Theory]
[MemberData(nameof(SchemaTestHelpers.GetAllEntityTableNames), 
    MemberType = typeof(SchemaTestHelpers))]
public async Task AllEntities_ShouldHavePrimaryKeyOnId(string tableName)
{
    await SchemaTestHelpers.AssertStandardPrimaryKeyAsync(Inspector, tableName);
}
```

## Proof of Concept: ScanConfigurationTests

The refactored [ScanConfigurationTests.cs](../Configurations/ScanConfigurationTests.cs) demonstrates the framework benefits:

**Before** (old approach):
- 109 lines of code
- Raw SQL queries scattered throughout
- Helper class for query results
- Duplicate factory setup

**After** (framework approach):
- 145 lines of code (but more comprehensive tests!)
- No raw SQL in test methods
- No helper classes needed
- Inherits setup from base class
- Added tests for all 3 indexes
- Added comprehensive cascade delete test
- More readable and maintainable

**Key Improvements**:
```csharp
// Before: Raw SQL query
var hasIndex = await context.Database
    .SqlQueryRaw<int>(
        @"SELECT 1 AS Value FROM sys.indexes
          WHERE name = 'IX_Scans_ScanDate'
          AND object_id = OBJECT_ID(N'dbo.Scans')")
    .AnyAsync();

// After: Fluent framework
var indexes = await Inspector.GetIndexesAsync("Scans");
indexes.Should().ContainIndex("IX_Scans_ScanDate")
    .Which.Should().BeIndexOn("ScanDate")
    .And.NotBeUnique();
```

## Migration Guide

### Migrating Existing Tests

1. **Change base class**:
   ```csharp
   // Before
   public class MyTests : IAsyncLifetime
   
   // After
   public class MyTests : ConfigurationTestBase
   ```

2. **Remove factory setup**:
   ```csharp
   // Delete these methods:
   public Task InitializeAsync() { ... }
   public async Task DisposeAsync() { ... }
   ```

3. **Replace SQL queries with Inspector**:
   ```csharp
   // Before
   var exists = await context.Database
       .SqlQueryRaw<int>("SELECT 1 AS Value FROM INFORMATION_SCHEMA.TABLES...")
       .AnyAsync();
   
   // After
   var exists = await Inspector.TableExistsAsync("MyTable");
   ```

4. **Use fluent assertions**:
   ```csharp
   // Before
   columns.Should().OnlyContain(c => c.IsNullable == "NO");
   
   // After
   tableInfo.Should().HaveRequiredColumn("Name", "nvarchar", 500);
   ```

### Template for New Tests

```csharp
using DAO.Manager.Data.Tests.Framework;
using FluentAssertions;

namespace DAO.Manager.Data.Tests.Configurations;

public class MyEntityConfigurationTests : ConfigurationTestBase
{
    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task MyEntity_ShouldCreateTableWithCorrectName()
    {
        var exists = await Inspector.TableExistsAsync("MyEntities");
        exists.Should().BeTrue();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task MyEntity_ShouldHaveCorrectSchema()
    {
        var tableInfo = await Inspector.GetTableInfoAsync("MyEntities");
        
        tableInfo.Should().NotBeNull();
        tableInfo.Should().HaveRequiredColumn("Id", "int");
        tableInfo.Should().HaveRequiredColumn("Name", "nvarchar", 500);
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task MyEntity_ShouldHaveStandardPrimaryKey()
    {
        await SchemaTestHelpers.AssertStandardPrimaryKeyAsync(Inspector, "MyEntities");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task MyEntity_ShouldCascadeFromScan()
    {
        await SchemaTestHelpers.AssertStandardScanRelationshipAsync(
            Inspector, "MyEntities", "ScanId");
    }
}
```

## Best Practices

1. **Use Inspector for all schema queries** - Don't write raw SQL in tests
2. **Use fluent extensions** - Makes tests read like specifications
3. **Use SchemaTestHelpers for common patterns** - Don't repeat yourself
4. **Test one concern per test method** - Keep tests focused and clear
5. **Add descriptive `because` messages** - Helps diagnose failures
6. **Use traits for organization** - `[Trait("Category", "P1")]` for priority
7. **Test cascade behavior comprehensively** - Verify circular cascade prevention

## Troubleshooting

### Test Database Connection Issues

If tests fail with connection errors, verify environment variables in `.devcontainer/devcontainer.json`:
```json
"containerEnv": {
    "TEST_DB_PASSWORD": "YourStrong@Passw0rd",
    "TEST_DB_SERVER": "localhost,1433",
    "TEST_DB_USER": "sa"
}
```

### Schema Inspection Returns Null

Ensure `EnsureCreatedAsync()` is called before schema inspection. The base class handles this automatically in `InitializeAsync()`.

### FluentAssertions Extensions Not Found

Ensure you have `using DAO.Manager.Data.Tests.Framework;` at the top of your test file.

## Next Steps

1. **Validate POC**: Run ScanConfigurationTests to ensure framework works correctly
2. **Migrate remaining tests**: Apply framework to ScanEvent, Solution, Project, Package, Assembly configurations
3. **Add parameterized tests**: Create CommonConfigurationTests with Theory tests for cross-entity patterns
4. **Extend as needed**: Add new extension methods or helpers for project-specific patterns

## Files

- [ConfigurationTestBase.cs](ConfigurationTestBase.cs) - Base test class
- [SchemaInspector.cs](SchemaInspector.cs) - Schema inspection utilities
- [SchemaAssertionExtensions.cs](SchemaAssertionExtensions.cs) - FluentAssertions extensions
- [SchemaTestHelpers.cs](SchemaTestHelpers.cs) - High-level test patterns
- [../Configurations/ScanConfigurationTests.cs](../Configurations/ScanConfigurationTests.cs) - Proof of concept

---

**Framework Version**: 1.0  
**Last Updated**: February 11, 2026  
**Target Framework**: .NET 8.0 / EF Core 8.0 / xUnit 2.5.3
