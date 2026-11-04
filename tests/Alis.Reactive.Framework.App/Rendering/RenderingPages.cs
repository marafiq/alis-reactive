namespace Alis.Reactive.Framework.App.Rendering;

public sealed class ShiftHandover
{
    public string? Resident { get; set; }

    public string? Notes { get; set; }
}

public sealed class RoundsPage
{
    public VitalsCardPlan Card { get; set; }

    public string? Resident { get; set; }
}

public enum VitalsCardPlan
{
    InTheCardsBody,

    WhenItsButtonIsWritten,
}

public sealed class VitalsCard
{
    public string? Pulse { get; set; }
}

public sealed class ReliefNurse
{
    public ReliefPrefix Prefix { get; set; }

    public string? Name { get; set; }
}

public sealed class ReliefShift
{
    public ReliefNurse Relief { get; set; } = new();
}

public enum ReliefPrefix
{
    BeforeThePlanStarts,

    AfterThePlanStarts,
}

public sealed class HandoverEcho
{
    public string Summary { get; set; } = "";
}
