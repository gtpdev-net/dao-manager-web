# DAO.Manager.Data - Detailed Test Cases

**Version:** 1.0  
**Date:** February 11, 2026  
**Parent Document**: [dao-manager-data-test-plan.md](dao-manager-data-test-plan.md)

---

## Overview

This document provides a comprehensive catalog of test cases for the DAO.Manager.Data project, organized by priority level and component. Each test case includes:

- **Test Name**: Descriptive name following naming convention
- **Priority**: P1 (Critical) → P5 (Low)
- **Category**: Configuration, Entity, Relationship, CRUD, Integration, Migration
- **Objective**: What the test verifies
- **Setup**: Prerequisites and test data needed
- **Assertions**: Expected outcomes
- **Related Code**: Links to source files being tested

For implementation strategy and testing approach, see [dao-manager-data-test-plan.md](dao-manager-data-test-plan.md).

---

## Priority 1: Configuration Tests

### P1.1: Scan Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/ScanConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/ScanConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ScanConfiguration.cs)  
**Estimated Tests**: 8

#### P1.1.1: Table Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanConfiguration_ShouldCreateTableWithCorrectName` | Verify table name is "Scans" | Table exists with exact name "Scans" |
| `ScanConfiguration_ShouldHavePrimaryKeyOnId` | Verify primary key on Id column | Primary key constraint exists on Id, auto-increment enabled |
| `ScanConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | RepositoryPath: nvarchar(2000)<br>GitCommit: nvarchar(100)<br>ScanDate: datetime2<br>CreatedAt: datetime2 |
| `ScanConfiguration_ShouldSetCorrectMaxLengths` | Verify max length constraints | RepositoryPath max length = 2000<br>GitCommit max length = 100 |
| `ScanConfiguration_ShouldSetRequiredFields` | Verify NOT NULL constraints | RepositoryPath, GitCommit, ScanDate, CreatedAt are NOT NULL |

#### P1.1.2: Index Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanConfiguration_ShouldCreateIndexOnScanDate` | Verify index on ScanDate | Index IX_Scans_ScanDate exists, non-unique |
| `ScanConfiguration_ShouldCreateIndexOnCreatedAt` | Verify index on CreatedAt | Index IX_Scans_CreatedAt exists, non-unique |
| `ScanConfiguration_ShouldCreateIndexOnGitCommit` | Verify index on GitCommit | Index IX_Scans_GitCommit exists, non-unique |

---

### P1.2: ScanEvent Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/ScanEventConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/ScanEventConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ScanEventConfiguration.cs)  
**Estimated Tests**: 7

#### P1.2.1: Table Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanEventConfiguration_ShouldCreateTableWithCorrectName` | Verify table name is "ScanEvents" | Table exists with exact name "ScanEvents" |
| `ScanEventConfiguration_ShouldHavePrimaryKeyOnId` | Verify primary key on Id column | Primary key constraint exists on Id |
| `ScanEventConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | Phase: nvarchar(100)<br>Message: nvarchar(2000)<br>OccurredAt: datetime2 |
| `ScanEventConfiguration_ShouldSetMaxLengths` | Verify max length constraints | Phase max length = 100<br>Message max length = 2000 |

#### P1.2.2: Foreign Key and Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanEventConfiguration_ShouldHaveForeignKeyToScans` | Verify FK to Scans table | Foreign key exists: ScanEvents.ScanId → Scans.Id |
| `ScanEventConfiguration_ShouldCascadeDeleteOnScanDelete` | Verify cascade delete rule | FK cascade rule = CASCADE (ON DELETE CASCADE) |

#### P1.2.3: Index Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanEventConfiguration_ShouldCreateCompositeIndexOnScanIdAndOccurredAt` | Verify composite index for temporal queries | Index IX_ScanEvents_ScanId_OccurredAt exists on (ScanId, OccurredAt) |

---

### P1.3: Solution Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/SolutionConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/SolutionConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/SolutionConfiguration.cs)  
**Estimated Tests**: 9

#### P1.3.1: Table Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `SolutionConfiguration_ShouldCreateTableWithCorrectName` | Verify table name is "Solutions" | Table exists with exact name "Solutions" |
| `SolutionConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | UniqueIdentifier: nvarchar(500)<br>VisualStudioGuid: nvarchar(100)<br>FilePath: nvarchar(2000)<br>Name: nvarchar(500) |

#### P1.3.2: Unique Constraint Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `SolutionConfiguration_ShouldHaveUniqueIndexOnScanIdAndUniqueIdentifier` | Verify unique constraint | Unique index IX_Solutions_ScanId_UniqueIdentifier exists on (ScanId, UniqueIdentifier) |
| `SolutionConfiguration_ShouldPreventDuplicateUniqueIdentifierWithinScan` | Verify duplicate prevention | Attempting to insert duplicate (ScanId, UniqueIdentifier) throws DbUpdateException |

#### P1.3.3: Many-to-Many Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `SolutionConfiguration_ShouldCreateSolutionProjectsJunctionTable` | Verify junction table creation | SolutionProjects table exists with columns: SolutionId, ProjectId |
| `SolutionConfiguration_ShouldHaveForeignKeysInJunctionTable` | Verify junction FK constraints | FK: SolutionProjects.SolutionId → Solutions.Id (NO ACTION)<br>FK: SolutionProjects.ProjectId → Projects.Id (CASCADE) |
| `SolutionConfiguration_ShouldHaveCompositePrimaryKeyOnJunctionTable` | Verify junction PK | Primary key on (SolutionId, ProjectId) |
| `SolutionConfiguration_ShouldCascadeDeleteJunctionRecordsOnProjectDelete` | Verify cascade on junction from Project | Deleting Project removes SolutionProjects rows via CASCADE |
| `SolutionConfiguration_ShouldUseNoActionOnSolutionDelete` | Verify NO ACTION prevents circular cascade from Scan | FK SolutionProjects.SolutionId has NO ACTION to avoid circular cascade path from Scan |

---

### P1.4: Project Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/ProjectConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/ProjectConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ProjectConfiguration.cs)  
**Estimated Tests**: 15 (Most complex entity)

#### P1.4.1: Table Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldCreat eTableWithCorrectName` | Verify table name is "Projects" | Table exists with exact name "Projects" |
| `ProjectConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | All string columns have correct nvarchar types and max lengths |
| `ProjectConfiguration_ShouldHaveUniqueIndexOnScanIdAndUniqueIdentifier` | Verify unique constraint | Unique index IX_Projects_ScanId_UniqueIdentifier exists |

#### P1.4.2: Self-Referencing Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldCreateProjectReferencesJunctionTable` | Verify self-referencing junction | ProjectReferences table exists with ReferencingProjectId, ReferencedProjectId |
| `ProjectConfiguration_ShouldUseNoActionOnReferencedProjectDelete` | Verify NO ACTION prevents circular cascade | FK ProjectReferences.ReferencedProjectId has NO ACTION on delete |
| `ProjectConfiguration_ShouldUseCascadeOnReferencingProjectDelete` | Verify CASCADE on referencing side | FK ProjectReferences.ReferencingProjectId has CASCADE on delete |
| `ProjectConfiguration_ShouldPreventCircularCascadeConflict` | Verify schema prevents circular cascade | Can create and delete projects with mutual references without errors |

