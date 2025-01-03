using Balance_Support.DataClasses.Records.UserData;
using FluentValidation;

namespace Balance_Support.DataClasses.Validators;

public class UserSettingsUpdateRequestValidator : AbstractValidator<UserSettingsUpdateRequest>
{
    public UserSettingsUpdateRequestValidator()
    {
        RuleFor(x => x.UserId)
            .NotNull()
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.SelectedGroups)
            .NotNull()
            .WithMessage("SelectedGroup is required.");

        RuleFor(x => x.RowsCount)
            .NotNull()
            .NotEmpty()
            .GreaterThan(0)
            .WithMessage("RowsCount is required.");
    }
}