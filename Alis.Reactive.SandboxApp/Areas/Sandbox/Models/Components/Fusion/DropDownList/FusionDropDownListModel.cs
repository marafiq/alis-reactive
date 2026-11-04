
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.DropDownList
{
    public sealed class FusionDropDownListModel
    {
        public string? CareLevel { get; set; }

        public string? Wing { get; set; }

        public IReadOnlyList<CareLevelOption> CareLevels { get; init; } = [];

        public string Community { get; init; } = "";
    }

    public sealed class CareLevelOption
    {
        public string Code { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class PlacementWing
    {
        public string Code { get; init; } = "";

        public string Name { get; init; } = "";

        public string Building { get; init; } = "";
    }

    public sealed class WingListQuery
    {
        public string? CareLevel { get; set; }
    }

    public sealed class WingList
    {
        public IReadOnlyList<PlacementWing> Wings { get; init; } = [];

        public string OpenRoomWing { get; init; } = "";
    }

    public sealed class PlacementConfirmation
    {
        public string Message { get; init; } = "";
    }

    public sealed class AdmissionPlacementValidator : ReactiveValidator<FusionDropDownListModel>
    {
        public AdmissionPlacementValidator()
        {
            RuleFor(x => x.CareLevel).NotEmpty().WithMessage("Choose the care level the resident needs.");
            RuleFor(x => x.Wing).NotEmpty().WithMessage("Choose the wing the resident moves into.");
        }
    }
}
