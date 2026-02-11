using Microsoft.EntityFrameworkCore;
using DAO.Manager.Data.Data;

namespace DAO.Manager.Data.Tests.Helpers;

/// <summary>
/// Factory for creating test DbContext instances with isolated databases.
///
/// <para><strong>Strategy: Shared SQL Server with Isolated Databases</strong></para>
///
/// This factory connects to a pre-existing SQL Server instance (typically running in a devcontainer
/// or CI service) and creates uniquely-named databases for each test fixture. This approach provides:
///
/// <list type="bullet">
/// <item>✅ Real SQL Server behavior (cascade deletes, constraints, indexes work correctly)</item>
/// <item>✅ Fast database creation (milliseconds vs. seconds for containers)</item>
/// <item>✅ Complete isolation between test fixtures</item>
/// <item>✅ Works seamlessly in VS Code devcontainers and CI/CD pipelines</item>
/// </list>
///
/// <para><strong>Configuration (Required):</strong></para>
/// Set environment variables in <c>.devcontainer/devcontainer.json</c> or CI configuration:
/// <code>
/// "containerEnv": {
///     "TEST_DB_PASSWORD": "YourStrong@Passw0rd",  // REQUIRED
///     "TEST_DB_SERVER": "localhost,1433",         // Optional, defaults to localhost,1433
///     "TEST_DB_USER": "sa"                        // Optional, defaults to sa
/// }
/// </code>
///
/// <para><strong>Alternative Approaches:</strong></para>
///
/// <list type="number">
/// <item><strong>TestContainers</strong>: Use when SQL Server is not pre-configured (requires Docker).
/// Slower but provides complete isolation with per-test-class containers. Requires the
/// <c>Testcontainers.MsSql</c> NuGet package (not currently installed).</item>
///
/// <item><strong>SQLite In-Memory</strong>: Use only for simple entity property tests where SQL Server-specific
/// behavior (cascade deletes, constraints, indexes) is not being validated. NOT suitable for configuration,
/// relationship, or integration tests.</item>
/// </list>
///
/// <para><strong>Usage Example:</strong></para>
/// <code>
/// public class MyTestFixture : IAsyncLifetime
/// {
///     private readonly TestDbContextFactory _factory = new();
///     private ApplicationDbContext _context = null!;
///
///     public async Task InitializeAsync()
///     {
///         _context = await _factory.CreateDbContextAsync();
///         // Database is created and migrated automatically
///     }
///
///     public async Task DisposeAsync()
///     {
///         await _factory.DisposeAsync();
///         // Database is optionally dropped (see DropDatabaseAsync)
///     }
/// }
/// </code>
/// </summary>
public class TestDbContextFactory : IAsyncDisposable
{
    private readonly string _connectionString;
    private readonly string _databaseName;
    private readonly List<ApplicationDbContext> _contexts = new();
    private bool _disposed;

    public TestDbContextFactory()
    {
        // Generate unique database name for this test instance
        _databaseName = $"TestDb_{DateTime.UtcNow:yyyyMMdd_HHmmss}_{Guid.NewGuid():N}";

        // Validate database name format for security
        ValidateDatabaseName(_databaseName);

        // Build connection string from environment variables
        _connectionString = BuildConnectionString();
    }

