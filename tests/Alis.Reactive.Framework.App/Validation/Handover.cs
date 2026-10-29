using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A note about a resident.</summary>
public interface IShiftNote
{
    string? Resident { get; }
}

/// <summary>The note one shift leaves the next: saved as a draft while it is written, then handed over.</summary>
public sealed class HandoverNote : IShiftNote
{
    public string? Resident { get; set; }

    public string? Note { get; set; }
}

/// <summary>A note of an incident, checked by rules written once for any note.</summary>
public sealed class IncidentNote : IShiftNote
{
    public string? Resident { get; set; }
}

/// <summary>The rules every handover note is checked by, whether it is saved as a draft or handed over.</summary>
public sealed class HandoverRules : ReactiveValidator<HandoverNote>
{
    public HandoverRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
    }
}

/// <summary>Rules a draft alone would be checked by: the note is written before it is handed over.</summary>
public sealed class HandoverDraftRules : ReactiveValidator<HandoverNote>
{
    public HandoverDraftRules()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Write the note.");
    }
}

/// <summary>Rules written once for any note about a resident.</summary>
public sealed class NoteRules<TNote> : ReactiveValidator<TNote> where TNote : class, IShiftNote
{
    public NoteRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
    }
}

/// <summary>The second validator a handover page names for its one form.</summary>
public enum HandoverMistake
{
    DraftAndHandoverRules,
    OneRulesForTwoNotes,
}
