namespace Balance_Support.Scripts.Providers;

public class FirebaseToSqlServerMigrator
{
    private readonly ApplicationDbContext context;

    public FirebaseToSqlServerMigrator(ApplicationDbContext context)
    {
        this.context = context;
    }
    
    public async Task Migrate()
    {
        // // Read the JSON file
        // var jsonString = File.ReadAllText("C:\\Projects\\Asp. Net\\Balance Support\\Balance Support\\Third-party configs\\balance-support-b9da3-default-rtdb-export (3).json");
        // var jsonData = JsonSerializer.Deserialize<JsonData>(jsonString);

        // Insert data into the database
        
            // Users
            // foreach (var userEntry in jsonData.Users)
            // {
            //     var user = userEntry.Value;
            //     context.Users.Add(new User()
            //     {
            //         Id = user.Id,
            //         Email = user.Email,
            //         DisplayName = user.DisplayName
            //     });
            // }
        
            // Accounts
            // foreach (var accountEntry in jsonData.Accounts)
            // {
            //     var account = accountEntry.Value;
            //     var searchId = jsonData.Relations
            //         .SelectMany(r => r.Value.Values) // Flatten to get RelationMigration objects only
            //         .FirstOrDefault(rm => rm.AccountId == account.AccountId);
            //     
            //     context.Accounts.Add(new Account()
            //     {
            //         Id = account.AccountId.ToString(),
            //         AccountNumber = account.AccountNumber,
            //         BankCardNumber = account.BankCardNumber,
            //         BankType = account.BankType,
            //         Description = account.Description,
            //         LastName = account.LastName,
            //         SimCardNumber = account.SimCardNumber,
            //         SimSlot = account.SimSlot,
            //         DeviceId = account.DeviceId,
            //         AccountGroup = account.AccountGroup,
            //         UserId = searchId.UserId
            //     });
            //     
            // }
            
            // UserTokens
            // foreach (var userTokenEntry in jsonData.UserTokens)
            // {
            //     var userToken = userTokenEntry.Value;
            //     context.UserTokens.Add(new UserToken(){UserId = userToken.UserId, Token = userToken.Token});
            // }

            // Devices
            // foreach (var deviceEntry in jsonData.Devices)
            // {
            //     var device = deviceEntry.Value;
            //     context.Users.Add();
            // }

            // Relations
            // foreach (var relationGroup in jsonData.Relations["User-Account"])
            // {
            //     var relation = relationGroup.Value;
            //     context.Relations.Add(relation);
            // }

            // Transactions
            // foreach (var transactionEntry in jsonData.Transactions)
            // {
            //     var transaction = transactionEntry.Value;
            //     context.Transactions.Add(new Transaction(){Id = Guid.NewGuid().ToString(), AccountId = transaction.AccountId.ToString(), Amount = transaction.Amount,Balance = transaction.Balance, Time = transaction.Time,TransactionType = transaction.TransactionType,Message = transaction.Message});
            // }
            // var user = await context.Users.FirstOrDefaultAsync(u => u.Id == "sDAmWae7RqMsmWIC74lVdLuQRpq1");
            // var accGet1 = await context.Accounts.FirstOrDefaultAsync(x=>x.Id=="aaaa");
            // var accGet = await context.Accounts.FirstOrDefaultAsync(x=>x.Id=="802fcd49-e45e-43a2-a025-63cc9d6036cd");
           
            // accGet.LastName = "Petriv";
            

            // var acc = new Account()
            // {
            //     Id = Guid.NewGuid().ToString(),
            //     AccountNumber = "aaaa",
            //     LastName = "aaaa",
            //     AccountGroup = 1,
            //     DeviceId = 1,
            //     SimSlot = 1,
            //     SimCardNumber = "aaaa",
            //     BankCardNumber = "aaaa",
            //     BankType = "aaaa",
            //     Description = "aaaa",
            //     UserId = "sDAmWae7RqMsmWIC74lVdLuQRpq1"
            // };
            //     var rec = context.Accounts.Add(acc);
        
            
        //  var a =   await context.SaveChangesAsync();
        // Console.WriteLine($"Data inserted successfully!{a}");
    }
}
public class JsonData
{
    public Dictionary<string, AccountMigration> Accounts { get; set; }
    public Dictionary<string, Dictionary<string, RelationMigration>> Relations { get; set; }
    public Dictionary<string, TransactionMigration> Transactions { get; set; }
    public Dictionary<string, UserTokenMigration> UserTokens { get; set; }
    public Dictionary<string, UserMigration> Users { get; set; }
}

public class AccountMigration
{
    public int AccountGroup { get; set; }
    public Guid AccountId { get; set; }
    public string AccountNumber { get; set; }
    public string BankCardNumber { get; set; }
    public string BankType { get; set; }
    public string Description { get; set; }
    public int DeviceId { get; set; }
    public string LastName { get; set; }
    public string SimCardNumber { get; set; }
    public int SimSlot { get; set; }
}


public class RelationMigration
{
    public Guid AccountId { get; set; }
    public string AccountRecordId { get; set; }
    public string UserId { get; set; }
}

public class TransactionMigration
{
    public Guid AccountId { get; set; }
    public decimal Amount { get; set; }
    public decimal Balance { get; set; }
    public string Message { get; set; }
    public DateTime Time { get; set; }
    public int TransactionType { get; set; }
}

public class UserMigration
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Email { get; set; }
}

public class UserTokenMigration
{
    public string Token { get; set; }
    public string UserId { get; set; }  // Foreign key
    
}