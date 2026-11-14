using System;

namespace Alis.Reactive.ValuesAndSources;

/// <summary>
/// One part of a joined text: a text the view holds, or what a source of any type reads.
/// </summary>
/// <remarks>
/// C# converts a text and a <see cref="TypedSource{T}"/> to it, so a joined text is written with the parts
/// themselves. A source part shows as an element's text shows it, and a part that cannot be read or holds null adds
/// nothing.
/// </remarks>
public sealed class TextPart
{
    private TextPart()
    {
    }

    /// <summary>Joins a text, written into the plan where the view renders.</summary>
    /// <param name="text">The text; null adds nothing.</param>
    public static implicit operator TextPart(string? text) => throw new NotImplementedException();
}
