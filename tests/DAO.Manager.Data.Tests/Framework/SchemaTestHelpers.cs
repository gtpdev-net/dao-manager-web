using FluentAssertions;
using Xunit;

namespace DAO.Manager.Data.Tests.Framework;

/// <summary>
/// Static helper methods and common test patterns for schema validation.
/// Provides high-level test patterns that combine SchemaInspector and assertion extensions.
/// </summary>
/// <remarks>
/// <para><strong>Usage:</strong></para>
/// <code>
/// // Validate entire table schema in one call
/// await SchemaTestHelpers.AssertTableSchemaAsync(
///     inspector,
///     "Scans",
///     new Dictionary&lt;string, ColumnExpectation&gt;
///     {
///         ["Id"] = new("int", IsRequired: true, IsIdentity: true),
///         ["RepositoryPath"] = new("nvarchar", 2000, IsRequired: true),
///         ["ScanDate"] = new("datetime2", IsRequired: true)
///     });
///
/// // Validate standard relationship to Scans table
/// await SchemaTestHelpers.AssertStandardScanRelationshipAsync(
///     inspector,
///     "ScanEvents",
///     "ScanId");
/// </code>
/// </remarks>
public static class SchemaTestHelpers
{
    #region Theory Data Providers

    /// <summary>
    /// Gets theory data for all entity table names in the application.
    /// Useful for parameterized tests that validate patterns across all entities.
    /// </summary>
    /// <returns>Theory data with entity table names.</returns>
    public static TheoryData<string> GetAllEntityTableNames()
    {
        return new TheoryData<string>
        {
            "Scans",
            "ScanEvents",
            "Solutions",
            "Projects",
            "Packages",
            "Assemblies"
        };
    }

    /// <summary>
    /// Gets theory data for all tables that should have a foreign key to Scans.
    /// </summary>
    /// <returns>Theory data with child table name and foreign key column name.</returns>
    public static TheoryData<string, string> GetScanChildTables()
    {
        return new TheoryData<string, string>
        {
            { "ScanEvents", "ScanId" },
            { "Solutions", "ScanId" },
            { "Projects", "ScanId" },
            { "Packages", "ScanId" },
            { "Assemblies", "ScanId" }
        };
    }

    /// <summary>
    /// Gets theory data for all junction tables in the schema.
    /// </summary>
    /// <returns>Theory data with junction table name, first FK table, second FK table.</returns>
    public static TheoryData<string, string, string> GetJunctionTables()
    {
        return new TheoryData<string, string, string>
        {
            { "SolutionProjects", "Solutions", "Projects" },
            { "ProjectReferences", "Projects", "Projects" },
            { "ProjectPackageReferences", "Projects", "Packages" },
            { "ProjectAssemblyReferences", "Projects", "Assemblies" },
            { "AssemblyDependencies", "Assemblies", "Assemblies" }
        };
    }

    #endregion

    #region Common Test Patterns

    /// <summary>
    /// Validates an entire table schema including columns, data types, nullability, and identity columns.
    /// This high-level method combines multiple assertions into a single, readable test.
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    /// <param name="tableName">The table to validate.</param>
    /// <param name="expectedColumns">Dictionary of column name to expected column configuration.</param>
    public static async Task AssertTableSchemaAsync(
        SchemaInspector inspector,
        string tableName,
        Dictionary<string, ColumnExpectation> expectedColumns)
    {
        // Get table info
        var tableInfo = await inspector.GetTableInfoAsync(tableName);
        tableInfo.Should().NotBeNull($"table {tableName} should exist");

        // Validate all expected columns exist
        tableInfo!.Columns.Should().HaveCount(expectedColumns.Count,
            $"table {tableName} should have exactly {expectedColumns.Count} columns");

        // Validate each column
        foreach (var (columnName, expectation) in expectedColumns)
        {
            var column = tableInfo.Columns.FirstOrDefault(c => c.Name == columnName);
            column.Should().NotBeNull($"column {columnName} should exist in table {tableName}");

            // Validate data type
            column!.DataType.Should().Be(expectation.DataType,
                $"column {columnName} should have data type {expectation.DataType}");

            // Validate max length (if specified)
            if (expectation.MaxLength.HasValue)
            {
                column.MaxLength.Should().Be(expectation.MaxLength.Value,
                    $"column {columnName} should have max length {expectation.MaxLength.Value}");
            }

            // Validate nullability
            if (expectation.IsRequired)
            {
                column.IsNullable.Should().Be("NO",
                    $"column {columnName} should be required (NOT NULL)");
            }
            else
            {
                column.IsNullable.Should().Be("YES",
                    $"column {columnName} should be nullable");
            }

            // Validate identity
            if (expectation.IsIdentity)
            {
                column.IsIdentity.Should().Be("YES",
                    $"column {columnName} should be an identity column");
            }
            else
            {
                column.IsIdentity.Should().Be("NO",
                    $"column {columnName} should not be an identity column");
            }
        }
    }

