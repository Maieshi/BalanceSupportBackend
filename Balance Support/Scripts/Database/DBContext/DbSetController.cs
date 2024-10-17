using Balance_Support.Scripts.Database.Providers;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;

public abstract class DbSetController<T>(IDbSetContainer container,ISaveDbChanges saver)
    where T : class
{
    protected readonly DbSet<T> Table   = container.ReceiveSet<T>();
    protected readonly ISaveDbChanges Saver = saver;
}