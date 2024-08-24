using FluentValidation;
using Balance_Support.DataClasses.Records.UserData;
namespace Balance_Support.Scripts.Validators;

public class UserLoginDataValidator: AbstractValidator<UserLoginData>
{
    public UserLoginDataValidator()
    {
        RuleFor(x=>x.UserCred).NotNull().NotEmpty();
        RuleFor(x => x.Password).NotNull().NotEmpty();
    }
}