#### P1.4.3: Many-to-Many with Solutions

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldConfigureManyToManyWithSolutions` | Verify SolutionProjects junction configured | SolutionProjects table has correct structure for many-to-many |

#### P1.4.4: Many-to-Many with Packages

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldCreateProjectPackageReferencesJunctionTable` | Verify package junction table | ProjectPackageReferences table exists with ProjectId, PackageId |
| `ProjectConfiguration_ShouldHaveCorrectCascadeBehaviorForPackages` | Verify cascade behaviors | FK ProjectPackageReferences.ProjectId has CASCADE (delete junction when Project deleted)<br>FK ProjectPackageReferences.PackageId has NO ACTION (avoid circular cascade from Scan) |

#### P1.4.5: Many-to-Many with Assemblies

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldCreateProjectAssemblyReferencesJunctionTable` | Verify assembly junction table | ProjectAssemblyReferences table exists with ProjectId, AssemblyId |
| `ProjectConfiguration_ShouldHaveCorrectCascadeBehaviorForAssemblies` | Verify cascade behaviors | FK ProjectAssemblyReferences.ProjectId has CASCADE (delete junction when Project deleted)<br>FK ProjectAssemblyReferences.AssemblyId has NO ACTION (avoid circular cascade from Scan) |

#### P1.4.6: All Relationships Integration Test

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ProjectConfiguration_ShouldManageAllFourRelationshipsCorrectly` | Verify all 4 many-to-many relationships work | Project can have: Solutions, ReferencedProjects, Packages, Assemblies<br>All junction tables populated correctly |
| `ProjectConfiguration_ShouldCascadeDeleteAllRelationshipsOnScanDelete` | Verify complete cascade from Scan via Project | Deleting Scan cascades to Projects, then Projects cascade to all 4 junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences) |

---

### P1.5: Package Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/PackageConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/PackageConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/PackageConfiguration.cs)  
**Estimated Tests**: 6

#### P1.5.1: Normalization Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `PackageConfiguration_ShouldHaveUniqueIndexOnScanIdNameAndVersion` | Verify normalization constraint | Unique index IX_Packages_ScanId_Name_Version exists on (ScanId, Name, Version) |
| `PackageConfiguration_ShouldPreventDuplicatePackagesWithinScan` | Verify duplicate prevention | Cannot insert two Package rows with same (ScanId, Name, Version) |
| `PackageConfiguration_ShouldAllowSamePackageInDifferentScans` | Verify cross-scan independence | Same (Name, Version) can exist in multiple ScanIds |

#### P1.5.2: Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `PackageConfiguration_ShouldCreateTableWithCorrectName` | Verify table name is "Packages" | Table exists with exact name "Packages" |
| `PackageConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | Name: nvarchar(500)<br>Version: nvarchar(100) |

#### P1.5.3: Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `PackageConfiguration_ShouldCascadeDeleteFromScan` | Verify cascade from Scan | Deleting Scan removes all associated Packages |

---

### P1.6: Assembly Configuration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Configurations/AssemblyConfigurationTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/Configurations/AssemblyConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/AssemblyConfiguration.cs)  
**Estimated Tests**: 10

#### P1.6.1: Schema Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `AssemblyConfiguration_ShouldCreateTableWithCorrectName` | Verify table name is "Assemblies" | Table exists with exact name "Assemblies" |
| `AssemblyConfiguration_ShouldSetCorrectColumnTypes` | Verify column data types | Name, Type, FilePath, Version have correct nvarchar types |
| `AssemblyConfiguration_ShouldHaveUniqueIndexOnScanIdAndFilePath` | Verify unique constraint | Unique index IX_Assemblies_ScanId_FilePath exists |

#### P1.6.2: Self-Referencing Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `AssemblyConfiguration_ShouldCreateAssemblyDependenciesJunctionTable` | Verify self-referencing junction | AssemblyDependencies table exists with ReferencingAssemblyId, ReferencedAssemblyId |
| `AssemblyConfiguration_ShouldUseNoActionOnReferencedAssemblyDelete` | Verify NO ACTION prevents circular cascade | FK AssemblyDependencies.ReferencedAssemblyId has NO ACTION on delete |
| `AssemblyConfiguration_ShouldUseCascadeOnReferencingAssemblyDelete` | Verify CASCADE on referencing side | FK AssemblyDependencies.ReferencingAssemblyId has CASCADE on delete |
| `AssemblyConfiguration_ShouldPreventCircularCascadeConflict` | Verify schema handles circular refs | Can create/delete assemblies with mutual dependencies without errors |

#### P1.6.3: Navigation Property Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `AssemblyConfiguration_ShouldConfigureDependenciesNavigationProperty` | Verify Dependencies navigation | Assembly.Dependencies navigation configured correctly |
| `AssemblyConfiguration_ShouldConfigureDependentAssembliesNavigationProperty` | Verify DependentAssemblies navigation | Assembly.DependentAssemblies navigation configured correctly |

#### P1.6.4: Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `AssemblyConfiguration_ShouldCascadeDeleteFromScan` | Verify cascade from Scan | Deleting Scan removes all Assemblies and AssemblyDependencies |

---

### P1.7: ApplicationDbContext Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/DbContext/ApplicationDbContextTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/ApplicationDbContext.cs](../../src/DAO.Manager.Data/Data/ApplicationDbContext.cs)  
**Estimated Tests**: 8

#### P1.7.1: DbSet Configuration Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ApplicationDbContext_ShouldHaveDbSetForScans` | Verify Scans DbSet exists | context.Scans is not null and queryable |
| `ApplicationDbContext_ShouldHaveDbSetForScanEvents` | Verify ScanEvents DbSet exists | context.ScanEvents is not null and queryable |
| `ApplicationDbContext_ShouldHaveDbSetForSolutions` | Verify Solutions DbSet exists | context.Solutions is not null and queryable |
| `ApplicationDbContext_ShouldHaveDbSetForProjects` | Verify Projects DbSet exists | context.Projects is not null and queryable |
| `ApplicationDbContext_ShouldHaveDbSetForPackages` | Verify Packages DbSet exists | context.Packages is not null and queryable |
| `ApplicationDbContext_ShouldHaveDbSetForAssemblies` | Verify Assemblies DbSet exists | context.Assemblies is not null and queryable |

#### P1.7.2: Model Configuration Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ApplicationDbContext_ShouldApplyAllConfigurationsFromAssembly` | Verify all IEntityTypeConfiguration applied | Model has 6 entity types registered (Scan, ScanEvent, Solution, Project, Package, Assembly) |

