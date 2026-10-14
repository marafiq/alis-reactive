using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A visitor the front desk signs in: the phone a returning visitor is found by, the name that goes on
    /// the visitor badge, and the resident they came to see.
    /// </summary>
    public class NativeTextBoxModel
    {
        public string? Phone { get; set; }

        public string? VisitorName { get; set; }

        public string? ResidentVisited { get; set; }
    }

    /// <summary>The phone number the front desk looks a returning visitor up by.</summary>
    public sealed class ReturningVisitorQuery
    {
        public string? Phone { get; set; }
    }

    /// <summary>A visitor the front desk has signed in before, found by their phone.</summary>
    public sealed class ReturningVisitor
    {
        public string Name { get; set; } = "";

        public string Welcome { get; set; } = "";
    }

    /// <summary>The front desk's answer once a visitor is signed in.</summary>
    public sealed class VisitorSignedIn
    {
        public string Message { get; set; } = "";
    }

    /// <summary>What the front desk needs before a visitor goes up: their first and last name, and whom they visit.</summary>
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
