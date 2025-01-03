using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;

public class ProtectionKeysDbContext: DbContext, IDataProtectionKeyContext
{
    public ProtectionKeysDbContext(DbContextOptions<ProtectionKeysDbContext> options)
        : base(options)
    {
    }
    public DbSet<DataProtectionKey> DataProtectionKeys { get; }
}