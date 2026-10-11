using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeCheckListFormValidator : ReactiveValidator<NativeCheckListModel>
    {
        public NativeCheckListFormValidator()
        {
            RuleFor(x => x.ResidentName).NotEmpty().WithMessage("'Resident Name' is required.");
            RuleFor(x => x.DietaryNeeds).NotEmpty().WithMessage("Select at least one dietary need.");
        }
    }
}