#### P1.7.3: Database Creation Test

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ApplicationDbContext_ShouldCreateDatabaseWithMigrations` | Verify database creation works | context.Database.Migrate() succeeds<br>All tables exist after migration |

---

### P1.8: ApplicationDbContextFactory Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/DbContext/ApplicationDbContextFactoryTests.cs`  
**Source**: [src/DAO.Manager.Data/Data/ApplicationDbContextFactory.cs](../../src/DAO.Manager.Data/Data/ApplicationDbContextFactory.cs)  
**Estimated Tests**: 3

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ApplicationDbContextFactory_ShouldImplementIDesignTimeDbContextFactory` | Verify interface implementation | Factory implements IDesignTimeDbContextFactory<ApplicationDbContext> |
| `ApplicationDbContextFactory_ShouldCreateDbContextWithConnectionString` | Verify CreateDbContext works | CreateDbContext(args) returns valid ApplicationDbContext |
| `ApplicationDbContextFactory_ShouldConfigureSqlServerProvider` | Verify provider configuration | DbContext uses SQL Server provider |

---

## Priority 2: Entity and Relationship Tests

### P2.1: Scan Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/ScanTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/Scan.cs](../../src/DAO.Manager.Data/Models/Entities/Scan.cs)  
**Estimated Tests**: 8

#### P2.1.1: Entity Creation Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Scan_ShouldInitializeWithDefaultValues` | Verify constructor initializes properly | New Scan has: ScanEvents = empty list, Solutions = empty list, Projects = empty list, Packages = empty list, Assemblies = empty list |
| `Scan_ShouldAcceptValidProperties` | Verify properties can be set | Can set: RepositoryPath, GitCommit, ScanDate, CreatedAt |
| `Scan_ShouldPersistToDatabase` | Verify entity can be saved | Scan inserted successfully, Id assigned by database |
| `Scan_ShouldRetrieveFromDatabase` | Verify entity can be queried | Scan retrieved with same property values as inserted |

#### P2.1.2: Navigation Property Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Scan_ShouldAllowAddingScanEvents` | Verify ScanEvents collection works | Can add ScanEvent objects to Scan.ScanEvents |
| `Scan_ShouldAllowAddingSolutions` | Verify Solutions collection works | Can add Solution objects to Scan.Solutions |
| `Scan_ShouldAllowAddingProjects` | Verify Projects collection works | Can add Project objects to Scan.Projects |
| `Scan_ShouldAllowAddingPackagesAndAssemblies` | Verify Packages/Assemblies collections work | Can add Package and Assembly objects |

---

### P2.2: ScanEvent Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/ScanEventTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/ScanEvent.cs](../../src/DAO.Manager.Data/Models/Entities/ScanEvent.cs)  
**Estimated Tests**: 6

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ScanEvent_ShouldAcceptValidProperties` | Verify properties can be set | Can set: ScanId, OccurredAt, Phase, Message |
| `ScanEvent_ShouldEnforceMaxLengthOnPhase` | Verify Phase max length (100) | Phase truncated/error at 101 characters |
| `ScanEvent_ShouldEnforceMaxLengthOnMessage` | Verify Message max length (2000) | Message truncated/error at 2001 characters |
| `ScanEvent_ShouldPersistWithScan` | Verify saves with parent Scan | ScanEvent saved when Scan is saved |
| `ScanEvent_ShouldHaveCorrectScanReference` | Verify navigation to Scan | ScanEvent.Scan navigation loads correct Scan entity |
| `ScanEvent_ShouldOrderByOccurredAt` | Verify temporal ordering | Multiple ScanEvents ordered by OccurredAt ASC |

---

### P2.3: Solution Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/SolutionTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/Solution.cs](../../src/DAO.Manager.Data/Models/Entities/Solution.cs)  
**Estimated Tests**: 7

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Solution_ShouldInitializeProjectsCollection` | Verify Projects collection initialized | New Solution has Projects = empty list |
| `Solution_ShouldAcceptValidProperties` | Verify properties can be set | Can set: ScanId, UniqueIdentifier, VisualStudioGuid, FilePath, Name |
| `Solution_ShouldPersistWithScan` | Verify saves with  parent Scan | Solution saved when Scan is saved |
| `Solution_ShouldPreventDuplicateUniqueIdentifierInScan` | Verify unique constraint enforced | Duplicate (ScanId, UniqueIdentifier) throws exception |
| `Solution_ShouldAllowSameUniqueIdentifierInDifferentScans` | Verify cross-scan independence | Same UniqueIdentifier allowed in different ScanIds |
| `Solution_ShouldLoadProjectsNavigation` | Verify many-to-many with Projects | Solution.Projects loads associated Project entities |
| `Solution_ShouldAddProjectsViaManyToMany` | Verify adding projects updates junction | Adding Project to Solution.Projects creates SolutionProjects row |

---

### P2.4: Project Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/ProjectTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/Project.cs](../../src/DAO.Manager.Data/Models/Entities/Project.cs)  
**Estimated Tests**: 12

#### P2.4.1: Basic Entity Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Project_ShouldInitializeAllCollections` | Verify all navigation collections initialized | New Project has: Solutions, ReferencingProjects, ReferencedProjects, Packages, ReferencingAssemblies, ReferencedAssemblies all = empty lists |
| `Project_ShouldAcceptValidProperties` | Verify properties can be set | Can set: ScanId, UniqueIdentifier, VisualStudioGuid, Name, FilePath, TargetFramework |
| `Project_ShouldPersistWithScan` | Verify saves with parent Scan | Project saved when Scan is saved |

#### P2.4.2: Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Project_ShouldHaveManyToManyWithSolutions` | Verify Solutions relationship | Project.Solutions loads associated Solution entities |
| `Project_ShouldHaveManyToManyWithPackages` | Verify Packages relationship | Project.Packages loads associated Package entities |
| `Project_ShouldHaveManyToManyWithAssemblies` | Verify Assemblies relationship | Project.ReferencedAssemblies loads associated Assembly entities |
| `Project_ShouldSupportSelfReferencingProjects` | Verify self-referencing relationship | Project.ReferencedProjects can contain other Project entities |
| `Project_ShouldSupportBidirectionalProjectReferences` | Verify bidirectional navigation | ProjectA.ReferencedProjects contains ProjectB<br>ProjectB.ReferencingProjects contains ProjectA |

