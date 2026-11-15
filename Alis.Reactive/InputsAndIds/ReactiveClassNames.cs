using System;
using System.Linq;

namespace Alis.Reactive.InputsAndIds;

/// <summary>
/// The class names every input field renders with: its wrapper, its label, its required marker, its message element,
/// and the class an invalid input takes while its message shows.
/// </summary>
/// <remarks>
/// Choose them once at startup with <c>services.AddReactiveClassNames(names =&gt; ..)</c>, so the app's own
/// stylesheet styles its fields. A name the app leaves unset keeps its default, and an app that never calls it
/// renders every default.
/// </remarks>
public sealed class ReactiveClassNames
{
    private string _field = "alis-field";
    private string _fieldLabel = "alis-field__label";
    private string _fieldRequired = "alis-field__required";
    private string _fieldMessage = "alis-field__message";
    private string _invalidInput = "alis-has-error";

    internal ReactiveClassNames()
    {
    }

    /// <summary>Gets or sets the classes of the wrapper around a field's label, component and message. Default <c>alis-field</c>.</summary>
    public string Field
    {
        get => _field;
        set => _field = Classes(value, nameof(Field));
    }

    /// <summary>Gets or sets the classes of a field's label. Default <c>alis-field__label</c>.</summary>
    public string FieldLabel
    {
        get => _fieldLabel;
        set => _fieldLabel = Classes(value, nameof(FieldLabel));
    }

    /// <summary>Gets or sets the classes of the marker beside a required field's label. Default <c>alis-field__required</c>.</summary>
    public string FieldRequired
    {
        get => _fieldRequired;
        set => _fieldRequired = Classes(value, nameof(FieldRequired));
    }

    /// <summary>Gets or sets the classes of the element a field's message shows in. Default <c>alis-field__message</c>.</summary>
    public string FieldMessage
    {
        get => _fieldMessage;
        set => _fieldMessage = Classes(value, nameof(FieldMessage));
    }

    /// <summary>
    /// Gets or sets the one class an invalid input's element takes while its message shows, and loses once the input
    /// is corrected. Default <c>alis-has-error</c>.
    /// </summary>
    public string InvalidInput
    {
        get => _invalidInput;
        set => _invalidInput = OneClass(value, nameof(InvalidInput));
    }

    private static string Classes(string names, string chosen)
    {
        if (!string.IsNullOrWhiteSpace(names)) return names;
        throw new ArgumentException($"{chosen} needs a class name; leave it unset to keep its default.");
    }

    private static string OneClass(string name, string chosen)
    {
        var isOneClass = !string.IsNullOrWhiteSpace(name) && !name.Any(char.IsWhiteSpace);
        if (isOneClass) return name;
        throw new ArgumentException($"{chosen} is one class name, which an invalid input takes and loses on its own; '{name}' is not one.");
    }
}
