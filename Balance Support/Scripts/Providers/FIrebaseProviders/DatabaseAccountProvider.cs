// using Firebase.Auth;

using System.Collections.ObjectModel;
using System.Diagnostics;
using Firebase.Auth;
// using FireSharp;
// using FireSharp.Interfaces;
// using FireSharp.Response;
// using FireSharp.Config;
using Firebase.Database;
using Firebase.Database.Query;
// using Google.Apis.Auth.OAuth2;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;
using Balance_Support.Interfaces;
using Balance_Support.Scripts.Extensions;
using Balance_Support.SerializationClasses;
using Balance_Support.DataClasses.Records.AccountData;
using Balance_Support.DataClasses.Records.NotificationData.DatabaseInfo;
using Balance_Support.Scripts.Extensions.RecordExtenstions;

namespace Balance_Support;

public class DatabaseAccountProvider : IDatabaseAccountProvider
{
    private FirebaseClient client;
    private IDatabaseUserProvider userProvider;

    public DatabaseAccountProvider(FirebaseClient client, IDatabaseUserProvider userProvider)
    {
        this.client = client;
        this.userProvider = userProvider;
    }

    public async Task<IResult> RegisterAccount(AccountRegisterRequest accountRegisterRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountRegisterRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");


        if (await IsAlreadyExistAccountWithGropAndDeviceId(accountRegisterRequest.UserId,
                accountRegisterRequest.AccountData.AccountGroup, accountRegisterRequest.AccountData.DeviceId,
                accountRegisterRequest.AccountData.SimSlot))
            return Results.Problem(statusCode: 500,
                title: "One account with same group and device id already registered");

        FirebaseObject<AccountData> userAccount = null;
        FirebaseObject<UserAccountRelationData> relaion = null;

        try
        {
            userAccount = await client.Child("Accounts").PostAsync(accountRegisterRequest.AccountData.NewAccountData());

            relaion = await RegisterRelations(accountRegisterRequest.UserId, userAccount);

            return Results.Created($"Accounts", userAccount.Object.AccountId);
        }
        catch (Exception ex)
        {
            if (userAccount != null)
            {
                await client
                    .Child("Devices")
                    .Child(userAccount.Key)
                    .DeleteAsync();
            }


            if (relaion != null)
            {
                await client
                    .Child("Relations")
                    .Child("User-Account")
                    .DeleteAsync();
            }

            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while registering device");
        }
    }

    public async Task<IResult> UpdateAccount(AccountUpdateRequest accountUpdateRequest)
    {
        try
        {
            var account = await FindAccountByAccountId(accountUpdateRequest.AccountId);
            if (account == null)
                return Results.Problem(statusCode: 500, title: "Account not found");

            if (account.Object.DeviceId != accountUpdateRequest.AccountDataRequest.DeviceId)
                return Results.BadRequest("Cannot change device id");

            if (await IsAlreadyExistAccountWithGropAndDeviceId(accountUpdateRequest.UserId,
                    accountUpdateRequest.AccountDataRequest.AccountGroup,
                    accountUpdateRequest.AccountDataRequest.DeviceId, accountUpdateRequest.AccountDataRequest.SimSlot))
                return Results.Problem(statusCode: 500,
                    title: "One account with same group and device id already registered");

            await client
                .Child("Devices")
                .Child(account.Key)
                .PutAsync(accountUpdateRequest.AccountDataRequest.NewAccountData());

            return Results.Ok($"Accounts/{accountUpdateRequest.AccountId}");
        }
        catch (Exception ex)
        {
            return Results.Problem(detail: ex.Message, statusCode: 500,
                title: "An error occurred while updating device ");
        }
    }


    public async Task<IResult> DeleteDevice(AccountDeleteRequest accountDeleteRequest)
    {
        try
        {
            var currentAccount = await FindAccountByAccountId(accountDeleteRequest.AccountId);
            if (currentAccount == null)
                return Results.Problem(statusCode: 500, title: "Account not found");

            var relations = await FindRelationByAccountId(accountDeleteRequest.AccountId);
            if (!relations.Any())
                return Results.Problem(statusCode: 500, title: "Relation not found");

            await client
                .Child("Accounts")
                .Child(currentAccount.Key)
                .DeleteAsync();

            await client
                .Child("Relations")
                .Child("User-Account")
                .Child(currentAccount.Key)
                .DeleteAsync();


            return Results.Ok($"Devices/{accountDeleteRequest.AccountId}");
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return Results.Problem(statusCode: 500, title: "Cannot delete account");
        }
    }

