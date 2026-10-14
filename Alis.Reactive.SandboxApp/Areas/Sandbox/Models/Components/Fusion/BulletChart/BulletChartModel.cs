namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart
{
    /// <summary>
    /// The director of nursing at the weekly staffing huddle: each wing's hours for the week against its
    /// budget, and the wings the huddle has gone through.
    /// </summary>
    public sealed class FusionBulletChartModel
    {
        /// <summary>The wings the huddle has reviewed, in the chart's order.</summary>
        public string[] ReviewedWings { get; set; } = [];
    }

    /// <summary>One wing's hours for the week: the bar, and the line it is measured against.</summary>
    /// <param name="Wing">The wing.</param>
    /// <param name="Hours">The hours worked.</param>
    /// <param name="Target">The hours budgeted, or the most the wing may use.</param>
    public sealed record WingHours(string Wing, int Hours, int Target);

    /// <summary>The week of October 5, wing by wing, in the order the huddle goes through them.</summary>
    public static class StaffingWeek
    {
        /// <summary>Nursing hours each wing worked against the hours budgeted for it.</summary>
        public static readonly IReadOnlyList<WingHours> Nursing = new[]
        {
            new WingHours("Maple", 812, 840),
            new WingHours("Birch", 868, 840),
            new WingHours("Cedar", 702, 756),
            new WingHours("Memory Care", 954, 980)
        };

        /// <summary>Agency nurse hours each wing used against the most it may use.</summary>
        public static readonly IReadOnlyList<WingHours> Agency = new[]
        {
            new WingHours("Maple", 24, 60),
            new WingHours("Birch", 96, 120),
            new WingHours("Cedar", 12, 60),
            new WingHours("Memory Care", 72, 96)
        };

        /// <summary>What happened on each wing's shifts this week.</summary>
        public static readonly IReadOnlyDictionary<string, string> ShiftNotes = new Dictionary<string, string>
        {
            ["Maple"] = "812 of 840 budgeted hours. Two evening shifts ran one aide short on Tuesday.",
            ["Birch"] = "868 of 840 budgeted hours. Three night shifts ran long while a resident was on fall watch.",
            ["Cedar"] = "702 of 756 budgeted hours. A Saturday day shift was left open after a call-out.",
            ["Memory Care"] = "954 of 980 budgeted hours. Every shift was covered."
        };
    }

    /// <summary>The week's shifts for the wing whose bar the director clicked.</summary>
    public sealed class WingShiftsRequest
    {
        /// <summary>The ID of the bar or budget line clicked, which ends with the wing's place in the chart.</summary>
        public string Bar { get; set; } = string.Empty;
    }

    /// <summary>A wing's shifts for the week.</summary>
    public sealed class WingShifts
    {
        /// <summary>The wing.</summary>
        public string Wing { get; set; } = string.Empty;

        /// <summary>Its hours against budget and what happened on its shifts.</summary>
        public string Summary { get; set; } = string.Empty;
    }
}
