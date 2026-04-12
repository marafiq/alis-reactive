using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class EditAssignmentValidator : AbstractValidator<EditAssignmentModel>
    {
        public EditAssignmentValidator()
        {
            RuleFor(m => m.StaffId)
                .NotEmpty().WithMessage("Staff member is required.");
        }
    }
}
