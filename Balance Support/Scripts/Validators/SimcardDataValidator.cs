using FluentValidation;

public class SimcardDataValidator : AbstractValidator<SimcardData>
{
    public SimcardDataValidator()
    {
        RuleFor(x => x.SimId).GreaterThanOrEqualTo(0);
        RuleFor(x => x.SimNumber).NotEmpty().Length(1, 20);
        RuleFor(x => x.BankType).NotEmpty().Length(1, 50);
        RuleFor(x => x.CardNumber).NotEmpty().GreaterThan(0);
        RuleFor(x => x.InitalBalance).NotEmpty().GreaterThanOrEqualTo(0);
    }
}