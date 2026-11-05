namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.BulletChart
{
    public sealed class FusionBulletChartModel
    {
        public string[] ReviewedWings { get; set; } = [];
    }

    public sealed record WingHours(string Wing, int Hours, int Target);

    public static class StaffingWeek
    {
        public static readonly IReadOnlyList<WingHours> Nursing = new[]
        {
            new WingHours("Maple", 812, 840),
            new WingHours("Birch", 868, 840),
            new WingHours("Cedar", 702, 756),
            new WingHours("Memory Care", 954, 980)
        };

        public static readonly IReadOnlyList<WingHours> Agency = new[]
        {
            new WingHours("Maple", 24, 60),
            new WingHours("Birch", 96, 120),
            new WingHours("Cedar", 12, 60),
            new WingHours("Memory Care", 72, 96)
        };

        public static readonly IReadOnlyDictionary<string, string> ShiftNotes = new Dictionary<string, string>
        {
            ["Maple"] = "812 of 840 budgeted hours. Two evening shifts ran one aide short on Tuesday.",
            ["Birch"] = "868 of 840 budgeted hours. Three night shifts ran long while a resident was on fall watch.",
            ["Cedar"] = "702 of 756 budgeted hours. A Saturday day shift was left open after a call-out.",
            ["Memory Care"] = "954 of 980 budgeted hours. Every shift was covered."
        };
    }

    public sealed class WingShiftsRequest
    {
        public string Bar { get; set; } = string.Empty;
    }

    public sealed class WingShifts
    {
        public string Wing { get; set; } = string.Empty;

        public string Summary { get; set; } = string.Empty;
    }
}
