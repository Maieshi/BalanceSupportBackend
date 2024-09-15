using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.DataClasses.Validators;

public class AccountGetForDeviceRequestValidator: AbstractValidator<AccountGetForDeviceRequest>
{
    public AccountGetForDeviceRequestValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.AccountGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceId).NotEmpty().GreaterThan(0);
    }
}