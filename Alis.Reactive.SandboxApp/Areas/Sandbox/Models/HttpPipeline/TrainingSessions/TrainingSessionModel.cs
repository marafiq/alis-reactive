using Alis.Reactive.FluentValidator;
using FluentValidation;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Training sessions: the coordinator books a caregiver training session at a time picked on their own
    /// clock, checks a room for it, and confirms the week's sessions. The server reads each time as the clock
    /// the coordinator picked, with that clock's offset.
    /// </summary>
    public class TrainingSessionModel
    {
        public DateTimeOffset? StartsAt { get; set; }
        public DateTimeOffset[]? HallDays { get; set; }
    }

    /// <summary>A session needs a start, and sessions start between 7:00 AM and 7:00 PM on the picked clock.</summary>
    public class TrainingSessionValidator : ReactiveValidator<TrainingSessionModel>
    {
        private static readonly TimeSpan FirstStart = TimeSpan.FromHours(7);
        private static readonly TimeSpan LastStart = TimeSpan.FromHours(19);

        public TrainingSessionValidator()
        {
            ClientRule(x => x.StartsAt).Required("Choose when the session starts.");
            RuleFor(x => x.StartsAt)
                .Must(startsAt => startsAt is null || StartsDuringTrainingHours(startsAt.Value))
                .WithMessage("Sessions start between 7:00 AM and 7:00 PM.");
        }

        private static bool StartsDuringTrainingHours(DateTimeOffset startsAt) =>
            startsAt.TimeOfDay >= FirstStart && startsAt.TimeOfDay <= LastStart;
    }

    /// <summary>The booking the server confirms, with the picked clock and its offset.</summary>
    public sealed record TrainingSessionBooked(string Confirmation);

    /// <summary>The room free at the picked time.</summary>
    public sealed record TrainingRoomAnswer(string Room);

    /// <summary>The trainer free at the time sent in a request header.</summary>
    public sealed record TrainerAvailability(string Trainer);

    /// <summary>The day's plan, for the time sent in the request path.</summary>
    public sealed record TrainingDayPlan(string Plan);

    /// <summary>The projector reserved for the time posted as form data.</summary>
    public sealed record ProjectorReservation(string Confirmation);

    /// <summary>The training hall for the days picked, sent as a list in the query string or in form data.</summary>
    public sealed record TrainingHallAnswer(string Hall);

    /// <summary>A session on the week's schedule, as the schedule loads it.</summary>
    public sealed record TrainingWeekSession(int Id, string Subject, DateTime StartTime, DateTime EndTime);

    /// <summary>The week's sessions.</summary>
    public sealed record TrainingWeek(IReadOnlyList<TrainingWeekSession> Sessions);

    /// <summary>The week the coordinator confirms: the schedule's sessions as it holds them.</summary>
    public class TrainingWeekConfirmation
    {
        public List<ConfirmedTrainingSession> Sessions { get; set; } = [];
    }

    /// <summary>A session the coordinator confirmed, at the clock the schedule showed.</summary>
    public class ConfirmedTrainingSession
    {
        public string Subject { get; set; } = "";
        public DateTimeOffset StartTime { get; set; }
    }

    /// <summary>The confirmation the server returns for the week.</summary>
    public sealed record TrainingWeekConfirmed(string Confirmation);
}