    /// <summary>
    /// Validates a standard foreign key relationship to the Scans table with CASCADE delete behavior.
    /// Most child entities of Scan follow this pattern.
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    /// <param name="childTableName">The child table name.</param>
    /// <param name="foreignKeyColumn">The foreign key column name (typically "ScanId").</param>
    public static async Task AssertStandardScanRelationshipAsync(
        SchemaInspector inspector,
        string childTableName,
        string foreignKeyColumn)
    {
        var foreignKeys = await inspector.GetForeignKeysAsync(childTableName);

        foreignKeys.Should().NotBeEmpty($"table {childTableName} should have foreign keys");

        var scanFk = foreignKeys.FirstOrDefault(fk => fk.ColumnName == foreignKeyColumn);
        scanFk.Should().NotBeNull(
            $"table {childTableName} should have a foreign key on {foreignKeyColumn}");

        scanFk!.Should().Reference("Scans", "Id")
            .And.HaveDeleteBehavior("CASCADE");
    }

    /// <summary>
    /// Validates that a table has a primary key on the Id column with identity.
    /// This is the standard pattern for all main entity tables.
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    /// <param name="tableName">The table name.</param>
    public static async Task AssertStandardPrimaryKeyAsync(
        SchemaInspector inspector,
        string tableName)
    {
        // Check primary key
        var pk = await inspector.GetPrimaryKeyAsync(tableName);
        pk.Should().NotBeNull($"table {tableName} should have a primary key");
        pk!.Should().BeOnColumn("Id");

        // Check that Id column is identity
        var tableInfo = await inspector.GetTableInfoAsync(tableName);
        var idColumn = tableInfo!.Columns.First(c => c.Name == "Id");
        idColumn.Should().BeIdentity();
    }

    /// <summary>
    /// Validates a many-to-many junction table structure.
    /// Checks for composite primary key and two foreign keys with specified delete behaviors.
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    /// <param name="junctionTableName">The junction table name.</param>
    /// <param name="firstTable">The first referenced table.</param>
    /// <param name="firstColumn">The first foreign key column.</param>
    /// <param name="firstDeleteBehavior">Expected delete behavior for first FK.</param>
    /// <param name="secondTable">The second referenced table.</param>
    /// <param name="secondColumn">The second foreign key column.</param>
    /// <param name="secondDeleteBehavior">Expected delete behavior for second FK.</param>
    public static async Task AssertJunctionTableAsync(
        SchemaInspector inspector,
        string junctionTableName,
        string firstTable,
        string firstColumn,
        string firstDeleteBehavior,
        string secondTable,
        string secondColumn,
        string secondDeleteBehavior)
    {
        var junction = await inspector.GetJunctionTableInfoAsync(junctionTableName);
        junction.Should().NotBeNull($"junction table {junctionTableName} should exist");

        // Validate composite primary key
        junction!.Should().HaveCompositePrimaryKey(firstColumn, secondColumn);

        // Validate foreign keys
        var firstFk = junction!.ForeignKeys.FirstOrDefault(fk => fk.ColumnName == firstColumn);
        firstFk.Should().NotBeNull($"junction table should have FK on {firstColumn}");
        if (firstFk is not null)
        {
            firstFk.Should().Reference(firstTable, "Id")
                .And.HaveDeleteBehavior(firstDeleteBehavior);
        }

        var secondFk = junction!.ForeignKeys.FirstOrDefault(fk => fk.ColumnName == secondColumn);
        secondFk.Should().NotBeNull($"junction table should have FK on {secondColumn}");
        if (secondFk is not null)
        {
            secondFk.Should().Reference(secondTable, "Id")
                .And.HaveDeleteBehavior(secondDeleteBehavior);
        }
    }

