using System;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>Model for the ChipFilter sandbox: multi-select chips as a filter over a resident grid.</summary>
    public sealed class ChipFilterModel
    {
    }

    /// <summary>
    /// Custom event payload carrying the chip selection. <see cref="FusionSelectedChips.Data"/> is the
    /// array of selected chip objects the array DSL operates on.
    /// </summary>
    public sealed class ChipFilterPayload
    {
        public FusionSelectedChips Selection { get; set; } = new FusionSelectedChips();
    }

    /// <summary>The resident roster (and filtered result) bound to the grid.</summary>
    public sealed class CareResidentResponse
    {
        public CareResident[] Residents { get; set; } = Array.Empty<CareResident>();
    }

    /// <summary>A resident with a care level matching the chip texts.</summary>
    public sealed class CareResident
    {
        public string Name { get; set; } = "";
        public string CareLevel { get; set; } = "";
    }

    /// <summary>POST body for the server-side care-level filter.</summary>
    public sealed class CareFilterRequest
    {
        public string[]? CareLevels { get; set; }
    }
}