#### P2.4.3: Complex Scenarios

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Project_ShouldHandleMultipleSolutions` | Verify project in multiple solutions | One Project can belong to multiple Solutions |
| `Project_ShouldHandleMultipleProjectReferences` | Verify multiple project dependencies | Project can reference multiple other Projects |
| `Project_ShouldHandleCircularProjectReferences` | Verify circular references allowed | ProjectA → ProjectB → ProjectA is valid (NO ACTION prevents cascade conflict) |
| `Project_ShouldManageAllRelationshipsSimultaneously` | Verify all 6 relationships work together | Project with Solutions, Projects, Packages, Assemblies all populated correctly |

---

### P2.5: Package Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/PackageTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/Package.cs](../../src/DAO.Manager.Data/Models/Entities/Package.cs)  
**Estimated Tests**: 7

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Package_ShouldInitializeProjectsCollection` | Verify Projects collection initialized | New Package has Projects = empty list |
| `Package_ShouldAcceptValidProperties` | Verify properties can be set | Can set: ScanId, Name, Version |
| `Package_ShouldPersistWithScan` | Verify saves with parent Scan | Package saved when Scan is saved |
| `Package_ShouldBeNormalizedWithinScan` | Verify uniqueness within scan | First insert succeeds, duplicate (ScanId, Name, Version) throws exception |
| `Package_ShouldAllowSamePackageInDifferentScans` | Verify cross-scan independence | Same (Name, Version) can exist in multiple ScanIds |
| `Package_ShouldBeSharedAcrossMultipleProjects` | Verify many-to-many sharing | One Package can be referenced by multiple Projects within same Scan |
| `Package_ShouldLoadProjectsNavigation` | Verify Projects navigation | Package.Projects loads all associated Project entities |

---

### P2.6: Assembly Entity Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Entities/AssemblyTests.cs`  
**Source**: [src/DAO.Manager.Data/Models/Entities/Assembly.cs](../../src/DAO.Manager.Data/Models/Entities/Assembly.cs)  
**Estimated Tests**: 10

#### P2.6.1: Basic Entity Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Assembly_ShouldInitializeAllCollections` | Verify all navigation collections initialized | New Assembly has: ReferencingProjects, Dependencies, DependentAssemblies all = empty lists |
| `Assembly_ShouldAcceptValidProperties` | Verify properties can be set | Can set: ScanId, Name, Type, FilePath, Version |
| `Assembly_ShouldPersistWithScan` | Verify saves with parent Scan | Assembly saved when Scan is saved |

#### P2.6.2: Uniqueness Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Assembly_ShouldBeUniqueByFilePathWithinScan` | Verify unique constraint | Duplicate (ScanId, FilePath) throws exception |
| `Assembly_ShouldAllowSameFilePathInDifferentScans` | Verify cross-scan independence | Same FilePath allowed in different ScanIds |

#### P2.6.3: Self-Referencing Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Assembly_ShouldSupportDependencyReferences` | Verify self-referencing relationship | Assembly.Dependencies can contain other Assembly entities |
| `Assembly_ShouldSupportBidirectionalDependencies` | Verify bidirectional navigation | AssemblyA.Dependencies contains AssemblyB<br>AssemblyB.DependentAssemblies contains AssemblyA |
| `Assembly_ShouldHandleMultipleDependencies` | Verify multiple dependencies | Assembly can depend on multiple other Assemblies |
| `Assembly_ShouldHandleCircularDependencies` | Verify circular dependencies allowed | AssemblyA → AssemblyB → AssemblyA is valid |

#### P2.6.4: Project Relationship Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Assembly_ShouldBeReferencedByMultipleProjects` | Verify many-to-many with Projects | One Assembly can be referenced by multiple Projects |

---

### P2.7: Cascade Delete Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Relationships/CascadeDeleteTests.cs`  
**Estimated Tests**: 10

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `CascadeDelete_ScanDeletion_ShouldDeleteAllScanEvents` | Verify Scan → ScanEvents cascade | Delete Scan → all ScanEvent rows deleted |
| `CascadeDelete_ScanDeletion_ShouldDeleteAllSolutions` | Verify Scan → Solutions cascade | Delete Scan → all Solution rows deleted |
| `CascadeDelete_ScanDeletion_ShouldDeleteAllProjects` | Verify Scan → Projects cascade | Delete Scan → all Project rows deleted |
| `CascadeDelete_ScanDeletion_ShouldDeleteAllPackages` | Verify Scan → Packages cascade | Delete Scan → all Package rows deleted |
| `CascadeDelete_ScanDeletion_ShouldDeleteAllAssemblies` | Verify Scan → Assemblies cascade | Delete Scan → all Assembly rows deleted |
| `CascadeDelete_ScanDeletion_ShouldDeleteAllJunctionRecords` | Verify cascade deletes junction rows | Delete Scan → all 5 junction tables cleaned (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences, AssemblyDependencies) via Project/Assembly cascade |
| `CascadeDelete_SolutionDeletion_ShouldRequireManualCleanup` | Verify NO ACTION prevents automatic cascade | Delete Solution with junction rows → blocked by FK constraint (NO ACTION on SolutionId); must delete junction rows manually first OR delete Projects (which cascade to junction) |
| `CascadeDelete_ProjectDeletion_ShouldCascadeToAllJunctions` | Verify Project CASCADE to all junctions | Delete Project → automatically cascades to SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences |
| `CascadeDelete_ProjectDeletion_ShouldNotDeleteReferencedProjects` | Verify NO ACTION prevents cascade | Delete ProjectA → ProjectReferences rows deleted, but referenced ProjectB remains (NO ACTION on ReferencedProjectId) |
| `CascadeDelete_AssemblyDeletion_ShouldNotDeleteReferencedAssemblies` | Verify NO ACTION prevents cascade | Delete AssemblyA → AssemblyDependencies rows deleted, but referenced AssemblyB remains |
| `CascadeDelete_CompleteScan_ShouldLeaveNoDatabaseOrphans` | Verify complete cleanup | Delete Scan with full object graph → no orphaned records in any table |

---

### P2.8: Many-to-Many Relationship Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Relationships/ManyToManyTests.cs`  
**Estimated Tests**: 10

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `ManyToMany_SolutionProjects_ShouldCreateJunctionRecords` | Verify SolutionProjects junction creation | Adding Project to Solution.Projects creates row in SolutionProjects |
| `ManyToMany_SolutionProjects_ShouldSupportBidirectionalNavigation` | Verify navigation both ways | Solution.Projects ↔ Project.Solutions both work |
| `ManyToMany_ProjectPackages_ShouldAllowPackageSharing` | Verify Package normalization | One Package shared by multiple Projects via ProjectPackageReferences |
| `ManyToMany_ProjectPackages_ShouldLoadCorrectly` | Verify lazy/eager loading | Include(p => p.Packages) loads all packages for project |
| `ManyToMany_ProjectAssemblies_ShouldLinkCorrectly` | Verify Project → Assembly link | ProjectAssemblyReferences junction populated correctly |
| `ManyToMany_ProjectReferences_ShouldSupportSelfReferencing` | Verify self-referencing many-to-many | ProjectReferences allows Project → Project relationships |
| `ManyToMany_ProjectReferences_ShouldDistinguishReferencingAndReferenced` | Verify direction matters | ProjectA.ReferencedProjects ≠ ProjectA.ReferencingProjects |
| `ManyToMany_AssemblyDependencies_ShouldSupportSelfReferencing` | Verify self-referencing many-to-many | AssemblyDependencies allows Assembly → Assembly relationships |
| `ManyToMany_AssemblyDependencies_ShouldDistinguishDependenciesAndDependents` | Verify direction matters | AssemblyA.Dependencies ≠ AssemblyA.DependentAssemblies |
| `ManyToMany_AllRelationships_ShouldWorkTogether` | Verify all 5 junction tables simultaneously | Complex object graph with all relationships works correctly |

