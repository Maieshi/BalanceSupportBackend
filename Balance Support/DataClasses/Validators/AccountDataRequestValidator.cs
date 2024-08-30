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
        RuleFor(x => x.SimCardNumber).NotEmpty().NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?\d{10,15}$").WithMessage("Phone number invalid format.");
        RuleFor(x => x.BankCardNumber).NotNull().NotEmpty();
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
    }
}