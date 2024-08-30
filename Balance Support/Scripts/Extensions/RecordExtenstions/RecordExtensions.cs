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
}