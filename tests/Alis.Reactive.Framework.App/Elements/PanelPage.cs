namespace Alis.Reactive.Framework.App.Elements;

public sealed class PanelPage
{
}

public sealed class PanelVisitor
{
    public string? Name { get; set; } = "Ada";
}

public sealed class Called
{
    public string Room { get; set; } = "";

    public string Ward { get; set; } = "";

    public string Alert { get; set; } = "";

    public string Settled { get; set; } = "";

    public string Note { get; set; } = "";

    public CallBed Bed { get; set; } = new();
}

public sealed class CallBed
{
    public int Number { get; set; }
}

public static class PanelEvents
{
    public static readonly AppEvent<Called> NurseCalled = AppEvent.Declare<Called>();
}

public sealed class RefusedDispatchPage
{
    public DispatchMistake Mistake { get; set; }
}

public enum DispatchMistake
{
    PayloadWithNoField,
    SelectorThatIsNoMember,
    MemberSetTwice,
    ObjectSetOverItsMember,
    MemberSetUnderItsObject,
}
