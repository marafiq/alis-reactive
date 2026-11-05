namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Staff training: the coordinator records a caregiver's training in the drawer, picking courses,
    /// a date and a trainer from lists and a calendar that open over the form.
    /// </summary>
    public class StaffTrainingPageModel
    {
    }

    /// <summary>The training record the coordinator fills in the drawer.</summary>
    public class StaffTrainingRecordModel
    {
        public string? Notes { get; set; }
        public string[]? Courses { get; set; }
        public DateTime? CompletedOn { get; set; }
        public string? TrainerId { get; set; }
    }

    /// <summary>A course a caregiver can complete.</summary>
    public sealed record TrainingCourse(string Value, string Text);

    /// <summary>A trainer who runs courses.</summary>
    public sealed record TrainingTrainer(string Id, string Name);

    /// <summary>The courses and trainers the training form offers.</summary>
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
    }

    /// <summary>The confirmation the server returns once the training is recorded.</summary>
    public sealed record StaffTrainingRecorded(string Confirmation);
}
