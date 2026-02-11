# DAO.Manager.Data - Test Plan

**Version:** 1.0  
**Date:** February 11, 2026  
**Project:** DAO.Manager.Data  
**Current Coverage:** 0% (0 of 1442 lines, 0 of 13 methods)  
**Test Framework:** xUnit 2.5.3  

---

## Executive Summary

This document outlines the comprehensive testing strategy for the DAO.Manager.Data project, which implements the data access layer for the DAO Manager application. The project currently has **zero test coverage** and requires a structured approach to achieve adequate test coverage and confidence in the data access layer's correctness.

### Project Context

The DAO.Manager.Data project implements a **scan-owned snapshot architecture** using Entity Framework Core 8.0 with SQL Server. The domain model consists of:

- **6 Entity Classes**: Scan (root aggregate), ScanEvent, Solution, Project, Package, Assembly
- **1 DbContext**: ApplicationDbContext managing all entity sets
- **6 Configuration Classes**: IEntityTypeConfiguration implementations for each entity
- **5 Junction Tables**: Supporting many-to-many relationships
- **1 Migration**: InitialNormalizedSchema (419 lines)
- **1 Design-Time Factory**: ApplicationDbContextFactory for EF Core tooling

For detailed entity specifications, see [src/DAO.Manager.Data/docs/entity-model-specification.md](../../src/DAO.Manager.Data/docs/entity-model-specification.md).

### Testing Goals

1. **Verify EF Core Configuration Correctness**: Ensure all entity configurations match the specification
2. **Validate Relationship Behavior**: Confirm cascade deletes, many-to-many, and self-referencing relationships work correctly
3. **Test Data Integrity**: Verify unique constraints, required fields, and validation rules
4. **Ensure CRUD Operations**: Validate basic create, read, update, delete operations
5. **Test Complex Scenarios**: Verify realistic usage patterns and cross-entity queries
6. **Prevent Regressions**: Establish baseline for future changes

### Success Criteria

- ✅ **Minimum 80% Line Coverage** on all configuration classes and DbContext
- ✅ **100% Method Coverage** on all testable methods
- ✅ **All P1 and P2 Tests Implemented** (Configuration and Entity tests)
- ✅ **All Cascade Delete Scenarios Tested** (critical for data integrity)
- ✅ **All Unique Constraints Verified** (prevent duplicate data within scans)
- ✅ **Migration Applied Successfully** in test environments
- ✅ **Zero Test Failures** in CI/CD pipeline

---

## Testing Strategy

### 1. Test Database Strategy

#### Primary: Shared SQL Server with Isolated Databases (Devcontainer/CI Approach)

**Rationale:**
- ✅ **Accurate Behavior**: Real SQL Server ensures cascade delete, NO ACTION, and constraint behavior matches production
- ✅ **Complete Feature Support**: All SQL Server features (indexes, triggers, stored procedures) work correctly
- ✅ **Fast**: No container startup overhead - databases are created in milliseconds
- ✅ **Isolation**: Each test fixture gets a uniquely named database on the shared server
- ✅ **Devcontainer Ready**: Works out-of-the-box in VS Code devcontainers with pre-configured SQL Server
- ✅ **CI/CD Ready**: Simple to configure in pipelines with a persistent SQL Server service
- ⚠️ **Requires Pre-existing SQL Server**: Must have SQL Server available on localhost:1433 or configured endpoint

**Implementation:**
```csharp
// Uses TestDbContextFactory which reads from environment variables:
// - TEST_DB_PASSWORD (required)
// - TEST_DB_SERVER (default: localhost,1433)
// - TEST_DB_USER (default: sa)
public class TestFixture : IAsyncLifetime
{
    private readonly TestDbContextFactory _factory = new();
    
    public async Task InitializeAsync()
    {
        var context = await _factory.CreateDbContextAsync();
        // Database is created and migrated automatically
    }
    
    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        // Database cleanup is optional (see factory configuration)
    }
}
```

**Configuration:**
Set environment variables in `.devcontainer/devcontainer.json`:
```json
"containerEnv": {
    "TEST_DB_PASSWORD": "YourStrong@Passw0rd",
    "TEST_DB_SERVER": "localhost,1433",
    "TEST_DB_USER": "sa"
}
```

