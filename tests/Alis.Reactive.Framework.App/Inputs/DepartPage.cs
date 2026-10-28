namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A visitor signing out, whose one member a view renders twice.</summary>
public sealed class DepartPage
{
    public DepartRendering Rendering { get; set; }

    public string? Visitor { get; set; }
}

/// <summary>How the view renders the visitor's field the second time.</summary>
public enum DepartRendering
{
    /// <summary>With the same component, as a partial rendered twice on one plan does.</summary>
    WithTheSameComponent,

    /// <summary>With another component: a defect of the view.</summary>
    WithAnotherComponent,
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class DepartEcho
{
    public string Summary { get; set; } = "";
}
