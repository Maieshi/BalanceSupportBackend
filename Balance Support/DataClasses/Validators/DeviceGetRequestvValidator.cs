using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceGetRequestvValidator: AbstractValidator<DeviceGetRequest>
{
    public DeviceGetRequestvValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.AccountGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceId).NotEmpty().GreaterThan(0);
    }
}