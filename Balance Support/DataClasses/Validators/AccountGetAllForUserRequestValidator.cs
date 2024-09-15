using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.DataClasses.Validators;
public class AccountGetAllForUserRequestValidator: AbstractValidator<AccountGetAllForUserRequest>
{
    public AccountGetAllForUserRequestValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}