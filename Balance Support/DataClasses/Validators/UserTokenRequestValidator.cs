using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;
namespace Balance_Support.DataClasses.Validators;

public class UserTokenRequestValidator:AbstractValidator<UserTokenRequest>
{
    public UserTokenRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
        RuleFor(x => x.Token).NotEmpty();
    }
}