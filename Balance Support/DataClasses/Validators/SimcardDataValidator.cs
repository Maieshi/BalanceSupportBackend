using FluentValidation;

using Balance_Support.DataClasses.Records.DeviceData;

namespace Balance_Support.DataClasses.Validators;


public class SimcardDataValidator : AbstractValidator<SimCardData>
{
    public SimcardDataValidator()
    {
        RuleFor(x => x.SimCardId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SimCardNumber).NotEmpty().Length(1, 20);
        RuleFor(x => x.BankType).NotEmpty().Length(1, 50);
        RuleFor(x => x.CardNumber).NotEmpty().GreaterThan(0);
        RuleFor(x => x.InitalBalance).NotEmpty().GreaterThanOrEqualTo(0);
    }
}