    public async Task<IResult> GetAccountsForDevice(AccountGetRequest accountGetRequest)
    {
        if (!await userProvider.IsUserWithIdExist(accountGetRequest.UserId))
            return Results.Problem(statusCode: 500, title: "User not found");

        var relations = (await FindRelationByUserId(accountGetRequest.UserId));

        if (!relations.Any())
            return Results.Problem(statusCode: 500, title: "Relations not found");

        var accounts = (await FindAccountsByUserId(accountGetRequest.UserId))
            .Where(x =>
                x.Object.AccountGroup == accountGetRequest.AccountGroup
                && x.Object.DeviceId == accountGetRequest.DeviceId).ToList();

        if (accounts.Any())
            return Results.Problem(statusCode: 500, title: "Accounts not found");


        return Results.Ok(new
        {
            Accounts = accounts.Select(x => x.Object)
        });
    }

    public async void Test()
    {
        var result = await RegisterAccount(
            new AccountRegisterRequest(
                "sDAmWae7RqMsmWIC74lVdLuQRpq1",
                new AccountDataRequest(
                    "123456789",
                    "Ivaniv",
                    3,
                    3,
                    1,
                    "+88005553535",
                    "1488",
                    "SberBank",
                    "Very rich person"
                )));
    }

    private async Task<FirebaseObject<UserAccountRelationData>> RegisterRelations(string userId,
        FirebaseObject<AccountData> account)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .PostAsync(new UserAccountRelationData(
                userId,
                account.Object.AccountId,
                account.Key));


    private async Task<bool> IsAccountExists(string accountId)
        => await FindAccount(accountId) != null;

    private async Task<AccountData?> FindAccount(string accountData)
    {
        var deviceInfo = await FindAccountByAccountId(accountData);
        if (deviceInfo != default)
            return deviceInfo.Object;

        return default;

        // deviceInfo = (await FindAccountByRecordId(accountData)==default)?;
        // return deviceInfo?.Object;
    }


    private async Task<AccountData> FindAccountByRecordId(string recordId)
    {
        var a1 = await client
            .Child("Accounts")
            .Child(recordId)
            .OnceSingleAsync<AccountData>();

        return a1;
    }


    public async Task<FirebaseObject<AccountData>?> FindAccountByAccountId(string accountId)
        => (await client
            .Child("Accounts")
            .OrderBy("AccountId")
            .EqualTo(accountId)
            .OnceAsync<AccountData>()).FirstOrDefault();


    private async Task<IReadOnlyCollection<FirebaseObject<UserAccountRelationData>>>
        FindRelationByAccountId(string deviceId)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .OrderBy("AccountId")
            .EqualTo(deviceId)
            .OnceAsync<UserAccountRelationData>();


    private async Task<IReadOnlyCollection<FirebaseObject<UserAccountRelationData>>>
        FindRelationByUserId(string userId)
        => await client
            .Child("Relations")
            .Child("User-Account")
            .OrderBy("UserId")
            .EqualTo(userId)
            .OnceAsync<UserAccountRelationData>();


    private async Task<ReadOnlyCollection<FirebaseObject<AccountData>>> FindAccountsByUserId(string userId)
        => (await Task.WhenAll(
                    (await FindRelationByUserId(userId))
                    .Select(r => r.Object.AccountId)
                    .Where(id => !string.IsNullOrEmpty(id))
                    .Select(FindAccountByAccountId)
                )
            )
            .Where(device => device != null)
            .Cast<FirebaseObject<AccountData>>() // Explicitly cast to non-nullable type
            .ToList()
            .AsReadOnly();

    public async Task<FirebaseObject<AccountData>?> GetAccountByUserIdAndBankCardNumber(string userId, string bankCardNumber)
        => (await FindAccountsByUserId(userId))
            .FirstOrDefault(x =>
                string.Equals(x.Object.BankCardNumber, bankCardNumber));

    private async Task<bool> IsAlreadyExistAccountWithGropAndDeviceId(string userId, int accountGroup,
        int deviceId, int simSlot)
    {
        return (await FindAccountsByUserId(userId)).Any(device =>
            device.Object.AccountGroup == accountGroup && device.Object.DeviceId == deviceId &&
            device.Object.SimSlot == simSlot);
    }
}