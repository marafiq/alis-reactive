using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NewAssignmentValidator : ReactiveValidator<NewAssignmentModel>
    {
        public NewAssignmentValidator()
        {
            RuleFor(m => m.StaffId).NotEmpty().WithMessage("Staff member is required.");
        }
    }
}
