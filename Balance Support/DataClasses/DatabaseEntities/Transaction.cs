using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace Balance_Support.DataClasses;

public class Transaction : BaseEntity
{
    [ForeignKey("Account")]
    public string AccountId { get; set; }  // Foreign key to Account

    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public DateTime Time { get; set; }
    public int TransactionType { get; set; }

    [StringLength(250)]
    public string Message { get; set; }

    public Account Account { get; set; } // Navigation property
}
