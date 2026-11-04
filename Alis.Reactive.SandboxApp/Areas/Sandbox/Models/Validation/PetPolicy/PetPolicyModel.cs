namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy
{
    public class PetPolicyModel
    {
        public List<string?> Tags { get; set; } = new() { null, null };
        public string? PetName { get; set; }
    }

    public class RefusedPetPolicyModel
    {
        public List<string?> Tags { get; set; } = new() { null, null };
        public string? PetName { get; set; }
    }

    public sealed record PetPolicySaved(string Confirmation);
}
