namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A visit, with the escort who walks the visitor in: a card of its own model, rendered for the visit's Escort member.</summary>
public sealed class EscortPage
{
    public EscortCard Escort { get; set; } = new();
}

/// <summary>The escort card: a partial whose inputs bind under the visit's Escort prefix.</summary>
public sealed class EscortCard
{
    public string? Name { get; set; }
}

/// <summary>A request with the escort's name at its root, where no input under the Escort prefix is sent.</summary>
public sealed class EscortRequest
{
    public string? Name { get; set; }
}

/// <summary>The request the escort card's named gather builds.</summary>
public enum EscortGather
{
    /// <summary>The card's own model, <c>Gather(g =&gt; ..)</c>.</summary>
    AsTheCard,

    /// <summary>The visit, the class the endpoint binds, <c>Gather&lt;EscortPage&gt;(g =&gt; ..)</c>.</summary>
    AsTheVisit,

    /// <summary>A class without the visit's Escort member, <c>Gather&lt;EscortRequest&gt;(g =&gt; ..)</c>.</summary>
    AsARequestWithoutThePrefix,
}
