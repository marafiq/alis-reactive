namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class StaffTrainingPageModel
    {
    }

    public class StaffTrainingRecordModel
    {
        public string? Notes { get; set; }
        public string[]? Courses { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string? TrainerId { get; set; }
    }

    public sealed record TrainingCourse(string Value, string Text);

    public sealed record TrainingTrainer(string Id, string Name);

    public sealed record CompletedTraining(string Course, string CompletedOn, string Trainer);

    public static class StaffTrainingOptions
    {
        public static readonly IReadOnlyList<TrainingCourse> Courses =
        [
            new("dementia-care", "Dementia care"),
            new("fall-prevention", "Fall prevention"),
            new("infection-control", "Infection control"),
            new("medication-safety", "Medication safety"),
        ];

        public static readonly IReadOnlyList<TrainingTrainer> Trainers =
        [
            new("rn-okafor", "Grace Okafor, RN"),
            new("rn-lindqvist", "Erik Lindqvist, RN"),
        ];

        public static readonly IReadOnlyList<CompletedTraining> History =
        [
            new("Infection control", "September 12", "Grace Okafor, RN"),
            new("Medication safety", "August 3", "Erik Lindqvist, RN"),
        ];
    }

    public sealed record StaffTrainingRecorded(string Confirmation);
}
