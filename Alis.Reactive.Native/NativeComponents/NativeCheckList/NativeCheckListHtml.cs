using System;
using System.Collections.Generic;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a check list bound to a member of its model.
/// </summary>
public static class NativeCheckListHtml
{
    /// <summary>Renders a check list for the field's member.</summary>
    /// <remarks>
    /// A form's own submission sends a member the app's JSON writes as a list as one entry for each checked box,
    /// and a member of text as the checked values joined by commas.
    /// </remarks>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <returns>The rendered check list, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckList, TModel> NativeCheckList<TModel, TValue>(this InputField<TModel, TValue> field)
        => throw new NotImplementedException();

    /// <summary>Renders a check list for the field's member, a box for each option, each box checked as the member holds its value.</summary>
    /// <remarks>
    /// A form's own submission sends a member the app's JSON writes as a list as one entry for each checked box,
    /// and a member of text as the checked values joined by commas.
    /// </remarks>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <param name="build">The list's options, and what the view sets on it.</param>
    /// <returns>The rendered check list, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckList, TModel> NativeCheckList<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeCheckListBuilder> build)
        => throw new NotImplementedException();
}

/// <summary>
/// The options of a check list a view renders, and what the view sets on it.
/// </summary>
public sealed class NativeCheckListBuilder
{
    internal NativeCheckListBuilder()
    {
    }

    /// <summary>Adds a box for each option.</summary>
    /// <param name="items">The options.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Items(IEnumerable<CheckListItem> items) => throw new NotImplementedException();

    /// <summary>Adds a box for an option with a value alone.</summary>
    /// <param name="value">The option's value.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value) => throw new NotImplementedException();

    /// <summary>Adds a box for an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value, string text) => throw new NotImplementedException();

    /// <summary>Adds a box for an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value, string text, string description) => throw new NotImplementedException();

    /// <summary>Sets the CSS classes of the list's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder CssClass(string names) => throw new NotImplementedException();

    /// <summary>Sets the CSS classes of each option's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder OptionCssClass(string names) => throw new NotImplementedException();
}

/// <summary>
/// An option of a check list: a value, a text, and a description that is null when none is given.
/// </summary>
public sealed class CheckListItem
{
    /// <summary>Makes an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    public CheckListItem(string value, string text)
    {
        Value = value;
        Text = text;
    }

    /// <summary>Makes an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    public CheckListItem(string value, string text, string description)
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
