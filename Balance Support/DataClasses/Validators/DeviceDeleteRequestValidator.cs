using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceDeleteRequestValidator: AbstractValidator<DeviceDeleteRequest>
{
    public DeviceDeleteRequestValidator()
    {
        RuleFor(x => x.AccountId).NotEmpty();
    }
}