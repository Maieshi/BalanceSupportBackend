using FluentValidation;
using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceRegisterDataValidator: AbstractValidator<DeviceRegisterRequest>
{
    public DeviceRegisterDataValidator()
    {
        RuleFor(x=> x.UserId).NotNull().NotEmpty();
        // RuleFor(x=> x.DeviceRecordId).NotNull().NotEmpty();
        RuleFor(x=> x.DeviceData).NotEmpty().SetValidator(new DeviceRequestInfoValidator());
    }
}