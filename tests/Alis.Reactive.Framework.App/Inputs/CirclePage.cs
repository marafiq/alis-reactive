namespace Alis.Reactive.Framework.App.Inputs;

public sealed class CirclePage
{
    public List<ContactCard> Members { get; set; } = new();
}

public sealed class CircleEcho
{
    public string Summary { get; set; } = "";
}
