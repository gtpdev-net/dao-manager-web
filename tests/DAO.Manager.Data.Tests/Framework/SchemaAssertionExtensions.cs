using FluentAssertions;
using FluentAssertions.Collections;
using FluentAssertions.Primitives;

namespace DAO.Manager.Data.Tests.Framework;

/// <summary>
/// FluentAssertions extension methods for database schema validation.
/// Provides readable, fluent syntax for asserting schema constraints.
/// </summary>
/// <remarks>
/// <para><strong>Benefits:</strong></para>
/// <list type="bullet">
/// <item>Makes tests read like specifications</item>
/// <item>Provides descriptive error messages when assertions fail</item>
/// <item>Hides SQL complexity behind domain-specific language</item>
/// <item>Enables method chaining for multiple assertions</item>
/// </list>
///
/// <para><strong>Usage Examples:</strong></para>
/// <code>
/// // Table assertions
/// tableInfo.Should().HaveColumn("Id")
///     .And.HaveRequiredColumn("RepositoryPath", "nvarchar", 2000);
///
/// // Index assertions
/// indexes.Should().ContainIndex("IX_Scans_ScanDate")
///     .Which.Should().BeIndexOn("ScanDate")
///     .And.NotBeUnique();
///
/// // Foreign key assertions
/// foreignKey.Should().Reference("Scans", "Id")
///     .And.HaveDeleteBehavior("CASCADE");
///
/// // Primary key assertions
/// primaryKey.Should().BeOnColumn("Id");
/// </code>
/// </remarks>
public static class SchemaAssertionExtensions
{
    #region TableInfo Extensions

