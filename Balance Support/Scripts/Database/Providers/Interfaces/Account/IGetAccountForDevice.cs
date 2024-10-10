namespace Balance_Support.Scripts.Database.Providers.Interfaces.Account;

public interface IGetAccountForDevice
{
    public  Task<List<DataClasses.DatabaseEntities.Account>> GetAccoutForDeivce(string userId, int accountGroup, int deviceId);
}