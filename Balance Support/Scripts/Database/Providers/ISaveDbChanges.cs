namespace Balance_Support.Scripts.Database.Providers;

public interface ISaveDbChanges
{
    public Task<int> SaveChangesAsync();
}