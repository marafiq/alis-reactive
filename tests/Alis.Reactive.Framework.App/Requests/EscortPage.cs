namespace Alis.Reactive.Framework.App.Requests;

public sealed class EscortPage
{
    public EscortCard Escort { get; set; } = new();
}

public sealed class EscortCard
{
    public string? Name { get; set; }
}

public sealed class EscortRequest
{
    public string? Name { get; set; }
}

public enum EscortGather
{
    AsTheCard,

    AsTheVisit,

    AsARequestWithoutThePrefix,
}