---

### P2.9: Self-Referencing Relationship Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Relationships/SelfReferencingTests.cs`  
**Estimated Tests**: 8

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `SelfReferencing_ProjectReferences_ShouldAllowCyclicDependencies` | Verify cycles allowed | A → B → C → A is valid |
| `SelfReferencing_ProjectReferences_ShouldPreventCascadeConflict` | Verify NO ACTION works | Deleting ProjectA doesn't cascade to referenced ProjectB |
| `SelfReferencing_ProjectReferences_ShouldCleanupReferencingRecords` | Verify referencing side cascades | Deleting ProjectA removes rows where ReferencingProjectId = A |
| `SelfReferencing_ProjectReferences_ShouldLeaveReferencedRecords` | Verify referenced side preserved | Deleting ProjectA keeps rows where ReferencedProjectId = A (after removing FK) |
| `SelfReferencing_AssemblyDependencies_ShouldAllowCyclicDependencies` | Verify cycles allowed | A → B → C → A is valid |
| `SelfReferencing_AssemblyDependencies_ShouldPreventCascadeConflict` | Verify NO ACTION works | Deleting AssemblyA doesn't cascade to referenced AssemblyB |
| `SelfReferencing_AssemblyDependencies_ShouldCleanupDependencyRecords` | Verify referencing side cascades | Deleting AssemblyA removes rows where ReferencingAssemblyId = A |
| `SelfReferencing_AssemblyDependencies_ShouldLeaveReferencedRecords` | Verify referenced side preserved | Deleting AssemblyA keeps rows where ReferencedAssemblyId = A |

---

### P2.10: Navigation Property Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Relationships/NavigationPropertyTests.cs`  
**Estimated Tests**: 12

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Navigation_Scan_ShouldLoadAllChildCollections` | Verify eager loading all children | Include multiple levels loads Scans, ScanEvents, Solutions, Projects, Packages, Assemblies |
| `Navigation_Scan_ShouldSupportSelectiveInclude` | Verify selective eager loading | Include(s => s.Projects) loads only Projects, not other collections |
| `Navigation_Project_ShouldLoadSolutionsCollection` | Verify Project.Solutions loads | Include(p => p.Solutions) populates Solutions |
| `Navigation_Project_ShouldLoadPackagesCollection` | Verify Project.Packages loads | Include(p => p.Packages) populates Packages |
| `Navigation_Project_ShouldLoadReferencedProjects` | Verify Project.ReferencedProjects loads | Include(p => p.ReferencedProjects) populates ReferencedProjects |
| `Navigation_Project_ShouldLoadReferencingProjects` | Verify Project.ReferencingProjects loads | Include(p => p.ReferencingProjects) populates ReferencingProjects |
| `Navigation_Solution_ShouldLoadProjectsCollection` | Verify Solution.Projects loads | Include(s => s.Projects) populates Projects |
| `Navigation_Package_ShouldLoadProjectsCollection` | Verify Package.Projects loads | Include(pkg => pkg.Projects) populates Projects |
| `Navigation_Assembly_ShouldLoadDependencies` | Verify Assembly.Dependencies loads | Include(a => a.Dependencies) populates Dependencies |
| `Navigation_Assembly_ShouldLoadDependentAssemblies` | Verify Assembly.DependentAssemblies loads | Include(a => a.DependentAssemblies) populates DependentAssemblies |
| `Navigation_Complex_ShouldLoadMultipleLevels` | Verify nested includes | Include(s => s.Projects).ThenInclude(p => p.Packages) loads 3 levels |
| `Navigation_LazyLoading_ShouldBeDisabled` | Verify no lazy loading by default | Accessing navigation without Include returns empty/null (no lazy load) |

---

## Priority 3: CRUD Operation Tests

### P3.1: Insert Operation Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Operations/InsertTests.cs`  
**Estimated Tests**: 10

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Insert_SingleScan_ShouldAssignId` | Verify auto-increment ID | SaveChanges assigns Scan.Id > 0 |
| `Insert_ScanWithScanEvents_ShouldSaveGraph` | Verify related entities saved | Saving Scan with ScanEvents saves all entities |
| `Insert_ScanWithCompleteGraph_ShouldSaveAllEntities` | Verify deep object graph | Scan with Solutions, Projects, Packages, Assemblies all saved |
| `Insert_MultipleScans_ShouldAssignUniqueIds` | Verify multiple entities | Each Scan gets unique Id |
| `Insert_DuplicateUniqueIdentifier_ShouldThrowException` | Verify unique constraint enforced | Duplicate (ScanId, UniqueIdentifier) throws DbUpdateException |
| `Insert_DuplicatePackage_ShouldThrowException` | Verify package normalization | Duplicate (ScanId, Name, Version) throws DbUpdateException |
| `Insert_DuplicateAssembly_ShouldThrowException` | Verify assembly uniqueness | Duplicate (ScanId, FilePath) throws DbUpdateException |
| `Insert_ProjectWithMultipleRelationships_ShouldPopulateJunctions` | Verify junction tables | Project with Solutions, Packages, Assemblies creates all junction rows |
| `Insert_BulkScans_ShouldHandleLargeVolumes` | Verify bulk insert performance | Can insert 100 Scans with 1000 ScanEvents performantly |
| `Insert_WithTransaction_ShouldSupportRollback` | Verify transaction support | Rollback after error leaves database unchanged |

---

### P3.2: Query Operation Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Operations/QueryTests.cs`  
**Estimated Tests**: 15

#### P3.2.1: Basic Query Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Query_ScanById_ShouldReturnCorrectEntity` | Verify FindAsync | FindAsync(id) returns correct Scan |
| `Query_ScansByDate_ShouldFilterCorrectly` | Verify filtering | Where(s => s.ScanDate >= date) returns correct subset |
| `Query_ScansByGitCommit_ShouldFindMatch` | Verify indexed search | FirstOrDefault(s => s.GitCommit == hash) uses index, returns result |
| `Query_ScanEventsForScan_ShouldReturnOrderedEvents` | Verify ordering | ScanEvents ordered by OccurredAt ASC |

#### P3.2.2: Eager Loading Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Query_ScanWithInclude_ShouldEagerLoadScanEvents` | Verify Include | Include(s => s.ScanEvents) loads events in single query |
| `Query_ScanWithMultipleIncludes_ShouldLoadAllRelations` | Verify multiple includes | Include(s => s.Projects).Include(s => s.Solutions) loads both |
| `Query_ProjectWithThenInclude_ShouldLoadNestedRelations` | Verify ThenInclude | Include(p => p.Packages).ThenInclude(...) loads nested data |

#### P3.2.3: Projection Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Query_ScanWithProjection_ShouldReturnAnonymousType` | Verify Select | Select(s => new { s.Id, s.GitCommit }) returns projected data |
| `Query_CountScans_ShouldReturnCorrectCount` | Verify aggregation | Count() returns correct number |
| `Query_GroupBy_ShouldAggregateCorrectly` | Verify grouping | GroupBy(s => s.RepositoryPath) aggregates correctly |

