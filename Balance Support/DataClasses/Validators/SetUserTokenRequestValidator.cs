using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;
namespace Balance_Support.DataClasses.Validators;

public class SetUserTokenRequestValidator:AbstractValidator<SetUserTokenRequest>
{
    public SetUserTokenRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}