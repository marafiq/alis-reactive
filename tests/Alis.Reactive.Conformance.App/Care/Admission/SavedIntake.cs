using System.Globalization;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class SavedIntake
{
    public DateTime MoveIn { get; set; }

    public DateTime? Orientation { get; set; }

    public DateTime? Discharge { get; set; }

    public static SavedIntake ForReview() => new()
    {
        MoveIn = new DateTime(2026, 11, 18),
        Orientation = new DateTime(2026, 11, 18, 14, 35, 8, 123),
    };

    public string Receipt()
    {
        var orientation = Orientation?.ToString("yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture) ?? "not set";
        var discharge = Discharge?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "not set";
        return FormattableString.Invariant($"Moving in {MoveIn:yyyy-MM-dd}; orientation {orientation}; discharge {discharge}");
    }
}
