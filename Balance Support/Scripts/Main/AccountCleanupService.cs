using System;
using System.Threading;
using System.Threading.Tasks;
using Balance_Support.Scripts.Database;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

public class AccountCleanupService : IHostedService, IDisposable
{
    private readonly ILogger<AccountCleanupService> _logger;
    private Timer? _timer;
    private ApplicationDbContext _dbContext;

    public AccountCleanupService(ApplicationDbContext dbContext, ILogger<AccountCleanupService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Account Cleanup Service is starting.");

        ScheduleMidnightTask();

        return Task.CompletedTask;
    }

    private void ScheduleMidnightTask()
    {
        var now = DateTime.UtcNow.AddHours(3); // Moscow Time (UTC+3)
        var midnight = now.Date.AddDays(1); // Next midnight
        var initialDelay = midnight - now;  // Time until next midnight

        _timer = new Timer(
            async _ => await ExecuteCleanupAsync(), // TimerCallback delegate
            null,
            initialDelay,
            TimeSpan.FromDays(1)); // Schedule daily

        _logger.LogInformation($"Next cleanup scheduled at {midnight:yyyy-MM-dd HH:mm:ss} Moscow Time.");
    }
    
    private async Task ExecuteCleanupAsync()
    {
        try
        {
            await CleanupExpiredAccountsAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during the scheduled cleanup.");
        }
    }

    public async Task CleanupExpiredAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Starting account cleanup...");

            var thresholdDate = DateTime.UtcNow.AddDays(-30); // Example: delete accounts marked as deleted over 30 days ago

            // Fetch accounts marked as deleted and older than the threshold date
            var accountsToDelete = await _dbContext.Accounts
                .Where(x => x.IsDeleted && x.DeletedAt <= thresholdDate)
                .ToListAsync();

            if (accountsToDelete.Any())
            {
                _logger.LogInformation($"Found {accountsToDelete.Count} accounts to delete.");

                _dbContext.Accounts.RemoveRange(accountsToDelete); // Remove accounts from the DbSet
                await _dbContext.SaveChangesAsync(); // Save changes to the database

                _logger.LogInformation("Account cleanup completed successfully.");
            }
            else
            {
                _logger.LogInformation("No accounts found for cleanup.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An error occurred during account cleanup.");
        }
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Account Cleanup Service is stopping.");
        _timer?.Change(Timeout.Infinite, 0);
        return Task.CompletedTask;
    }

    public void Dispose()
    {
        _timer?.Dispose();
    }
}
