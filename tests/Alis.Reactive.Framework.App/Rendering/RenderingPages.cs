namespace Alis.Reactive.Framework.App.Rendering;

/// <summary>A shift handed over to the next: the resident, and the notes the shift leaves.</summary>
public sealed class ShiftHandover
{
    public string? Resident { get; set; }

    public string? Notes { get; set; }
}

/// <summary>The round: the resident handed over, and the resident's vitals card.</summary>
public sealed class RoundsPage
{
    public VitalsCardPlan Card { get; set; }

    public string? Resident { get; set; }
}

/// <summary>Where the vitals card renders its plan: in its body, or in a section of its own.</summary>
public enum VitalsCardPlan
{
    InTheCardsBody,

    InTheCardsSection,
}

/// <summary>The resident's vitals, recorded on a card of their own during the round.</summary>
public sealed class VitalsCard
{
    public string? Pulse { get; set; }
}

/// <summary>The nurse who relieves the shift, rendered as the shift's Relief member.</summary>
public sealed class ReliefNurse
{
    public ReliefPrefix Prefix { get; set; }

    public string? Name { get; set; }
}

/// <summary>The shift the relief nurse takes over, as the endpoint binds it.</summary>
public sealed class ReliefShift
{
    public ReliefNurse Relief { get; set; } = new();
}

/// <summary>When the relief nurse's view takes the Relief prefix: before its plan starts, or after.</summary>
public enum ReliefPrefix
{
    BeforeThePlanStarts,

    AfterThePlanStarts,
}

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class HandoverEcho
{
    public string Summary { get; set; } = "";
}
