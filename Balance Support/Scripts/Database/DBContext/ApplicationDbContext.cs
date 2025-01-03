using Balance_Support.DataClasses.DatabaseEntities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Balance_Support.Scripts.Database;

public class ApplicationDbContext : DbContext, IDataProtectionKeyContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<UserSettings> UserSettings { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        #region User

        modelBuilder.Entity<User>()
            .Property(u => u.Id)
            .ValueGeneratedOnAdd(); // Ensure ID is generated on add

        modelBuilder.Entity<User>()
            .Property(u => u.DisplayName)
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .Property(u => u.Email)
            .HasMaxLength(50);

        modelBuilder.Entity<User>()
            .HasOne(u => u.UserSettings)
            .WithOne(us => us.User)
            .HasForeignKey<UserSettings>(us => us.UserId);

        modelBuilder.Entity<User>()
            .HasMany(u => u.Accounts)
            .WithOne(a => a.User)
            .HasForeignKey(a => a.UserId);

        #endregion

        #region UserSettings

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.Id)
            .ValueGeneratedOnAdd(); // Ensure ID is generated on add

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.UserName)
            .IsRequired(false)
            .HasDefaultValue(string.Empty); // Set a default value if needed

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.Nickname)
            .IsRequired(false)
            .HasDefaultValue(string.Empty); // Set a default value if needed

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.PhoneNumber)
            .IsRequired(false)
            .HasDefaultValue(100); // Set a default value if needed

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.Address)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.Country)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.About)
            .IsRequired(false)
            .HasDefaultValue(string.Empty);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.CommentsOnArticle)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.AnswersOnForm)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.OnFollower)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.NewsAnnouncements)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.ProductUpdates)
            .HasDefaultValue(false);


        modelBuilder.Entity<UserSettings>()
            .Property(us => us.BlogDigest)
            .HasDefaultValue(false);

        modelBuilder.Entity<UserSettings>()
            .Property(us => us.SelectedGroups)
            .HasColumnType("nvarchar(max)")
            .HasConversion(
                v => JsonConvert.SerializeObject(v),  // Convert list to JSON string for storage
                v => JsonConvert.DeserializeObject<List<int>>(v) ?? new List<int>() // Convert JSON string back to list
            );


        modelBuilder.Entity<UserSettings>()
            .Property(us => us.RowsCount)
            .HasDefaultValue(100);

        #endregion

        #region Account

        modelBuilder.Entity<Account>()
            .Property(a => a.Id)
            .ValueGeneratedOnAdd(); // Ensure ID is generated on add

        modelBuilder.Entity<Account>()
            .Property(a => a.AccountNumber)
            .HasMaxLength(50);

        modelBuilder.Entity<Account>()
            .Property(a => a.LastName)
            .HasMaxLength(50);

        modelBuilder.Entity<Account>()
            .Property(t => t.InitialBalance)
            .HasColumnType("money");

        modelBuilder.Entity<Account>()
            .Property(a => a.Description)
            .HasMaxLength(500)
            .IsRequired(false); // Nullable property

        modelBuilder.Entity<Account>()
            .Property(a => a.IsDeleted)
            .HasDefaultValue(false);
        
        modelBuilder.Entity<Account>()
            .Property(t => t.DeletedAt)
            .HasColumnType("datetime");

        modelBuilder.Entity<Account>()
            .HasMany(a => a.Transactions)
            .WithOne(t => t.Account)
            .HasForeignKey(t => t.AccountId);

        
        
        #endregion

        #region Transaction

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Id)
            .ValueGeneratedOnAdd(); // Ensure ID is generated on add

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("money");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Balance)
            .HasColumnType("money");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Time)
            .HasColumnType("datetime");

        modelBuilder.Entity<Transaction>()
            .Property(t => t.Message)
            .HasMaxLength(250);

        // Configure foreign key to User
        modelBuilder.Entity<Transaction>()
            .HasOne(t => t.User)
            .WithMany(u => u.Transactions) // Assuming User can have many Transactions
            .HasForeignKey(t => t.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        #endregion

        base.OnModelCreating(modelBuilder);
    }
}

