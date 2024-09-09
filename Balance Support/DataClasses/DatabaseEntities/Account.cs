using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Balance_Support.DataClasses;

public class Account : BaseEntity
{
    [StringLength(50)]
    public string AccountNumber { get; set; }

    [StringLength(50)]
    public string LastName { get; set; }

    public int AccountGroup { get; set; }
    public int SimSlot { get; set; }
    
    [StringLength(50)]
    public string SimCardNumber { get; set; }

    [StringLength(50)]
    public string BankCardNumber { get; set; }

    [StringLength(50)]
    public string BankType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; } // Nullable property

    public ICollection<Transaction> Transactions { get; set; } // Navigation property

    [ForeignKey("User")]
    public string UserId { get; set; }  // Foreign key to User

    public User User { get; set; } // Navigation property
}