using Microsoft.EntityFrameworkCore;
using Balance_Support.DataClasses;
using Balance_Support.DataClasses.DatabaseEntities;
using Microsoft.AspNetCore.DataProtection.EntityFrameworkCore;

public class ApplicationDbContext : DbContext,IDataProtectionKeyContext
{
    public virtual DbSet<User> Users { get; set; }
    public virtual DbSet<Account> Accounts { get; set; }
    public virtual DbSet<Transaction> Transactions { get; set; }
    public virtual DbSet<UserToken> UserTokens { get; set; }
    public virtual DbSet<UserSettings> UserSettings { get; set; }

    public DbSet<DataProtectionKey> DataProtectionKeys { get; set; }
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

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

        modelBuilder.Entity<User>()
            .HasMany(u => u.UserTokens)
            .WithOne(ut => ut.User)
            .HasForeignKey(ut => ut.UserId);

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

        modelBuilder.Entity<UserSettings>().Property(us => us.SelectedGroup)
            .HasDefaultValue(1);


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
            .Property(a => a.Description)
            .HasMaxLength(500)
            .IsRequired(false); // Nullable property

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

        #region UserToken

        modelBuilder.Entity<UserToken>()
            .Property(ut => ut.Id)
            .ValueGeneratedOnAdd(); // Ensure ID is generated on add

        modelBuilder.Entity<UserToken>()
            .Property(ut => ut.Token)
            .HasMaxLength(250);

        #endregion

        base.OnModelCreating(modelBuilder);
    }

}

public class UserDto
{
    public string Id { get; set; }
    public string Email { get; set; }
    public string DisplayName { get; set; }
    // Add other properties as needed

    public static List<UserDto> CreateDtos(List<User> users)
    {
        return users.Select(u => new UserDto
        {
            Id = u.Id,
            Email = u.Email,
            DisplayName = u.DisplayName
            // Map other properties as needed
        }).ToList();
    }
}

//TODO:remove dtos, create IDtoConvertable to each entity and with this format implementation: new{Id = entity.Id,...}, create for dbset and list of extension that converts it to list dtos   
public class AccountDto
{
    public string Id { get; set; }
    public string UserId { get; set; }
    public string AccountNumber { get; set; }
    public string LastName { get; set; }
    public int AccountGroup { get; set; }
    public int DeviceId { get; set; }
    public int SimSlot { get; set; }
    public string SimCardNumber { get; set; }
    public string BankCardNumber { get; set; }
    public string BankType { get; set; }
    public string Description { get; set; }

    public AccountDto(Account account)
    {
        UserId = account.UserId;
        Id = account.Id;
        AccountNumber = account.AccountNumber;
        LastName = account.LastName;
        AccountGroup = account.AccountGroup;
        DeviceId = account.DeviceId;
        SimSlot = account.SimSlot;
        SimCardNumber = account.SimCardNumber;
        BankCardNumber = account.BankCardNumber;
        BankType = account.BankType;
        Description = account.Description;
    }


    public static List<AccountDto> CreateDtos(List<Account> accounts)
        => accounts.Select(account => new AccountDto(account)).ToList();
}

public class TransactionDto
{
    public string Id { get; set; }
    public string AccountId { get; set; } // Foreign key to Account
    public string UserId { get; set; } // Foreign key to User
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public DateTime Time { get; set; }
    public int TransactionType { get; set; }
    public string Message { get; set; }

    // Add other properties as needed

    public TransactionDto(Transaction t)
    {
        
        Id = t.Id;
        UserId = t.UserId;
        AccountId = t.AccountId;
        Amount = t.Amount;
        Balance = t.Balance;
        Time = t.Time;
        TransactionType = t.TransactionType;
        Message = t.Message;
    }

    public static List<TransactionDto> CreateDtos(List<Transaction> transactions)
    {
        return transactions.Select(t => new TransactionDto(t)).ToList();
    }
}

public class UserTokenDto
{
    public string Id { get; set; }
    public string UserId { get; set; } // Foreign key to User
    public string Token { get; set; }
    // Add other properties as needed

    public static List<UserTokenDto> CreateDtos(List<UserToken> userTokens)
    {
        return userTokens.Select(ut => new UserTokenDto
        {
            Id = ut.Id,
            UserId = ut.UserId,
            Token = ut.Token
            // Map other properties as needed
        }).ToList();
    }
}

public class UserSettingsDto
{
    public string Id { get; set; }
    public string UserId { get; set; } // Foreign key to User
    public int SelectedGroup { get; set; }
    public int RowCount { get; set; }
    // Add other properties as needed

    public static List<UserSettingsDto> CreateDtos(List<UserSettings> userSettings)
    {
        return userSettings.Select(us => new UserSettingsDto
        {
            Id = us.Id,
            UserId = us.UserId, // Foreign key to User
            SelectedGroup = us.SelectedGroup,
            RowCount = us.RowsCount
            // Map other properties as needed
        }).ToList();
    }
}