    private string BuildConnectionString()
    {
        // Option 1: Use full connection string from environment (allows complete flexibility)
        var connectionStringTemplate = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionStringTemplate))
        {
            // Replace {database} placeholder with actual test database name
            return connectionStringTemplate.Replace("{database}", _databaseName);
        }

        // Option 2: Build from individual components
        var server = Environment.GetEnvironmentVariable("TEST_DB_SERVER") ?? "localhost,1433";
        var user = Environment.GetEnvironmentVariable("TEST_DB_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("TEST_DB_PASSWORD");
        var trustServerCert = Environment.GetEnvironmentVariable("TEST_DB_TRUST_SERVER_CERT") ?? "True";
        var encrypt = Environment.GetEnvironmentVariable("TEST_DB_ENCRYPT") ?? "False";

        // Validate required configuration
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "Database password not configured. Please set TEST_DB_PASSWORD environment variable " +
                "or provide a full TEST_DB_CONNECTION_STRING. " +
                "For local development, configure these in .devcontainer/devcontainer.json. " +
                "For CI/CD, set them as pipeline secrets or environment variables.");
        }

        return $"Server={server};Database={_databaseName};User Id={user};Password={password};TrustServerCertificate={trustServerCert};Encrypt={encrypt};";
    }

    /// <summary>
    /// Creates a new DbContext instance connected to the test database.
    /// The database is created and migrated automatically on first call.
    /// </summary>
    public async Task<ApplicationDbContext> CreateDbContextAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlServer(_connectionString)
            .EnableSensitiveDataLogging() // Helpful for debugging tests
            .Options;

        var context = new ApplicationDbContext(options);
        _contexts.Add(context);

        // Create and migrate database on first context creation
        if (_contexts.Count == 1)
        {
            await context.Database.EnsureCreatedAsync();
            // Alternatively, use migrations:
            // await context.Database.MigrateAsync();
        }

        return context;
    }

    /// <summary>
    /// Creates a new DbContext instance (synchronous version).
    /// </summary>
    public ApplicationDbContext CreateDbContext()
    {
        return CreateDbContextAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Disposes all contexts and optionally drops the test database.
    /// </summary>
    public async ValueTask DisposeAsync()
    {
        if (_disposed) return;
        _disposed = true;

        // Dispose all contexts
        foreach (var context in _contexts)
        {
            await context.DisposeAsync();
        }
        _contexts.Clear();

        // Optional: Drop test database to clean up
        // Uncomment to enable automatic cleanup
        // await DropDatabaseAsync();

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Drops the test database. Useful for cleanup or manual invocation.
    /// </summary>
    public async Task DropDatabaseAsync()
    {
        try
        {
            // Validate database name before using in DDL (defense in depth)
            ValidateDatabaseName(_databaseName);

            var masterConnectionString = BuildMasterConnectionString();
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer(masterConnectionString)
                .Options;

            using var masterContext = new ApplicationDbContext(options);

            // Drop database if it exists, using QUOTENAME() for safe identifier handling
            // Note: QUOTENAME() and database identifiers cannot be parameterized in EF Core ExecuteSqlRawAsync,
            // but we validate the format above to ensure safety
            var quotedDbName = $"QUOTENAME(N'{_databaseName.Replace("'", "''")}')";
            await masterContext.Database.ExecuteSqlRawAsync(
                $"DECLARE @DbName NVARCHAR(128) = {quotedDbName}; " +
                $"IF EXISTS (SELECT 1 FROM sys.databases WHERE name = @DbName) " +
                $"BEGIN " +
                $"  EXEC('ALTER DATABASE ' + @DbName + ' SET SINGLE_USER WITH ROLLBACK IMMEDIATE'); " +
                $"  EXEC('DROP DATABASE ' + @DbName); " +
                $"END");
        }
        catch (Exception ex)
        {
            // Swallow exceptions during cleanup
            System.Diagnostics.Debug.WriteLine($"Failed to drop test database {_databaseName}: {ex.Message}");
        }
    }

    /// <summary>
    /// Gets the name of the test database.
    /// </summary>
    public string DatabaseName => _databaseName;

    /// <summary>
    /// Gets the connection string for the test database.
    /// </summary>
    public string ConnectionString => _connectionString;

    private string BuildMasterConnectionString()
    {
        // Use the same configuration as the test connection, but target 'master' database
        var connectionStringTemplate = Environment.GetEnvironmentVariable("TEST_DB_CONNECTION_STRING");
        if (!string.IsNullOrWhiteSpace(connectionStringTemplate))
        {
            return connectionStringTemplate.Replace("{database}", "master");
        }

        var server = Environment.GetEnvironmentVariable("TEST_DB_SERVER") ?? "localhost,1433";
        var user = Environment.GetEnvironmentVariable("TEST_DB_USER") ?? "sa";
        var password = Environment.GetEnvironmentVariable("TEST_DB_PASSWORD")
            ?? throw new InvalidOperationException("TEST_DB_PASSWORD environment variable not set");
        var trustServerCert = Environment.GetEnvironmentVariable("TEST_DB_TRUST_SERVER_CERT") ?? "True";
        var encrypt = Environment.GetEnvironmentVariable("TEST_DB_ENCRYPT") ?? "False";

        return $"Server={server};Database=master;User Id={user};Password={password};TrustServerCertificate={trustServerCert};Encrypt={encrypt};";
    }

    /// <summary>
    /// Validates that the database name matches the expected test database pattern.
    /// This prevents potential SQL injection if _databaseName ever becomes user-influenced.
    /// </summary>
    private static void ValidateDatabaseName(string databaseName)
    {
        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name cannot be null or empty", nameof(databaseName));
        }

        // Allowlist pattern: TestDb_YYYYMMDD_HHMMSS_guid
        // Example: TestDb_20240211_143022_a1b2c3d4e5f6a7b8c9d0e1f2a3b4c5d6
        if (!System.Text.RegularExpressions.Regex.IsMatch(databaseName, @"^TestDb_[0-9]{8}_[0-9]{6}_[0-9a-f]{32}$"))
        {
            throw new ArgumentException(
                $"Database name '{databaseName}' does not match the expected test database pattern. " +
                "Expected format: TestDb_YYYYMMDD_HHMMSS_<32-hex-guid>",
                nameof(databaseName));
        }
    }
}
