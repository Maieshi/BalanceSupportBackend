using FluentValidation;
using Balance_Support.DataClasses.Records.DeviceData;

namespace Balance_Support.DataClasses.Validators;

public class DeviceDataValidator:AbstractValidator<DeviceData>
{
    public DeviceDataValidator()
    {
        RuleFor(x => x.DeviceId).NotNull().NotEmpty();
        RuleFor(x => x.DeviceGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceSubgroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
    }
}