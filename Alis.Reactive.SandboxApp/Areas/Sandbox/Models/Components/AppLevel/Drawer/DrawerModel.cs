using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.Drawer
{
    public class DrawerModel
    {
    }

    public class DrawerResidentModel
    {
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? CareLevel { get; set; }
        public string? Notes { get; set; }
    }

    public class DrawerResidentValidator : ReactiveValidator<DrawerResidentModel>
    {
        public DrawerResidentValidator()
        {
            RuleFor(x => x.Name).NotEmpty().WithMessage("'Name' is required.")
                .MinimumLength(2).WithMessage("'Name' must be at least 2 characters.");
            RuleFor(x => x.Email).NotEmpty().WithMessage("'Email' is required.")
                .EmailAddress().WithMessage("'Email' must be a valid email address.");
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("'Care Level' is required.");
        }
    }
}
