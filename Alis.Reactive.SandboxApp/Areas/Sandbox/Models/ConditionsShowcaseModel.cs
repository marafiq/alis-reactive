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

    // ── Nested payload (deep path conditions + null safety) ──

    public class AddressInfo
    {
        public string? City { get; set; }
        public string? Zip { get; set; }
    }

    public class NestedPayload
    {
        public AddressInfo? Address { get; set; }
        public int Id { get; set; }
    }

    // ── New payloads for expanded operators ──

    public class MembershipPayload
    {
        public string Category { get; set; } = "";
        public int Priority { get; set; }
    }

    public class TextPayload
    {
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }

    public class RangePayload
    {
        public int Age { get; set; }
    }

    // ── View model ──

    public class ConditionsShowcaseModel
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
        public string Role { get; set; } = "";
    }
}
