using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.Scripts.Controllers;

public class AccountsController: IRequestController<AccountRegisterRequest>
{
    public AccountsController()
    {
        
    }
    public Task<IResult> HandleRequestAsync(AccountRegisterRequest request)
    {
        
    }
}