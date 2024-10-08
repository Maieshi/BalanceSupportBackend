using Balance_Support.DataClasses.Records.UserData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;
public class UserSettingsGetRequestValidator : AbstractValidator<UserSettingsGetRequest>
{
    public UserSettingsGetRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .NotNull();
    }
}