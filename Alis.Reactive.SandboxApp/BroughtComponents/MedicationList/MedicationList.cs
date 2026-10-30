using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The medication list (Scripts/medication-list.ts) is a widget the sandbox wrote itself, with no
    // knowledge of Alis.Reactive. It holds a list of objects with members of their own, the way a chart
    // holds its series. This file is everything the framework needs to drive it, written against the
    // framework's public surface only.

    /// <summary>A resident's medication list, reconciled line by line against the pharmacy's delivery.</summary>
    /// <remarks>
    /// The list is read two ways: as data, for arrays over its lines (<c>p.From(list.Items).Count(..)</c>),
    /// and as objects, each line with members of its own (<c>list.Lines.Each(line =&gt; line.Name)</c>,
    /// <c>list.Lines.At(0).Reconcile()</c>).
    /// </remarks>
    public sealed class MedicationList : Component
    {
        /// <summary>
        /// The list keeps itself on its host element as <c>medicationList</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's payload.
        /// </summary>
        public static ComponentVendor MedicationListVendor { get; } = ComponentVendor.Define(
            "medicationlist",
            objectPath: "medicationList",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => MedicationListVendor;

        /// <summary>The lines as data, for arrays over them.</summary>
        public ReadOnly<IReadOnlyList<MedicationItem>> Items => ReadOnly<IReadOnlyList<MedicationItem>>(name: "lines");

        /// <summary>The lines as objects with members of their own.</summary>
        public Objects<MedicationLine> Lines => Objects<MedicationLine>();

        /// <summary>How many lines the list holds.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();

        /// <summary>The pharmacy deliveries the page recorded against the list.</summary>
        public Property<IReadOnlyList<PharmacyDelivery>> Deliveries => Property<IReadOnlyList<PharmacyDelivery>>();

        /// <summary>Adds a line, due at 08:00 and not yet reconciled.</summary>
        /// <param name="name">The medication.</param>
        /// <param name="units">How many units each dose is.</param>
        /// <param name="route">How it is given.</param>
        public void Add(Arg<string> name, Arg<int> units, Arg<MedicationRoute> route) => Method(name, units, route);

        /// <summary>Takes a line off the list.</summary>
        /// <param name="name">The medication.</param>
        public void Remove(Arg<string> name) => Method(name);

        /// <summary>Empties the list.</summary>
        public void Clear() => Method();

        /// <summary>Marks every line reconciled.</summary>
        public void ReconcileAll() => Method();

        /// <summary>Raised once for each edit made on the list.</summary>
        public Event<MedicationListChangedArgs> Changed => Event<MedicationListChangedArgs>();
    }

    /// <summary>One line of the list: an item of <see cref="MedicationList.Lines"/>, with members of its own.</summary>
    public sealed class MedicationLine : JsObject
    {
        /// <summary>The medication.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>How many units each dose is.</summary>
        public Property<int> Units => Property<int>();

        /// <summary>How the medication is given.</summary>
        public ReadOnly<MedicationRoute> Route => ReadOnly<MedicationRoute>();

        /// <summary>Whether the medication is a controlled substance.</summary>
        public ReadOnly<bool> Controlled => ReadOnly<bool>();

        /// <summary>Whether the line has been reconciled against the delivery.</summary>
        public ReadOnly<bool> Reconciled => ReadOnly<bool>();

        /// <summary>Marks the line reconciled.</summary>
        public void Reconcile() => Method();

        /// <summary>Whether the line's dose is due on the morning pass.</summary>
        public TypedSource<bool> IsDue() => Method<bool>();
    }

    /// <summary>Payload of <see cref="MedicationList.Changed"/>.</summary>
    public class MedicationListChangedArgs : JsObject
    {
        /// <summary>The medication whose line changed; empty when the whole list did.</summary>
        public ReadOnly<string> Changed => ReadOnly<string>();

        /// <summary>The lines as they now stand.</summary>
        public ReadOnly<List<MedicationItem>> Items => ReadOnly<List<MedicationItem>>();

        /// <summary>How many lines the list holds after the change.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();
    }

    /// <summary>One line of a medication list, as data: what the list renders from and what a delivery carries.</summary>
    public sealed class MedicationItem
    {
        public string Name { get; set; } = "";

        /// <summary>How many units each dose is.</summary>
        public int Units { get; set; }

        /// <summary>What a day's doses cost, in dollars and cents.</summary>
        public decimal DailyCost { get; set; }

        /// <summary>Hours between doses.</summary>
        public double HoursApart { get; set; }

        public MedicationRoute Route { get; set; }

        /// <summary>When the first dose of the day is due.</summary>
        public DateTime ScheduledAt { get; set; }

        /// <summary>Whether the medication is a controlled substance.</summary>
        public bool Controlled { get; set; }

        /// <summary>Whether the line has been reconciled against the delivery.</summary>
        public bool Reconciled { get; set; }
    }

    /// <summary>A pharmacy's delivery, as the page records it on the list.</summary>
    public sealed class PharmacyDelivery
    {
        public string Pharmacy { get; set; } = "";

        public DateTime DeliveredAt { get; set; }

        /// <summary>The medications delivered.</summary>
        public List<string> Lines { get; set; } = new List<string>();
    }

    /// <summary>How a medication is given, by the text the list's JavaScript uses.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum MedicationRoute
    {
        [JsonStringEnumMemberName("oral")]
        Oral,

        [JsonStringEnumMemberName("injection")]
        Injection,

        [JsonStringEnumMemberName("topical")]
        Topical,
    }

    /// <summary>Renders a <see cref="MedicationList"/> for a plan.</summary>
    public static class MedicationListHtmlExtensions
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Renders the list's host with the id the page chose and the lines it opens with; the app's bundle
        /// constructs the list on the element, and the framework reaches it by id.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="plan">The plan the list joins.</param>
        /// <param name="elementId">The id the page gives the list.</param>
        /// <param name="lines">The lines the list opens with.</param>
        /// <returns>The rendered list; wire its event with <c>.Reactive(evt =&gt; evt.Changed, ...)</c>.</returns>
        public static RenderedComponent<MedicationList, TModel> MedicationList<TModel>(
            this ReactivePlan<TModel> plan,
            string elementId,
            IReadOnlyList<MedicationItem> lines)
            where TModel : class
        {
            var markup = new StringBuilder()
                .Append("<div class=\"medication-list\" id=\"").Append(Encode(elementId))
                .Append("\" data-lines=\"").Append(Encode(JsonSerializer.Serialize(lines, Json)))
                .Append("\"></div>");
            return plan.Render<MedicationList>(elementId, new HtmlString(markup.ToString()));
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
