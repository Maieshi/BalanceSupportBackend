using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;

namespace Balance_Support.DataClasses.Validators;

public class MessagesGetRequestValidator:AbstractValidator<MessagesGetRequest>
{
    public MessagesGetRequestValidator()
    {
        RuleFor(x => x.UserId).NotNull().NotEmpty();
    }
}