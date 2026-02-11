using DAO.Manager.Data.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace DAO.Manager.Data.Tests.Configurations;

/// <summary>
/// Tests for Scan entity configuration.
/// Validates schema, indexes, and constraints are created correctly.
/// </summary>
public class ScanConfigurationTests : IAsyncLifetime
{
    private TestDbContextFactory _factory = null!;

    public Task InitializeAsync()
    {
        _factory = new TestDbContextFactory();
        return Task.CompletedTask;
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
        await using var context = await _factory.CreateDbContextAsync();

        var tableExists = await context.Database
            .SqlQueryRaw<int>("SELECT 1 AS Value FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Scans'")
            .AnyAsync();

        // Assert
        tableExists.Should().BeTrue("Scans table should be created by EF Core configuration");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIdAsPrimaryKey()
    {
        // Arrange & Act
        await using var context = await _factory.CreateDbContextAsync();

        var hasPrimaryKey = await context.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 AS Value FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE
                  WHERE TABLE_NAME = 'Scans'
                  AND COLUMN_NAME = 'Id'
                  AND CONSTRAINT_NAME LIKE 'PK_%'")
            .AnyAsync();

        // Assert
        hasPrimaryKey.Should().BeTrue("Id should be the primary key for Scans table");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveRequiredFields()
    {
        // Arrange & Act
        await using var context = await _factory.CreateDbContextAsync();

        var columns = await context.Database
            .SqlQueryRaw<ColumnInfo>(
                @"SELECT COLUMN_NAME as Name, IS_NULLABLE as IsNullable
                  FROM INFORMATION_SCHEMA.COLUMNS
                  WHERE TABLE_NAME = 'Scans'
                  AND COLUMN_NAME IN ('RepositoryPath', 'GitCommit', 'ScanDate', 'CreatedAt')")
            .ToListAsync();

        // Assert
        columns.Should().HaveCount(4, "all required fields should exist");
        columns.Should().OnlyContain(c => c.IsNullable == "NO", "all specified fields should be required");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIndexOnScanDate()
    {
        // Arrange & Act
        await using var context = await _factory.CreateDbContextAsync();

        var hasIndex = await context.Database
            .SqlQueryRaw<int>(
                @"SELECT 1 AS Value FROM sys.indexes
                  WHERE name = 'IX_Scans_ScanDate'
                  AND object_id = OBJECT_ID(N'dbo.Scans')")
            .AnyAsync();

        // Assert
        hasIndex.Should().BeTrue("IX_Scans_ScanDate index should exist for query performance");
    }

    // Helper class for querying column information
    private class ColumnInfo
    {
        public string Name { get; set; } = string.Empty;
        public string IsNullable { get; set; } = string.Empty;
    }
}
