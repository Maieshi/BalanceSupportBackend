using Balance_Support.DataClasses.Records.DeviceData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class DeviceUpdateRequestValidator : AbstractValidator<DeviceUpdateRequest>
{
    public DeviceUpdateRequestValidator()
    {
        RuleFor(x=> x.DeviceId).NotNull().NotEmpty();
        RuleFor(x=> x.DeviceData).NotNull().SetValidator(new DeviceDataValidator());
    }
}