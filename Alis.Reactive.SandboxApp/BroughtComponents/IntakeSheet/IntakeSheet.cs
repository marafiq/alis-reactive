using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class IntakeSheet : Component
    {
        public static ComponentVendor IntakeSheetVendor { get; } = ComponentVendor.Define(
            "intakesheet",
            objectPath: "intakeSheet",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => IntakeSheetVendor;

        public Property<string> Name => Property<string>();

        public Property<int> Age => Property<int>();

        public Property<DateTime> MoveInDate => Property<DateTime>();

        public Property<bool> Veteran => Property<bool>();

        public Property<CareLevel> CareLevel => Property<CareLevel>();

        public Property<decimal> MonthlyRate => Property<decimal>();

        public Property<string?> RoomPreference => Property<string?>();

        public Property<int?> PetCount => Property<int?>();

        public Property<DateTime?> PriorStayEnded => Property<DateTime?>();

        public Property<bool?> FluShot => Property<bool?>();

        public Property<Diet?> Diet => Property<Diet?>();

        public Property<int?> Waitlist => Property<int?>();

        public IntakeContact Contact => Object<IntakeContact>();

        public Property<IReadOnlyList<string>> Allergies => Property<IReadOnlyList<string>>();

        public ReadOnly<IEnumerable<string>> AllergySequence => ReadOnly<IEnumerable<string>>(name: "allergies");

        public TypedSource<string> Summary() => Method<string>();

        public TypedSource<int> YearsOver(Arg<int> minimum) => Method<int>(minimum);

        public TypedSource<int> DaysUntilMoveIn() => Method<int>();

        public TypedSource<int?> WaitlistPosition() => MethodAsync<int?>();

        public void Load(Arg<Applicant> applicant) => Method(applicant);

        public void Clear() => Method();

        public Event<IntakeChangedArgs> Changed => Event<IntakeChangedArgs>();
    }

    public sealed class IntakeContact : JsObject
    {
        public Property<string> Phone => Property<string>();

        public Property<string> Relation => Property<string>();
    }

    public sealed class IntakeChangedArgs : JsObject
    {
        public ReadOnly<string> Field => ReadOnly<string>();

        public ReadOnly<Applicant> Applicant => ReadOnly<Applicant>();

        public IntakeContact Contact => Object<IntakeContact>();
    }

    public sealed class Applicant
    {
        public string Name { get; set; } = "";

        public int Age { get; set; }

        public DateTime MoveInDate { get; set; }

        public bool Veteran { get; set; }

        public CareLevel CareLevel { get; set; }

        public decimal MonthlyRate { get; set; }

        public string? RoomPreference { get; set; }

        public int? PetCount { get; set; }

        public DateTime? PriorStayEnded { get; set; }

        public bool? FluShot { get; set; }

        public Diet? Diet { get; set; }

        public ApplicantContact Contact { get; set; } = new ApplicantContact();

        public List<string> Allergies { get; set; } = new List<string>();
    }

    public sealed class ApplicantContact
    {
        public string Phone { get; set; } = "";

        public string Relation { get; set; } = "";
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CareLevel
    {
        [JsonStringEnumMemberName("independent")]
        Independent,

        [JsonStringEnumMemberName("assisted")]
        Assisted,

        [JsonStringEnumMemberName("memoryCare")]
        MemoryCare,
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum Diet
    {
        [JsonStringEnumMemberName("regular")]
        Regular,

        [JsonStringEnumMemberName("diabetic")]
        Diabetic,

        [JsonStringEnumMemberName("pureed")]
        Pureed,
    }

    public static class IntakeSheetHtmlExtensions
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        public static RenderedComponent<IntakeSheet, TModel> IntakeSheet<TModel>(
            this ReactivePlan<TModel> plan,
            string elementId,
            Applicant applicant,
            int? waitlist)
            where TModel : class
        {
            var markup = new StringBuilder()
                .Append("<div class=\"intake-sheet\" id=\"").Append(Encode(elementId))
                .Append("\" data-applicant=\"").Append(Encode(JsonSerializer.Serialize(applicant, Json)))
                .Append("\" data-waitlist=\"").Append(waitlist == null ? "" : waitlist.Value.ToString(System.Globalization.CultureInfo.InvariantCulture))
                .Append("\"></div>");
            return plan.Render<IntakeSheet>(elementId, new HtmlString(markup.ToString()));
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
