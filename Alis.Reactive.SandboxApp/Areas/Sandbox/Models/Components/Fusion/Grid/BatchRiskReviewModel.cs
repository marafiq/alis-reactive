using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class FallsHuddleViewModel
    {
        public List<OvernightFall> Falls { get; set; } = new();
    }

    public class OvernightFall
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string WhatHappened { get; set; } = "";
    }

    public class FallsHuddleResident
    {
        public int ResidentId { get; set; }
        public string ResidentName { get; set; } = "";
        public string Room { get; set; } = "";
        public string FallRisk { get; set; } = "";
    }

    public class FallsHuddleChanges
    {
        public FusionGridBatchChanges<FallsHuddleResident> Changes { get; set; } = new();
    }

    public class FallsHuddleSent
    {
        public string Summary { get; set; } = "";
    }
}
