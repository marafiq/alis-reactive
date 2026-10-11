using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EditAssignmentValidator : ReactiveValidator<EditAssignmentModel>
    {
        public EditAssignmentValidator()
        {
            RuleFor(m => m.StaffId).NotEmpty().WithMessage("Staff member is required.");
        }
    }
}
