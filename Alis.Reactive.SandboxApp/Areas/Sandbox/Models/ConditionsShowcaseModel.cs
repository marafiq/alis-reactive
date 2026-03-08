using System;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    // ── Event payloads (typed args for When/Then/Else) ──

    public class ScorePayload
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
    }

    public class RolePayload
    {
        public string Role { get; set; } = "";
    }

    public class LongPayload
    {
        public long Balance { get; set; }
    }

    public class DoublePayload
    {
        public double Temperature { get; set; }
    }

    public class BoolPayload
    {
        public bool IsActive { get; set; }
    }

    public class DatePayload
    {
        public DateTime Deadline { get; set; }
    }

    public class NullablePayload
    {
        public int? NullableScore { get; set; }
        public DateTime? NullableDate { get; set; }
        public bool? NullableBool { get; set; }
    }

    // ── View model ──

    public class ConditionsShowcaseModel
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
