using Balance_Support.DataClasses.Records.AccountData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class DeviceUpdateRequestValidator : AbstractValidator<AccountUpdateRequest>
{
    public DeviceUpdateRequestValidator()
    {
        RuleFor(x=> x.AccountId).NotNull().NotEmpty();
        RuleFor(x=> x.AccountDataRequest).NotNull().SetValidator(new AccountDataRequestValidator());
    }
}