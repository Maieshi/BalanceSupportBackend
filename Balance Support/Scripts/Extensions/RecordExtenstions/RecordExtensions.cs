using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Extensions.RecordExtenstions;
public static class RecordExtensions
{
    public static Account NewAccount(this AccountRegisterRequest request)
    {
        return new Account(){
            Id= Guid.NewGuid().ToString(), // Generate a new GUID as the AccountId
            UserId = request.UserId,
            AccountNumber= request.AccountData.AccountNumber,
            LastName= request.AccountData.LastName,
            AccountGroup= request.AccountData.AccountGroup,
            DeviceId= request.AccountData.DeviceId,
            SimSlot= request.AccountData.SimSlot,
            SimCardNumber= request.AccountData.SimCardNumber,
            BankCardNumber= request.AccountData.BankCardNumber,
            InitialBalance = (decimal)request.AccountData.InitialBalance,
            Description= request.AccountData.Description,
            BankType= request.AccountData.BankType
        };
    }
}