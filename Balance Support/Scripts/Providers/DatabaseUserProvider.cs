using Balance_Support.Scripts.Providers.Interfaces;
using Microsoft.EntityFrameworkCore;
using User = Balance_Support.DataClasses.DatabaseEntities.User;

namespace Balance_Support.Scripts.Providers;

public class DatabaseUserProvider : IDatabaseUserProvider
{
    // private FirebaseClient client;
    private readonly ApplicationDbContext context;

    public DatabaseUserProvider(ApplicationDbContext context)
    {
        // this.client = client;
        this.context = context;
    }

    public async Task<(bool IsSuccess, string? ErrorMessage)> CreateUserAsync(User newUser)
    {
        try
        {
            // Add the new user to the DbContext
            context.Users.Add(newUser);

            // Save the changes to the database
            await context.SaveChangesAsync();

            // Indicate success
            return (true, null); // No error message needed on success
        }
        catch (DbUpdateException ex)
        {
            // Handle database-related errors
            return (false, $"Error while saving user: {ex.Message}");
        }
        catch (Exception ex)
        {
            // Handle any general errors
            return (false, $"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<User?> GetUser(string userCred)
    => await context.Users
            .Where(u => u.Email == userCred || u.DisplayName == userCred || u.Id == userCred)
            .FirstOrDefaultAsync();
    
    public async Task<bool> IsEmailAlreadyRegistered(string email)
    => await FindUserByEmail(email) != null;
    
    public async Task<bool> IsUserWithIdExist(string userId)
    => await FindUserById(userId) != null;

    public async Task<bool> IsUserWithUsernameExist(string userName)
    =>await FindUserByUsername(userName) != null;
    

    #region Private

    private async Task<User?> FindUser(User user)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Id == user.Id || u.Email == user.Email || u.DisplayName == user.DisplayName);

    private async Task<User?> FindUserByEmail(string email)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Email == email);

    private async Task<User?> FindUserById(string id)
        => await context.Users
            .FirstOrDefaultAsync(u => u.Id == id);

    private async Task<User?> FindUserByUsername(string displayName)
        => await context.Users
            .FirstOrDefaultAsync(u => u.DisplayName == displayName);

    #endregion
}