#### Alternative 1: SQL Server with TestContainers (Isolated Environments)

**When to Use:**
- ✅ Running tests on developer machines without devcontainer
- ✅ CI/CD environments that don't provide a persistent SQL Server service
- ✅ Need guaranteed complete isolation (separate container per test class)
- ✅ Testing against multiple SQL Server versions

**Limitations:**
- ⚠️ **Requires Docker**: Docker must be installed and running
- ⚠️ **Slower**: Container startup adds ~2-5 seconds per test class
- ⚠️ **Resource Intensive**: Each test class spawns a new container

**Implementation:**
```csharp
// NuGet: Testcontainers.MsSql (NOT currently installed)
private readonly MsSqlContainer _container = new MsSqlBuilder()
    .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
    .Build();
```

**Note:** TestContainers is not currently used in this project. To switch to TestContainers:
1. Install `Testcontainers.MsSql` NuGet package
2. Create an alternative factory class (e.g., `TestContainersDbContextFactory`)
3. Update test fixtures to use the new factory

#### Alternative 2: SQLite In-Memory (Fast Unit Tests Only)

**When to Use:**
- ✅ **Simple Property Tests**: Entity instantiation, property assignment
- ✅ **No SQL Server Available**: Quick feedback without infrastructure
- ⚠️ **NOT for Configuration Tests**: Schema, indexes, and constraints differ significantly
- ⚠️ **NOT for Relationship Tests**: Cascade delete behavior doesn't match SQL Server
- ⚠️ **NOT for Integration Tests**: Many SQL Server features unsupported

**Limitations:**
- ❌ **Limited SQL Server Features**: Some behaviors differ (cascade rules, computed columns, indexes)
- ❌ **Schema Differences**: Cannot verify actual SQL Server schema
- ❌ **Constraint Behavior**: Unique constraints and foreign key rules may differ

**Implementation:**
```csharp
// Connection must stay open for in-memory database to persist
private readonly DbConnection _connection = new SqliteConnection("DataSource=:memory:");
```

**Note:** SQLite should only be used for simple entity tests where SQL Server-specific behavior is not being validated.

#### Decision Matrix

| Test Category | Primary Strategy | Alternative Strategy |
|--------------|------------------|----------------------|
| Configuration Tests | **Shared SQL Server** (Isolated DBs) | TestContainers (if no pre-existing SQL Server) |
| Entity Property Tests | **Shared SQL Server** (Isolated DBs) | SQLite In-Memory (simple tests only) |
| CRUD Operations | **Shared SQL Server** (Isolated DBs) | TestContainers |
| Cascade Delete Tests | **Shared SQL Server** (Isolated DBs) | TestContainers (N/A for SQLite) |
| Relationship Tests | **Shared SQL Server** (Isolated DBs) | TestContainers (N/A for SQLite) |
| Migration Tests | **Shared SQL Server** (Isolated DBs) | TestContainers |
| Integration Tests | **Shared SQL Server** (Isolated DBs) | TestContainers (N/A for SQLite) |

**Current Implementation:** All tests use the **Shared SQL Server** approach via `TestDbContextFactory`.

### 2. Test Infrastructure Components

#### A. TestDbContextFactory

**Purpose**: Create configured DbContext instances for tests with unique databases

**Features:**
- Apply migrations automatically
- Generate unique database names per test
- Clean up after test execution (optional)
- Support both TestContainers and in-memory providers

**Location**: `tests/DAO.Manager.Data.Tests/Helpers/TestDbContextFactory.cs`

#### B. Entity Builder Pattern

**Purpose**: Fluent API for creating test entities with reasonable defaults

**Benefits:**
- Reduces boilerplate in test code
- Ensures valid entity graphs
- Makes tests readable and maintainable
- Allows customization of specific properties

**Example:**
```csharp
var scan = new ScanBuilder()
    .WithRepositoryPath("/repo/path")
    .WithGitCommit("abc123...")
    .WithScanDate(DateTime.UtcNow)
    .Build();

var project = new ProjectBuilder()
    .WithScan(scan)
    .WithName("MyProject")
    .WithFilePath("/src/MyProject/MyProject.csproj")
    .WithPackages(
        new PackageBuilder().WithName("Newtonsoft.Json").WithVersion("13.0.1"),
        new PackageBuilder().WithName("Serilog").WithVersion("3.1.0")
    )
    .Build();
```

