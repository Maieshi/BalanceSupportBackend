using Balance_Support.DataClasses.Records.DeviceData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class DeviceRequestInfoValidator : AbstractValidator<DeviceData>
{
    public DeviceRequestInfoValidator()
    {
        RuleFor(x => x.DeviceId).NotNull().NotEmpty();
        RuleFor(x => x.LastName).NotNull().NotEmpty();
        RuleFor(x => x.DeviceGroup).NotEmpty().GreaterThan(0);
        RuleFor(x => x.DeviceSubgroup).NotEmpty().GreaterThan(0);
        RuleForEach(x => x.SimcardsData).SetValidator(new SimcardDataValidator());
        RuleFor(x => x.Description).NotNull().MaximumLength(500);
    }
}