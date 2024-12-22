using Balance_Support.DataClasses.DatabaseEntities;
using Balance_Support.DataClasses.Records.NotificationData;
using Balance_Support.Scripts.Database.Providers.Interfaces;
using Balance_Support.Scripts.Database.Providers.Interfaces.Transaction;
using Balance_Support.Scripts.Parsing;
using FirebaseAdmin.Messaging;
using Microsoft.EntityFrameworkCore;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;

// using Google.Apis.Auth.OAuth2;

namespace Balance_Support.Scripts.Database.Providers;

public class DatabaseTransactionProvider :DbSetController<Transaction>, IDatabaseTransactionProvider
{
    public DatabaseTransactionProvider(IDbSetContainer container, ISaveDbChanges saver):base(container, saver)
    {
       
    }

    public async Task<Transaction> Register(
        string userId,
        string accountId,
        TransactionType transactionType,
        decimal amount,
        decimal balance,
        string message
    )
    {
       
        var newId = Guid.NewGuid().ToString();
        var mskTime = DateTime.UtcNow.AddHours(3);  // Moscow is 3 hours ahead of UTC
        
        var transactionData = new Transaction
        {
            Id = newId,
            AccountId = accountId,
            UserId = userId,
            TransactionType = (int)transactionType,
            Amount = amount,
            Balance = balance,
            Message = message,
            Time = DateTime.SpecifyKind(mskTime,DateTimeKind.Utc)
        };
        Table.Add(transactionData);
        await Saver.SaveChangesAsync();
        return transactionData;
       
    }


    public async Task<List<Transaction>> GetMessages(MessagesGetRequest messagesGetRequest,List<string> accountIds)
    {
        List<Transaction> check = new List<Transaction>();
        var query = Table.AsQueryable();
        query = query.Where(x => x.UserId == messagesGetRequest.UserId);
        check = await query.ToListAsync();

        query = query.Where(x => accountIds.Contains(x.AccountId));
        check = await query.ToListAsync();
    
        if (!string.IsNullOrEmpty(messagesGetRequest.SearchText))
        {
            query = query.Where(t => t.Message.Contains(messagesGetRequest.SearchText));
            check = await query.ToListAsync();
        }

        if (messagesGetRequest.StartingDate.HasValue)
        {
            query = query.Where(t => t.Time >= messagesGetRequest.StartingDate.Value);
            check = await query.ToListAsync();
        }

        if (messagesGetRequest.EndingDate.HasValue)
        {
            query = query.Where(t => t.Time <= messagesGetRequest.EndingDate.Value);
            check = await query.ToListAsync();
        }

        if (messagesGetRequest.MessageType.HasValue&&messagesGetRequest.MessageType.Value!=-1)
        {
            query = query.Where(t => t.TransactionType == messagesGetRequest.MessageType.Value);
            check = await query.ToListAsync();
        }


        return await query
            .ToListAsync();
        
    }

    public async Task<List<Transaction>> GetTransactionsByAccountId(string accountId)
        => await Table.Where(x => x.AccountId == accountId).ToListAsync();

    
}