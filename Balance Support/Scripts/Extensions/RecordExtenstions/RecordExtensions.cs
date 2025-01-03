using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Extensions.RecordExtenstions;
public static class RecordExtensions
{
    public static Account NewAccount(this AccountRegisterRequest request)
    {
       var acc =  new Account(){
            Id= Guid.NewGuid().ToString(), // Generate a new GUID as the AccountId
            UserId = request.UserId,
            AccountNumber= request.AccountData.AccountNumber,
            LastName= request.AccountData.LastName,
            AccountGroup= request.AccountData.AccountGroup,
            DeviceId= request.AccountData.DeviceId,
            SimSlot= request.AccountData.SimSlot,
            SimCardNumber= request.AccountData.SimCardNumber,
            BankCardNumber= request.AccountData.BankCardNumber,
            InitialBalance = request.AccountData.InitialBalance,
            SmsBalance = 0,
            Description= request.AccountData.Description,
            BankType= request.AccountData.BankType
            
        };
        if (request.AccountData.InitialSmsBalance.HasValue) // Check if not null
        {
            acc.SmsBalance = request.AccountData.InitialSmsBalance.Value; // Set SmsBalance to the value of InitialSmsBalance
        }
        return acc;
    }
}