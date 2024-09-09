using Microsoft.EntityFrameworkCore;
using Balance_Support.DataClasses;
public class ApplicationDbContext : DbContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<UserToken> UserTokens { get; set; }

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // Configure User entity
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
        .HasMany(u => u.Accounts)
        .WithOne(a => a.User)
        .HasForeignKey(a => a.UserId);

    modelBuilder.Entity<User>()
        .HasMany(u => u.UserTokens)
        .WithOne(ut => ut.User)
        .HasForeignKey(ut => ut.UserId);

    // Configure Account entity
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
        .Property(a => a.Description)
        .HasMaxLength(500)
        .IsRequired(false); // Nullable property

    modelBuilder.Entity<Account>()
        .HasMany(a => a.Transactions)
        .WithOne(t => t.Account)
        .HasForeignKey(t => t.AccountId);

    // Configure Transaction entity
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

    // Configure UserToken entity
    modelBuilder.Entity<UserToken>()
        .Property(ut => ut.Id)
        .ValueGeneratedOnAdd(); // Ensure ID is generated on add

    modelBuilder.Entity<UserToken>()
        .Property(ut => ut.Token)
        .HasMaxLength(250);

    base.OnModelCreating(modelBuilder);
}


}
