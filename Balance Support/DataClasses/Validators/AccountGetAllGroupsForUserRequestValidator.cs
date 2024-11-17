using Balance_Support.DataClasses.Records.AccountData;
using FluentValidation;
namespace Balance_Support.DataClasses.Validators;

public class AccountGetAllGroupsForUserRequestValidator: AbstractValidator<AccountGetAllGroupsForUserRequest>
{
    public AccountGetAllGroupsForUserRequestValidator()
    {
        RuleFor(x => x.userId).NotNull().NotEmpty();
    }
}