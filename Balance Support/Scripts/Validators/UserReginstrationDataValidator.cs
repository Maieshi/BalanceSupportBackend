using FluentValidation;
namespace Balance_Support.Scripts.Validators;

public class UserReginstrationDataValidator:AbstractValidator<UserReginstrationData>
{
    public UserReginstrationDataValidator()
    {
        RuleFor(x => x.Email).NotNull().NotEmpty();
        RuleFor(x => x.DisplayName).NotNull().NotEmpty(); 
        RuleFor(x => x.Password).NotNull().NotEmpty().MinimumLength(8);
    }
}