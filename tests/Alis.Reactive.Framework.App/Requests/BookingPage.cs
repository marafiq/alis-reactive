namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A room booked by the page, which the endpoint may refuse with a reason of its own.</summary>
public sealed class BookingPage
{
}

/// <summary>Why the endpoint refused a booking, as its error body says it.</summary>
public sealed class BookingProblem
{
    public string Reason { get; set; } = "";
}
