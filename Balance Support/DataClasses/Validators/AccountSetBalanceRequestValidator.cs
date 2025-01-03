using Balance_Support.DataClasses.Records.AccountData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class AccountSetBalanceRequestValidator: AbstractValidator<AccountSetBalanceRequest>
{
    public AccountSetBalanceRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty().NotNull();
        RuleFor(x => x.Balance).NotNull();
    }
}