**Location**: `tests/DAO.Manager.Data.Tests/Helpers/EntityBuilders/`

#### C. Assertion Extensions

**Purpose**: Custom assertions for verifying EF Core behaviors

**Examples:**
```csharp
dbContext.ShouldHaveTable("Scans");
dbContext.ShouldHaveIndex("Scans", "IX_Scans_ScanDate");
dbContext.ShouldHaveForeignKey("ScanEvents", "ScanId", "Scans", "Id");
entity.ShouldBePersistedCorrectly(dbContext);
```

**Location**: `tests/DAO.Manager.Data.Tests/Helpers/AssertionExtensions.cs`

#### D. Test Data Repository

**Purpose**: Pre-built, realistic test data for complex scenarios

**Contents:**
- Complete scan with multiple solutions, projects, packages, assemblies
- Edge cases: Empty scans, single-project scans, circular dependencies
- Performance test data: Large scans with hundreds of projects

**Location**: `tests/DAO.Manager.Data.Tests/Helpers/TestData.cs`

### 3. Test Organization Structure

```
tests/DAO.Manager.Data.Tests/
├── Configurations/              # P1: EF Core configuration tests
│   ├── ScanConfigurationTests.cs
│   ├── ScanEventConfigurationTests.cs
│   ├── SolutionConfigurationTests.cs
│   ├── ProjectConfigurationTests.cs
│   ├── PackageConfigurationTests.cs
│   └── AssemblyConfigurationTests.cs
├── DbContext/                   # P1: DbContext behavior tests
│   ├── ApplicationDbContextTests.cs
│   └── ApplicationDbContextFactoryTests.cs
├── Entities/                    # P2: Entity behavior tests
│   ├── ScanTests.cs
│   ├── ScanEventTests.cs
│   ├── SolutionTests.cs
│   ├── ProjectTests.cs
│   ├── PackageTests.cs
│   └── AssemblyTests.cs
├── Relationships/               # P2: Relationship behavior tests
│   ├── CascadeDeleteTests.cs
│   ├── ManyToManyTests.cs
│   ├── SelfReferencingTests.cs
│   └── NavigationPropertyTests.cs
├── Operations/                  # P3: CRUD operation tests
│   ├── InsertTests.cs
│   ├── QueryTests.cs
│   ├── UpdateTests.cs
│   └── DeleteTests.cs
├── Integration/                 # P4: Complex scenario tests
│   ├── CompleteScanLifecycleTests.cs
│   ├── CrossEntityQueryTests.cs
│   ├── TemporalAnalysisTests.cs
│   └── BulkOperationsTests.cs
├── Migrations/                  # P5: Migration tests
│   └── InitialNormalizedSchemaTests.cs
├── Helpers/                     # Test infrastructure
│   ├── TestDbContextFactory.cs
│   ├── AssertionExtensions.cs
│   ├── TestData.cs
│   └── EntityBuilders/
│       ├── ScanBuilder.cs
│       ├── SolutionBuilder.cs
│       ├── ProjectBuilder.cs
│       ├── PackageBuilder.cs
│       └── AssemblyBuilder.cs
└── UnitTest1.cs                 # DELETE after migration
```

### 4. Test Execution Strategy

#### Local Development

```bash
# Run all tests
dotnet test

# Run with coverage
./test-coverage.sh

# Run specific test class
dotnet test --filter "FullyQualifiedName~ScanConfigurationTests"

# Run specific priority
dotnet test --filter "Category=P1"
```

#### CI/CD Pipeline

1. **Pull Request Validation**
   - Run all P1 and P2 tests (fast, critical)
   - Generate coverage report
   - Fail if coverage drops below 80%

2. **Main Branch Integration**
   - Run all tests (P1-P5)
   - Generate full coverage report
   - Archive coverage artifacts
   - Update coverage badge

3. **Nightly Builds**
   - Run all tests including performance tests
   - Test against multiple SQL Server versions
   - Generate trend reports

---

## Test Priority Levels

### Priority 1 (P1): Configuration & DbContext Tests - CRITICAL

**Scope**: Verify EF Core setup is correct

