using Alis.Reactive.Fusion.Components;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>The falls huddle page: the residents who fell overnight; the risk board loads from the record.</summary>
    public class FallsHuddleViewModel
    {
        public List<OvernightFall> Falls { get; set; } = new();
    }

    /// <summary>A fall the night shift reported to the morning huddle.</summary>
    public class OvernightFall
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string WhatHappened { get; set; } = "";
    }

    /// <summary>A resident on Aspen wing's risk board, with the fall risk on record.</summary>
    public class FallsHuddleResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public string FallRisk { get; set; } = "";
    }

    /// <summary>The risk levels the huddle changed, sent to care planning.</summary>
    public class FallsHuddleChanges
    {
        public FusionGridBatchChanges<FallsHuddleResident> Changes { get; set; } = new();
    }

    /// <summary>What care planning will update, in the words the huddle reads.</summary>
    public class FallsHuddleSent
    {
        public string Summary { get; set; } = "";
    }
}
