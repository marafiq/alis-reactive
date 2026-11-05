using System.Text.Json.Serialization;

namespace Alis.Reactive.Framework.App.Subscriptions;

public sealed class GatePage;

public sealed class Announcement
{
    public string? Type { get; set; }

    public int PartySize { get; set; }

    public GateUrgency Urgency { get; set; }

    [JsonPropertyName("called_by")]
    public string? CalledBy { get; set; }
}

public enum GateUrgency
{
    Routine,
    Urgent,
}

public sealed class EscortedVisit
{
    public GateVisitor Visitor { get; set; } = new();
}

public sealed class GateVisitor
{
    public string Name { get; set; } = "";
}

public sealed class SignedVisit
{
    [JsonPropertyName("Signature")]
    public string Signature { get; set; } = "";
}

public static class GateEvents
{
    public static readonly AppEvent Opened = AppEvent.Declare();

    public static readonly AppEvent<Announcement> Announced = AppEvent.Declare<Announcement>();

    public static readonly AppEvent<EscortedVisit> Escorted = AppEvent.Declare<EscortedVisit>();

    public static readonly AppEvent<SignedVisit> Signed = AppEvent.Declare<SignedVisit>();
}
