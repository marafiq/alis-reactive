using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a text box bound to a member of its model.
/// </summary>
public static partial class NativeTextBoxHtml
{
    /// <summary>Renders a text box for the field's member, holding the member's text.</summary>
    /// <remarks>After a post that failed, the box holds the text the person sent.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered text box, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeTextBox, TModel> NativeTextBox<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.NativeTextBox(_ => { });

    /// <summary>Renders a text box for the field's member, holding the member's text, with what the view sets on it.</summary>
    /// <remarks>After a post that failed, the box holds the text the person sent.</remarks>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered text box, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeTextBox, TModel> NativeTextBox<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeTextBoxBuilder> build)
    {
        var builder = new NativeTextBoxBuilder();
        build(builder);
        return MvcTextBox(field, builder.AttributesUnder(field.Id));
    }

    private static partial Rendered<NativeTextBox, TModel> MvcTextBox<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, object> attributes);
}

/// <summary>
/// What a view sets on a text box it renders.
/// </summary>
public sealed class NativeTextBoxBuilder
{
    private readonly Dictionary<string, object> _attributes = new();

    internal NativeTextBoxBuilder()
    {
    }

    /// <summary>Shows a text in the box while it is empty.</summary>
    /// <param name="text">The text to show.</param>
    /// <returns>The builder.</returns>
    public NativeTextBoxBuilder Placeholder(string text) => Sets("placeholder", text ?? throw new ArgumentNullException(nameof(text)));

    /// <summary>Sets the CSS classes of the box's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeTextBoxBuilder CssClass(string names) => Sets("class", names ?? throw new ArgumentNullException(nameof(names)));

    internal Dictionary<string, object> AttributesUnder(string id) => new(_attributes) { ["id"] = id };

    private NativeTextBoxBuilder Sets(string attribute, string value)
    {
        _attributes[attribute] = value;
        return this;
    }
}
