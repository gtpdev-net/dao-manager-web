# DAO.Manager.Data.Tests

Test suite for the DAO.Manager.Data project.

## Test Database Configuration

Tests use the **shared SQL Server with isolated databases** approach.

### Primary Strategy: Shared SQL Server (Current Implementation)

Tests connect to a pre-existing SQL Server instance (running in devcontainer or CI service) and create uniquely-named databases for each test fixture.

**Benefits:**
- ✅ Real SQL Server behavior (accurate cascade deletes, constraints, indexes)
- ✅ Fast database creation (milliseconds vs. seconds)
- ✅ Complete isolation between test fixtures
- ✅ Works seamlessly in devcontainers and CI/CD

**Requirements:**
- SQL Server must be available (typically on `localhost:1433`)
- Environment variables must be configured (see below)

### Alternative Strategies

#### TestContainers (Not Currently Used)

**When to use:**
- Running tests without a pre-configured SQL Server instance
- Need guaranteed complete isolation (separate container per test class)
- Testing against multiple SQL Server versions
- CI environments that prefer ephemeral containers

**Limitations:**
- Requires Docker installed and running
- Slower (container startup adds 2-5 seconds per test class)
- More resource-intensive

**To switch to TestContainers:**
1. Install NuGet package: `dotnet add package Testcontainers.MsSql`
2. Create `TestContainersDbContextFactory.cs` (see test plan for implementation)
3. Update test fixtures to use the new factory

#### SQLite In-Memory (Not Recommended for Most Tests)

**When to use:**
- Simple entity property tests only
- Quick validation of property assignment logic
- No SQL Server or Docker available

**DO NOT use for:**
- ❌ Configuration tests (schema/indexes/constraints differ)
- ❌ Relationship tests (cascade behavior doesn't match SQL Server)
- ❌ Integration tests (many SQL Server features unsupported)

See the [Test Plan](../../docs/tests/dao-manager-data-test-plan.md) for detailed strategy comparison.

### Configuration

Database credentials are configured via **environment variables** to avoid hardcoding secrets in source control:

#### Environment Variables

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `TEST_DB_CONNECTION_STRING` | Full connection string template (use `{database}` placeholder) | - | No* |
| `TEST_DB_SERVER` | SQL Server address | `localhost,1433` | No |
| `TEST_DB_USER` | Database user | `sa` | No |
| `TEST_DB_PASSWORD` | Database password | - | **Yes** |
| `TEST_DB_TRUST_SERVER_CERT` | Trust server certificate | `True` | No |
| `TEST_DB_ENCRYPT` | Enable encryption | `False` | No |

*Either `TEST_DB_CONNECTION_STRING` or `TEST_DB_PASSWORD` (with optional other variables) is required.

#### Local Development (Dev Container)

Environment variables are configured in `.devcontainer/devcontainer.json`:
```jsonc
"remoteEnv": {
  "TEST_DB_PASSWORD": "P@ssw0rd",
  "TEST_DB_SERVER": "localhost,1433",
  // ... other settings
}
```

#### CI/CD Pipeline

Set environment variables as pipeline secrets:
```bash
# GitHub Actions example
export TEST_DB_PASSWORD='${{ secrets.TEST_DB_PASSWORD }}'
export TEST_DB_SERVER='your-ci-server'

# Azure Pipelines example
- task: DotNetCoreCLI@2
  env:
    TEST_DB_PASSWORD: $(TEST_DB_PASSWORD)
    TEST_DB_SERVER: $(TEST_DB_SERVER)
```

#### Alternative: Full Connection String

For maximum flexibility, use `TEST_DB_CONNECTION_STRING`:
```bash
export TEST_DB_CONNECTION_STRING="Server=myserver;Database={database};User Id=myuser;Password=mypass;TrustServerCertificate=True;"
```
Note: Use `{database}` as a placeholder - it will be replaced with the unique test database name.

### How It Works

1. **TestDbContextFactory** connects to your existing SQL Server using environment variables
2. Each test class creates a **unique database** (e.g., `TestDb_20260211_143052_abc123`)
3. Tests run isolated from each other
4. Databases are preserved after tests (helpful for debugging)

### Security Note

⚠️ **Never commit database passwords to source control.** Passwords are configured via environment variables that are:
- Set in `.devcontainer/devcontainer.json` for local development (not committed to repos with sensitive data)
- Provided as secrets in CI/CD pipelines
- Validated at test startup with clear error messages if missing

### Running Tests

```bash
# Run all tests
dotnet test

# Run with coverage
./test-coverage.sh

# Run specific test class
dotnet test --filter "FullyQualifiedName~ScanConfigurationTests"

# Run P1 priority tests only
dotnet test --filter "Category=P1"
```

### Database Cleanup

By default, test databases are **NOT automatically dropped** after tests complete. This allows you to:
- Inspect test data after failures
- Debug schema issues
- Verify test assertions manually

To enable automatic cleanup, uncomment this line in `TestDbContextFactory.cs`:
```csharp
// await DropDatabaseAsync();
```

### Manual Cleanup

To manually drop test databases:

```sql
-- Connect to localhost:1433 as sa
-- List all test databases
SELECT name FROM sys.databases WHERE name LIKE 'TestDb_%'

-- Drop all test databases
USE master;
DECLARE @dbname VARCHAR(255);
DECLARE db_cursor CURSOR FOR 
    SELECT name FROM sys.databases WHERE name LIKE 'TestDb_%';
OPEN db_cursor;
FETCH NEXT FROM db_cursor INTO @dbname;
WHILE @@FETCH_STATUS = 0
BEGIN
    EXEC('ALTER DATABASE [' + @dbname + '] SET SINGLE_USER WITH ROLLBACK IMMEDIATE;');
    EXEC('DROP DATABASE [' + @dbname + '];');
    FETCH NEXT FROM db_cursor INTO @dbname;
END;
CLOSE db_cursor;
DEALLOCATE db_cursor;
```

## Test Structure

```
tests/DAO.Manager.Data.Tests/
├── Configurations/          # P1: EF Core configuration tests
│   ├── ScanConfigurationTests.cs
│   └── ... (more to come)
├── DbContext/              # P1: DbContext behavior tests
├── Entities/               # P2: Entity behavior tests
├── Relationships/          # P2: Relationship tests
├── Operations/             # P3: CRUD tests
├── Integration/            # P4: Complex scenario tests
├── Migrations/             # P5: Migration tests
└── Helpers/                # Test infrastructure
    ├── TestDbContextFactory.cs
    └── ... (more to come)
```

## Next Steps

1. ✅ Test infrastructure created
2. ✅ Sample configuration test added
3. ⏳ Add remaining P1 configuration tests
4. ⏳ Add entity builder helpers
5. ⏳ Add CRUD and relationship tests

See [Test Plan](../../docs/tests/dao-manager-data-test-plan.md) for full roadmap.
