
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete
{
    public sealed class FusionAutoCompleteModel
    {
        public string? Physician { get; set; }

        public string? Concern { get; set; }

        public IReadOnlyList<PhysicianListing> CareTeam { get; init; } = [];

        public string Attending { get; init; } = "";
    }

    public sealed class PhysicianListing
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class DirectorySearch
    {
        public string? Name { get; set; }
    }

    public sealed class DirectoryMatches
    {
        public IReadOnlyList<PhysicianListing> Physicians { get; init; } = [];

        public int Count { get; init; }

        public string Summary { get; init; } = "";
    }

    public sealed class PhysicianContactQuery
    {
        public string? Physician { get; set; }
    }

    public sealed class PhysicianContact
    {
        public string Specialty { get; init; } = "";

        public string Callback { get; init; } = "";
    }

    public sealed class OnCallPhysicians
    {
        public IReadOnlyList<PhysicianListing> Physicians { get; init; } = [];
    }

    public sealed class PhysicianPageSent
    {
        public string Message { get; init; } = "";
    }

    public sealed class PhysicianPageValidator : ReactiveValidator<FusionAutoCompleteModel>
    {
        public PhysicianPageValidator()
        {
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Choose the physician to page.");
            RuleFor(x => x.Concern).NotEmpty().WithMessage("Say what changed for her.");
        }
    }
}
