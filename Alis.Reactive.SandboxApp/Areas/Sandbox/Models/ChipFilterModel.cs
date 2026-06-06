using System;
using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>ChipFilter page model for multi-select chip filtering over a resident grid.</summary>
    public sealed class ChipFilterModel
    {
    }

    /// <summary>
    /// Custom event payload carrying selected chips; <see cref="FusionSelectedChips.Data"/>
    /// supplies the chip objects used by the array DSL.
    /// </summary>
    public sealed class ChipFilterPayload
    {
        public FusionSelectedChips Selection { get; set; } = new FusionSelectedChips();
    }

    /// <summary>Resident roster response bound to the grid before and after filtering.</summary>
    public sealed class CareResidentResponse
    {
        public CareResident[] Residents { get; set; } = Array.Empty<CareResident>();
    }

    /// <summary>Resident row matched against selected care-level chip text.</summary>
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
