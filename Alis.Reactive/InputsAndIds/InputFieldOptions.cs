namespace Alis.Reactive.InputsAndIds;

/// <summary>
/// What an input field renders around its component: its label and its required marker, or nothing.
/// </summary>
public sealed class InputFieldOptions
{
    internal InputFieldOptions()
    {
    }

    internal bool HasLabel { get; private set; }

    internal string LabelText { get; private set; } = "";

    internal bool IsRequired { get; private set; }

    internal bool IsHidden { get; private set; }

    /// <summary>Gives the field its label.</summary>
    /// <param name="text">The label's text.</param>
    /// <returns>The options.</returns>
    public InputFieldOptions Label(string text)
    {
        HasLabel = true;
        LabelText = text;
        return this;
    }

    /// <summary>Renders the field's required marker.</summary>
    /// <returns>The options.</returns>
    public InputFieldOptions Required()
    {
        IsRequired = true;
        return this;
    }

    /// <summary>Renders the field's component alone: no label, no required marker and no message element.</summary>
    /// <remarks>
    /// A request sends a hidden field and a check reads it as any input. Its failure shows in the request's
    /// summary place only, since it has no message element of its own.
    /// </remarks>
    /// <returns>The options.</returns>
    public InputFieldOptions Hidden()
    {
        IsHidden = true;
        return this;
    }
}
