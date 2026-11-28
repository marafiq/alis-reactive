using System;
using System.Collections.Generic;
using System.Linq;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a radio group bound to a member of its model.
/// </summary>
public static partial class NativeRadioGroupHtml
{
    /// <summary>Renders a radio group for the field's member.</summary>
    /// <param name="field">The input field the group is rendered for.</param>
    /// <returns>The rendered radio group, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeRadioGroup, TModel> NativeRadioGroup<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.NativeRadioGroup(_ => { });

    /// <summary>Renders a radio group for the field's member, a radio for each option, the radio of the member's value checked.</summary>
    /// <param name="field">The input field the group is rendered for.</param>
    /// <param name="build">The group's options, and what the view sets on it.</param>
    /// <returns>The rendered radio group, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeRadioGroup, TModel> NativeRadioGroup<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeRadioGroupBuilder> build)
    {
        var builder = new NativeRadioGroupBuilder();
        build(builder);
        return MvcRadioGroup(field, builder.RadiosUnder(field.Id), builder);
    }

    private static partial Rendered<NativeRadioGroup, TModel> MvcRadioGroup<TModel, TValue>(InputField<TModel, TValue> field, IReadOnlyList<GroupRadio> radios, NativeRadioGroupBuilder group);

    private static Dictionary<string, string> GroupInputAttributes<TModel, TValue>(InputField<TModel, TValue> field, IReadOnlyList<GroupRadio> radios)
        => new() { ["type"] = "hidden", ["id"] = field.Id, ["data-native-radio-group"] = field.Json(radios.Select(radio => radio.Id).ToList()) };
}

/// <summary>
/// The options of a radio group a view renders, and what the view sets on it.
/// </summary>
public sealed class NativeRadioGroupBuilder
{
    private readonly List<RadioButtonItem> _options = [];

    internal NativeRadioGroupBuilder()
    {
    }

    internal Dictionary<string, string> HolderAttributes { get; } = new();

    internal Dictionary<string, string> OptionAttributes { get; } = new();

    /// <summary>Adds a radio for each option.</summary>
    /// <param name="items">The options.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Items(IEnumerable<RadioButtonItem> items)
    {
        foreach (var item in items ?? throw new ArgumentNullException(nameof(items))) Adds(item ?? throw new ArgumentException("An option of a radio group is null.", nameof(items)));
        return this;
    }

    /// <summary>Adds a radio for an option with a value alone.</summary>
    /// <param name="value">The option's value.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value) => Adds(new RadioButtonItem(value, value));

    /// <summary>Adds a radio for an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value, string text) => Adds(new RadioButtonItem(value, text));

    /// <summary>Adds a radio for an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder Option(string value, string text, string description) => Adds(new RadioButtonItem(value, text, description));

    /// <summary>Sets the CSS classes of the element that holds the group's radios.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder CssClass(string names)
    {
        HolderAttributes["class"] = names ?? throw new ArgumentNullException(nameof(names));
        return this;
    }

    /// <summary>Sets the CSS classes of each option's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeRadioGroupBuilder OptionCssClass(string names)
    {
        OptionAttributes["class"] = names ?? throw new ArgumentNullException(nameof(names));
        return this;
    }

    internal IReadOnlyList<GroupRadio> RadiosUnder(string groupId) => _options.Select((option, place) => new GroupRadio(option, $"{groupId}-{place}")).ToList();

    private NativeRadioGroupBuilder Adds(RadioButtonItem option)
    {
        _options.Add(option);
        return this;
    }
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
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Text = text ?? throw new ArgumentNullException(nameof(text));
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

internal sealed class GroupRadio
{
    internal GroupRadio(RadioButtonItem option, string id)
    {
        Option = option;
        Id = id;
    }

    internal RadioButtonItem Option { get; }

    internal string Id { get; }

    internal string DescriptionId => $"{Id}-description";

    internal Dictionary<string, object> Attributes => Option.Description is null
        ? new() { ["id"] = Id }
        : new() { ["id"] = Id, ["aria-describedby"] = DescriptionId };
}
