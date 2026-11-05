namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy
{
    public class PetPolicyValidator : ReactiveValidator<PetPolicyModel>
    {
        public PetPolicyValidator()
        {
            WhenOnServer(x => x.Tags.Any(tag => tag != null && tag.Length > 3), () =>
                RuleFor(x => x.PetName).NotEmpty().WithMessage("Name the pet."));
        }
    }

    public class RefusedPetPolicyValidator : ReactiveValidator<RefusedPetPolicyModel>
    {
        public RefusedPetPolicyValidator()
        {
            When(x => x.Tags.Any(tag => tag != null && tag.Length > 3), () =>
                RuleFor(x => x.PetName).NotEmpty().WithMessage("Name the pet."));
        }
    }
}
