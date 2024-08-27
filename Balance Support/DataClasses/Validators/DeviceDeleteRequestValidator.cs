using FluentValidation;
using Balance_Support.DataClasses.Records.DeviceData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceDeleteRequestValidator: AbstractValidator<DeviceDeleteRequest>
{
    public DeviceDeleteRequestValidator()
    {
        RuleFor(x => x.DeviceId).NotEmpty();
    }
}