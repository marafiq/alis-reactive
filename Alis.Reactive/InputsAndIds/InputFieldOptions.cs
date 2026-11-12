using System;

namespace Alis.Reactive.InputsAndIds;

/// <summary>
/// What an input field renders around its component: its label and its required marker, or nothing.
/// </summary>
public sealed class InputFieldOptions
{
    internal InputFieldOptions()
    {
    }

    /// <summary>Gives the field its label.</summary>
    /// <param name="text">The label's text.</param>
    /// <returns>The options.</returns>
    public InputFieldOptions Label(string text) => throw new NotImplementedException();

    /// <summary>Renders the field's required marker.</summary>
    /// <returns>The options.</returns>
    public InputFieldOptions Required() => throw new NotImplementedException();

    /// <summary>Renders the field's component alone: no label, no required marker and no message element.</summary>
    /// <remarks>
    /// A request sends a hidden field and a check reads it as any input. Its failure shows in the request's
    /// summary place only, since it has no message element of its own.
    /// </remarks>
    /// <returns>The options.</returns>
    public InputFieldOptions Hidden() => throw new NotImplementedException();
}
