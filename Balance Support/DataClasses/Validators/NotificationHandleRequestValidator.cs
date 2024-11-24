using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.DataClasses.Validators;

public class NotificationHandleRequestValidator : AbstractValidator<NotificationHandleRequest>
{
    public NotificationHandleRequestValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
        RuleFor(x => x.DeviceId).NotNull().NotEmpty().GreaterThanOrEqualTo(0);
        RuleFor(x => x.GroupId).NotNull().NotEmpty().GreaterThanOrEqualTo(0);
        RuleFor(x => x.NotificationText).NotNull().NotEmpty();
    }
}