using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NewAssignmentValidator : ReactiveValidator<NewAssignmentModel>
    {
        public NewAssignmentValidator()
        {
            RuleFor(m => m.StaffId)
                .NotEmpty().WithMessage("Staff member is required.");
            ClientRule(m => m.StaffId)
                .Required("Staff member is required.");
        }
    }
}
