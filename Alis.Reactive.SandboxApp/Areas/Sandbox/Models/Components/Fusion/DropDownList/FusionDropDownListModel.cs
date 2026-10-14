using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownList
{
    /// <summary>
    /// An admissions coordinator places a new resident: the care level she needs, then a wing that offers
    /// it, the wing list following the care level from the server.
    /// </summary>
    public sealed class FusionDropDownListModel
    {
        /// <summary>The care level the resident needs, by its code.</summary>
        public string? CareLevel { get; set; }

        /// <summary>The wing the resident moves into, or waits for, by its code.</summary>
        public string? Wing { get; set; }

        /// <summary>The care levels the community offers.</summary>
        public IReadOnlyList<CareLevelOption> CareLevels { get; init; } = [];

        /// <summary>The community the resident moves into, by its code; the wing list is that community's.</summary>
        public string Community { get; init; } = "";
    }

    /// <summary>A care level: the code the placement keeps and the name the coordinator reads.</summary>
    public sealed class CareLevelOption
    {
        public string Code { get; init; } = "";

        public string Name { get; init; } = "";
    }

    /// <summary>A wing that offers a care level, and the building it is in.</summary>
    public sealed class PlacementWing
    {
        public string Code { get; init; } = "";

        public string Name { get; init; } = "";

        public string Building { get; init; } = "";
    }

    /// <summary>Asks for the wings that offer a care level.</summary>
    public sealed class WingListQuery
    {
        public string? CareLevel { get; set; }
    }

    /// <summary>The wings that offer a care level, and the one with a room open this week; empty when none has.</summary>
    public sealed class WingList
    {
        public IReadOnlyList<PlacementWing> Wings { get; init; } = [];

        public string OpenRoomWing { get; init; } = "";
    }

    /// <summary>What admissions says once the placement is made.</summary>
    public sealed class PlacementConfirmation
    {
        public string Message { get; init; } = "";
    }

    /// <summary>A resident is placed only with a care level and a wing.</summary>
    public sealed class AdmissionPlacementValidator : ReactiveValidator<FusionDropDownListModel>
    {
        public AdmissionPlacementValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Choose the care level the resident needs.");
            RuleFor(x => x.Wing).NotEmpty().WithMessage("Choose the wing the resident moves into.");
        }
    }
}
