
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class MoveInPreferencesValidator : ReactiveValidator<NativeRadioGroupModel>
    {
        public MoveInPreferencesValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Choose the care level.");

            RuleFor(x => x.RoomType)
                .NotEmpty().WithMessage("Choose the room type.")
                .Must((preferences, room) => !(preferences.CareLevel == "Memory Care" && room == "Private suite"))
                .WithMessage("The memory care floor has no suites. Choose a private or shared room.");

            RuleFor(x => x.MealPlan).NotEmpty().WithMessage("Choose the meal plan.");
        }
    }
}
