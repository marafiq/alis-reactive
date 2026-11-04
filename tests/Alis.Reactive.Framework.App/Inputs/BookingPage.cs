namespace Alis.Reactive.Framework.App.Inputs;

public sealed class BookingPage
{
    public DateTime Day { get; set; }

    public DateTimeOffset Called { get; set; }

    public int Guests { get; set; }

    public List<string>? Tags { get; set; }
}

public sealed class BookingEcho
{
    public string Summary { get; set; } = "";
}
