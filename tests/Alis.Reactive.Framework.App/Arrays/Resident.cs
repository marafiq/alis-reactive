using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Arrays;

public sealed class Resident
{
    public string Name { get; set; } = "";

    public string? Nickname { get; set; }

    public int Age { get; set; }

    public bool Present { get; set; }

    public decimal Balance { get; set; }

    public double Weight { get; set; }

    public DateTime Admitted { get; set; }

    public DateTime? LeftOn { get; set; }

    public DateTimeOffset NextVisit { get; set; }

    public Wing Wing { get; set; }

    public string Room { get; set; } = "";

    public Keyworker Keyworker { get; set; } = new();

    public bool? Discharged { get; set; }

    public int? Score { get; set; }

    public TimeSpan CareThisWeek { get; set; }

    public TimeSpan? LastOuting { get; set; }
}

[JsonConverter(typeof(JsonStringEnumConverter))]
public enum Wing
{
    North,
    East,
    South,
}

public sealed class Keyworker
{
    public string Name { get; set; } = "";
}

public sealed class Census
{
    public IEnumerable<Resident> Residents { get; set; } = [];

    public List<string>? Visitors { get; set; }
}

public sealed class Notice
{
    public List<string> Names { get; set; } = [];
}

public sealed class NoticeEcho
{
    public string Summary { get; set; } = "";
}

public static class Register
{
    public static List<Resident> Residents() =>
    [
        new()
        {
            Name = "Cyd Marsh", Nickname = "", Age = 81, Present = true, Balance = 12.50m, Weight = 61.5, Score = 7,
            Admitted = new DateTime(2026, 1, 5, 9, 45, 30), NextVisit = new DateTimeOffset(2026, 10, 27, 7, 30, 0, TimeSpan.FromHours(5.5)),
            Wing = Wing.North, Room = "  n12 ", Keyworker = new() { Name = "Fay" },
            CareThisWeek = TimeSpan.FromHours(23), LastOuting = TimeSpan.FromHours(12),
        },
        new()
        {
            Name = "Ada Hartley", Nickname = null, Age = 102, Present = false, Balance = 0.25m, Weight = 70.25, Score = null,
            Admitted = new DateTime(2026, 3, 14), LeftOn = new DateTime(2026, 4, 30), NextVisit = new DateTimeOffset(2026, 10, 27, 8, 0, 0, TimeSpan.FromHours(9)),
            Wing = Wing.East, Room = "E4  ", Keyworker = new() { Name = "Gus" },
            CareThisWeek = TimeSpan.FromHours(26), LastOuting = null,
        },
        new()
        {
            Name = "Ben Hart", Nickname = "Benny", Age = 79, Present = true, Balance = 30m, Weight = 82, Score = 0,
            Admitted = new DateTime(2026, 6, 19, 14, 9, 45), NextVisit = new DateTimeOffset(2026, 10, 26, 21, 0, 0, TimeSpan.FromHours(-4)),
            Wing = Wing.North, Room = " n7", Keyworker = new() { Name = "Fay" },
            CareThisWeek = TimeSpan.FromHours(4), LastOuting = TimeSpan.FromHours(28),
        },
        new()
        {
            Name = "Dot Ashton", Nickname = "Dottie", Age = 80, Present = false, Balance = 7.75m, Weight = 55.5, Score = 9,
            Admitted = new DateTime(2025, 11, 2, 21, 30, 9), NextVisit = new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero),
            Wing = Wing.South, Room = "S2", Keyworker = new() { Name = "Hal" },
            CareThisWeek = TimeSpan.FromHours(53), LastOuting = TimeSpan.FromHours(5),
        },
    ];
}
