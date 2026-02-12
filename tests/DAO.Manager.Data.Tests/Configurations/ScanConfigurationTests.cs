using DAO.Manager.Data.Tests.Framework;
using FluentAssertions;

namespace DAO.Manager.Data.Tests.Configurations;

/// <summary>
/// Tests for Scan entity configuration.
/// Validates schema, indexes, and relationships are created correctly.
/// </summary>
/// <remarks>
/// <para><strong>Framework Benefits Demonstrated:</strong></para>
/// <list type="bullet">
/// <item>Inherits from ConfigurationTestBase - eliminates factory setup boilerplate</item>
/// <item>Uses SchemaInspector - replaces raw SQL queries with reusable methods</item>
/// <item>Uses FluentAssertions extensions - improves test readability</item>
/// <item>Reduced from ~110 lines to ~80 lines while adding more comprehensive tests</item>
/// </list>
/// </remarks>
public class ScanConfigurationTests : ConfigurationTestBase
{
    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldCreateTableWithCorrectName()
    {
        // Act
        var tableExists = await Inspector.TableExistsAsync("Scans");

        // Assert
        tableExists.Should().BeTrue("Scans table should be created by EF Core configuration");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIdAsPrimaryKey()
    {
        // Act
        var primaryKey = await Inspector.GetPrimaryKeyAsync("Scans");

        // Assert
        primaryKey.Should().NotBeNull("Scans table should have a primary key");
        primaryKey!.Should().BeOnColumn("Id");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIdAsIdentity()
    {
        // Act
        var tableInfo = await Inspector.GetTableInfoAsync("Scans");
        tableInfo.Should().NotBeNull("Scans table should exist to verify identity column");
        var idColumn = tableInfo!.Columns.First(c => c.Name == "Id");

        // Assert
        idColumn.Should().BeIdentity("Id should auto-increment for new scans");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveCorrectColumnTypes()
    {
        // Act
        var tableInfo = await Inspector.GetTableInfoAsync("Scans");

        // Assert
        tableInfo.Should().NotBeNull();
        tableInfo.Should().HaveRequiredColumn("RepositoryPath", "nvarchar", 2000);
        tableInfo.Should().HaveRequiredColumn("GitCommit", "nvarchar", 100);
        tableInfo.Should().HaveRequiredColumn("ScanDate", "datetime2");
        tableInfo.Should().HaveRequiredColumn("CreatedAt", "datetime2");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIndexOnScanDate()
    {
        // Act
        var indexes = await Inspector.GetIndexesAsync("Scans");

        // Assert
        indexes.Should().ContainIndex("IX_Scans_ScanDate")
            .Which.Should().BeIndexOn("ScanDate")
            .And.NotBeUnique();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIndexOnCreatedAt()
    {
        // Act
        var indexes = await Inspector.GetIndexesAsync("Scans");

        // Assert
        indexes.Should().ContainIndex("IX_Scans_CreatedAt")
            .Which.Should().BeIndexOn("CreatedAt")
            .And.NotBeUnique();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveIndexOnGitCommit()
    {
        // Act
        var indexes = await Inspector.GetIndexesAsync("Scans");

        // Assert
        indexes.Should().ContainIndex("IX_Scans_GitCommit")
            .Which.Should().BeIndexOn("GitCommit")
            .And.NotBeUnique();
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldHaveOneToManyRelationshipWithScanEvents()
    {
        // Act
        var foreignKeys = await Inspector.GetForeignKeysAsync("ScanEvents");

        // Assert
        foreignKeys.Should().ContainForeignKeyOn("ScanId")
            .Which.Should().Reference("Scans", "Id")
            .And.HaveDeleteBehavior("CASCADE");
    }

    [Fact]
    [Trait("Category", "P1")]
    [Trait("Area", "Configuration")]
    public async Task ScanConfiguration_ShouldCascadeDeleteToAllChildEntities()
    {
        // Arrange - Define all expected child tables with CASCADE delete
        var expectedChildren = new[]
        {
            "ScanEvents",
            "Solutions",
            "Projects",
            "Packages",
            "Assemblies"
        };

        // Act & Assert - Verify each child has CASCADE delete from Scans
        foreach (var childTable in expectedChildren)
        {
            var foreignKeys = await Inspector.GetForeignKeysAsync(childTable);
            foreignKeys.Should().ContainForeignKeyOn("ScanId")
                .Which.Should().Reference("Scans", "Id")
                .And.HaveDeleteBehavior("CASCADE",
                    because: $"{childTable} should be deleted when its parent Scan is deleted");
        }

        // Assert - Verify all child tables have proper indexes on ScanId
        // Each should have both a regular index and a unique composite index
        await SchemaTestHelpers.AssertAllScanChildTableIndexesAsync(Inspector);

        // Assert - Verify all junction tables have proper configuration
        // Includes composite PKs and correct delete behaviors (CASCADE vs NO ACTION for self-references)
        await SchemaTestHelpers.AssertAllScanJunctionTablesAsync(Inspector);
    }
}
