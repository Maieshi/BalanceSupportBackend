using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.Scripts.Database.Providers;
using Microsoft.EntityFrameworkCore;

namespace Balance_Support.Scripts.Database;
public class DbContextContainer:IDbSetContainer,ISaveDbChanges
{
    private readonly ApplicationDbContext context;
    private readonly Dictionary<Type, object> dbSets = new Dictionary<Type, object>();
    

    public DbContextContainer(ApplicationDbContext context)
    {
        this.context = context;
        // Add DbSets to the dictionary
        dbSets[typeof(User)] = context.Users;
        dbSets[typeof(Account)] = context.Accounts;
        dbSets[typeof(Transaction)] = context.Transactions;
        dbSets[typeof(UserToken)] = context.UserTokens;
        dbSets[typeof(UserSettings)] = context.UserSettings;
    }

    // Method to get a specific DbSet based on the entity type
    public DbSet<T> ReceiveSet<T>() where T : class
    {
        if (dbSets.TryGetValue(typeof(T), out var dbSet))
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

    public async Task<int> SaveChangesAsync()
        =>await context.SaveChangesAsync();
}