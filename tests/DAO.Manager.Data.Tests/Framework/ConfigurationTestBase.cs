using DAO.Manager.Data.Data;
using DAO.Manager.Data.Tests.Helpers;

namespace DAO.Manager.Data.Tests.Framework;

/// <summary>
/// Abstract base class for entity configuration tests.
/// Provides common test infrastructure including DbContext initialization,
/// schema inspection utilities, and lifecycle management.
/// </summary>
/// <remarks>
/// <para><strong>Benefits:</strong></para>
/// <list type="bullet">
/// <item>Eliminates duplicate TestDbContextFactory setup across test classes</item>
/// <item>Provides convenient access to SchemaInspector for schema validation</item>
/// <item>Standardizes test lifecycle (setup/teardown) across all configuration tests</item>
/// <item>Enables virtual hooks for test-specific customization</item>
/// </list>
///
/// <para><strong>Usage:</strong></para>
/// <code>
/// public class ScanConfigurationTests : ConfigurationTestBase
/// {
///     [Fact]
///     public async Task ScanConfiguration_ShouldCreateTableWithCorrectName()
///     {
///         // Use protected Context and Inspector properties
///         var tableExists = await Inspector.TableExistsAsync("Scans");
///         tableExists.Should().BeTrue();
///     }
/// }
/// </code>
/// </remarks>
public abstract class ConfigurationTestBase : IAsyncLifetime
{
    private TestDbContextFactory? _factory;
    private ApplicationDbContext? _context;
    private SchemaInspector? _inspector;

    /// <summary>
    /// Gets the database context for the current test.
    /// Available after InitializeAsync is called.
    /// </summary>
    protected ApplicationDbContext Context => _context
        ?? throw new InvalidOperationException("Context not initialized. Ensure test infrastructure is set up correctly.");

    /// <summary>
    /// Gets the schema inspector for database schema validation.
    /// Available after InitializeAsync is called.
    /// </summary>
    protected SchemaInspector Inspector => _inspector
        ?? throw new InvalidOperationException("Inspector not initialized. Ensure test infrastructure is set up correctly.");

    /// <summary>
    /// Initializes the test infrastructure.
    /// Creates a new test database and DbContext for the test fixture.
    /// </summary>
    public virtual async Task InitializeAsync()
    {
        _factory = new TestDbContextFactory();
        _context = await _factory.CreateDbContextAsync();
        _inspector = new SchemaInspector(_context);
    }

    /// <summary>
    /// Cleans up test infrastructure.
    /// Disposes the DbContext and factory, optionally dropping the test database.
    /// </summary>
    public virtual async Task DisposeAsync()
    {
        if (_context != null)
        {
            await _context.DisposeAsync();
        }

        if (_factory != null)
        {
            await _factory.DisposeAsync();
        }
    }
}
