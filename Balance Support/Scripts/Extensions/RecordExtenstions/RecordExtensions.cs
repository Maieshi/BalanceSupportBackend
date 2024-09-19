using Balance_Support.DataClasses;
using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.Scripts.Extensions.RecordExtenstions;


public static class RecordExtensions
{
    public static AccountData NewAccountData(this AccountDataRequest request)
    {
        return new AccountData(
            AccountId: Guid.NewGuid().ToString(), // Generate a new GUID as the AccountId
            AccountNumber: request.AccountNumber,
            LastName: request.LastName,
            AccountGroup: request.AccountGroup,
            DeviceId: request.DeviceId,
            SimSlot: request.SimSlot,
            SimCardNumber: request.SimCardNumber,
            BankCardNumber: request.BankCardNumber,
            Description: request.Description,
            BankType: request.BankType
        );
    }
    
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
            Description= request.AccountData.Description,
            BankType= request.AccountData.BankType
        };
    }
}