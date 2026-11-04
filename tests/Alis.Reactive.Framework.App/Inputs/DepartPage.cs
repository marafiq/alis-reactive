namespace Alis.Reactive.Framework.App.Inputs;

public sealed class DepartPage
{
    public DepartRendering Rendering { get; set; }

    public string? Visitor { get; set; }
}

public enum DepartRendering
{
    Once,

    TwiceOnOnePlan,

    AgainInAPartial,
}

public sealed class DepartEcho
{
    public string Summary { get; set; } = "";
}
