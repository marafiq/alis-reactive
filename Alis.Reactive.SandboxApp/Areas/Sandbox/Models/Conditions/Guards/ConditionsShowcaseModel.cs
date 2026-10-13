namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.Guards
{
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

    public class ConditionsShowcaseModel
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
        public string Role { get; set; } = "";
    }

    /// <summary>The document events the ConditionsShowcase page raises and hears.</summary>
    public static class ConditionsShowcaseEvents
    {
        public static readonly AppEvent<ScorePayload> CheckAnd = AppEvent.Named<ScorePayload>("check-and");
        public static readonly AppEvent<RangePayload> CheckBetween = AppEvent.Named<RangePayload>("check-between");
        public static readonly AppEvent<BoolPayload> CheckBool = AppEvent.Named<BoolPayload>("check-bool");
        public static readonly AppEvent<ScorePayload> CheckConfirm = AppEvent.Named<ScorePayload>("check-confirm");
        public static readonly AppEvent<TextPayload> CheckContains = AppEvent.Named<TextPayload>("check-contains");
        public static readonly AppEvent<DatePayload> CheckDate = AppEvent.Named<DatePayload>("check-date");
        public static readonly AppEvent<ScorePayload> CheckDirectAnd = AppEvent.Named<ScorePayload>("check-direct-and");
        public static readonly AppEvent<RolePayload> CheckDirectOr = AppEvent.Named<RolePayload>("check-direct-or");
        public static readonly AppEvent<DoublePayload> CheckDouble = AppEvent.Named<DoublePayload>("check-double");
        public static readonly AppEvent<TextPayload> CheckEndswith = AppEvent.Named<TextPayload>("check-endswith");
        public static readonly AppEvent<MembershipPayload> CheckIn = AppEvent.Named<MembershipPayload>("check-in");
        public static readonly AppEvent<TextPayload> CheckIsempty = AppEvent.Named<TextPayload>("check-isempty");
        public static readonly AppEvent<LongPayload> CheckLong = AppEvent.Named<LongPayload>("check-long");
        public static readonly AppEvent<TextPayload> CheckMatches = AppEvent.Named<TextPayload>("check-matches");
        public static readonly AppEvent<TextPayload> CheckMinlength = AppEvent.Named<TextPayload>("check-minlength");
        public static readonly AppEvent<NestedPayload> CheckNested = AppEvent.Named<NestedPayload>("check-nested");
        public static readonly AppEvent<NestedPayload> CheckNestedAnd = AppEvent.Named<NestedPayload>("check-nested-and");
        public static readonly AppEvent<ScorePayload> CheckNestedBranch = AppEvent.Named<ScorePayload>("check-nested-branch");
        public static readonly AppEvent<NestedPayload> CheckNestedNull = AppEvent.Named<NestedPayload>("check-nested-null");
        public static readonly AppEvent<RolePayload> CheckNot = AppEvent.Named<RolePayload>("check-not");
        public static readonly AppEvent<MembershipPayload> CheckNotin = AppEvent.Named<MembershipPayload>("check-notin");
        public static readonly AppEvent<NestedPayload> CheckNullLeaf = AppEvent.Named<NestedPayload>("check-null-leaf");
        public static readonly AppEvent<NullablePayload> CheckNullable = AppEvent.Named<NullablePayload>("check-nullable");
        public static readonly AppEvent<RolePayload> CheckOr = AppEvent.Named<RolePayload>("check-or");
        public static readonly AppEvent<ScorePayload> CheckSingleCommandCondition = AppEvent.Named<ScorePayload>("check-single-command-condition");
        public static readonly AppEvent<TextPayload> CheckStartswith = AppEvent.Named<TextPayload>("check-startswith");
        public static readonly AppEvent<RolePayload> CheckString = AppEvent.Named<RolePayload>("check-string");
        public static readonly AppEvent<ScorePayload> SetScore = AppEvent.Named<ScorePayload>("set-score");
    }
}
