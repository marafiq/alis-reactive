using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a check box bound to a member of bool.
/// </summary>
public static partial class NativeCheckBoxHtml
{
    /// <summary>Renders a check box for the field's member, checked as the member is.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <returns>The rendered check box, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckBox, TModel> NativeCheckBox<TModel>(this InputField<TModel, bool> field)
        => field.NativeCheckBox(_ => { });

    /// <summary>Renders a check box for the field's member, checked as the member is, with what the view sets on it.</summary>
    /// <param name="field">The input field the box is rendered for.</param>
    /// <param name="build">What the view sets on the box.</param>
    /// <returns>The rendered check box, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckBox, TModel> NativeCheckBox<TModel>(this InputField<TModel, bool> field, Action<NativeCheckBoxBuilder> build)
    {
        var builder = new NativeCheckBoxBuilder();
        build(builder);
        return MvcCheckBox(field, builder.AttributesUnder(field.Id));
    }

    private static partial Rendered<NativeCheckBox, TModel> MvcCheckBox<TModel>(InputField<TModel, bool> field, Dictionary<string, object> attributes);
}

/// <summary>
/// What a view sets on a check box it renders.
/// </summary>
public sealed class NativeCheckBoxBuilder
{
    private readonly Dictionary<string, object> _attributes = new();

    internal NativeCheckBoxBuilder()
    {
    }

    /// <summary>Sets the CSS classes of the box's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeCheckBoxBuilder CssClass(string names) => Sets("class", names ?? throw new ArgumentNullException(nameof(names)));

    internal Dictionary<string, object> AttributesUnder(string id) => new(_attributes) { ["id"] = id };

    private NativeCheckBoxBuilder Sets(string attribute, string value)
    {
        _attributes[attribute] = value;
        return this;
    }
}
