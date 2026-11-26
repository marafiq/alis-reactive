using System.Collections.Generic;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class HandoverNotes
{
    public List<string?> Notes { get; set; } = [];

    public string? HandedOverBy { get; set; }

    public string OnRecord { get; set; } = "";
}

public sealed class HandoverNotesRules : ReactiveValidator<HandoverNotes>
{
    public HandoverNotesRules()
    {
        RuleForEach(handover => handover.Notes).NotNull().WithMessage("Write each note.");
        RuleFor(handover => handover.HandedOverBy).NotNull().WithMessage("Name who hands over.");
    }
}

internal sealed partial class HandoverNotesBook
{
    private readonly object _gate = new();
    private string _onRecord = "";

    internal static HandoverNotesBook Open() => new();

    internal string OnRecord
    {
        get
        {
            lock (_gate) return _onRecord;
        }
    }

    internal string Record(HandoverNotes handover)
    {
        lock (_gate) _onRecord = $"{handover.HandedOverBy}: {string.Join("; ", handover.Notes)}";
        return $"{handover.Notes.Count} notes handed over by {handover.HandedOverBy}";
    }
}