#### P3.2.4: Complex Query Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Query_ProjectsUsingPackage_ShouldJoinCorrectly` | Verify many-to-many query | Find all Projects using specific Package via junction table |
| `Query_SolutionsContainingProject_ShouldJoinCorrectly` | Verify reverse query | Find all Solutions containing specific Project |
| `Query_AssemblyDependencyChain_ShouldTraverseGraph` | Verify recursive query | Find transitive dependencies (A → B → C) |

#### P3.2.5: Performance Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Query_WithoutInclude_ShouldNotCauseNPlusOne` | Verify N+1 detection | Accessing navigation without Include doesn't trigger lazy load |
| `Query_LargeResultSet_ShouldPageEfficiently` | Verify pagination | Skip/Take with OrderBy performs efficiently |

---

### P3.3: Update Operation Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Operations/UpdateTests.cs`  
**Estimated Tests**: 10

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Update_ScanProperties_ShouldPersistChanges` | Verify property update | Updating Scan.RepositoryPath saves to database |
| `Update_AddScanEvent_ShouldInsertNewEvent` | Verify adding to collection | Adding ScanEvent to Scan.ScanEvents inserts new row |
| `Update_RemoveScanEvent_ShouldDeleteEvent` | Verify removing from collection | Removing ScanEvent from Scan.ScanEvents deletes row |
| `Update_AddProjectToSolution_ShouldUpdateJunction` | Verify junction update | Adding Project to Solution.Projects creates SolutionProjects row |
| `Update_RemoveProjectFromSolution_ShouldDeleteJunction` | Verify junction deletion | Removing Project from Solution.Projects deletes SolutionProjects row |
| `Update_ReplacePackageInProject_ShouldUpdateJunction` | Verify replacing collection items | Clearing and adding Packages updates ProjectPackageReferences |
| `Update_AddProjectReference_ShouldCreateJunctionRecord` | Verify self-referencing update | Adding to ReferencedProjects creates ProjectReferences row |
| `Update_RemoveProjectReference_ShouldDeleteJunctionRecord` | Verify self-referencing delete | Removing from ReferencedProjects deletes ProjectReferences row |
| `Update_ConcurrentUpdate_ShouldHandleConflict` | Verify concurrency handling | Concurrent updates handled appropriately (optimistic concurrency if configured) |
| `Update_DetachAndAttach_ShouldUpdateCorrectly` | Verify detached entity update | Detaching, modifying, and reattaching entity updates correctly |

---

### P3.4: Delete Operation Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Operations/DeleteTests.cs`  
**Estimated Tests**: 8

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Delete_Scan_ShouldRemoveFromDatabase` | Verify entity deletion | Remove(scan) + SaveChanges deletes row |
| `Delete_Scan_ShouldCascadeDeleteAllChildren` | Verify cascade delete | Deleting Scan removes all related entities and junction rows (via Project/Assembly CASCADE) |
| `Delete_ScanEvent_ShouldNotAffectScan` | Verify child deletion | Deleting ScanEvent doesn't delete Scan |
| `Delete_Solution_ShouldRequireJunctionCleanup` | Verify NO ACTION on junction | Deleting Solution blocked if SolutionProjects rows exist (NO ACTION); must manually remove junction rows first |
| `Delete_Project_ShouldCascadeToJunctions` | Verify CASCADE from Project | Deleting Project cascades to all junction tables (SolutionProjects, ProjectReferences, ProjectPackageReferences, ProjectAssemblyReferences) |
| `Delete_Project_ShouldNotDeleteReferencedProjects` | Verify NO ACTION respected | Deleting Project doesn't delete referenced Projects (only junction rows deleted via CASCADE) |
| `Delete_Assembly_ShouldNotDeleteReferencedAssemblies` | Verify NO ACTION respected | Deleting Assembly doesn't delete referenced Assemblies |
| `Delete_MultipleEntities_ShouldDeleteAll` | Verify bulk delete | RemoveRange(entities) deletes all |
| `Delete_WithTransaction_ShouldSupportRollback` | Verify transaction rollback | Rollback after delete restores entity |

---

## Priority 4: Integration Tests

### P4.1: Complete Scan Lifecycle Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Integration/CompleteScanLifecycleTests.cs`  
**Estimated Tests**: 5

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Lifecycle_CreateScanWithFullGraph_ShouldPersistEverything` | Verify complete creation workflow | Create Scan with Solutions, Projects (with refs), Packages, Assemblies (with deps) → all saved correctly |
| `Lifecycle_QueryScanWithAllRelationships_ShouldLoadCorrectly` | Verify complete query workflow | Retrieve Scan with all Includes → all relationships loaded |
| `Lifecycle_UpdateRelationships_ShouldModifyJunctions` | Verify relationship update workflow | Add/remove Projects from Solutions → junction updated |
| `Lifecycle_DeleteScan_ShouldCleanupCompletely` | Verify complete cleanup workflow | Delete Scan → verify 0 rows in all 11 tables for that ScanId |
| `Lifecycle_MultipleScansWithSharedInfo_ShouldIsolateCorrectly` | Verify scan isolation | Two scans can have same package names, projects isolated by ScanId |

---

### P4.2: Cross-Entity Query Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Integration/CrossEntityQueryTests.cs`  
**Estimated Tests**: 10

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `CrossEntity_FindAllProjectsUsingPackage_ShouldReturnCorrectSet` | Verify package usage query | Find all Projects referencing "Newtonsoft.Json" v13.0.0 within scan |
| `CrossEntity_FindAllPackagesInSolution_ShouldAggregateFromProjects` | Verify solution package aggregation | Distinct packages across all projects in solution |
| `CrossEntity_FindProjectDependencyChain_ShouldTraverseReferences` | Verify transitive project deps | Starting from ProjectA, find all transitive ProjectReferences |
| `CrossEntity_FindAssemblyDependencyChain_ShouldTraverseReferences` | Verify transitive assembly deps | Starting from AssemblyA, find all transitive Dependencies |
| `CrossEntity_FindOrphanedProjects_ShouldIdentifyProjectsWithoutSolutions` | Verify orphan detection | Find Projects not in any Solution.Projects |
| `CrossEntity_FindMostReferencedPackage_ShouldRankByUsage` | Verify ranking query | GroupBy Package, OrderByDescending Count of Projects |
| `CrossEntity_FindCircularProjectDependencies_ShouldDetectCycles` | Verify cycle detection | Detect A → B → C → A circular references |
| `CrossEntity_FindProjectsByTargetFramework_ShouldFilterCorrectly` | Verify framework filtering | Where(p => p.TargetFramework == "net8.0") |
| `CrossEntity_FindSolutionsWithMostProjects_ShouldRankCorrectly` | Verify solution ranking | GroupBy Solution, OrderByDescending Count of Projects |
| `CrossEntity_FindAssembliesByType_ShouldCategorize` | Verify assembly categorization | GroupBy Assembly.Type (DLL vs EXE) |

---

