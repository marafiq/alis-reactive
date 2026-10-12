using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public sealed class ChipListModel
    {
    }

    public sealed class ChipSelectionPayload
    {
        public FusionSelectedChips Selection { get; set; } = new FusionSelectedChips();
        public FusionChipData Found { get; set; } = new FusionChipData();
    }

    public sealed class ChipQuickFilterRequest
    {
        public string[] Filters { get; set; } = System.Array.Empty<string>();
    }

    public sealed class ChipQuickFilterResponse
    {
        public string[] Filters { get; set; } = System.Array.Empty<string>();
        public string[] Names { get; set; } = System.Array.Empty<string>();
        public int Count { get; set; }
    }

    /// <summary>The document events the ChipList page raises and hears.</summary>
    public static class ChipListEvents
    {
        public static readonly AppEvent<ChipSelectionPayload> ChipSelectionRead = AppEvent.Named<ChipSelectionPayload>("chip-selection-read");
    }
}
