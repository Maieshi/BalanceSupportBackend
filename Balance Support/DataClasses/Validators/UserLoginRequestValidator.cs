using Balance_Support.DataClasses.Records.UserData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class UserLoginRequestValidator: AbstractValidator<UserLoginRequest>
{
    public UserLoginRequestValidator()
    {
        RuleFor(x=>x.UserCred).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}