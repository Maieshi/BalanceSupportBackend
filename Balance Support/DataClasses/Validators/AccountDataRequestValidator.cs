using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;

namespace Balance_Support.DataClasses.Validators;

public class AccountDataRequestValidator:AbstractValidator<AccountDataRequest>
{
    public AccountDataRequestValidator()
    {
        RuleFor(x => x.AccountNumber).NotNull().NotEmpty();
        RuleFor(x=>x.LastName).NotNull().NotEmpty();
        RuleFor(x => x.AccountGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceId).NotEmpty().GreaterThan(0);
        RuleFor(x => x.SimSlot).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
    }
}