using FluentValidation;
using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceRegisterRequestValidator: AbstractValidator<DeviceRegisterRequest>
{
    public DeviceRegisterRequestValidator()
    {
        RuleFor(x=> x.UserId).NotNull().NotEmpty();
        RuleFor(x=> x.DeviceData).NotNull().SetValidator(new DeviceDataValidator());
        RuleForEach(x=> x.SimcardsData).SetValidator(new SimcardDataValidator());
    }
}