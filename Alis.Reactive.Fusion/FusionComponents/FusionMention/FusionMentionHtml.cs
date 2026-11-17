using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2.DropDowns;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// The doors of a view to a mention.
/// </summary>
public static partial class FusionMentionHtml
{
    private const string Marker = "data-fusion-mention";

    private static readonly ConditionalWeakTable<MentionBuilder, Func<ItemNames, MentionFieldSettings>> NamedFields = new();

    /// <summary>Names the members of an item the mention shows as its text and holds as its value.</summary>
    /// <remarks>Type the first lambda's item, as <c>(Resident r) =&gt; r.Name</c>.</remarks>
    /// <param name="build">The mention's builder.</param>
    /// <param name="text">The member shown as an item's text.</param>
    /// <param name="value">The member held as an item's value.</param>
    /// <returns>The builder.</returns>
    public static MentionBuilder Fields<TItem, TText, TItemValue>(this MentionBuilder build, Expression<Func<TItem, TText>> text, Expression<Func<TItem, TItemValue>> value)
        => Named(build, names => new MentionFieldSettings { Text = names.Of(text), Value = names.Of(value) });

    private static string HostOf(string elementId) => (elementId ?? throw new ArgumentNullException(nameof(elementId))) + "-host";

    private static MentionBuilder Built<TModel>(ReactivePlan<TModel> plan, MentionBuilder mention, Action<MentionBuilder> build)
    {
        (build ?? throw new ArgumentNullException(nameof(build)))(mention);
        if (NamedFields.TryGetValue(mention, out var fields)) mention.Fields(fields(new PlanNames<TModel>(plan)));
        return mention;
    }

    private static MentionBuilder Items<TModel, TItem>(ReactivePlan<TModel> plan, MentionBuilder mention, IReadOnlyList<TItem> items)
        => mention.DataSource(FusionData.AsWritten(plan.Json(items ?? throw new ArgumentNullException(nameof(items)))));

    private static Dictionary<string, string> BridgeAttributes<TModel>(ReactivePlan<TModel> plan, string elementId)
        => new() { ["id"] = elementId, ["hidden"] = "hidden", [Marker] = plan.Json(new MentionStart(HostOf(elementId))) };

    private static MentionBuilder Named(MentionBuilder build, Func<ItemNames, MentionFieldSettings> fields)
    {
        NamedFields.Remove(build);
        NamedFields.Add(build, fields);
        return build;
    }

    private abstract class ItemNames
    {
        internal abstract string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member);
    }

    private sealed class PlanNames<TModel>(ReactivePlan<TModel> plan) : ItemNames
    {
        internal override string Of<TItem, TMember>(Expression<Func<TItem, TMember>> member) => string.Join(".", plan.JsonPath(member));
    }
}

internal sealed class MentionStart
{
    internal MentionStart(string host) => Host = host;

    [JsonPropertyName("host")]
    public string Host { get; }
}
