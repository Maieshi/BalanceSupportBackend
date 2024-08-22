namespace Balance_Support.Interfaces;

public interface IDatabaseDeviceProvider
{
    public Task<IResult> RegisterDevice(DeviceRequestData deviceRequestData);
    public Task<IResult> UpdateDeviceData(DeviceRequestData deviceRequestData);
    public Task<IResult> DeleteDeviceData(DeviceDeleteData deviceDeleteData);
}