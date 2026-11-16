using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// The doors of a view to a check list bound to a member of its model.
/// </summary>
public static partial class NativeCheckListHtml
{
    /// <summary>Renders a check list for the field's member.</summary>
    /// <remarks>
    /// A form's own submission sends a member the app's JSON writes as a list as one entry for each checked box,
    /// and a member of text as the checked values joined by commas.
    /// </remarks>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <returns>The rendered check list, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckList, TModel> NativeCheckList<TModel, TValue>(this InputField<TModel, TValue> field)
        => field.NativeCheckList(_ => { });

    /// <summary>Renders a check list for the field's member, a box for each option, each box checked as the member holds its value.</summary>
    /// <remarks>
    /// A form's own submission sends a member the app's JSON writes as a list as one entry for each checked box,
    /// and a member of text as the checked values joined by commas.
    /// </remarks>
    /// <param name="field">The input field the list is rendered for.</param>
    /// <param name="build">The list's options, and what the view sets on it.</param>
    /// <returns>The rendered check list, which writes its markup where the view puts it.</returns>
    public static Rendered<NativeCheckList, TModel> NativeCheckList<TModel, TValue>(this InputField<TModel, TValue> field, Action<NativeCheckListBuilder> build)
    {
        var builder = new NativeCheckListBuilder();
        build(builder);
        return MvcCheckList(field, MarkupOf(field, builder));
    }

    private static partial Rendered<NativeCheckList, TModel> MvcCheckList<TModel, TValue>(InputField<TModel, TValue> field, CheckListMarkup list);

    private static CheckListMarkup MarkupOf<TModel, TValue>(InputField<TModel, TValue> field, NativeCheckListBuilder builder)
    {
        var eachBoxSends = field.BindsAList;
        var sendsJoinedText = !eachBoxSends;
        var boxes = builder.BoxesUnder(field.Id, HeldIn(field.ValueJson), eachBoxSends, field.Name);
        var joinedId = $"{field.Id}-joined";
        var joined = new Dictionary<string, string>
        {
            ["type"] = "hidden",
            ["id"] = joinedId,
            ["name"] = field.Name,
            ["value"] = string.Join(",", boxes.Where(box => box.IsChecked).Select(box => box.Option.Value)),
        };
        var start = new CheckListStart(boxes.Select(box => box.Id).ToList(), sendsJoinedText ? joinedId : null);
        var holder = new Dictionary<string, string>(builder.HolderAttributes) { ["id"] = field.Id, ["data-native-check-list"] = field.Json(start) };
        return new CheckListMarkup(holder, builder.OptionAttributes, boxes, sendsJoinedText, joined);
    }

    private static HashSet<string> HeldIn(string valueJson)
    {
        using var held = JsonDocument.Parse(valueJson);
        var value = held.RootElement;
        return value.ValueKind switch
        {
            JsonValueKind.Array => new HashSet<string>(value.EnumerateArray().Select(item => item.ToString())),
            JsonValueKind.Null => [],
            _ => new HashSet<string>(value.ToString().Split(',')),
        };
    }
}

/// <summary>
/// The options of a check list a view renders, and what the view sets on it.
/// </summary>
public sealed class NativeCheckListBuilder
{
    private readonly List<CheckListItem> _options = [];

    internal NativeCheckListBuilder()
    {
    }

    internal Dictionary<string, string> HolderAttributes { get; } = new();

    internal Dictionary<string, string> OptionAttributes { get; } = new();

    /// <summary>Adds a box for each option.</summary>
    /// <param name="items">The options.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Items(IEnumerable<CheckListItem> items)
    {
        foreach (var item in items ?? throw new ArgumentNullException(nameof(items))) Adds(item ?? throw new ArgumentException("An option of a check list is null.", nameof(items)));
        return this;
    }

    /// <summary>Adds a box for an option with a value alone.</summary>
    /// <param name="value">The option's value.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value) => Adds(new CheckListItem(value, value));

    /// <summary>Adds a box for an option with a value and a text.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value, string text) => Adds(new CheckListItem(value, text));

    /// <summary>Adds a box for an option with a value, a text and a description.</summary>
    /// <param name="value">The option's value.</param>
    /// <param name="text">The option's text.</param>
    /// <param name="description">The option's description.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder Option(string value, string text, string description) => Adds(new CheckListItem(value, text, description));

    /// <summary>Sets the CSS classes of the list's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder CssClass(string names)
    {
        HolderAttributes["class"] = names ?? throw new ArgumentNullException(nameof(names));
        return this;
    }

    /// <summary>Sets the CSS classes of each option's element.</summary>
    /// <param name="names">The class names, separated by spaces.</param>
    /// <returns>The builder.</returns>
    public NativeCheckListBuilder OptionCssClass(string names)
    {
        OptionAttributes["class"] = names ?? throw new ArgumentNullException(nameof(names));
        return this;
    }

    internal IReadOnlyList<ListBox> BoxesUnder(string listId, HashSet<string> held, bool eachBoxSends, string bindingName)
        => _options.Select((option, place) => new ListBox(option, $"{listId}-{place}", held.Contains(option.Value), eachBoxSends, bindingName)).ToList();

    private NativeCheckListBuilder Adds(CheckListItem option)
    {
        _options.Add(option);
        return this;
    }
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
        Value = value ?? throw new ArgumentNullException(nameof(value));
        Text = text ?? throw new ArgumentNullException(nameof(text));
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

internal sealed class ListBox
{
    internal ListBox(CheckListItem option, string id, bool isChecked, bool sends, string bindingName)
    {
        Option = option;
        Id = id;
        IsChecked = isChecked;
        Attributes = new() { ["type"] = "checkbox", ["id"] = id, ["value"] = option.Value };
        if (sends) Attributes["name"] = bindingName;
        if (isChecked) Attributes["checked"] = "checked";
        if (option.Description is not null) Attributes["aria-describedby"] = DescriptionId;
    }

    internal CheckListItem Option { get; }

    internal string Id { get; }

    internal bool IsChecked { get; }

    internal string DescriptionId => $"{Id}-description";

    internal Dictionary<string, string> Attributes { get; }
}

internal sealed class CheckListMarkup
{
    internal CheckListMarkup(Dictionary<string, string> holderAttributes, Dictionary<string, string> optionAttributes, IReadOnlyList<ListBox> boxes, bool sendsJoinedText, Dictionary<string, string> joinedAttributes)
    {
        HolderAttributes = holderAttributes;
        OptionAttributes = optionAttributes;
        Boxes = boxes;
        SendsJoinedText = sendsJoinedText;
        JoinedAttributes = joinedAttributes;
    }

    internal Dictionary<string, string> HolderAttributes { get; }

    internal Dictionary<string, string> OptionAttributes { get; }

    internal IReadOnlyList<ListBox> Boxes { get; }

    internal bool SendsJoinedText { get; }

    internal Dictionary<string, string> JoinedAttributes { get; }
}

internal sealed class CheckListStart
{
    internal CheckListStart(IReadOnlyList<string> boxes, string? joined)
    {
        Boxes = boxes;
        Joined = joined;
    }

    [JsonPropertyName("boxes")]
    public IReadOnlyList<string> Boxes { get; }

    [JsonPropertyName("joined")]
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? Joined { get; }
}
