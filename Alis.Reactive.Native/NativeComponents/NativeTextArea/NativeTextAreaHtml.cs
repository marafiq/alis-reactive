using System;
using System.Collections.Generic;
using System.Globalization;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a text area bound to a member of its model.
/// </summary>
public static partial class NativeTextAreaHtml
{
    /// <summary>Renders a text area for the field's member, holding the member's text.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <returns>The rendered text area, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeTextArea, TModel> NativeTextArea<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.NativeTextArea(_ => { });

    /// <summary>Renders a text area for the field's member, holding the member's text, with what the view sets on it.</summary>
    /// <param name="field">The input field the area is rendered for.</param>
    /// <param name="build">What the view sets on the area.</param>
    /// <returns>The rendered text area, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeTextArea, TModel> NativeTextArea<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeTextAreaBuilder> build)
    {
        var builder = new NativeTextAreaBuilder();
        build(builder);
        return MvcTextArea(field, builder.AttributesUnder(field.Id));
    }

    private static partial Rendered<NativeTextArea, TModel> MvcTextArea<TModel, TValue>(InputField<TModel, TValue> field, Dictionary<string, object> attributes);
}

/// <summary>
/// What a view sets on a text area it renders.
/// </summary>
public sealed class NativeTextAreaBuilder
{
    private readonly Dictionary<string, object> _attributes = new();

    internal NativeTextAreaBuilder()
    {
    }

    /// <summary>Sets the number of text lines the area shows.</summary>
    /// <param name="rows">The number of lines.</param>
    /// <returns>The builder.</returns>
    public NativeTextAreaBuilder Rows(int rows) => Sets("rows", rows.ToString(CultureInfo.InvariantCulture));

    /// <summary>Shows a text in the area while it is empty.</summary>
    /// <param name="text">The text to show.</param>
    /// <returns>The builder.</returns>
    public NativeTextAreaBuilder Placeholder(string text) => Sets("placeholder", text ?? throw new ArgumentNullException(nameof(text)));

    /// <summary>Sets the CSS classes of the area's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeTextAreaBuilder CssClass(string names) => Sets("class", names ?? throw new ArgumentNullException(nameof(names)));

    internal Dictionary<string, object> AttributesUnder(string id) => new(_attributes) { ["id"] = id };

    private NativeTextAreaBuilder Sets(string attribute, string value)
    {
        _attributes[attribute] = value;
        return this;
    }
}
