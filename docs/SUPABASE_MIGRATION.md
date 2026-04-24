# Supabase Migration Guide

## Migration Date

**April 18, 2026**

## Overview

Successfully migrated MyShop project from local PostgreSQL to **Supabase** cloud database.

## Connection Details

| Property              | Value                                      |
| --------------------- | ------------------------------------------ |
| **Host**              | `aws-1-ap-northeast-1.pooler.supabase.com` |
| **Database**          | `postgres`                                 |
| **Username**          | `postgres.oivqtyqiwschphohiblr`            |
| **Port**              | `5432`                                     |
| **SSL Mode**          | `Require`                                  |
| **Connection Pooler** | Enabled (Connection Idle Lifetime: 300s)   |

> **⚠️ Security Note:** Credentials stored in `appsettings.json`. For production, use environment variables or secrets management.

## Migrated Data

| Table                   | Rows | Status          |
| ----------------------- | ---- | --------------- |
| Users                   | 25   | ✅ Migrated     |
| Customers               | 6    | ✅ Migrated     |
| Products                | 23   | ✅ Migrated     |
| Categories              | 4    | ✅ Migrated     |
| Orders                  | 0    | ✅ Schema ready |
| OrderDetails            | 0    | ✅ Schema ready |
| Promotions              | 0    | ✅ Schema ready |
| UserSettings            | 0    | ✅ Schema ready |
| WorkSchedules           | 0    | ✅ Schema ready |
| AppConfigs              | 0    | ✅ Schema ready |
| \_\_EFMigrationsHistory | 14   | ✅ Migrated     |

**Total Tables:** 11  
**Total Rows Migrated:** 72

## Migration Steps Performed

### 1. **Database Backup** ✅

```bash
pg_dump -h localhost -U nguyentienluat -d myshop --no-owner --no-privileges > migration_backup.sql
```

- Created backup: `migration_backup.sql` (794 lines)
- Stored in project root for reference

### 2. **Supabase Setup** ✅

- Created Supabase project
- Connection pooler configured in Supabase dashboard
- SSL/TLS enabled

### 3. **Data Import** ✅

```bash
PGPASSWORD="laptrinhwindow" psql -h "aws-1-ap-northeast-1.pooler.supabase.com" \
  -U "postgres.oivqtyqiwschphohiblr" -d "postgres" -f migration_backup.sql
```

### 4. **Application Configuration** ✅

- Updated `appsettings.json` with Supabase credentials
- No code changes required (DatabaseConfig already supports Supabase)
- Build successful: ✅ 0 errors, 48 warnings

### 5. **Verification** ✅

```sql
-- Verified all 11 tables created
-- Verified data counts match source database
-- Tested connection from .NET application
```

## Connection String

**PostgreSQL Format:**

```
Host=aws-1-ap-northeast-1.pooler.supabase.com;
Port=5432;
Database=postgres;
Username=postgres.oivqtyqiwschphohiblr;
Password=[PASSWORD];
SSL Mode=Require;
Trust Server Certificate=false;
Connection Idle Lifetime=300;
Pooling=true;
```

## Architecture Notes

### Thread Safety

- Uses `AppDbContextFactory` with `IDesignTimeDbContextFactory` pattern
- Fresh context created per operation
- Connection pooling handled by Supabase pooler

### Retry Strategy

- Enabled retry on failure (max 3 retries)
- Retry delay: 5 seconds
- Handles transient Supabase connectivity issues

### Performance Optimization

- Connection pooling: Enabled
- Idle lifetime: 300 seconds
- Npgsql retry strategy conflict prevention via ExecutionStrategy

## Accessing Supabase Dashboard

1. Go to [Supabase Console](https://app.supabase.com)
2. Login with Supabase account
3. Project: Find MyShop project
4. View tables, data, and logs in real-time

## Local Development

### Switch Back to Local PostgreSQL

If you need to develop against local PostgreSQL:

```json
{
  "Database": {
    "Host": "localhost",
    "Database": "myshop",
    "Username": "nguyentienluat",
    "Password": "",
    "Port": 5432,
    "SslMode": "Disable"
  }
}
```

Then restart the application.

### Keep Supabase for Production

Current setup uses Supabase for production. Local development can use local PostgreSQL while keeping production on Supabase.

## Troubleshooting

### Connection Fails

- Check firewall: Supabase must be able to reach port 5432
- Verify credentials in `appsettings.json`
- Check Supabase project status in dashboard

### Slow Queries

- Enable query logging in Supabase console
- Check connection pool status
- Monitor `Connection Idle Lifetime` setting

### Migration Issues

- Backup file: `migration_backup.sql` available for re-import
- Drop command script in git history for table cleanup

## Security Recommendations

### For Production Deployment

1. **Do NOT** commit `appsettings.json` with real passwords
2. Use environment variables:
   ```bash
   export DB_HOST=aws-1-ap-northeast-1.pooler.supabase.com
   export DB_USER=postgres.oivqtyqiwschphohiblr
   export DB_PASSWORD=[PASSWORD]
   ```
3. Rotate credentials periodically
4. Enable Row Level Security (RLS) in Supabase for sensitive data

### Backup Strategy

- Supabase provides automated backups
- Point-in-time recovery available
- Keep `migration_backup.sql` in secure storage

## Related Documentation

- [Supabase Official Docs](https://supabase.com/docs)
- [Entity Framework Core + PostgreSQL](https://learn.microsoft.com/en-us/ef/core/providers/postgresql/)
- [Npgsql Connection Strings](https://www.npgsql.org/doc/connection-string-parameters.html)
- Local PostgreSQL setup: [SETUP_FOR_TEAM.md](./SETUP_FOR_TEAM.md)

## Rollback Plan

If you need to rollback to local PostgreSQL:

1. **Stop application**
2. **Update `appsettings.json`** to use local connection
3. **Restart application** - EF Core will manage schema
4. **Verify data integrity**

> Note: This would require local PostgreSQL to have data. Keep `migration_backup.sql` for this purpose.

---

**Migration Branch:** `feature/supabase-migration`  
**Main Branch:** Ready to merge after testing  
**Status:** ✅ Completed and verified

