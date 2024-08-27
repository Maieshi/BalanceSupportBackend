using FluentValidation;
using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceGetRequestvValidator: AbstractValidator<DeviceGetRequest>
{
    public DeviceGetRequestvValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.DeviceGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceSubgroup).NotEmpty().GreaterThan(0);
    }
}