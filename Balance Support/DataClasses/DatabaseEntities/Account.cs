using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Balance_Support.DataClasses.Records.AccountData;
using Newtonsoft.Json;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class Account : BaseEntity
{
    [StringLength(50)]
    public string AccountNumber { get; set; }

    [StringLength(50)]
    public string LastName { get; set; }

    public int AccountGroup { get; set; }
    public int DeviceId { get; set; }
    public int SimSlot { get; set; }
    
    [StringLength(50)]
    public string SimCardNumber { get; set; }

    [StringLength(50)]
    public string BankCardNumber { get; set; }

    [StringLength(50)]
    public string BankType { get; set; }

    [StringLength(500)]
    public string? Description { get; set; } // Nullable property

    [JsonIgnore]
    public ICollection<Transaction> Transactions { get; set; } // Navigation property

    [ForeignKey("User")]
    public string UserId { get; set; }  // Foreign key to User
    
    [JsonIgnore]
    public User User { get; set; } // Navigation property

    public void UpdateAccount(AccountUpdateRequest accountUpdateRequest)
    {
        AccountNumber = accountUpdateRequest.AccountDataRequest.AccountNumber;
        LastName = accountUpdateRequest.AccountDataRequest.LastName;
        AccountGroup = accountUpdateRequest.AccountDataRequest.AccountGroup;
        DeviceId = accountUpdateRequest.AccountDataRequest.DeviceId;
        SimSlot = accountUpdateRequest.AccountDataRequest.SimSlot;
        SimCardNumber = accountUpdateRequest.AccountDataRequest.SimCardNumber;
        BankCardNumber = accountUpdateRequest.AccountDataRequest.BankCardNumber;
        BankType = accountUpdateRequest.AccountDataRequest.BankType;
        Description = accountUpdateRequest.AccountDataRequest.Description;
    }
}