using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class MedicationList : Component
    {
        public static ComponentVendor MedicationListVendor { get; } = ComponentVendor.Define(
            "medicationlist",
            objectPath: "medicationList",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => MedicationListVendor;

        public ReadOnly<IReadOnlyList<MedicationItem>> Items => ReadOnly<IReadOnlyList<MedicationItem>>(name: "lines");

        public Objects<MedicationLine> Lines => Objects<MedicationLine>();

        public ReadOnly<int> Count => ReadOnly<int>();

        public Property<IReadOnlyList<PharmacyDelivery>> Deliveries => Property<IReadOnlyList<PharmacyDelivery>>();

        public void Add(Arg<string> name, Arg<int> units, Arg<MedicationRoute> route) => Method(name, units, route);

        public void Remove(Arg<string> name) => Method(name);

        public void Clear() => Method();

        public void ReconcileAll() => Method();

        public Event<MedicationListChangedArgs> Changed => Event<MedicationListChangedArgs>();
    }

    public sealed class MedicationLine : JsObject
    {
        public ReadOnly<string> Name => ReadOnly<string>();

        public Property<int> Units => Property<int>();

        public ReadOnly<MedicationRoute> Route => ReadOnly<MedicationRoute>();

        public ReadOnly<bool> Controlled => ReadOnly<bool>();

        public ReadOnly<bool> Reconciled => ReadOnly<bool>();

        public void Reconcile() => Method();

        public TypedSource<bool> IsDue() => Method<bool>();
    }

    public sealed class MedicationListChangedArgs : JsObject
    {
        public ReadOnly<string> Changed => ReadOnly<string>();

        public ReadOnly<List<MedicationItem>> Items => ReadOnly<List<MedicationItem>>();

        public ReadOnly<int> Count => ReadOnly<int>();
    }

    public sealed class MedicationItem
    {
        public string Name { get; set; } = "";

        public int Units { get; set; }

        public decimal DailyCost { get; set; }

        public double HoursApart { get; set; }

        public MedicationRoute Route { get; set; }

        public DateTime ScheduledAt { get; set; }

        public bool Controlled { get; set; }

        public bool Reconciled { get; set; }
    }

    public sealed class PharmacyDelivery
    {
        public string Pharmacy { get; set; } = "";

        public DateTime DeliveredAt { get; set; }

        public List<string> Lines { get; set; } = new List<string>();
    }

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

    public static class MedicationListHtmlExtensions
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

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