**Test Count**: ~40 tests  
**Estimated Effort**: 2-3 days  
**Coverage Target**: 100% of configuration classes and DbContext  

**Why Critical:**
- Incorrect configuration causes runtime errors in production
- Schema mismatches lead to data corruption
- Cascade delete bugs can orphan or delete wrong data
- Index misconfiguration causes performance issues

**Test Categories:**
- ✅ Schema verification (table names, column types, max lengths)
- ✅ Index verification (primary keys, unique indexes, composite indexes)
- ✅ Relationship verification (foreign keys, cascade rules, junction tables)
- ✅ Constraint verification (required fields, unique constraints, defaults)
- ✅ DbContext configuration (DbSets registered, model building)

**Detailed Test Cases**: See [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md#priority-1-configuration-tests)

### Priority 2 (P2): Entity & Relationship Tests - HIGH

**Scope**: Verify entity behaviors and relationship operations

**Test Count**: ~50 tests  
**Estimated Effort**: 3-4 days  
**Coverage Target**: 90% of entity code and relationship paths  

**Why High Priority:**
- Validates domain model works as designed
- Ensures navigation properties function correctly
- Verifies cascade delete behavior in practice
- Catches relationship configuration errors

**Test Categories:**
- ✅ Entity instantiation and property assignment
- ✅ Navigation property initialization
- ✅ Cascade delete execution (Scan → all children)
- ✅ Many-to-many relationship operations
- ✅ Self-referencing relationship operations
- ✅ Bidirectional navigation property consistency

**Detailed Test Cases**: See [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md#priority-2-entity-and-relationship-tests)

### Priority 3 (P3): CRUD Operation Tests - MEDIUM

**Scope**: Verify basic database operations work correctly

**Test Count**: ~40 tests  
**Estimated Effort**: 2-3 days  
**Coverage Target**: 80% of common usage patterns  

**Why Medium Priority:**
- Most CRUD operations are standard EF Core
- Validates assumptions about EF Core behavior
- Necessary for integration testing
- Useful for documentation

**Test Categories:**
- ✅ Insert operations (single entity, entity graphs, bulk)
- ✅ Query operations (filtering, eager loading, projections)
- ✅ Update operations (property changes, collection modifications)
- ✅ Delete operations (single entity, cascade verification)
- ✅ Transaction handling (commit, rollback, concurrency)

**Detailed Test Cases**: See [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md#priority-3-crud-operation-tests)

### Priority 4 (P4): Integration & Complex Scenario Tests - MEDIUM

**Scope**: Verify realistic usage patterns and complex queries

**Test Count**: ~30 tests  
**Estimated Effort**: 3-4 days  
**Coverage Target**: 70% of integration scenarios  

**Why Medium Priority:**
- Validates end-to-end workflows
- Catches integration issues missed by unit tests
- Documents intended usage patterns
- Useful for performance baseline

**Test Categories:**
- ✅ Complete scan lifecycle (create → query → update → delete)
- ✅ Cross-entity queries (dependency analysis, package usage)
- ✅ Temporal analysis (compare scans, track changes over time)
- ✅ Bulk operations (import large scans, batch updates)
- ✅ Performance benchmarks (N+1 detection, query optimization)

**Detailed Test Cases**: See [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md#priority-4-integration-tests)

### Priority 5 (P5): Migration & Edge Case Tests - LOW

**Scope**: Verify migrations and handle edge cases

**Test Count**: ~20 tests  
**Estimated Effort**: 1-2 days  
**Coverage Target**: 60% of edge cases  

**Why Low Priority:**
- Migrations tested manually during development
- Edge cases are rare in production
- Nice-to-have for completeness
- Can be added incrementally

**Test Categories:**
- ✅ Migration application (up, down, idempotency)
- ✅ Schema verification after migration
- ✅ Data migration scenarios
- ✅ Edge cases (empty collections, null references, max lengths)
- ✅ Error handling (duplicate keys, constraint violations)

**Detailed Test Cases**: See [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md#priority-5-migration-and-edge-case-tests)

---

## Implementation Roadmap

### Phase 1: Foundation (Week 1)

**Goal**: Set up test infrastructure and P1 tests

**Tasks:**
1. ✅ Create TestDbContextFactory with shared SQL Server support
2. ✅ Configure environment variables for devcontainer SQL Server
3. ✅ Create basic assertion extensions
4. ✅ Implement all P1 configuration tests (~40 tests)
5. ✅ Verify 100% coverage on configuration classes
6. ✅ Document any schema discrepancies found

**Deliverables:**
- Working TestDbContextFactory
- All 6 configuration test classes complete
- ApplicationDbContext tests complete
- Coverage report showing configuration coverage

**Exit Criteria:**
- All P1 tests passing
- Configuration class coverage ≥ 95%
- No schema validation errors

### Phase 2: Core Testing (Week 2)

**Goal**: Implement P2 entity and relationship tests

**Tasks:**
1. ✅ Create entity builder pattern for all 6 entities
2. ✅ Implement entity instantiation tests (~30 tests)
3. ✅ Implement cascade delete tests (~10 tests)
4. ✅ Implement many-to-many relationship tests (~10 tests)
5. ✅ Verify navigation properties work bidirectionally
6. ✅ Test self-referencing relationships (Project → Project, Assembly → Assembly)

**Deliverables:**
- All 6 entity test classes complete
- All 4 relationship test classes complete
- Entity builder implementations
- Test data repository with sample data

**Exit Criteria:**
- All P2 tests passing
- Overall coverage ≥ 70%
- All cascade delete scenarios verified

### Phase 3: Operations Testing (Week 3)

**Goal**: Implement P3 CRUD operation tests

**Tasks:**
1. ✅ Implement insert tests with various scenarios (~10 tests)
2. ✅ Implement query tests with filtering and eager loading (~15 tests)
3. ✅ Implement update tests for properties and collections (~10 tests)
4. ✅ Implement delete tests with cleanup verification (~5 tests)
5. ✅ Test transaction boundaries and concurrency

**Deliverables:**
- All 4 operation test classes complete
- CRUD operation test documentation
- Performance baseline measurements

**Exit Criteria:**
- All P3 tests passing
- Overall coverage ≥ 80%
- No performance regressions identified

### Phase 4: Integration Testing (Week 4)

**Goal**: Implement P4 integration and complex scenario tests

**Tasks:**
1. ✅ Implement complete scan lifecycle tests (~5 tests)
2. ✅ Implement cross-entity query tests (~10 tests)
3. ✅ Implement temporal analysis tests (~5 tests)
4. ✅ Create bulk operation tests (~5 tests)
5. ✅ Add performance benchmarks and N+1 detection

**Deliverables:**
- All integration test classes complete
- Performance benchmark report
- Identified optimization opportunities

**Exit Criteria:**
- All P4 tests passing
- Overall coverage ≥ 85%
- Performance benchmarks documented

### Phase 5: Completion (Week 5)

**Goal**: Implement P5 tests and finalize

**Tasks:**
1. ✅ Implement migration tests (~10 tests)
2. ✅ Implement edge case tests (~10 tests)
3. ✅ Review and refactor test code
4. ✅ Generate final coverage report
5. ✅ Update documentation
6. ✅ Remove placeholder UnitTest1.cs

**Deliverables:**
- All P5 tests complete
- Final coverage report
- Test documentation complete
- CI/CD pipeline configured

**Exit Criteria:**
- All tests passing (P1-P5)
- Overall coverage ≥ 80%
- Documentation complete
- CI/CD pipeline working

---

## Required NuGet Packages

### Test Project Dependencies

#### Core Testing Packages (Currently Installed)

```xml
<PackageReference Include="xunit" Version="2.5.3" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.3" />
<PackageReference Include="coverlet.collector" Version="6.0.0" />
<PackageReference Include="coverlet.msbuild" Version="6.0.0" />

<!-- FluentAssertions for better assertions -->
<PackageReference Include="FluentAssertions" Version="6.12.0" />

<!-- For verifying EF Core behaviors -->
<PackageReference Include="Microsoft.EntityFrameworkCore.Relational" Version="8.0.1" />
```

#### Optional Packages (Not Currently Used)

```xml
<!-- TestContainers - Only needed if switching from shared SQL Server -->
<!-- <PackageReference Include="Testcontainers.MsSql" Version="3.7.0" /> -->

<!-- SQLite - Only needed for simple property tests without SQL Server -->
<!-- <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="8.0.1" /> -->

<!-- Moq - Only needed if mocking is required -->
<!-- <PackageReference Include="Moq" Version="4.20.70" /> -->
```

### Installation Commands

```bash
cd tests/DAO.Manager.Data.Tests

# Core packages (already installed)
dotnet add package FluentAssertions --version 6.12.0

# Optional: Install TestContainers (only if not using shared SQL Server)
# dotnet add package Testcontainers.MsSql --version 3.7.0

# Optional: Install SQLite (only for simple property tests)
# dotnet add package Microsoft.EntityFrameworkCore.Sqlite --version 8.0.1
```

---

## Coverage Targets by Component

| Component | Target Coverage | Rationale |
|-----------|----------------|-----------|
| Configuration Classes | 95-100% | Critical for data integrity |
| ApplicationDbContext | 90-100% | Core data access component |
| ApplicationDbContextFactory | 80-90% | Design-time tooling, less critical |
| Entity Classes | 70-80% | POCOs with minimal logic |
| Migrations | 60-70% | Manually verified, tests for confidence |
| **Overall Project** | **≥ 80%** | Industry standard for data access layers |

---

## Testing Best Practices

### 1. Test Naming Convention

Use descriptive names that explain the scenario and expected outcome:

```csharp
[Fact]
public void ScanConfiguration_ShouldCreateTableWithCorrectName()

[Fact]
public void ScanConfiguration_ShouldCreateIndexOnScanDate()

[Fact]
public void DeleteScan_ShouldCascadeDeleteAllScanEvents()

[Fact]
public void ProjectConfiguration_ShouldPreventCircularCascadeOnSelfReference()

[Theory]
[InlineData("", "GitCommit cannot be empty")]
[InlineData(null, "GitCommit is required")]
public void Scan_ShouldValidateGitCommit(string gitCommit, string expectedError)
```

### 2. Arrange-Act-Assert Pattern

Structure tests clearly with AAA pattern:

```csharp
[Fact]
public async Task Example()
{
    // Arrange
    using var context = await _factory.CreateDbContextAsync();
    var scan = new ScanBuilder().Build();
    
    // Act
    context.Scans.Add(scan);
    await context.SaveChangesAsync();
    
    // Assert
    var saved = await context.Scans.FindAsync(scan.Id);
    saved.Should().NotBeNull();
    saved!.RepositoryPath.Should().Be(scan.RepositoryPath);
}
```

### 3. Database Isolation

Each test or test class should use an isolated database:

```csharp
public class ScanConfigurationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container;
    private ApplicationDbContext _context;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        _context = CreateContext();
        await _context.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }
}
```

### 4. Test Data Builders

Use builders to reduce boilerplate and improve readability:

```csharp
// Bad: Verbose and hard to maintain
var scan = new Scan
{
    RepositoryPath = "/repo",
    GitCommit = "abc123",
    ScanDate = DateTime.UtcNow,
    CreatedAt = DateTime.UtcNow
};

// Good: Concise and flexible
var scan = new ScanBuilder()
    .WithRepositoryPath("/repo")
    .Build();
```

### 5. Async Test Methods

Use async/await consistently for database operations:

```csharp
[Fact]
public async Task DeleteScan_ShouldCascadeDelete()
{
    // Arrange
    using var context = await _factory.CreateDbContextAsync();
    var scan = await CreateScanWithEventsAsync(context);
    
    // Act
    context.Scans.Remove(scan);
    await context.SaveChangesAsync();
    
    // Assert
    var events = await context.ScanEvents
        .Where(e => e.ScanId == scan.Id)
        .ToListAsync();
    events.Should().BeEmpty();
}
```

### 6. Test Categories

Use traits to categorize tests for selective execution:

```csharp
[Fact]
[Trait("Category", "P1")]
[Trait("Area", "Configuration")]
public void ScanConfiguration_ShouldCreateCorrectSchema()
{
    // Test implementation
}
```

---

## Risk Assessment

### High Risk Areas

1. **Cascade Delete Strategy**
   - **Risk**: Incorrect cascade configuration could delete unintended data or leave orphans
   - **Mitigation**: Comprehensive P1 and P2 tests for all cascade scenarios
   - **Priority**: P1

2. **Self-Referencing Relationships with NO ACTION**
   - **Risk**: Circular cascade conflicts or incorrect constraint setup
   - **Mitigation**: Dedicated tests for ProjectReferences and AssemblyDependencies
   - **Priority**: P1

3. **Many-to-Many Junction Tables**
   - **Risk**: Junction records not created/deleted correctly
   - **Mitigation**: Tests for all 5 junction tables (SolutionProjects, ProjectReferences, etc.)
   - **Priority**: P2

4. **Unique Constraints**
   - **Risk**: Duplicate data within scans or constraint failures
   - **Mitigation**: Tests for all unique indexes (ScanId + UniqueIdentifier, ScanId + Name + Version)
   - **Priority**: P1

### Medium Risk Areas

5. **Query Performance (N+1)**
   - **Risk**: Inefficient queries cause performance degradation
   - **Mitigation**: P4 performance tests with query profiling
   - **Priority**: P4

6. **Concurrency**
   - **Risk**: Concurrent updates cause data corruption
   - **Mitigation**: P3 transaction and concurrency tests
   - **Priority**: P3

### Low Risk Areas

7. **Entity Property Validation**
   - **Risk**: Invalid data saved to database
   - **Mitigation**: P2 entity validation tests
   - **Priority**: P2

---

## References

- **Entity Specification**: [src/DAO.Manager.Data/docs/entity-model-specification.md](../../src/DAO.Manager.Data/docs/entity-model-specification.md)
- **Test Cases**: [dao-manager-data-test-cases.md](dao-manager-data-test-cases.md)
- **Coverage Setup**: [coverage/code-coverage.md](coverage/code-coverage.md)
- **Coverage Examples**: [coverage/code-coverage-examples.md](coverage/code-coverage-examples.md)
- **Entity Framework Core Documentation**: https://learn.microsoft.com/en-us/ef/core/
- **TestContainers Documentation**: https://dotnet.testcontainers.org/
- **xUnit Documentation**: https://xunit.net/

---

## Appendix: Example Test Template

### Using Shared SQL Server (Current Implementation)

```csharp
using FluentAssertions;
using Xunit;
using DAO.Manager.Data.Tests.Helpers;

namespace DAO.Manager.Data.Tests.Configurations;

public class ScanConfigurationTests : IAsyncLifetime
{
    private readonly TestDbContextFactory _factory = new();
    private ApplicationDbContext _context = null!;
    
    public async Task InitializeAsync()
    {
        _context = await _factory.CreateDbContextAsync();
        // Database is created and migrated automatically
    }
    
    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
    }
    
    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldCreateTableWithCorrectName()
    {
        // Arrange & Act
        var tableExists = await _context.Database
            .ExecuteSqlRawAsync("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Scans'") > 0;
        
        // Assert
        tableExists.Should().BeTrue("Scans table should be created by migration");
    }
    
    // Additional tests...
}
```

### Alternative: Using TestContainers (If Needed)

```csharp
using FluentAssertions;
using Testcontainers.MsSql;
using Xunit;

namespace DAO.Manager.Data.Tests.Configurations;

public class ScanConfigurationTests : IAsyncLifetime
{
    private readonly MsSqlContainer _container = new MsSqlBuilder()
        .WithImage("mcr.microsoft.com/mssql/server:2022-latest")
        .Build();
    
    private ApplicationDbContext _context = null!;
    
    public async Task InitializeAsync()
    {
        await _container.StartAsync();
        
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_container.GetConnectionString())
            .Options;
        
        _context = new ApplicationDbContext(options);
        await _context.Database.MigrateAsync();
    }
    
    public async Task DisposeAsync()
    {
        await _context.DisposeAsync();
        await _container.DisposeAsync();
    }
    
    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldCreateTableWithCorrectName()
    {
        // Arrange & Act
        var tableExists = await _context.Database
            .ExecuteSqlRawAsync("SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Scans'") > 0;
        
        // Assert
        tableExists.Should().BeTrue("Scans table should be created by migration");
    }
    
    // Additional tests...
}
```

---

**Document Status**: ✅ Ready for Implementation  
**Next Steps**: Review test plan → Create detailed test cases → Begin Phase 1 implementation
