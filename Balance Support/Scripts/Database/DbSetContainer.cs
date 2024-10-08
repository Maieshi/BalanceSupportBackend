using Balance_Support.DataClasses.DatabaseEntities;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;
public class DbSetContainer:IDbSetContainer
{
    private readonly Dictionary<Type, object> _dbSets = new Dictionary<Type, object>();

    public DbSetContainer(ApplicationDbContext context)
    {
        // Add DbSets to the dictionary
        _dbSets[typeof(User)] = context.Users;
        _dbSets[typeof(Account)] = context.Accounts;
        _dbSets[typeof(Transaction)] = context.Transactions;
        _dbSets[typeof(UserToken)] = context.UserTokens;
        _dbSets[typeof(UserSettings)] = context.UserSettings;
    }

    // Method to get a specific DbSet based on the entity type
    public DbSet<T> ReceiveSet<T>() where T : class
    {
        if (_dbSets.TryGetValue(typeof(T), out var dbSet))
        {
            // Cast the object back to DbSet<T> and ensure it's of the correct type
            if (dbSet is DbSet<T> typedDbSet)
            {
                return typedDbSet;
            }

            throw new InvalidOperationException($"Stored DbSet is not of the expected type {typeof(T).Name}.");
        }

        throw new InvalidOperationException($"DbSet for {typeof(T).Name} not found.");
    }
}