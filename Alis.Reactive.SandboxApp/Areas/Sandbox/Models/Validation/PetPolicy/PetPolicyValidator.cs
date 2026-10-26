using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy
{
    public class PetPolicyValidator : ReactiveValidator<PetPolicyModel>
    {
        public PetPolicyValidator()
        {
            // The tags are read through a query only the server runs: declared so, the rule under it stays
            // with the server and the page sends the policy as it is.
            WhenOnServer(x => x.Tags.Any(tag => tag != null && tag.Length > 3), () =>
                RuleFor(x => x.PetName).NotEmpty().WithMessage("Name the pet."));
        }
    }

    public class RefusedPetPolicyValidator : ReactiveValidator<RefusedPetPolicyModel>
    {
        public RefusedPetPolicyValidator()
        {
            // The same condition as a plain When: the browser cannot read it, and the page says so as it renders.
            When(x => x.Tags.Any(tag => tag != null && tag.Length > 3), () =>
                RuleFor(x => x.PetName).NotEmpty().WithMessage("Name the pet."));
        }
    }
}
