namespace Alis.Reactive.Framework.App.Requests;

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

public sealed class SignInRequest
{
    public string? Visitor { get; set; }

    public int Party { get; set; }

    public string? Note { get; set; }
}

public sealed class PassRequest
{
    public int PassID { get; set; }
}

public sealed class PartyRequest
{
    public int? Party { get; set; }
}

public sealed class LineNotes
{
    public IEnumerable<LineNote>? Lines { get; set; }
}

public sealed class LineNote
{
    public string? Note { get; set; }
}

public sealed class DoseRecord
{
    public Dictionary<int, string>? Doses { get; set; }
}

public sealed class GuideExtras
{
    public object? Guide { get; set; }
}
