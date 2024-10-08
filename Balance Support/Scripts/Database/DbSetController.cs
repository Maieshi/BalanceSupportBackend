using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;

public abstract class DbSetController<T>(IDbSetContainer container)
    where T : class
{
    protected DbSet<T> Table = container.ReceiveSet<T>();
}