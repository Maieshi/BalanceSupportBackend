using Balance_Support.DataClasses.Records.NotificationData;
using FluentValidation;
namespace Balance_Support.DataClasses.Validators;

public class CalculateBalanceRequestValidator:AbstractValidator<CalculateBalanceRequest>
{
    public CalculateBalanceRequestValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}