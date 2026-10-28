using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Arrays;

/// <summary>A resident of the home, as the residents endpoint answers one: a member of each kind an array operation reads.</summary>
public sealed class Resident
{
    public string Name { get; set; } = "";

    /// <summary>What the resident likes to be called: some have none, and the register left one blank.</summary>
    public string? Nickname { get; set; }

    public int Age { get; set; }

    /// <summary>Whether the resident is in the home today.</summary>
    public bool Present { get; set; }

    public decimal Balance { get; set; }

    public double Weight { get; set; }

    public DateTime Admitted { get; set; }

    /// <summary>The day the resident left the home, when they have.</summary>
    public DateTime? LeftOn { get; set; }

    /// <summary>When the next visitor arrives, in the visitor's own zone.</summary>
    public DateTimeOffset NextVisit { get; set; }

    public Wing Wing { get; set; }

    /// <summary>The room as the old register wrote it: padded, in either case.</summary>
    public string Room { get; set; } = "";

    public Keyworker Keyworker { get; set; } = new();

    /// <summary>Whether the resident has left the home, when the register says.</summary>
    public bool? Discharged { get; set; }

    /// <summary>Today's wellbeing score, from 0 to 10, when the round took one.</summary>
    public int? Score { get; set; }
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

/// <summary>The census: the residents as a sequence, and the visitors, which the census does not hold.</summary>
public sealed class Census
{
    public IEnumerable<Resident> Residents { get; set; } = [];

    public List<string>? Visitors { get; set; }
}

/// <summary>A notice the page posts: the names it is about.</summary>
public sealed class Notice
{
    public List<string> Names { get; set; } = [];
}

/// <summary>What the notice endpoint bound, in its own words.</summary>
public sealed class NoticeEcho
{
    public string Summary { get; set; } = "";
}

/// <summary>
/// The residents in the order the home keeps them. Every order a page asks for differs from it. Of the
/// admissions, only Cyd's hour, only Ben's minute and only Dot's second is 9, and Cyd's day of the month, 5, is
/// the number of Ben's weekday (Friday), so a lambda that reads the wrong part of a date keeps someone else.
/// </summary>
public static class Register
{
    public static List<Resident> Residents() =>
    [
        new()
        {
            Name = "Cyd Marsh", Nickname = "", Age = 81, Present = true, Balance = 12.50m, Weight = 61.5, Score = 7,
            Admitted = new DateTime(2026, 1, 5, 9, 45, 30), NextVisit = new DateTimeOffset(2026, 10, 27, 7, 30, 0, TimeSpan.FromHours(5.5)),
            Wing = Wing.North, Room = "  n12 ", Keyworker = new() { Name = "Fay" },
        },
        new()
        {
            Name = "Ada Hartley", Nickname = null, Age = 102, Present = false, Balance = 0.25m, Weight = 70.25, Score = null,
            Admitted = new DateTime(2026, 3, 14), LeftOn = new DateTime(2026, 4, 30), NextVisit = new DateTimeOffset(2026, 10, 27, 8, 0, 0, TimeSpan.FromHours(9)),
            Wing = Wing.East, Room = "E4  ", Keyworker = new() { Name = "Gus" },
        },
        new()
        {
            Name = "Ben Hart", Nickname = "Benny", Age = 79, Present = true, Balance = 30m, Weight = 82, Score = 0,
            Admitted = new DateTime(2026, 6, 19, 14, 9, 45), NextVisit = new DateTimeOffset(2026, 10, 26, 21, 0, 0, TimeSpan.FromHours(-4)),
            Wing = Wing.North, Room = " n7", Keyworker = new() { Name = "Fay" },
        },
        new()
        {
            Name = "Dot Ashton", Nickname = "Dottie", Age = 80, Present = false, Balance = 7.75m, Weight = 55.5, Score = 9,
            Admitted = new DateTime(2025, 11, 2, 21, 30, 9), NextVisit = new DateTimeOffset(2026, 10, 27, 0, 0, 0, TimeSpan.Zero),
            Wing = Wing.South, Room = "S2", Keyworker = new() { Name = "Hal" },
        },
    ];
}