### P4.3: Temporal Analysis Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Integration/TemporalAnalysisTests.cs`  
**Estimated Tests**: 8

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Temporal_ComparePackageVersionsAcrossScans_ShouldDetectUpgrades` | Verify version drift detection | Compare Package versions between Scan1 and Scan2 |
| `Temporal_IdentifyAddedProjects_ShouldFindNewProjects` | Verify new project detection | Projects in Scan2 not in Scan1 (by UniqueIdentifier) |
| `Temporal_IdentifyRemovedProjects_ShouldFindDeletedProjects` | Verify deleted project detection | Projects in Scan1 not in Scan2 |
| `Temporal_TrackProjectReferencesOverTime_ShouldDetectChanges` | Verify dependency changes | Compare ProjectReferences between scans |
| `Temporal_DetectNewPackageIntroductions_ShouldFindAdditions` | Verify new package detection | Packages in Scan2 not in any previous scan |
| `Temporal_AnalyzeScanEventTiming_ShouldCalculateDurations` | Verify scan performance analysis | Calculate phase durations from ScanEvents |
| `Temporal_CompareRepositoryAtCommits_ShouldShowEvolution` | Verify commit comparison | Compare structure at CommitA vs CommitB |
| `Temporal_DetectPackageVulnerabilities_ShouldFlagOldVersions` | Verify version auditing | Find Packages older than specific version |

---

### P4.4: Bulk Operations Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Integration/BulkOperationsTests.cs`  
**Estimated Tests**: 5

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Bulk_InsertLargeScan_ShouldHandleThousandsOfEntities` | Verify large insert performance | Insert Scan with 1000 Projects, 5000 Packages → completes in reasonable time |
| `Bulk_QueryLargeDataset_ShouldPageEfficiently` | Verify large query performance | Query scans with pagination (Skip/Take) performs efficiently |
| `Bulk_UpdateMultipleScans_ShouldBatchCorrectly` | Verify bulk update | Update properties on 100 Scans → batched efficiently |
| `Bulk_DeleteOldScans_ShouldCleanupEfficiently` | Verify bulk delete performance | Delete 50 scans → cascade deletes complete in reasonable time |
| `Bulk_MergeData_ShouldHandleUpserts` | Verify upsert pattern | Insert new + update existing entities in single transaction |

---

## Priority 5: Migration and Edge Case Tests

### P5.1: Migration Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/Migrations/InitialNormalizedSchemaTests.cs`  
**Source**: [src/DAO.Manager.Data/Migrations/20260209023651_InitialNormalizedSchema.cs](../../src/DAO.Manager.Data/Migrations/20260209023651_InitialNormalizedSchema.cs)  
**Estimated Tests**: 8

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `Migration_InitialNormalized_ShouldApplySuccessfully` | Verify migration applies | Database.Migrate() succeeds on empty database |
| `Migration_InitialNormalized_ShouldCreateAllTables` | Verify all tables created | 6 main tables + 5 junction tables exist |
| `Migration_InitialNormalized_ShouldMatchSpecification` | Verify schema matches spec | Schema matches [entity-model-specification.md](../../src/DAO.Manager.Data/docs/entity-model-specification.md) exactly |
| `Migration_InitialNormalized_ShouldCreateAllIndexes` | Verify all indexes created | All specified indexes exist (PKs, FKs, unique, performance) |
| `Migration_InitialNormalized_ShouldSetCorrectCascadeRules` | Verify cascade delete configured | FK cascade rules match specification |
| `Migration_InitialNormalized_ShouldBeIdempotent` | Verify idempotency | Running migration twice doesn't error or change schema |
| `Migration_Down_ShouldRevertSchema` | Verify down migration | Database.Migrate("0") removes all tables |
| `Migration_Snapshot_ShouldMatchCurrentModel` | Verify snapshot accuracy | ModelSnapshot matches current ApplicationDbContext model |

---

### P5.2: Edge Case Tests

**Test Class**: `tests/DAO.Manager.Data.Tests/EdgeCases/EdgeCaseTests.cs`  
**Estimated Tests**: 12

#### P5.2.1: Empty/Null Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `EdgeCase_ScanWithNoChildren_ShouldSaveSuccessfully` | Verify empty scan | Scan with empty collections saves and retrieves correctly |
| `EdgeCase_EmptyStringProperties_ShouldValidateOrError` | Verify empty string validation | Empty strings in required fields throw validation error |
| `EdgeCase_NullNavigationProperties_ShouldHandleGracefully` | Verify null navigation handling | Accessing null navigation doesn't throw (returns empty or null) |

#### P5.2.2: Boundary Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `EdgeCase_MaxLengthRepositoryPath_ShouldAccept2000Chars` | Verify max length boundary | 2000-char RepositoryPath accepted, 2001 chars rejected |
| `EdgeCase_MaxLengthGitCommit_ShouldAccept100Chars` | Verify max length boundary | 100-char GitCommit accepted, 101 chars rejected |
| `EdgeCase_MaxLengthScanEventMessage_ShouldAccept2000Chars` | Verify max length boundary | 2000-char Message accepted, 2001 chars rejected |

#### P5.2.3: Constraint Violation Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `EdgeCase_DuplicatePrimaryKey_ShouldThrowException` | Verify PK uniqueness | Manually setting duplicate Id throws exception |
| `EdgeCase_InvalidForeignKey_ShouldThrowException` | Verify FK constraint | Inserting ScanEvent with non-existent ScanId throws exception |
| `EdgeCase_ViolateUniqueConstraint_ShouldThrowDbUpdateException` | Verify unique constraint | Violating unique index throws DbUpdateException |

#### P5.2.4: Special Character Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `EdgeCase_SpecialCharactersInStrings_ShouldHandleCorrectly` | Verify special char handling | Unicode, quotes, special chars in strings save/retrieve correctly |
| `EdgeCase_PathsWithSpecialCharacters_ShouldHandleCorrectly` | Verify file path handling | Paths with spaces, Unicode, backslashes handled correctly |

#### P5.2.5: Transaction Tests

| Test Name | Objective | Assertions |
|-----------|-----------|------------|
| `EdgeCase_ExceptionDuringSave_ShouldRollbackTransaction` | Verify transaction rollback | Exception during SaveChanges leaves database unchanged |

---

## Test Data Patterns

### Entity Builder Examples

#### Scan Builder

```csharp
public class ScanBuilder
{
    private string _repositoryPath = "/default/repo/path";
    private string _gitCommit = "abc123def456789012345678901234567890abcd";
    private DateTime _scanDate = DateTime.UtcNow;
    private DateTime _createdAt = DateTime.UtcNow;
    
    public ScanBuilder WithRepositoryPath(string path)
    {
        _repositoryPath = path;
        return this;
    }
    
    public ScanBuilder WithGitCommit(string commit)
    {
        _gitCommit = commit;
        return this;
    }
    
    public ScanBuilder WithScanDate(DateTime date)
    {
        _scanDate = date;
        return this;
    }
    
    public Scan Build()
    {
        return new Scan
        {
            RepositoryPath = _repositoryPath,
            GitCommit = _gitCommit,
            ScanDate = _scanDate,
            CreatedAt = _createdAt
        };
    }
    
    public Scan BuildWithEvents(int eventCount)
    {
        var scan = Build();
        for (int i = 0; i < eventCount; i++)
        {
            scan.ScanEvents.Add(new ScanEventBuilder()
                .WithPhase($"Phase{i}")
                .WithMessage($"Event message {i}")
                .Build());
        }
        return scan;
    }
}
```

