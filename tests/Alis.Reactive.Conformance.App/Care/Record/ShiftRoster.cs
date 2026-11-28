using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class Roster
{
    public List<ShiftRow> Rows { get; set; } = [];

    public bool Approved { get; set; }

    public List<string> Saved { get; set; } = [];
}

public sealed class ShiftRow
{
    public int Id { get; set; }

    public string? Carer { get; set; }

    public int Hours { get; set; }
}

public sealed class ShiftRota
{
    public List<ShiftRow> Rows { get; set; } = [];
}

public sealed class ShiftSheet
{
    public List<ShiftRow> Rows { get; set; } = [];
}

public sealed class ShiftNote
{
    public string? Note { get; set; }
}

public sealed class ShiftRules : ReactiveValidator<ShiftRow>
{
    public const string Approval = nameof(Approval);

    public ShiftRules()
    {
        RuleFor(shift => shift.Carer).NotEmpty().WithMessage("Choose a carer.");
        RuleFor(shift => shift.Hours).InclusiveBetween(1, 12).WithMessage("A shift is 1 to 12 hours.");
        RuleSet(Approval, () => RuleFor(shift => shift.Hours).LessThanOrEqualTo(8).WithMessage("An approved shift is 8 hours at most."));
    }
}

public sealed class RosterRules : ReactiveValidator<Roster>
{
    public RosterRules() => RuleSet("default, " + ShiftRules.Approval, () => RuleForEach(roster => roster.Rows).SetValidator(new ShiftRules()));
}

internal sealed partial class ShiftRosterBook
{
    private readonly object _gate = new();
    private Roster _roster = new()
    {
        Rows =
        [
            new ShiftRow { Id = 5, Carer = "Ben", Hours = 6 },
            new ShiftRow { Id = 6, Carer = "Ann", Hours = 8 },
            new ShiftRow { Id = 7, Hours = 8 },
        ],
    };

    internal static ShiftRosterBook Open() => new();

    internal Roster Roster
    {
        get
        {
            lock (_gate) return _roster;
        }
    }

    internal Roster Approve(List<ShiftRow> rows)
    {
        var approved = new Roster { Rows = rows, Approved = true };
        lock (_gate) _roster = approved;
        return approved;
    }

    internal void Save(ShiftRow shift, IEnumerable<string> fields)
    {
        lock (_gate)
        {
            _roster = Replaced(shift);
            _roster.Saved.Add($"Shift {shift.Id}: {shift.Carer}, {shift.Hours} hours, from {string.Join(", ", fields.OrderBy(field => field, StringComparer.Ordinal))}");
        }
    }

    internal void ApproveShift(ShiftRow shift)
    {
        lock (_gate)
        {
            _roster = Replaced(shift);
            _roster.Saved.Add($"Shift {shift.Id} approved");
        }
    }

    private Roster Replaced(ShiftRow shift)
    {
        if (_roster.Rows.All(row => row.Id != shift.Id)) throw new InvalidOperationException($"The roster holds no shift {shift.Id}.");
        return new Roster
        {
            Rows = [.. _roster.Rows.Select(row => row.Id == shift.Id ? shift : row)],
            Approved = _roster.Approved,
            Saved = [.. _roster.Saved],
        };
    }
}
