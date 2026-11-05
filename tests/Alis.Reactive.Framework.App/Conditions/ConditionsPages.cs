namespace Alis.Reactive.Framework.App.Conditions;

public sealed class LevelsPage;

public sealed class GoalPage;

public sealed class NamesPage
{
    public string? Name { get; set; }

    public string? Room { get; set; }
}

public sealed class RoundPage;

public sealed class VisitPage;

public sealed class StayPage;

public sealed class TriagePage
{
    public string? Ward { get; set; }
}

public sealed class CarePlanPage;

public sealed class AvailabilityPage;

public sealed class CalendarPage;

public sealed class QueuePage;

public sealed class DecisionsPage;

public sealed class RefusedPage;

public sealed class ShiftsPage;

public sealed class ShiftQuery
{
    public TimeOnly? Starts { get; set; }
}

public sealed class StayQuery
{
    public TimeSpan? Length { get; set; }
}
