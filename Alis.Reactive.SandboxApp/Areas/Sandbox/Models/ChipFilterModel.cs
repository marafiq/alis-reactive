using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class ChipFilterModel
    {
    }

    public sealed class ChipFilterPayload
    {
        public FusionSelectedChips Selection { get; set; } = new FusionSelectedChips();
    }

    public sealed class CareResidentResponse
    {
        public CareResident[] Residents { get; set; } = Array.Empty<CareResident>();
    }

    public sealed class CareResident
    {
        public string Name { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    public sealed class CareFilterRequest
    {
        public string[]? CareLevels { get; set; }
    }

    public static class ChipFilterEvents
    {
        public static readonly AppEvent<ChipFilterPayload> FiltersApplied = AppEvent.Declare<ChipFilterPayload>();
    }
}
