namespace Alis.Reactive.Framework.App.Requests;

/// <summary>A visitor signing in at the front desk: inputs a request gathers by name, into the classes the desk's endpoints bind.</summary>
public sealed class SignInPage
{
    public string? Visitor { get; set; }

    public int Party { get; set; }

    public string? Note { get; set; }

    public int PassId { get; set; }

    public List<SignInLine> Lines { get; set; } = [new()];

    public Dictionary<int, string?> Doses { get; set; } = new() { [1042] = null };

    public SignInGuide Guide { get; set; } = new();
}

public sealed class SignInLine
{
    public string? Note { get; set; }
}

public sealed class SignInGuide
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

/// <summary>The sign-in the desk binds: the visitor, the party, and a note.</summary>
public sealed class SignInRequest
{
    public string? Visitor { get; set; }

    public int Party { get; set; }

    public string? Note { get; set; }
}

/// <summary>The visitor's pass, named in capitals the way the badge system names it.</summary>
public sealed class PassRequest
{
    public int PassID { get; set; }
}

/// <summary>The party, which the desk's head count may leave empty.</summary>
public sealed class PartyRequest
{
    public int? Party { get; set; }
}

/// <summary>The lines of a sign-in, as a sequence of notes.</summary>
public sealed class LineNotes
{
    public IEnumerable<LineNote>? Lines { get; set; }
}

public sealed class LineNote
{
    public string? Note { get; set; }
}

/// <summary>The doses given, by resident number.</summary>
public sealed class DoseRecord
{
    public Dictionary<int, string>? Doses { get; set; }
}

/// <summary>Whatever the desk keeps about the guide, as it arrives.</summary>
public sealed class GuideExtras
{
    public object? Guide { get; set; }
}
