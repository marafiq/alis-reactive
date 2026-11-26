using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class BedNotesPage(bool onTheEveningShift)
{
    public bool OnTheEveningShift { get; } = onTheEveningShift;

    public string? Shift { get; set; }

    public string ShiftOnDuty => OnTheEveningShift ? "Evening" : string.Empty;
}

public sealed class BedNote
{
    public string? Ward { get; set; }

    public string? Shift { get; set; }

    public string? Text { get; set; }
}

public sealed class EveningBedNote
{
    public string? Ward { get; set; }

    public string? Shift { get; set; } = "Evening";

    public string? Text { get; set; }
}

public sealed class BedHandover
{
    public string? Ward { get; set; }

    public string? Shift { get; set; }

    public string? Text { get; set; }
}

public sealed class BedNoteRules : ReactiveValidator<BedNote>
{
    public BedNoteRules()
    {
        RuleFor(note => note.Ward).MaximumLength(5).WithMessage("A ward's name holds 5 letters at most.");
        RuleFor(note => note.Shift).MaximumLength(5).WithMessage("A shift's name holds 5 letters at most.");
    }
}

public sealed class EveningBedNoteRules : ReactiveValidator<EveningBedNote>
{
    public EveningBedNoteRules()
    {
        RuleFor(note => note.Ward).MaximumLength(5).WithMessage("A ward's name holds 5 letters at most.");
        RuleFor(note => note.Shift).MaximumLength(5).WithMessage("A shift's name holds 5 letters at most.");
    }
}

public sealed class BedHandoverRules : ReactiveValidator<BedHandover>
{
    public BedHandoverRules()
    {
        RuleFor(handover => handover.Shift).NotNull().WithMessage("Name the shift.");
    }
}

public static class BedNoteEndpoints
{
    public const string Notes = CarePages.BedNotes + "/wards/{ward}/notes";

    public const string Photos = CarePages.BedNotes + "/wards/{ward}/photos";

    public const string SavedNotes = CarePages.BedNotes + "/wards/{ward}/saved-notes";

    public const string EveningSavedNotes = CarePages.BedNotesOnTheEveningShift + "/wards/{ward}/saved-notes";

    public const string Handovers = CarePages.BedNotes + "/wards/{ward}/handovers";
}

public static class BedNoteAnswers
{
    public static string Of(string? ward, string? shift) => shift is null ? ward ?? string.Empty : $"{ward}, {shift}";
}
