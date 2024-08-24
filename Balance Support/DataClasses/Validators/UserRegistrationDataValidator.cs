using Balance_Support.DataClasses.Records.UserData;
using FluentValidation;
namespace Balance_Support.Scripts.Validators;

public class UserRegistrationDataValidator:AbstractValidator<UserRegistrationData>
{
    public UserRegistrationDataValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.DisplayName).NotNull().NotEmpty(); 
        RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8);
    }
}