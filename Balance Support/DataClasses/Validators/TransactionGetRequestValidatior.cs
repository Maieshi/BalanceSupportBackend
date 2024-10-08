using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;
namespace Balance_Support.DataClasses.Validators;

public class TransactionGetRequestValidatior : AbstractValidator<TransactionGetRequest>
{
    public TransactionGetRequestValidatior()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.Amount).NotNull().NotEmpty().GreaterThan(0);
    }
}