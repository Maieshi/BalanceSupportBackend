using Balance_Support.DataClasses.Records.AccountData;
using FluentValidation;
namespace Balance_Support.DataClasses.Validators;

public class AccountGetAllNameNumberBankForUserRequestValidator: AbstractValidator<AccountGetAllNameNumberBankForUserRequest>
{
    public AccountGetAllNameNumberBankForUserRequestValidator()
    {
        RuleFor(x => x.userId).NotNull().NotEmpty();
    }
}