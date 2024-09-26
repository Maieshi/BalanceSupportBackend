using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Newtonsoft.Json;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class Transaction : BaseEntity
{
    [ForeignKey("Account")]
    public string AccountId { get; set; }  // Foreign key to Account

    [ForeignKey("User")] // New foreign key to User
    public string UserId { get; set; }     // Foreign key to User

    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public DateTime Time { get; set; }
    public int TransactionType { get; set; }

    [StringLength(250)]
    public string Message { get; set; }
    [JsonIgnore]
    public Account Account { get; set; }  // Navigation property
    [JsonIgnore]
    public User User { get; set; }   
}
