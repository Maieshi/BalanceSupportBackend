using System.ComponentModel.DataAnnotations;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class User : BaseEntity
{
    [StringLength(50)] public string DisplayName { get; set; }

    [StringLength(50)] public string Email { get; set; }

    public ICollection<Account> Accounts { get; set; } // Navigation property
    public ICollection<UserToken> UserTokens { get; set; } // Navigation property

    public UserSettings UserSettings { get; set; }
    public ICollection<Transaction> Transactions { get; set; }
}