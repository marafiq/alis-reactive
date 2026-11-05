using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Requests;

public sealed class BookingPage
{
}

public sealed class BookingConfirmation
{
    [JsonPropertyName("room_number")]
    public string Room { get; set; } = "";
}

public sealed class BookingProblem
{
    public string Reason { get; set; } = "";
}
