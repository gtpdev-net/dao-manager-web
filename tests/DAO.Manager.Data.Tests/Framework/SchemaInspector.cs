using DAO.Manager.Data.Data;
using Microsoft.EntityFrameworkCore;

namespace DAO.Manager.Data.Tests.Framework;

/// <summary>
/// Provides methods to inspect database schema elements (tables, columns, indexes, foreign keys).
/// Uses SQL Server INFORMATION_SCHEMA views and system tables for accurate schema introspection.
/// </summary>
/// <remarks>
/// <para><strong>Design:</strong> All methods query the actual database schema using SQL Server
/// metadata views rather than relying on EF Core's model. This ensures tests validate what's
/// actually created in the database.</para>
///
/// <para><strong>Usage:</strong></para>
/// <code>
/// var inspector = new SchemaInspector(dbContext);
///
/// // Check table existence (uses default "dbo" schema)
/// var exists = await inspector.TableExistsAsync("Scans");
///
/// // Check table in a specific schema
/// var existsInCustomSchema = await inspector.TableExistsAsync("Scans", "custom");
///
/// // Get full table schema
/// var tableInfo = await inspector.GetTableInfoAsync("Scans");
///
/// // Get indexes
/// var indexes = await inspector.GetIndexesAsync("Scans");
///
/// // Get foreign keys
/// var fks = await inspector.GetForeignKeysAsync("ScanEvents");
/// </code>
/// </remarks>
public class SchemaInspector
{
    private readonly ApplicationDbContext _context;

