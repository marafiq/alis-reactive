namespace Alis.Reactive.Framework.App.Requests;

/// <summary>
/// Names as families spell them, and a street as a German address writes it: text holding every character a
/// path, a query or a form treats as its own (a slash, an ampersand, an equals sign, a plus, a space, quotes,
/// a percent sign, a hash, a question mark) and text beyond ASCII, sent in a route, a query and form data.
/// </summary>
public sealed class SpellingsPage;

/// <summary>A resident's name, in a route, a query or a form post.</summary>
public sealed class SpelledName
{
    public string? Name { get; set; }
}

/// <summary>An address whose member is named beyond ASCII, as its form names its field.</summary>
public sealed class SpelledAddress
{
    public string? Straße { get; set; }
}
