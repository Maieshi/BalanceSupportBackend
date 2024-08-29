using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.Interfaces;

public interface IDatabaseAccountProvider
{
    public Task<IResult> RegisterAccount(DeviceRegisterRequest deviceRegisterRequest);
    public Task<IResult> UpdateAccount(DeviceUpdateRequest deviceUpdateRequest);
    public Task<IResult> DeleteDevice(DeviceDeleteRequest deviceDeleteRequest);
    public Task<IResult> GetAccountsByGroupAndDeviceId(DeviceGetRequest deviceGetRequest);
    public void Test();
}