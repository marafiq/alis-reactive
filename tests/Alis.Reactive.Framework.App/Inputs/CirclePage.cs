namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A resident's circle: the people they keep in touch with, a list of contact cards, one partial rendered for each item.</summary>
public sealed class CirclePage
{
    public List<ContactCard> Members { get; set; } = new();
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class CircleEcho
{
    public string Summary { get; set; } = "";
}
