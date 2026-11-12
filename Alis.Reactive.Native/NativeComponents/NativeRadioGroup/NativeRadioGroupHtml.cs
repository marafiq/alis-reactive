using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a radio group bound to a member of its model.
/// </summary>
public static class NativeRadioGroupHtml
{
    /// <summary>Renders a radio group for the field's member.</summary>
    /// <param name="field">The input field the group is rendered for.</param>
    /// <returns>The rendered radio group, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeRadioGroup, TModel> NativeRadioGroup<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a radio group for the field's member, a radio for each option, the radio of the member's value checked.</summary>
    /// <param name="field">The input field the group is rendered for.</param>
    /// <param name="build">The group's options, and what the view sets on it.</param>
    /// <returns>The rendered radio group, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeRadioGroup, TModel> NativeRadioGroup<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeRadioGroupBuilder> build)
        => throw new NotImplementedException();
}

/// <summary>
/// The options of a radio group a view renders, and what the view sets on it.
/// </summary>
public sealed class NativeRadioGroupBuilder
{
    internal NativeRadioGroupBuilder()
    {
    }

    /// <summary>Adds a radio for each option.</summary>
    /// <param name="items">The options.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Items(IEnumerable<RadioButtonItem> items) => throw new NotImplementedException();

    /// <summary>Adds a radio for an option with a value alone.</summary>
    /// <param name="value">The option's value.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value) => throw new NotImplementedException();

    /// <summary>Adds a radio for an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value, string text) => throw new NotImplementedException();

    /// <summary>Adds a radio for an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value, string text, string description) => throw new NotImplementedException();

    /// <summary>Sets the CSS classes of the group's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder CssClass(string names) => throw new NotImplementedException();

    /// <summary>Sets the CSS classes of each option's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder OptionCssClass(string names) => throw new NotImplementedException();
}

/// <summary>
/// An option of a radio group: a value, a text, and a description that is null when none is given.
/// </summary>
public sealed class RadioButtonItem
{
    /// <summary>Makes an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    public RadioButtonItem(string value, string text)
    {
        Value = value;
        Text = text;
    }

    /// <summary>Makes an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    public RadioButtonItem(string value, string text, string description)
        : this(value, text)
    {
        Description = description ?? throw new ArgumentNullException(nameof(description));
    }

    /// <summary>Gets the option's value.</summary>
    public string Value { get; }

    /// <summary>Gets the option's text.</summary>
    public string Text { get; }

    /// <summary>Gets the option's description, null when none is given.</summary>
    public string? Description { get; }
}