    /// <summary>
    /// Asserts that a table has a column with the specified name.
    /// </summary>
    public static AndWhichConstraint<ObjectAssertions, ColumnInfo> HaveColumn(
        this ObjectAssertions assertions,
        string columnName,
        string because = "",
        params object[] becauseArgs)
    {
        var tableInfo = assertions.Subject as TableInfo;
        tableInfo.Should().NotBeNull("a TableInfo object is required");

        var column = tableInfo!.Columns.FirstOrDefault(c => c.Name == columnName);

        column.Should().NotBeNull(
            because: $"table {tableInfo.Name} should have column {columnName}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndWhichConstraint<ObjectAssertions, ColumnInfo>(assertions, column!);
    }

    /// <summary>
    /// Asserts that a table has a required column with specific data type and optional max length.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveRequiredColumn(
        this ObjectAssertions assertions,
        string columnName,
        string dataType,
        int? maxLength = null,
        string because = "",
        params object[] becauseArgs)
    {
        var tableInfo = assertions.Subject as TableInfo;
        tableInfo.Should().NotBeNull("a TableInfo object is required");

        var column = tableInfo!.Columns.FirstOrDefault(c => c.Name == columnName);

        column.Should().NotBeNull(
            because: $"table {tableInfo.Name} should have column {columnName}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        column!.DataType.Should().Be(
            dataType,
            because: $"column {columnName} should have data type {dataType}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        column.IsNullable.Should().Be(
            "NO",
            because: $"column {columnName} should be required (NOT NULL)" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        if (maxLength.HasValue)
        {
            column.MaxLength.Should().Be(
                maxLength.Value,
                because: $"column {columnName} should have max length {maxLength.Value}" +
                         (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
                becauseArgs: becauseArgs);
        }

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a column is required (NOT NULL).
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeRequired(
        this ObjectAssertions assertions,
        string because = "",
        params object[] becauseArgs)
    {
        var column = assertions.Subject as ColumnInfo;
        column.Should().NotBeNull("a ColumnInfo object is required");

        column!.IsNullable.Should().Be("NO",
            because: $"column {column.Name} should be required" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a column is an identity column.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeIdentity(
        this ObjectAssertions assertions,
        string because = "",
        params object[] becauseArgs)
    {
        var column = assertions.Subject as ColumnInfo;
        column.Should().NotBeNull("a ColumnInfo object is required");

        column!.IsIdentity.Should().Be("YES",
            because: $"column {column.Name} should be an identity column" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    #endregion

    #region PrimaryKeyInfo Extensions

    /// <summary>
    /// Asserts that a primary key is on a specific single column.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeOnColumn(
        this ObjectAssertions assertions,
        string columnName,
        string because = "",
        params object[] becauseArgs)
    {
        var pk = assertions.Subject as PrimaryKeyInfo;
        pk.Should().NotBeNull("a PrimaryKeyInfo object is required");

        pk!.Columns.Should().ContainSingle(
            because: "primary key should be on a single column" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        pk.Columns[0].Should().Be(columnName,
            because: $"primary key should be on column {columnName}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a primary key is on specific columns (for composite keys).
    /// </summary>
    public static AndConstraint<ObjectAssertions> BePrimaryKeyOn(
        this ObjectAssertions assertions,
        params string[] columnNames)
    {
        var pk = assertions.Subject as PrimaryKeyInfo;
        pk.Should().NotBeNull("a PrimaryKeyInfo object is required");

        pk!.Columns.Should().BeEquivalentTo(columnNames, options => options.WithStrictOrdering(),
            because: $"primary key should be on columns [{string.Join(", ", columnNames)}] in order");

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    #endregion

    #region IndexInfo Extensions

    /// <summary>
    /// Asserts that a collection of indexes contains an index with the specified name.
    /// </summary>
    public static AndWhichConstraint<GenericCollectionAssertions<IndexInfo>, IndexInfo> ContainIndex(
        this GenericCollectionAssertions<IndexInfo> assertions,
        string indexName,
        string because = "",
        params object[] becauseArgs)
    {
        var indexes = assertions.Subject as IEnumerable<IndexInfo>;
        indexes.Should().NotBeNull("an IEnumerable<IndexInfo> object is required");

        var index = indexes!.FirstOrDefault(i => i.Name == indexName);

        index.Should().NotBeNull(
            because: $"should contain index {indexName}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndWhichConstraint<GenericCollectionAssertions<IndexInfo>, IndexInfo>(assertions, index!);
    }

    /// <summary>
    /// Asserts that an index is on specific columns.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeIndexOn(
        this ObjectAssertions assertions,
        params string[] columnNames)
    {
        var index = assertions.Subject as IndexInfo;
        index.Should().NotBeNull("an IndexInfo object is required");

        index!.Columns.Should().BeEquivalentTo(columnNames, options => options.WithStrictOrdering(),
            because: $"index {index.Name} should be on columns [{string.Join(", ", columnNames)}] in order");

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that an index is unique.
    /// </summary>
    public static AndConstraint<ObjectAssertions> BeUnique(
        this ObjectAssertions assertions,
        string because = "",
        params object[] becauseArgs)
    {
        var index = assertions.Subject as IndexInfo;
        index.Should().NotBeNull("an IndexInfo object is required");

        index!.IsUnique.Should().BeTrue(
            because: $"index {index.Name} should be unique" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that an index is not unique.
    /// </summary>
    public static AndConstraint<ObjectAssertions> NotBeUnique(
        this ObjectAssertions assertions,
        string because = "",
        params object[] becauseArgs)
    {
        var index = assertions.Subject as IndexInfo;
        index.Should().NotBeNull("an IndexInfo object is required");

        index!.IsUnique.Should().BeFalse(
            because: $"index {index.Name} should not be unique" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    #endregion

    #region ForeignKeyInfo Extensions

    /// <summary>
    /// Asserts that a foreign key references a specific table and column.
    /// </summary>
    public static AndConstraint<ObjectAssertions> Reference(
        this ObjectAssertions assertions,
        string referencedTable,
        string referencedColumn,
        string because = "",
        params object[] becauseArgs)
    {
        var fk = assertions.Subject as ForeignKeyInfo;
        fk.Should().NotBeNull("a ForeignKeyInfo object is required");

        fk!.ReferencedTable.Should().Be(referencedTable,
            because: $"foreign key {fk.Name} should reference table {referencedTable}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        fk.ReferencedColumn.Should().Be(referencedColumn,
            because: $"foreign key {fk.Name} should reference column {referencedColumn}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a foreign key has a specific delete behavior.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveDeleteBehavior(
        this ObjectAssertions assertions,
        string expectedBehavior,
        string because = "",
        params object[] becauseArgs)
    {
        var fk = assertions.Subject as ForeignKeyInfo;
        fk.Should().NotBeNull("a ForeignKeyInfo object is required");

        // Normalize expected behavior (accept both "CASCADE" and "CASCADE_ACTION" style)
        var normalizedExpected = expectedBehavior.Replace("_", " ").ToUpperInvariant();
        var normalizedActual = fk!.DeleteBehavior.Replace("_", " ").ToUpperInvariant();

        normalizedActual.Should().Be(normalizedExpected,
            because: $"foreign key {fk.Name} should have delete behavior {expectedBehavior}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a collection of foreign keys contains a foreign key on a specific column.
    /// </summary>
    public static AndWhichConstraint<GenericCollectionAssertions<ForeignKeyInfo>, ForeignKeyInfo> ContainForeignKeyOn(
        this GenericCollectionAssertions<ForeignKeyInfo> assertions,
        string columnName,
        string because = "",
        params object[] becauseArgs)
    {
        var foreignKeys = assertions.Subject as IEnumerable<ForeignKeyInfo>;
        foreignKeys.Should().NotBeNull("an IEnumerable<ForeignKeyInfo> object is required");

        var fk = foreignKeys!.FirstOrDefault(f => f.ColumnName == columnName);

        fk.Should().NotBeNull(
            because: $"should contain a foreign key on column {columnName}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        return new AndWhichConstraint<GenericCollectionAssertions<ForeignKeyInfo>, ForeignKeyInfo>(assertions, fk!);
    }

    #endregion

    #region JunctionTableInfo Extensions

    /// <summary>
    /// Asserts that a junction table has a composite primary key on specified columns.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveCompositePrimaryKey(
        this ObjectAssertions assertions,
        string column1,
        string column2,
        string because = "",
        params object[] becauseArgs)
    {
        var junction = assertions.Subject as JunctionTableInfo;
        junction.Should().NotBeNull("a JunctionTableInfo object is required");

        junction!.PrimaryKey.Should().NotBeNull(
            because: $"junction table {junction.TableInfo.Name} should have a primary key");

        junction.PrimaryKey!.Should().BePrimaryKeyOn(column1, column2);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    /// <summary>
    /// Asserts that a junction table has a foreign key to a specific referenced table with expected delete behavior.
    /// </summary>
    public static AndConstraint<ObjectAssertions> HaveForeignKeyTo(
        this ObjectAssertions assertions,
        string referencedTable,
        string deleteBehavior,
        string because = "",
        params object[] becauseArgs)
    {
        var junction = assertions.Subject as JunctionTableInfo;
        junction.Should().NotBeNull("a JunctionTableInfo object is required");

        var fk = junction!.ForeignKeys.FirstOrDefault(f => f.ReferencedTable == referencedTable);

        fk.Should().NotBeNull(
            because: $"junction table {junction.TableInfo.Name} should have foreign key to {referencedTable}" +
                     (string.IsNullOrEmpty(because) ? "" : $" because {because}"),
            becauseArgs: becauseArgs);

        fk!.Should().HaveDeleteBehavior(deleteBehavior);

        return new AndConstraint<ObjectAssertions>(assertions);
    }

    #endregion
}
