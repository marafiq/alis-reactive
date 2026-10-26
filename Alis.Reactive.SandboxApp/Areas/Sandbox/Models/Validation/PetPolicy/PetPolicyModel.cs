namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Validation.PetPolicy
{
    // Journey: a resident brings a pet. A tag on the policy longer than three letters asks for the pet's
    // name; the server alone reads the tags, through a query the browser has no form of.
    public class PetPolicyModel
    {
        public List<string?> Tags { get; set; } = new() { null, null };
        public string? PetName { get; set; }
    }

    /// <summary>The same policy with its condition written as a plain When: the page refuses to render.</summary>
    public class RefusedPetPolicyModel
    {
        public List<string?> Tags { get; set; } = new() { null, null };
        public string? PetName { get; set; }
    }

    public sealed record PetPolicySaved(string Confirmation);
}
