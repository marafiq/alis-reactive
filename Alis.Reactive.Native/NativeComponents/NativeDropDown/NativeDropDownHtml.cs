using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a drop-down bound to a member of its model.
/// </summary>
public static class NativeDropDownHtml
{
    /// <summary>Renders a drop-down for the field's member.</summary>
    /// <param name="field">The input field the drop-down is rendered for.</param>
    /// <returns>The rendered drop-down, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeDropDown, TModel> NativeDropDown<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.NativeDropDown(_ => { });

    /// <summary>Renders a drop-down for the field's member, the option the member holds selected.</summary>
    /// <param name="field">The input field the drop-down is rendered for.</param>
    /// <param name="build">The drop-down's options, and what the view sets on it.</param>
    /// <returns>The rendered drop-down, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeDropDown, TModel> NativeDropDown<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeDropDownBuilder> build)
    {
        var builder = new NativeDropDownBuilder();
        build(builder);
        return builder.MvcDropDown(field);
    }
}

/// <summary>
/// The options of a drop-down a view renders, and what the view sets on it.
/// </summary>
public sealed partial class NativeDropDownBuilder
{
    private readonly Dictionary<string, object> _attributes = new();
    private bool _hasOptionLabel;
    private string _optionLabel = "";

    internal NativeDropDownBuilder()
    {
    }

    /// <summary>Adds an empty first option with a label.</summary>
    /// <param name="optionLabel">The label of the empty option.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder Placeholder(string optionLabel)
    {
        _optionLabel = optionLabel ?? throw new ArgumentNullException(nameof(optionLabel));
        _hasOptionLabel = true;
        return this;
    }

    /// <summary>Enables or disables the drop-down.</summary>
    /// <param name="enabled">Whether the drop-down is enabled.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder Enabled(bool enabled)
    {
        if (enabled) _attributes.Remove("disabled");
        else _attributes["disabled"] = "disabled";
        return this;
    }

    /// <summary>Sets the CSS classes of the drop-down's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeDropDownBuilder CssClass(string names)
    {
        _attributes["class"] = names ?? throw new ArgumentNullException(nameof(names));
        return this;
    }

    internal partial Rendered<NativeDropDown, TModel> MvcDropDown<TModel, TValue>(InputField<TModel, TValue> field);

    private Dictionary<string, object> AttributesUnder(string id) => new(_attributes) { ["id"] = id };
}
