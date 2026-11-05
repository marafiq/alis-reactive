namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class TrainingSessionModel
    {
        public DateTimeOffset? StartsAt { get; set; }
        public DateTimeOffset[]? HallDays { get; set; }
    }

    public class TrainingSessionValidator : ReactiveValidator<TrainingSessionModel>
    {
        private static readonly TimeSpan FirstStart = TimeSpan.FromHours(7);
        private static readonly TimeSpan LastStart = TimeSpan.FromHours(19);

        public TrainingSessionValidator()
        {
            RuleFor(x => x.StartsAt)
                .NotNull().WithMessage("Choose when the session starts.")
                .Must(startsAt => startsAt is null || StartsDuringTrainingHours(startsAt.Value))
                .WithMessage("Sessions start between 7:00 AM and 7:00 PM.");
        }

        private static bool StartsDuringTrainingHours(DateTimeOffset startsAt)
            => startsAt.TimeOfDay >= FirstStart && startsAt.TimeOfDay <= LastStart;
    }

    public sealed record TrainingSessionBooked(string Confirmation);

    public sealed record TrainingRoomAnswer(string Room);

    public sealed record TrainerAvailability(string Trainer);

    public sealed record TrainingDayPlan(string Plan);

    public sealed record ProjectorReservation(string Confirmation);

    public sealed record TrainingHallAnswer(string Hall);

    public sealed record TrainingWeekSession(int Id, string Subject, DateTime StartTime, DateTime EndTime);

    public sealed record TrainingWeek(IReadOnlyList<TrainingWeekSession> Sessions);

    public class TrainingWeekConfirmation
    {
        public List<ConfirmedTrainingSession> Sessions { get; set; } = [];
    }

    public class ConfirmedTrainingSession
    {
        public string Subject { get; set; } = "";
        public DateTimeOffset StartTime { get; set; }
    }

    public sealed record TrainingWeekConfirmed(string Confirmation);
}
