namespace Alis.Reactive.Framework.App.Elements;

/// <summary>The ward panel: notices shown and hidden, a class taken off and toggled, the app's banner, a visitor.</summary>
public sealed class PanelPage
{
}

/// <summary>A visitor at the panel, rendered by a plan of its own.</summary>
public sealed class PanelVisitor
{
    public string? Name { get; set; } = "Ada";
}

/// <summary>What the panel announces.</summary>
public sealed class Called
{
    public string Room { get; set; } = "";
}

public static class PanelEvents
{
    public static readonly AppEvent<Called> NurseCalled = AppEvent.Declare<Called>();
}

/// <summary>A page whose plan makes one mistake a dispatch cannot carry, so it is refused when it renders.</summary>
public sealed class RefusedDispatchPage
{
    public DispatchMistake Mistake { get; set; }
}

/// <summary>The mistakes a dispatch can make.</summary>
public enum DispatchMistake
{
    PayloadWithNoField,
    SelectorThatIsNoMember,
}
