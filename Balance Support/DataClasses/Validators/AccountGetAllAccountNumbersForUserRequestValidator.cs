using Balance_Support.DataClasses.Records.AccountData;
using FluentValidation;
namespace Balance_Support.DataClasses.Validators;

public class AccountGetAllAccountNumbersForUserRequestValidator: AbstractValidator<AccountGetAllAccountNumbersForUserRequest>
{
    public AccountGetAllAccountNumbersForUserRequestValidator()
    {
        RuleFor(x => x.userId).NotNull().NotEmpty();
    }
}