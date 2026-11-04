
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeTextBoxModel
    {
        public string? Phone { get; set; }

        public string? VisitorName { get; set; }

        public string? ResidentVisited { get; set; }
    }

    public sealed class ReturningVisitorQuery
    {
        public string? Phone { get; set; }
    }

    public sealed class ReturningVisitor
    {
        public string Name { get; set; } = "";

        public string Welcome { get; set; } = "";
    }

    public sealed class VisitorSignedIn
    {
        public string Message { get; set; } = "";
    }

    public sealed class VisitorSignInValidator : ReactiveValidator<NativeTextBoxModel>
    {
        public VisitorSignInValidator()
        {
            RuleFor(x => x.VisitorName)
                .NotEmpty().WithMessage("Write the visitor's name.")
                .Matches(@"^\S+\s+\S+").WithMessage("Write the visitor's first and last name.");

            RuleFor(x => x.ResidentVisited).NotEmpty().WithMessage("Choose the resident being visited.");
        }
    }
}
