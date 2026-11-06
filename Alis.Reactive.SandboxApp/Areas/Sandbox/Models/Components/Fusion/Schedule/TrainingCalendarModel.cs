namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Training calendar: the coordinator opens a session's details from its card, and reads the
    /// sessions of the week on screen in Agenda.
    /// </summary>
    public class TrainingCalendarPageModel
    {
    }

    /// <summary>A staff training session on the calendar.</summary>
    public sealed record TrainingCalendarSession(int Id, string Subject, string Trainer, DateTime StartTime, DateTime EndTime);

    /// <summary>The sessions the calendar loads.</summary>
    public sealed record TrainingCalendarSessions(IReadOnlyList<TrainingCalendarSession> Sessions, string Summary);

    /// <summary>The session a card's Details button names.</summary>
    public class TrainingCalendarSessionPicked
    {
        public int Id { get; set; }
    }
}
