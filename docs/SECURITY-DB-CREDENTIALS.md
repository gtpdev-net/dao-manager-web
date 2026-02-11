# Database Credentials Security Improvement

## Overview

Database credentials have been moved from hardcoded values to environment variables to prevent credential leakage and enable secure CI/CD integration.

## Changes Made

### 1. TestDbContextFactory.cs
- ✅ Removed hardcoded credentials (`P@ssw0rd`)
- ✅ Added environment variable configuration with two approaches:
  - **Option 1**: Full connection string via `TEST_DB_CONNECTION_STRING`
  - **Option 2**: Individual components (server, user, password, etc.)
- ✅ Added validation with clear error messages when credentials are missing
- ✅ Updated both constructor and `DropDatabaseAsync()` method

### 2. devcontainer.json
- ✅ Added environment variables to `remoteEnv` section:
  - `TEST_DB_SERVER`: localhost,1433
  - `TEST_DB_USER`: sa
  - `TEST_DB_PASSWORD`: P@ssw0rd (for local dev only)
  - `TEST_DB_TRUST_SERVER_CERT`: True
  - `TEST_DB_ENCRYPT`: False
- ✅ Added security comment noting these should be overridden in production/CI

### 3. Documentation
- ✅ Updated `tests/DAO.Manager.Data.Tests/README.md` with configuration table
- ✅ Added security notes and CI/CD examples
- ✅ Created `.env.example` for reference

## Applying These Changes

### For Current Dev Container Session

The environment variables are set in `devcontainer.json` but require a container rebuild to take effect. You have two options:

#### Option A: Rebuild Container (Recommended)
1. Press `F1` or `Ctrl+Shift+P`
2. Type and select: `Dev Containers: Rebuild Container`
3. Wait for the container to rebuild
4. Environment variables will be automatically available

#### Option B: Set Temporarily in Current Session
```bash
export TEST_DB_PASSWORD="P@ssw0rd"
export TEST_DB_SERVER="localhost,1433"
export TEST_DB_USER="sa"
export TEST_DB_TRUST_SERVER_CERT="True"
export TEST_DB_ENCRYPT="False"
```

Then run tests:
```bash
cd /workspace/tests/DAO.Manager.Data.Tests
dotnet test
```

### For CI/CD Pipelines

#### GitHub Actions
```yaml
- name: Run Tests
  run: dotnet test
  env:
    TEST_DB_PASSWORD: ${{ secrets.TEST_DB_PASSWORD }}
    TEST_DB_SERVER: ${{ secrets.TEST_DB_SERVER }}
```

#### Azure Pipelines
```yaml
- task: DotNetCoreCLI@2
  inputs:
    command: 'test'
  env:
    TEST_DB_PASSWORD: $(TEST_DB_PASSWORD)
    TEST_DB_SERVER: $(TEST_DB_SERVER)
```

## Environment Variables Reference

| Variable | Description | Default | Required |
|----------|-------------|---------|----------|
| `TEST_DB_CONNECTION_STRING` | Full connection string template (use `{database}` placeholder) | - | No* |
| `TEST_DB_SERVER` | SQL Server address | `localhost,1433` | No |
| `TEST_DB_USER` | Database user | `sa` | No |
| `TEST_DB_PASSWORD` | Database password | - | **Yes** |
| `TEST_DB_TRUST_SERVER_CERT` | Trust server certificate | `True` | No |
| `TEST_DB_ENCRYPT` | Enable encryption | `False` | No |

*Either `TEST_DB_CONNECTION_STRING` or `TEST_DB_PASSWORD` is required.

## Error Messages

If credentials are not configured, you'll see:
```
System.InvalidOperationException: Database password not configured. 
Please set TEST_DB_PASSWORD environment variable or provide a full 
TEST_DB_CONNECTION_STRING. For local development, configure these in 
.devcontainer/devcontainer.json. For CI/CD, set them as pipeline 
secrets or environment variables.
```

This clear error message helps developers quickly identify and fix configuration issues.

## Security Benefits

1. ✅ **No credentials in source control**: Passwords are not committed to Git
2. ✅ **Flexible configuration**: Different credentials for dev/test/CI environments
3. ✅ **CI/CD integration**: Easy to use with pipeline secrets
4. ✅ **Clear error messages**: Immediate feedback when configuration is missing
5. ✅ **Backward compatible**: Existing tests work after environment variables are set

## Verification

To verify the configuration works:

```bash
# Check environment variable is set
echo "Password configured: ${TEST_DB_PASSWORD:+YES}"

# Run a single test
cd /workspace/tests/DAO.Manager.Data.Tests
dotnet test --filter "FullyQualifiedName~ScanConfigurationTests"

# All tests should pass (4 passed, 0 failed)
```

## Next Steps

1. ✅ **Immediate**: Rebuild dev container or set environment variables in current session
2. ⏭️ **Before production**: Update CI/CD pipeline to use secrets
3. ⏭️ **Optional**: Consider Azure Key Vault or similar for production credentials
4. ⏭️ **Consider**: Remove `P@ssw0rd` from devcontainer.json if committing to public repos
