using FluentValidation;
using Balance_Support.DataClasses.Records.NotificationData;
namespace Balance_Support.DataClasses.Validators;

public class DeleteUserTokenRequestValidator:AbstractValidator<DeleteUserTokenRequest>
{
    public DeleteUserTokenRequestValidator()
    {
        RuleFor(x => x.UserId).NotEmpty();
    }
}