    /// <summary>
    /// Validates that all Scan child tables have correct indexes on ScanId.
    /// Each child table should have both a regular index on ScanId and a composite index.
    /// Some tables require unique composite indexes to enforce uniqueness within a scan.
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    public static async Task AssertAllScanChildTableIndexesAsync(SchemaInspector inspector)
    {
        // Tables with unique composite indexes (enforce uniqueness within scan)
        var uniqueCompositeIndexes = new Dictionary<string, string[]>
        {
            { "Solutions", new[] { "ScanId", "UniqueIdentifier" } },
            { "Projects", new[] { "ScanId", "UniqueIdentifier" } },
            { "Packages", new[] { "ScanId", "Name", "Version" } },
            { "Assemblies", new[] { "ScanId", "FilePath" } }
        };

        // Tables with regular composite indexes (for query performance, not uniqueness)
        var regularCompositeIndexes = new Dictionary<string, string[]>
        {
            { "ScanEvents", new[] { "ScanId", "OccurredAt" } }
        };

        // Validate tables with unique composite indexes
        foreach (var (tableName, uniqueColumns) in uniqueCompositeIndexes)
        {
            var indexes = await inspector.GetIndexesAsync(tableName);

            // Assert regular index on ScanId exists
            var scanIdIndex = indexes.FirstOrDefault(i =>
                i.Columns.Count == 1 && i.Columns[0] == "ScanId");

            scanIdIndex.Should().NotBeNull(
                $"table {tableName} should have a regular index on ScanId for efficient lookups");

            // Assert unique composite index exists
            var expectedIndexName = $"IX_{tableName}_{string.Join("_", uniqueColumns)}";
            indexes.Should().ContainIndex(expectedIndexName)
                .Which.Should().BeIndexOn(uniqueColumns)
                .And.BeUnique(
                    because: $"{tableName} should enforce uniqueness within a scan using {string.Join(", ", uniqueColumns)}");
        }

        // Validate tables with regular composite indexes
        foreach (var (tableName, indexColumns) in regularCompositeIndexes)
        {
            var indexes = await inspector.GetIndexesAsync(tableName);

            // Assert regular index on ScanId exists
            var scanIdIndex = indexes.FirstOrDefault(i =>
                i.Columns.Count == 1 && i.Columns[0] == "ScanId");

            scanIdIndex.Should().NotBeNull(
                $"table {tableName} should have a regular index on ScanId for efficient lookups");

            // Assert composite index exists (not unique)
            var expectedIndexName = $"IX_{tableName}_{string.Join("_", indexColumns)}";
            indexes.Should().ContainIndex(expectedIndexName)
                .Which.Should().BeIndexOn(indexColumns)
                .And.NotBeUnique(
                    because: $"{tableName} uses a composite index on {string.Join(", ", indexColumns)} for query performance");
        }
    }

    /// <summary>
    /// Validates all junction tables that are indirectly cascade-deleted through Scans.
    /// Junction tables should have proper composite PKs and correct delete behaviors.
    /// NOTE: Due to SQL Server circular cascade path limitations, several junction tables
    /// use NO ACTION on one FK instead of CASCADE. Junction records are still cleaned up
    /// when Scan is deleted through the remaining CASCADE path (via Projects).
    /// </summary>
    /// <param name="inspector">The schema inspector.</param>
    public static async Task AssertAllScanJunctionTablesAsync(SchemaInspector inspector)
    {
        // SolutionProjects: NO ACTION on Solutions FK to avoid circular cascade
        // (cleaned up when Scan deletes Projects via CASCADE)
        await AssertJunctionTableAsync(
            inspector, "SolutionProjects",
            "Solutions", "SolutionId", "NO ACTION",
            "Projects", "ProjectId", "CASCADE");

        // ProjectPackageReferences: NO ACTION on Packages FK to avoid circular cascade
        // (cleaned up when Scan deletes Projects via CASCADE)
        await AssertJunctionTableAsync(
            inspector, "ProjectPackageReferences",
            "Projects", "ProjectId", "CASCADE",
            "Packages", "PackageId", "NO ACTION");

        // ProjectAssemblyReferences: NO ACTION on Assemblies FK to avoid circular cascade
        // (cleaned up when Scan deletes Projects via CASCADE)
        await AssertJunctionTableAsync(
            inspector, "ProjectAssemblyReferences",
            "Projects", "ProjectId", "CASCADE",
            "Assemblies", "AssemblyId", "NO ACTION");

        // Self-referencing junction tables: CASCADE + NO ACTION to avoid circular cascade
        await AssertJunctionTableAsync(
            inspector, "ProjectReferences",
            "Projects", "ReferencingProjectId", "CASCADE",
            "Projects", "ReferencedProjectId", "NO ACTION");

        await AssertJunctionTableAsync(
            inspector, "AssemblyDependencies",
            "Assemblies", "ReferencingAssemblyId", "CASCADE",
            "Assemblies", "ReferencedAssemblyId", "NO ACTION");
    }

    #endregion
}

/// <summary>
/// Represents expected column configuration for schema validation.
/// </summary>
/// <param name="DataType">The SQL data type (e.g., "nvarchar", "int", "datetime2").</param>
/// <param name="MaxLength">Optional maximum length for string types.</param>
/// <param name="IsRequired">Whether the column should be NOT NULL.</param>
/// <param name="IsIdentity">Whether the column should be an identity column.</param>
public record ColumnExpectation(
    string DataType,
    int? MaxLength = null,
    bool IsRequired = false,
    bool IsIdentity = false);
