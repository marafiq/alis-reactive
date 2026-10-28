namespace Alis.Reactive.Framework.App.Conditions;

/// <summary>A pain score held by a meter, against fixed thresholds.</summary>
public sealed class LevelsPage;

/// <summary>A pain score against a comfort goal the nurse sets on a second meter.</summary>
public sealed class GoalPage;

/// <summary>A resident looked up by name, and the room they are in.</summary>
public sealed class NamesPage
{
    public string? Name { get; set; }

    public string? Room { get; set; }
}

/// <summary>The day round, held by a slate whose rows are the residents on it.</summary>
public sealed class RoundPage;

/// <summary>A visit booked on a day picked around the open day.</summary>
public sealed class VisitPage;

/// <summary>A stay: the day a resident arrives and the day they leave, each picked.</summary>
public sealed class StayPage;

/// <summary>A resident triaged by their pain, their fall risk and the ward they are on.</summary>
public sealed class TriagePage
{
    public string? Ward { get; set; }
}

/// <summary>A care plan chosen by the pain score, one arm after another.</summary>
public sealed class CarePlanPage;

/// <summary>A visit booked once the picker has checked, a moment later, that the day is free.</summary>
public sealed class AvailabilityPage;

/// <summary>Decisions the nurse is asked to confirm.</summary>
public sealed class DecisionsPage;

/// <summary>A page whose branches are written wrongly, which the framework refuses to render.</summary>
public sealed class RefusedPage;

/// <summary>A shift: the time it starts, given by the address, and who is on its rota.</summary>
public sealed class ShiftsPage;

/// <summary>The query a shift is opened with, as its action would bind it with [FromQuery].</summary>
public sealed class ShiftQuery
{
    public TimeOnly? Starts { get; set; }
}
