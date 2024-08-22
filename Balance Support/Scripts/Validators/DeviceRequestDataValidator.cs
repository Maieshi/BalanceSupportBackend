using FluentValidation;
namespace Balance_Support.Scripts.Validators;

public class DeviceRequestDataValidator: AbstractValidator<DeviceRequestData>
{
    public DeviceRequestDataValidator()
    {
        RuleFor(x=> x.UserRecordId).NotNull().NotEmpty();
        RuleFor(x=> x.DeviceRecordId).NotNull().NotEmpty();
        RuleFor(x=> x.DeviceInfo).NotEmpty().SetValidator(new DeviceInfoValidator());
    }
}