    public SchemaInspector(ApplicationDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context));
    }

    #region Table Inspection

    /// <summary>
    /// Checks if a table exists in the database.
    /// </summary>
    /// <param name="tableName">The name of the table to check.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>True if the table exists; otherwise, false.</returns>
    public async Task<bool> TableExistsAsync(string tableName, string schemaName = "dbo")
    {
        var result = await _context.Database
            .SqlQueryRaw<int>(
                "SELECT 1 AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = {0} AND TABLE_NAME = {1}",
                schemaName, tableName)
            .AnyAsync();

        return result;
    }

    /// <summary>
    /// Gets detailed information about a table including all its columns.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>Table information with column details, or null if table doesn't exist.</returns>
    public async Task<TableInfo?> GetTableInfoAsync(string tableName, string schemaName = "dbo")
    {
        var qualifiedTableName = $"[{schemaName}].[{tableName}]";

        var columns = await _context.Database
            .SqlQueryRaw<ColumnInfo>(
                @"SELECT
                    c.COLUMN_NAME as Name,
                    c.DATA_TYPE as DataType,
                    c.CHARACTER_MAXIMUM_LENGTH as MaxLength,
                    c.IS_NULLABLE as IsNullable,
                    CASE WHEN ic.name IS NOT NULL THEN 'YES' ELSE 'NO' END as IsIdentity
                FROM INFORMATION_SCHEMA.COLUMNS c
                LEFT JOIN sys.identity_columns ic ON ic.object_id = OBJECT_ID({0})
                    AND ic.name = c.COLUMN_NAME
                WHERE c.TABLE_SCHEMA = {1} AND c.TABLE_NAME = {2}
                ORDER BY c.ORDINAL_POSITION",
                qualifiedTableName, schemaName, tableName)
            .ToListAsync();

        if (!columns.Any())
        {
            return null;
        }

        return new TableInfo(tableName, columns);
    }

    #endregion

    #region Primary Key Inspection

    /// <summary>
    /// Gets primary key information for a table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>Primary key information, or null if no primary key exists.</returns>
    public async Task<PrimaryKeyInfo?> GetPrimaryKeyAsync(string tableName, string schemaName = "dbo")
    {
        var pkColumns = await _context.Database
            .SqlQueryRaw<PrimaryKeyColumnInfo>(
                @"SELECT
                    kcu.CONSTRAINT_NAME as ConstraintName,
                    kcu.COLUMN_NAME as ColumnName,
                    kcu.ORDINAL_POSITION as OrdinalPosition
                FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE kcu
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
                    ON kcu.CONSTRAINT_NAME = tc.CONSTRAINT_NAME
                    AND kcu.TABLE_SCHEMA = tc.TABLE_SCHEMA
                WHERE tc.CONSTRAINT_TYPE = 'PRIMARY KEY'
                    AND kcu.TABLE_SCHEMA = {0}
                    AND kcu.TABLE_NAME = {1}
                ORDER BY kcu.ORDINAL_POSITION",
                schemaName, tableName)
            .ToListAsync();

        if (!pkColumns.Any())
        {
            return null;
        }

        return new PrimaryKeyInfo(
            pkColumns.First().ConstraintName,
            pkColumns.Select(c => c.ColumnName).ToList());
    }

    #endregion

    #region Index Inspection

    /// <summary>
    /// Gets all indexes for a table (excluding primary key indexes).
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>List of indexes on the table.</returns>
    public async Task<List<IndexInfo>> GetIndexesAsync(string tableName, string schemaName = "dbo")
    {
        var qualifiedTableName = $"[{schemaName}].[{tableName}]";

        var indexData = await _context.Database
            .SqlQueryRaw<IndexColumnInfo>(
                @"SELECT
                    i.name as IndexName,
                    COL_NAME(ic.object_id, ic.column_id) as ColumnName,
                    CAST(ic.key_ordinal as int) as KeyOrdinal,
                    i.is_unique as IsUnique,
                    i.type_desc as TypeDesc
                FROM sys.indexes i
                INNER JOIN sys.index_columns ic ON i.object_id = ic.object_id AND i.index_id = ic.index_id
                WHERE i.object_id = OBJECT_ID({0})
                    AND i.is_primary_key = 0
                    AND i.type > 0
                    AND ic.key_ordinal > 0
                ORDER BY i.name, ic.key_ordinal",
                qualifiedTableName)
            .ToListAsync();

        var indexes = indexData
            .GroupBy(x => x.IndexName)
            .Select(g => new IndexInfo(
                g.Key,
                g.OrderBy(x => x.KeyOrdinal).Select(x => x.ColumnName).ToList(),
                g.First().IsUnique,
                g.First().TypeDesc == "CLUSTERED"))
            .ToList();

        return indexes;
    }

    /// <summary>
    /// Gets a specific index by name.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="indexName">The name of the index.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>Index information, or null if the index doesn't exist.</returns>
    public async Task<IndexInfo?> GetIndexAsync(string tableName, string indexName, string schemaName = "dbo")
    {
        var indexes = await GetIndexesAsync(tableName, schemaName);
        return indexes.FirstOrDefault(i => i.Name == indexName);
    }

    #endregion

    #region Foreign Key Inspection

    /// <summary>
    /// Gets all foreign keys for a table.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>List of foreign keys on the table.</returns>
    public async Task<List<ForeignKeyInfo>> GetForeignKeysAsync(string tableName, string schemaName = "dbo")
    {
        var qualifiedTableName = $"[{schemaName}].[{tableName}]";

        var fkData = await _context.Database
            .SqlQueryRaw<ForeignKeyRawInfo>(
                @"SELECT
                    fk.name as ForeignKeyName,
                    OBJECT_NAME(fk.parent_object_id) as TableName,
                    COL_NAME(fkc.parent_object_id, fkc.parent_column_id) as ColumnName,
                    OBJECT_NAME(fk.referenced_object_id) as ReferencedTable,
                    COL_NAME(fkc.referenced_object_id, fkc.referenced_column_id) as ReferencedColumn,
                    fk.delete_referential_action_desc as DeleteBehavior
                FROM sys.foreign_keys fk
                INNER JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
                WHERE fk.parent_object_id = OBJECT_ID({0})
                ORDER BY fk.name",
                qualifiedTableName)
            .ToListAsync();

        var foreignKeys = fkData
            .Select(fk => new ForeignKeyInfo(
                fk.ForeignKeyName,
                fk.TableName,
                fk.ColumnName,
                fk.ReferencedTable,
                fk.ReferencedColumn,
                fk.DeleteBehavior))
            .ToList();

        return foreignKeys;
    }

    /// <summary>
    /// Gets a specific foreign key by name.
    /// </summary>
    /// <param name="tableName">The name of the table.</param>
    /// <param name="foreignKeyName">The name of the foreign key.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>Foreign key information, or null if the foreign key doesn't exist.</returns>
    public async Task<ForeignKeyInfo?> GetForeignKeyAsync(string tableName, string foreignKeyName, string schemaName = "dbo")
    {
        var foreignKeys = await GetForeignKeysAsync(tableName, schemaName);
        return foreignKeys.FirstOrDefault(fk => fk.Name == foreignKeyName);
    }

    #endregion

    #region Junction Table Inspection

    /// <summary>
    /// Gets comprehensive information about a junction table including its primary key and foreign keys.
    /// Useful for validating many-to-many relationship configurations.
    /// </summary>
    /// <param name="junctionTableName">The name of the junction table.</param>
    /// <param name="schemaName">The schema name (defaults to "dbo").</param>
    /// <returns>Junction table information including table schema, primary key, and foreign keys.</returns>
    public async Task<JunctionTableInfo?> GetJunctionTableInfoAsync(string junctionTableName, string schemaName = "dbo")
    {
        var tableInfo = await GetTableInfoAsync(junctionTableName, schemaName);
        if (tableInfo == null)
        {
            return null;
        }

        var primaryKey = await GetPrimaryKeyAsync(junctionTableName, schemaName);
        var foreignKeys = await GetForeignKeysAsync(junctionTableName, schemaName);

        return new JunctionTableInfo(tableInfo, primaryKey, foreignKeys);
    }

    #endregion
}

