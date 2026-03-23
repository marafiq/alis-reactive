using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.RadioGroup
{
    /// <summary>
    /// Full validator — all fields across all sections.
    /// </summary>
    public class NativeRadioGroupValidator : ReactiveValidator<NativeRadioGroupModel>
    {
        public NativeRadioGroupValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");
            RuleFor(x => x.RoomType).NotEmpty().WithMessage("'Room Type' is required.");
            RuleFor(x => x.MealPlan).NotEmpty().WithMessage("'Meal Plan' is required.");

            WhenField(x => x.CareLevel, "Memory Care", () =>
            {
                RuleFor(x => x.ResidentName).MinimumLength(3)
                    .WithMessage("'Resident Name' must be at least 3 characters for Memory Care records.");
            });
        }
    }

    /// <summary>
    /// Scoped validator — only the form fields (ResidentName + RoomType).
    /// CareLevel and MealPlan are in demo sections outside the form.
    /// </summary>
    public class NativeRadioGroupFormValidator : ReactiveValidator<NativeRadioGroupModel>
    {
        public NativeRadioGroupFormValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
            RuleFor(x => x.RoomType).NotEmpty().WithMessage("'Room Type' is required.");
        }
    }
}
