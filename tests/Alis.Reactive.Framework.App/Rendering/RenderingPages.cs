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

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class HandoverEcho
{
    public string Summary { get; set; } = "";
}
