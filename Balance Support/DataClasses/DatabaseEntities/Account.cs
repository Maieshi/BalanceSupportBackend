using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Balance_Support.DataClasses.Records.AccountData;
using Newtonsoft.Json;

namespace Balance_Support.DataClasses.DatabaseEntities;

public class Account : BaseEntity
{
    [StringLength(50)] public string AccountNumber { get; set; }

    [StringLength(50)] public string LastName { get; set; }

    public int AccountGroup { get; set; }
    public int DeviceId { get; set; }
    public int SimSlot { get; set; }

    [StringLength(50)] public string SimCardNumber { get; set; }

    [StringLength(50)] public string BankCardNumber { get; set; }

    [StringLength(50)] public string BankType { get; set; }

    public decimal InitialBalance { get; set; }

    public decimal SmsBalance { get; set; }

    [StringLength(500)] public string? Description { get; set; } // Nullable property

    [JsonIgnore] public ICollection<Transaction> Transactions { get; set; } // Navigation property

    [ForeignKey("User")] public string UserId { get; set; } // Foreign key to User

    [JsonIgnore] public User User { get; set; } // Navigation property

    public void UpdateAccount(AccountUpdateRequest accountUpdateRequest)
    {
        AccountNumber = accountUpdateRequest.AccountData.AccountNumber;
        LastName = accountUpdateRequest.AccountData.LastName;
        AccountGroup = accountUpdateRequest.AccountData.AccountGroup;
        DeviceId = accountUpdateRequest.AccountData.DeviceId;
        SimSlot = accountUpdateRequest.AccountData.SimSlot;
        SimCardNumber = accountUpdateRequest.AccountData.SimCardNumber;
        BankCardNumber = accountUpdateRequest.AccountData.BankCardNumber;
        BankType = accountUpdateRequest.AccountData.BankType;
        InitialBalance = accountUpdateRequest.AccountData.InitialBalance;
        Description = accountUpdateRequest.AccountData.Description;
    }

    public override object Convert()
    {
        return new
        {
            UserId = UserId,
            Id = Id,
            AccountNumber = AccountNumber,
            LastName = LastName,
            AccountGroup = AccountGroup,
            DeviceId = DeviceId,
            SimSlot = SimSlot,
            SimCardNumber = SimCardNumber,
            BankCardNumber = BankCardNumber,
            BankType = BankType,
            InitialBalance = InitialBalance,
            SmsBalance = SmsBalance,
            Description = Description
        };
    }
}