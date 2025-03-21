using FluentValidation;
using Balance_Support.DataClasses.Records.AccountData;
namespace Balance_Support.DataClasses.Validators;

public class DeviceRegisterRequestValidator: AbstractValidator<AccountRegisterRequest>
{
    public DeviceRegisterRequestValidator()
    {
        RuleFor(x=> x.UserId).NotNull().NotEmpty();
        RuleFor(x=> x.AccountData).NotNull().SetValidator(new AccountDataRequestValidator());
    }
}