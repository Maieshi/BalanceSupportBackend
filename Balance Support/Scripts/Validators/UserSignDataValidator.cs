using FluentValidation;
namespace Balance_Support.Scripts.Validators;

public class UserSignDataValidator: AbstractValidator<UserSignData>
{
    public UserSignDataValidator()
    {
        RuleFor(x=>x.UserCred).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}