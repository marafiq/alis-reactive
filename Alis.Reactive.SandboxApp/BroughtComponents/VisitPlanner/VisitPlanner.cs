using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Alis.Reactive.Native;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The visit planner (Scripts/visit-planner.ts) is a widget the sandbox wrote itself, with no
    // knowledge of Alis.Reactive. This file is everything the framework needs to drive it, written
    // against the framework's public surface only: where its object lives, its members by their
    // JavaScript names, its event payloads, and a render helper.

    /// <summary>A week of family visit times for one resident; its value is the booked time ids.</summary>
    public sealed class VisitPlanner : Component, IInputComponent
    {
        /// <summary>
        /// The planner keeps itself on its host element as <c>visitPlanner</c>, adds listeners with
        /// <c>on</c>/<c>off</c>, and hands a listener the event's payload.
        /// </summary>
        public static ComponentVendor VisitPlannerVendor { get; } = ComponentVendor.Define(
            "visitplanner",
            objectPath: "visitPlanner",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        /// <inheritdoc />
        public override ComponentVendor Vendor => VisitPlannerVendor;
        /// <summary>The booked time ids, in the week's order. Setting it raises no event.</summary>
        public ComponentProperty<IReadOnlyList<string>> Value => Property<IReadOnlyList<string>>("value");

        /// <summary>How many visits the resident's week allows.</summary>
        public ComponentProperty<int> Capacity => Property<int>("capacity");

        /// <summary>The wing the planner names in its heading.</summary>
        public ComponentProperty<string> Wing => Property<string>("wing");

        /// <summary>When true, the times cannot be booked or released.</summary>
        public ComponentProperty<bool> ReadOnly => Property<bool>("readOnly");

        /// <summary>The times offered this week. Setting them raises <see cref="SlotsLoaded"/>.</summary>
        public ComponentProperty<IReadOnlyList<VisitSlot>> Slots => Property<IReadOnlyList<VisitSlot>>("slots");

        /// <summary>The day of each offered time: every item of <c>slots</c>, read at <c>day</c>.</summary>
        public ComponentProperty<IReadOnlyList<string>> OfferedDays => Property<IReadOnlyList<string>>("slots[].day");

        /// <summary>What the planner says before any time is offered.</summary>
        public ComponentProperty<string> EmptyMessage => Property<string>("labels.empty");

        /// <summary>How many times are booked. Read only.</summary>
        public ComponentProperty<int> Count => Property<int>("count");

        /// <summary>How many more visits the week allows.</summary>
        public ComponentFunction<int> Remaining => Function<int>("remaining");

        /// <summary>Whether a time is booked.</summary>
        public ComponentFunction<string?, bool> IsBooked => Function<string?, bool>("isBooked");

        /// <summary>The first time that is neither booked nor in quiet hours; empty when there is none.</summary>
        public ComponentFunction<string> FirstOpen => Function<string>("firstOpen");

        /// <summary>The earliest booked time; empty when none is booked.</summary>
        public ComponentFunction<string> FirstBooked => Function<string>("firstBooked");

        /// <summary>Books a time: raises <see cref="BeforeBook"/>, then <see cref="Change"/>, or <see cref="Full"/>.</summary>
        public ComponentMethod<string> Select => Method<string>("select");

        /// <summary>Releases a booked time and raises <see cref="Change"/>.</summary>
        public ComponentMethod<string> Release => Method<string>("release");

        /// <summary>Releases every booked time and raises <see cref="Change"/>.</summary>
        public ComponentMethod Clear => Method("clear");

        /// <summary>Records who is coming to a booked time: its id, how many visitors, and a note.</summary>
        public ComponentMethod<string?, decimal, string?> Book => Method<string?, decimal, string?>("book");

        /// <summary>Prepares the summary the family receives. Returns a promise the plan waits for.</summary>
        public ComponentMethod PrepareSummary => Method("prepareSummary").Awaited();

        /// <summary>The prepared summary, empty until one is prepared. Returns a promise the plan waits for.</summary>
        public ComponentFunction<string> Summary => Function<string>("summary").Awaited();

        /// <summary>Moves focus to the first time that can be booked.</summary>
        public ComponentMethod Focus => Method("focus");

        /// <summary>
        /// Raised when the booked times change, by the user or by a method. The planner names it its own
        /// way; as its input's commit event it is the event a form checks the field again on.
        /// </summary>
        public TypedEvent<VisitPlannerWeekChangedArgs> WeekChanged => Event<VisitPlannerWeekChangedArgs>("weekChanged");

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: WeekChanged);

        /// <summary>Raised instead of booking when the week is full.</summary>
        public TypedEvent<VisitPlannerFullArgs> Full => Event<VisitPlannerFullArgs>("full");

        /// <summary>Raised before a time is booked; set <see cref="VisitPlannerBeforeBookArgs.Cancel"/> to refuse it.</summary>
        public TypedEvent<VisitPlannerBeforeBookArgs> BeforeBook => Event<VisitPlannerBeforeBookArgs>("beforeBook");

        /// <summary>Raised when the week's times are offered.</summary>
        public TypedEvent<VisitPlannerSlotsLoadedArgs> SlotsLoaded => Event<VisitPlannerSlotsLoadedArgs>("slotsLoaded");
    }

    /// <summary>A time the planner offers.</summary>
    public sealed class VisitSlot
    {
        public string Id { get; set; } = "";

        public string Label { get; set; } = "";

        public string Day { get; set; } = "";

        /// <summary>Quiet hours: the community asks families not to visit then.</summary>
        public bool Quiet { get; set; }
    }

    /// <summary>Payload of <see cref="VisitPlanner.WeekChanged"/>.</summary>
    public class VisitPlannerWeekChangedArgs
    {
        public List<string> Value { get; set; } = new List<string>();

        /// <summary>The time just booked, when the change booked one.</summary>
        public string? Added { get; set; }

        /// <summary>The time just released, when the change released one.</summary>
        public string? Removed { get; set; }

        public int Count { get; set; }

        public int Remaining { get; set; }
    }

    /// <summary>Payload of <see cref="VisitPlanner.Full"/>; it carries a method as well as data.</summary>
    public class VisitPlannerFullArgs : JsObject
    {
        public int Capacity { get; set; }

        /// <summary>The time that could not be booked.</summary>
        public string Attempted { get; set; } = "";

        /// <summary>Puts the time that could not be booked on the waitlist; the planner shows it.</summary>
        public ComponentMethod Waitlist => Method("waitlist");
    }

    /// <summary>Payload of <see cref="VisitPlanner.BeforeBook"/>; the planner reads <see cref="Cancel"/> back.</summary>
    public class VisitPlannerBeforeBookArgs
    {
        public VisitSlot Slot { get; set; } = new VisitSlot();

        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="VisitPlanner.SlotsLoaded"/>.</summary>
    public class VisitPlannerSlotsLoadedArgs
    {
        public int Count { get; set; }

        public string Wing { get; set; } = "";
    }

    /// <summary>Renders a <see cref="VisitPlanner"/> for a model-bound list of time ids.</summary>
    public static class VisitPlannerHtmlExtensions
    {
        /// <summary>
        /// Renders the planner's host with the framework's element ID and binding name, the week's
        /// capacity, and the times already booked; the app's bundle constructs the planner on it.
        /// </summary>
        public static RenderedComponent<VisitPlanner, TModel> VisitPlanner<TModel>(
            this InputBoundField<TModel, List<string>> field,
            int capacity)
            where TModel : class
        {
            var html = new StringBuilder()
                .Append("<div class=\"visit-planner\" id=\"").Append(Encode(field.ElementId))
                .Append("\" data-name=\"").Append(Encode(field.BindingPath))
                .Append("\" data-capacity=\"").Append(capacity)
                .Append("\" data-value=\"").Append(Encode(string.Join(",", Booked(field))))
                .Append("\"></div>");
            return field.Render<VisitPlanner>(new HtmlString(html.ToString()));
        }

        private static IEnumerable<string> Booked<TModel>(InputBoundField<TModel, List<string>> field)
            where TModel : class
        {
            var model = field.Helper.ViewData.Model;
            return model == null ? Enumerable.Empty<string>() : field.Expression.Compile()(model) ?? new List<string>();
        }

        private static string Encode(string value) => HtmlEncoder.Default.Encode(value);
    }
}
