namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A visit booked at the desk: a day, the moment the visitor called, and how many guests come.</summary>
public sealed class BookingPage
{
    public DateTime Day { get; set; }

    public DateTimeOffset Called { get; set; }

    public int Guests { get; set; }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class BookingEcho
{
    public string Summary { get; set; } = "";
}
