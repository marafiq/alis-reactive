using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.TextArea
{
    /// <summary>The day shift's entry in Margaret Hale's daily care log.</summary>
    public class DailyCareLogModel
    {
        /// <summary>What the night shift wrote; the day shift may start from it.</summary>
        public string NightShiftNote { get; set; } = "";

        public string? CareNote { get; set; }

        /// <summary>The caregiver signs the note as complete; a signed note is locked.</summary>
        public bool Signed { get; set; }
    }

    // A note reaches the daily log only written and signed. Both rules hold in the browser and on the server.
    public class DailyCareLogValidator : ReactiveValidator<DailyCareLogModel>
    {
        public DailyCareLogValidator()
        {
            RuleFor(x => x.CareNote)
                .NotEmpty().WithMessage("Write how Margaret's day went before saving.")
                .MaximumLength(1000).WithMessage("Keep the note under 1,000 characters.");
            RuleFor(x => x.Signed).Equal(true).WithMessage("Sign the note before saving it.");
        }
    }

    /// <summary>The draft kept each time the caregiver leaves the note.</summary>
    public sealed class CareLogDraft
    {
        public string? Note { get; set; }
    }

    /// <summary>The note sent to the daily log.</summary>
    public sealed class CareLogEntry
    {
        public string? CareNote { get; set; }

        public bool Signed { get; set; }
    }

    /// <summary>What the daily log answers.</summary>
    public sealed class CareLogAnswer
    {
        public string Message { get; set; } = "";
    }
}
