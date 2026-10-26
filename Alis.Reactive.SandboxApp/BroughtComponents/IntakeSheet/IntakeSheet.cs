using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The intake sheet (Scripts/intake-sheet.ts) is a widget the sandbox wrote itself, with no knowledge
    // of Alis.Reactive. It holds an applicant's facts as every kind of value a page compares: text, a
    // number, a date, a yes or no, an option, each of those or nothing, an object and a list. This file
    // is everything the framework needs to drive it, written against the framework's public surface only.

    /// <summary>An applicant's intake sheet, filled in by the admissions coordinator.</summary>
    /// <remarks>
    /// Each member is declared by the sheet's own JavaScript name, which is the C# name camel-cased, and by
    /// the kind of member it is. A page reaches the sheet and uses those members directly:
    /// <code>
    /// var sheet = p.Component&lt;IntakeSheet&gt;("intake");
    /// p.When(sheet.Age).Gte(62).Then(..);
    /// p.When(sheet.PetCount).IsNull().Then(..);
    /// p.Element("summary").Text.Set(sheet.Summary());
    /// p.When(sheet.WaitlistPosition()).Lte(3).Then(..);
    /// </code>
    /// </remarks>
    public sealed class IntakeSheet : Component
    {
        /// <summary>
        /// The sheet keeps itself on its host element as <c>intakeSheet</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's payload.
        /// </summary>
        public static ComponentVendor IntakeSheetVendor { get; } = ComponentVendor.Define(
            "intakesheet",
            objectPath: "intakeSheet",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => IntakeSheetVendor;

        // The facts the sheet always holds: text, a number, a date, a yes or no, an option and a number with cents.

        /// <summary>The applicant's name.</summary>
        public Property<string> Name => Property<string>();

        /// <summary>The applicant's age in years.</summary>
        public Property<int> Age => Property<int>();

        /// <summary>The day the applicant moves in.</summary>
        public Property<DateTime> MoveInDate => Property<DateTime>();

        /// <summary>Whether the applicant is a veteran.</summary>
        public Property<bool> Veteran => Property<bool>();

        /// <summary>The level of care the applicant needs.</summary>
        public Property<CareLevel> CareLevel => Property<CareLevel>();

        /// <summary>The monthly rate quoted, in dollars and cents.</summary>
        public Property<decimal> MonthlyRate => Property<decimal>();

        // The facts the sheet may not hold yet: each is its kind, or nothing.

        /// <summary>The room the applicant asked for, or nothing when no preference was given.</summary>
        public Property<string?> RoomPreference => Property<string?>();

        /// <summary>How many pets the applicant brings, or nothing until asked.</summary>
        public Property<int?> PetCount => Property<int?>();

        /// <summary>When the applicant's previous stay with the community ended, or nothing for a first stay.</summary>
        public Property<DateTime?> PriorStayEnded => Property<DateTime?>();

        /// <summary>Whether the applicant had this season's flu shot, or nothing until asked.</summary>
        public Property<bool?> FluShot => Property<bool?>();

        /// <summary>The applicant's diet, or nothing when none is recorded.</summary>
        public Property<Diet?> Diet => Property<Diet?>();

        /// <summary>The applicant's number on the waiting list, or nothing while the list has not answered.</summary>
        public Property<int?> Waitlist => Property<int?>();

        /// <summary>The family contact: an object with members of its own.</summary>
        public IntakeContact Contact => Object<IntakeContact>();

        /// <summary>The applicant's allergies, as the sheet lists them.</summary>
        public Property<IReadOnlyList<string>> Allergies => Property<IReadOnlyList<string>>();

        /// <summary>The same allergies, read as a sequence.</summary>
        public ReadOnly<IEnumerable<string>> AllergySequence => ReadOnly<IEnumerable<string>>(name: "allergies");

        // Functions: a value, a value from an argument, and a promise of a value.

        /// <summary>The applicant in one line. The sheet counts each summary it writes.</summary>
        public TypedSource<string> Summary() => Method<string>();

        /// <summary>How many years the applicant is over a minimum age; negative when under it.</summary>
        /// <param name="minimum">The minimum age.</param>
        public TypedSource<int> YearsOver(Arg<int> minimum) => Method<int>(minimum);

        /// <summary>How many days until the move-in date, counted from 1 June 2026.</summary>
        public TypedSource<int> DaysUntilMoveIn() => Method<int>();

        /// <summary>
        /// Asks the waiting list for the applicant's number; the step that first uses the answer waits for
        /// it. The sheet counts each time it is asked.
        /// </summary>
        public TypedSource<int?> WaitlistPosition() => MethodAsync<int?>();

        // Methods that return nothing.

        /// <summary>Replaces every fact on the sheet with the applicant's.</summary>
        /// <param name="applicant">The applicant, written in the page or read from a response.</param>
        public void Load(Arg<Applicant> applicant) => Method(applicant);

        /// <summary>Empties the sheet.</summary>
        public void Clear() => Method();

        /// <summary>Raised once for each edit the coordinator commits on the sheet.</summary>
        public Event<IntakeChangedArgs> Changed => Event<IntakeChangedArgs>();
    }

    /// <summary>The sheet's <c>contact</c>: the family member to call.</summary>
    public sealed class IntakeContact : JsObject
    {
        /// <summary>The contact's phone number.</summary>
        public Property<string> Phone => Property<string>();

        /// <summary>How the contact is related to the applicant.</summary>
        public Property<string> Relation => Property<string>();
    }

    /// <summary>Payload of <see cref="IntakeSheet.Changed"/>.</summary>
    public class IntakeChangedArgs : JsObject
    {
        /// <summary>The field the coordinator edited, by its JavaScript name.</summary>
        public ReadOnly<string> Field => ReadOnly<string>();

        /// <summary>The applicant's facts as the sheet now holds them; read into them with <c>args.Applicant.Get(a =&gt; a.Age)</c>.</summary>
        public ReadOnly<Applicant> Applicant => ReadOnly<Applicant>();

        /// <summary>The family contact, an object of the payload with members of its own.</summary>
        public IntakeContact Contact => Object<IntakeContact>();
    }

    /// <summary>An applicant's facts: the argument of <see cref="IntakeSheet.Load"/>, and what the sheet renders from.</summary>
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

    /// <summary>The family member to call about an applicant.</summary>
    public sealed class ApplicantContact
    {
        public string Phone { get; set; } = "";

        public string Relation { get; set; } = "";
    }

    /// <summary>The levels of care the community offers, by the text the sheet's JavaScript uses.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CareLevel
    {
        /// <summary>An apartment with no care staff.</summary>
        [JsonStringEnumMemberName("independent")]
        Independent,

        /// <summary>Help with daily living.</summary>
        [JsonStringEnumMemberName("assisted")]
        Assisted,

        /// <summary>The secured wing.</summary>
        [JsonStringEnumMemberName("memoryCare")]
        MemoryCare,
    }

    /// <summary>The diets the kitchen prepares, by the text the sheet's JavaScript uses.</summary>
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

    /// <summary>Renders an <see cref="IntakeSheet"/> for a plan.</summary>
    public static class IntakeSheetHtmlExtensions
    {
        private static readonly JsonSerializerOptions Json = new JsonSerializerOptions(JsonSerializerDefaults.Web);

        /// <summary>
        /// Renders the sheet's host with the id the page chose and the applicant's facts as the sheet opens
        /// with them; the app's bundle constructs the sheet on the element, and the framework reaches it by id.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="plan">The plan the sheet joins.</param>
        /// <param name="elementId">The id the page gives the sheet.</param>
        /// <param name="applicant">The facts the sheet opens with.</param>
        /// <param name="waitlist">The applicant's number on the waiting list, or nothing while the list has not answered.</param>
        /// <returns>The rendered sheet; wire its event with <c>.Reactive(evt =&gt; evt.Changed, ...)</c>.</returns>
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
