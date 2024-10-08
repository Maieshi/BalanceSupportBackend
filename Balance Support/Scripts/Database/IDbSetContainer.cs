using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;

public interface IDbSetContainer
{
    public DbSet<T> ReceiveSet<T>()where T: class;
}