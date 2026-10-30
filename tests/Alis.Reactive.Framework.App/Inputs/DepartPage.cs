namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A visitor signing out, whose one member a view renders once, or a second time.</summary>
public sealed class DepartPage
{
    public DepartRendering Rendering { get; set; }

    public string? Visitor { get; set; }
}

/// <summary>How often, and where, the visitor's field is rendered.</summary>
public enum DepartRendering
{
    /// <summary>Once, by the page.</summary>
    Once,

    /// <summary>Again by the page, on its own plan.</summary>
    TwiceOnOnePlan,

    /// <summary>Again by the visitor's pass, a partial of the page's model with a plan of its own.</summary>
    AgainInAPartial,
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class DepartEcho
{
    public string Summary { get; set; } = "";
}
