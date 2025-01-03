using Balance_Support.DataClasses.Records.UserData;
using Balance_Support.Scripts.Database.Providers.Interfaces.User;
using Microsoft.EntityFrameworkCore;
using User = Balance_Support.DataClasses.DatabaseEntities.User;

namespace Balance_Support.Scripts.Database.Providers;

public class DatabaseUserProvider :  DbSetController<User>, IDatabaseUserProvider
{
    // private FirebaseClient client;
    // private readonly ApplicationDbContext context;

    #region Public
    
    public DatabaseUserProvider(IDbSetContainer container,ISaveDbChanges saver) : base(container,saver)
    {
    }
    // public DatabaseUserProvider()
    // {
    //     // this.client = client;
    //     this.context = context;
    // }

    public async Task RegisterUser(string userId, string email,string displayName)
    {
        var user = new User() { Id = userId, Email = email, DisplayName = displayName };
        
        Table.Add(user);

        // Save the changes to the database
        await Saver.SaveChangesAsync();

        // try
        // {
        //     // Add the new user to the DbContext
        //     context.Users.Add(newUser);
        //
        //     // Save the changes to the database
        //     await context.SaveChangesAsync();
        //
        //     // Indicate success
        //     return (true, null); // No error message needed on success
        // }
        // catch (DbUpdateException ex)
        // {
        //     // Handle database-related errors
        //     return (false, $"Error while saving user: {ex.Message}");
        // }
        // catch (Exception ex)
        // {
        //     
        //     // Handle any general errors
        //     return (false, $"An unexpected error occurred: {ex.Message}");
        // }
    }

    public async Task<User?> GetUser(string userCred)
    => await Table
            .Where(u => u.Email == userCred || u.DisplayName == userCred || u.Id == userCred)
            .FirstOrDefaultAsync();
    
    public async Task<bool> CheckUserWithEmailExist(string email)
    => await FindUserByEmail(email) != null;
    
    public async Task<bool> CheckUserWithIdExist(string userId)
    => await FindUserById(userId) != null;

    public async Task<bool> CheckUserWithUsernameExist(string userName)
    =>await FindUserByUsername(userName) != null;
    
    #endregion
    
    #region Private

    // private async Task<User?> FindUser(User user)
    //     => await Table
    //         .FirstOrDefaultAsync(u => u.Id == user.Id || u.Email == user.Email || u.DisplayName == user.DisplayName);

    private async Task<User?> FindUserByEmail(string email)
        => await Table
            .FirstOrDefaultAsync(u => u.Email == email);

    private async Task<User?> FindUserById(string id)
        => await Table
            .FirstOrDefaultAsync(u => u.Id == id);

    private async Task<User?> FindUserByUsername(string displayName)
        => await Table
            .FirstOrDefaultAsync(u => u.DisplayName == displayName);

    #endregion
}

