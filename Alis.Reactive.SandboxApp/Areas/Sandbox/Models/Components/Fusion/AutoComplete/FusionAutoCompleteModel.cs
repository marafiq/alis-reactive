using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.AutoComplete
{
    /// <summary>
    /// A charge nurse on the evening shift reports Margaret Hale's change of condition and pages the
    /// physician who should hear about it, found by typing part of a name in the physician directory.
    /// </summary>
    public sealed class FusionAutoCompleteModel
    {
        /// <summary>The physician to page, by directory id.</summary>
        public string? Physician { get; set; }

        /// <summary>What changed for the resident, in the nurse's words.</summary>
        public string? Concern { get; set; }

        /// <summary>Her care team, offered before the directory is searched.</summary>
        public IReadOnlyList<PhysicianListing> CareTeam { get; init; } = [];

        /// <summary>The directory id of her attending physician.</summary>
        public string Attending { get; init; } = "";
    }

    /// <summary>A physician the field offers: the directory id it keeps and the name it shows.</summary>
    public sealed class PhysicianListing
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>The part of a name the nurse typed, sent to the directory search.</summary>
    public sealed class DirectorySearch
    {
        public string? Name { get; set; }
    }

    /// <summary>The physicians the directory matched, and the line the page says about them.</summary>
    public sealed class DirectoryMatches
    {
        public IReadOnlyList<PhysicianListing> Physicians { get; init; } = [];

        public int Count { get; init; }

        public string Summary { get; init; } = "";
    }

    /// <summary>Asks how the chosen physician is reached.</summary>
    public sealed class PhysicianContactQuery
    {
        public string? Physician { get; set; }
    }

    /// <summary>How a physician is reached: their specialty and the number the answering service calls back on.</summary>
    public sealed class PhysicianContact
    {
        public string Specialty { get; init; } = "";

        public string Callback { get; init; } = "";
    }

    /// <summary>The physicians on call tonight.</summary>
    public sealed class OnCallPhysicians
    {
        public IReadOnlyList<PhysicianListing> Physicians { get; init; } = [];
    }

    /// <summary>The line the page shows once the physician is paged.</summary>
    public sealed class PhysicianPageSent
    {
        public string Message { get; init; } = "";
    }

    /// <summary>A page goes to one physician and says what changed.</summary>
    public sealed class PhysicianPageValidator : ReactiveValidator<FusionAutoCompleteModel>
    {
        public PhysicianPageValidator()
        {
            RuleFor(x => x.Physician).NotEmpty().WithMessage("Choose the physician to page.");
            RuleFor(x => x.Concern).NotEmpty().WithMessage("Say what changed for her.");
        }
    }
}
