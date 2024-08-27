using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.Interfaces;

public interface IDatabaseDeviceProvider
{
    public Task<IResult> RegisterDevice(DeviceRegisterRequest deviceRegisterRequest);
    public Task<IResult> UpdateDevice(DeviceUpdateRequest deviceRequestRequest);
    public Task<IResult> DeleteDevice(DeviceDeleteRequest deviceDeleteRequest);
    
    public  Task<string> GetBankBySimCardId(string simCardId);
    
    public void Test();
}