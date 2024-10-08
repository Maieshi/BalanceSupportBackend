using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;
namespace Balance_Support.DataClasses.Validators;

public class UserTokenDeleteRequestValidator:AbstractValidator<DeleteUserTokenRequest>
{
    public UserTokenDeleteRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}