#### Project Builder

```csharp
public class ProjectBuilder
{
    private Scan? _scan;
    private int? _scanId;
    private string _uniqueIdentifier = Guid.NewGuid().ToString();
    private string _name = "DefaultProject";
    private string _filePath = "/src/DefaultProject/DefaultProject.csproj";
    private string _targetFramework = "net8.0";
    private List<Package> _packages = new();
    
    public ProjectBuilder WithScan(Scan scan)
    {
        _scan = scan;
        _scanId = scan.Id;
        return this;
    }
    
    public ProjectBuilder WithScanId(int scanId)
    {
        _scanId = scanId;
        return this;
    }
    
    public ProjectBuilder WithName(string name)
    {
        _name = name;
        return this;
    }
    
    public ProjectBuilder WithPackages(params PackageBuilder[] packages)
    {
        _packages.AddRange(packages.Select(pb => pb.Build()));
        return this;
    }
    
    public Project Build()
    {
        var project = new Project
        {
            ScanId = _scanId ?? throw new InvalidOperationException("ScanId required"),
            UniqueIdentifier = _uniqueIdentifier,
            Name = _name,
            FilePath = _filePath,
            TargetFramework = _targetFramework
        };
        
        foreach (var package in _packages)
        {
            project.Packages.Add(package);
        }
        
        return project;
    }
}
```

### Sample Test Data

```csharp
public static class TestData
{
    public static Scan CreateSimpleScan()
    {
        return new ScanBuilder()
            .WithRepositoryPath("/test/repo")
            .WithGitCommit("abc123")
            .Build();
    }
    
    public static Scan CreateScanWithCompleteGraph()
    {
        var scan = new ScanBuilder()
            .WithRepositoryPath("/complex/repo")
            .Build();
        
        // Add solutions
        var solution1 = new SolutionBuilder()
            .WithName("MySolution.sln")
            .Build();
        scan.Solutions.Add(solution1);
        
        // Add projects
        var project1 = new ProjectBuilder()
            .WithName("WebProject")
            .WithPackages(
                new PackageBuilder().WithName("Newtonsoft.Json").WithVersion("13.0.1"),
                new PackageBuilder().WithName("Serilog").WithVersion("3.1.0")
            )
            .Build();
        scan.Projects.Add(project1);
        
        // Link solution and project
        solution1.Projects.Add(project1);
        
        return scan;
    }
    
    public static Scan CreateScanWithCircularProjectReferences()
    {
        var scan = new ScanBuilder().Build();
        
        var projectA = new ProjectBuilder().WithName("ProjectA").WithScan(scan).Build();
        var projectB = new ProjectBuilder().WithName("ProjectB").WithScan(scan).Build();
        var projectC = new ProjectBuilder().WithName("ProjectC").WithScan(scan).Build();
        
        // Create circular reference: A → B → C → A
        projectA.ReferencedProjects.Add(projectB);
        projectB.ReferencedProjects.Add(projectC);
        projectC.ReferencedProjects.Add(projectA);
        
        scan.Projects.Add(projectA);
        scan.Projects.Add(projectB);
        scan.Projects.Add(projectC);
        
        return scan;
    }
}
```

---

## Appendix: Coverage Mapping

| Source File | Primary Test Class | Secondary Test Classes | Coverage Target |
|-------------|-------------------|------------------------|-----------------|
| [Scan.cs](../../src/DAO.Manager.Data/Models/Entities/Scan.cs) | ScanTests.cs | CascadeDeleteTests.cs, CompleteScanLifecycleTests.cs | 75% |
| [ScanEvent.cs](../../src/DAO.Manager.Data/Models/Entities/ScanEvent.cs) | ScanEventTests.cs | - | 70% |
| [Solution.cs](../../src/DAO.Manager.Data/Models/Entities/Solution.cs) | SolutionTests.cs | ManyToManyTests.cs | 75% |
| [Project.cs](../../src/DAO.Manager.Data/Models/Entities/Project.cs) | ProjectTests.cs | ManyToManyTests.cs, SelfReferencingTests.cs | 80% |
| [Package.cs](../../src/DAO.Manager.Data/Models/Entities/Package.cs) | PackageTests.cs | CrossEntityQueryTests.cs | 70% |
| [Assembly.cs](../../src/DAO.Manager.Data/Models/Entities/Assembly.cs) | AssemblyTests.cs | SelfReferencingTests.cs | 75% |
| [ScanConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ScanConfiguration.cs) | ScanConfigurationTests.cs | - | 100% |
| [ScanEventConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ScanEventConfiguration.cs) | ScanEventConfigurationTests.cs | - | 100% |
| [SolutionConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/SolutionConfiguration.cs) | SolutionConfigurationTests.cs | - | 100% |
| [ProjectConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/ProjectConfiguration.cs) | ProjectConfigurationTests.cs | - | 100% |
| [PackageConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/PackageConfiguration.cs) | PackageConfigurationTests.cs | - | 100% |
| [AssemblyConfiguration.cs](../../src/DAO.Manager.Data/Data/Configurations/AssemblyConfiguration.cs) | AssemblyConfigurationTests.cs | - | 100% |
| [ApplicationDbContext.cs](../../src/DAO.Manager.Data/Data/ApplicationDbContext.cs) | ApplicationDbContextTests.cs | All test classes | 95% |
| [ApplicationDbContextFactory.cs](../../src/DAO.Manager.Data/Data/ApplicationDbContextFactory.cs) | ApplicationDbContextFactoryTests.cs | - | 85% |
| [InitialNormalizedSchema.cs](../../src/DAO.Manager.Data/Migrations/20260209023651_InitialNormalizedSchema.cs) | InitialNormalizedSchemaTests.cs | - | 65% |

---

## Summary

**Total Estimated Tests**: ~180 tests across all priority levels

**Test Distribution**:
- **P1 (Configuration)**: 75 tests
- **P2 (Entity & Relationships)**: 50 tests
- **P3 (CRUD Operations)**: 43 tests
- **P4 (Integration)**: 28 tests
- **P5 (Migration & Edge Cases)**: 20 tests

**Estimated Implementation Time**: 4-5 weeks (1 developer)

**Expected Final Coverage**: 82-85% overall, 95%+ on configuration classes

---

**Document Status**: ✅ Ready for Implementation  
**Parent Document**: [dao-manager-data-test-plan.md](dao-manager-data-test-plan.md)  
**Next Steps**: Review test cases → Set up test infrastructure → Begin Phase 1 (P1 tests)