#region Schema Information Records

/// <summary>
/// Represents information about a database table and its columns.
/// </summary>
/// <param name="Name">The table name.</param>
/// <param name="Columns">Collection of column information.</param>
public record TableInfo(string Name, List<ColumnInfo> Columns);

/// <summary>
/// Represents information about a table column.
/// </summary>
/// <param name="Name">The column name.</param>
/// <param name="DataType">The SQL data type (e.g., "nvarchar", "int", "datetime2").</param>
/// <param name="MaxLength">Maximum length for string types, or null for non-string types.</param>
/// <param name="IsNullable">"YES" if the column is nullable, "NO" if required.</param>
/// <param name="IsIdentity">"YES" if the column is an identity column, "NO" otherwise.</param>
public record ColumnInfo(string Name, string DataType, int? MaxLength, string IsNullable, string IsIdentity);

/// <summary>
/// Represents information about a primary key constraint.
/// </summary>
/// <param name="ConstraintName">The name of the primary key constraint.</param>
/// <param name="Columns">List of column names that comprise the primary key (in order).</param>
public record PrimaryKeyInfo(string ConstraintName, List<string> Columns);

/// <summary>
/// Represents information about an index.
/// </summary>
/// <param name="Name">The index name.</param>
/// <param name="Columns">List of column names in the index (in order).</param>
/// <param name="IsUnique">True if the index enforces uniqueness.</param>
/// <param name="IsClustered">True if the index is clustered.</param>
public record IndexInfo(string Name, List<string> Columns, bool IsUnique, bool IsClustered);

/// <summary>
/// Represents information about a foreign key constraint.
/// </summary>
/// <param name="Name">The foreign key constraint name.</param>
/// <param name="TableName">The table containing the foreign key.</param>
/// <param name="ColumnName">The column name in the table.</param>
/// <param name="ReferencedTable">The referenced table name.</param>
/// <param name="ReferencedColumn">The referenced column name.</param>
/// <param name="DeleteBehavior">The delete behavior (e.g., "CASCADE", "NO_ACTION").</param>
public record ForeignKeyInfo(
    string Name,
    string TableName,
    string ColumnName,
    string ReferencedTable,
    string ReferencedColumn,
    string DeleteBehavior);

/// <summary>
/// Represents comprehensive information about a junction table.
/// </summary>
/// <param name="TableInfo">Basic table and column information.</param>
/// <param name="PrimaryKey">Primary key information (typically composite).</param>
/// <param name="ForeignKeys">All foreign key constraints (typically two for many-to-many).</param>
public record JunctionTableInfo(
    TableInfo TableInfo,
    PrimaryKeyInfo? PrimaryKey,
    List<ForeignKeyInfo> ForeignKeys);

// Internal helper records for query results
internal record PrimaryKeyColumnInfo(string ConstraintName, string ColumnName, int OrdinalPosition);
internal record IndexColumnInfo(string IndexName, string ColumnName, int KeyOrdinal, bool IsUnique, string TypeDesc);
internal record ForeignKeyRawInfo(
    string ForeignKeyName,
    string TableName,
    string ColumnName,
    string ReferencedTable,
    string ReferencedColumn,
    string DeleteBehavior);

#endregion
