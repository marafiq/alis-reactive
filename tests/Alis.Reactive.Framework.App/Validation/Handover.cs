
namespace Alis.Reactive.Framework.App.Validation;

public interface IShiftNote
{
    string? Resident { get; }
}

public sealed class HandoverNote : IShiftNote
{
    public string? Resident { get; set; }

    public string? Note { get; set; }
}

public sealed class IncidentNote : IShiftNote
{
    public string? Resident { get; set; }
}

public sealed class HandoverRules : ReactiveValidator<HandoverNote>
{
    public HandoverRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
    }
}

public sealed class HandoverDraftRules : ReactiveValidator<HandoverNote>
{
    public HandoverDraftRules()
    {
        RuleFor(x => x.Note).NotEmpty().WithMessage("Write the note.");
    }
}

public sealed class NoteRules<TNote> : ReactiveValidator<TNote> where TNote : class, IShiftNote
{
    public NoteRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Name the resident.");
    }
}

public enum HandoverMistake
{
    DraftAndHandoverRules,
    OneRulesForTwoNotes,
}
