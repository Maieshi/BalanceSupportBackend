using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.Interfaces;

public interface IDatabaseDeviceProvider
{
    public Task<IResult> RegisterDevice(DeviceRegisterRequest deviceRegisterRequest);
    public Task<IResult> UpdateDeviceData(DeviceUpdateRequest deviceRequestRequest);
    public Task<IResult> DeleteDeviceData(DeviceDeleteRequest deviceDeleteRequest);
    
    public void Test();
}