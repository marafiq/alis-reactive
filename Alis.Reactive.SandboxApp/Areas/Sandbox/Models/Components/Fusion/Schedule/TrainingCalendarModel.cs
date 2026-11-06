namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Training calendar: the coordinator opens a session's details from its card, and reads the
    /// sessions of the week on screen in Agenda.
    /// </summary>
    public class TrainingCalendarPageModel
    {
    }

    /// <summary>A staff training session on the calendar, with how many registered staff have yet to complete it.</summary>
    public sealed record TrainingCalendarSession(
        int Id, string Subject, string Trainer, DateTime StartTime, DateTime EndTime, int RegisteredCount, int IncompleteCount);

    /// <summary>The sessions the calendar loads.</summary>
    public sealed record TrainingCalendarSessions(IReadOnlyList<TrainingCalendarSession> Sessions, string Summary);

    /// <summary>The session a card's button names.</summary>
    public class TrainingCalendarSessionPicked
    {
        public int Id { get; set; }
    }

    /// <summary>The session whose incomplete staff the coordinator reminds.</summary>
    public class TrainingCalendarReminderRequest
    {
        public int SessionId { get; set; }
    }

    /// <summary>What the server reports after a reminder, in the words the toast shows.</summary>
    public sealed record TrainingCalendarReminderSent(string Title, string Message);
}
