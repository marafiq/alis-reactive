using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    // The visit planner (Scripts/visit-planner.ts) is a widget the sandbox wrote itself, with no
    // knowledge of Alis.Reactive. This file is everything the framework needs to drive it, written
    // against the framework's public surface only: where its object lives, its members by their
    // JavaScript names, its event payloads, and a render helper.

    /// <summary>A week of family visit times for one resident; its value is the booked time ids.</summary>
    /// <remarks>
    /// Each member is declared by the planner's own JavaScript name. A page reaches the planner and uses
    /// those members directly, one statement each:
    /// <code>
    /// var planner = p.Component&lt;VisitPlanner&gt;(m =&gt; m.VisitSlots);
    /// planner.Select(planner.FirstOpen());
    /// p.Element("visits-left").Text.Set(planner.Remaining());
    /// p.When(planner.Count).Gt(0).Then(..);
    /// </code>
    /// </remarks>
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
        public Prop<IReadOnlyList<string>> Value => Prop<IReadOnlyList<string>>();

        /// <summary>How many visits the resident's week allows.</summary>
        public Prop<int> Capacity => Prop<int>();

        /// <summary>The wing the planner names in its heading.</summary>
        public Prop<string> Wing => Prop<string>();

        /// <summary>When <see langword="true"/>, the times cannot be booked or released.</summary>
        public Prop<bool> ReadOnly => Prop<bool>();

        /// <summary>The times offered this week. Setting them raises <see cref="SlotsLoaded"/>.</summary>
        public Prop<IReadOnlyList<VisitSlot>> Slots => Prop<IReadOnlyList<VisitSlot>>();

        /// <summary>The day of each offered time: every item of <c>slots</c>, read at <c>day</c>.</summary>
        public Prop<IReadOnlyList<string>> OfferedDays => Prop<IReadOnlyList<string>>("slots[].day");

        /// <summary>What the planner says before any time is offered.</summary>
        public Prop<string> EmptyMessage => Prop<string>("labels.empty");

        /// <summary>How many times are booked.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();

        /// <summary>How many more visits the week allows.</summary>
        public TypedSource<int> Remaining() => Invoke<int>();

        /// <summary>Whether a time is booked.</summary>
        /// <param name="slot">The time's id.</param>
        public TypedSource<bool> IsBooked(Arg<string> slot) => Invoke<bool>(slot);

        /// <summary>The first time that is neither booked nor in quiet hours; empty when there is none.</summary>
        public TypedSource<string> FirstOpen() => Invoke<string>();

        /// <summary>The earliest booked time; empty when none is booked.</summary>
        public TypedSource<string> FirstBooked() => Invoke<string>();

        /// <summary>
        /// Books a time: raises <see cref="BeforeBook"/>, then <see cref="WeekChanged"/>; raises
        /// <see cref="Full"/> instead when the week is full.
        /// </summary>
        /// <param name="slot">The time's id.</param>
        public void Select(Arg<string> slot) => Call(slot);

        /// <summary>Releases a booked time and raises <see cref="WeekChanged"/>.</summary>
        /// <param name="slot">The time's id.</param>
        public void Release(Arg<string> slot) => Call(slot);

        /// <summary>Releases every booked time and raises <see cref="WeekChanged"/>.</summary>
        public void Clear() => Call();

        /// <summary>Records who is coming to a booked time; the planner shows it under the time.</summary>
        /// <param name="slot">The booked time's id.</param>
        /// <param name="visitors">How many visitors are coming.</param>
        /// <param name="note">A note for the front desk.</param>
        public void Book(Arg<string> slot, Arg<decimal> visitors, Arg<string> note) => Call(slot, visitors, note);

        /// <summary>Prepares the summary the family receives; the steps after it wait for its promise to settle.</summary>
        public void PrepareSummary() => CallAsync();

        /// <summary>The prepared summary, empty until one is prepared; the step that reads it waits for its promise.</summary>
        public TypedSource<string> Summary() => InvokeAsync<string>();

        /// <summary>Marks a time another family holds; it can no longer be booked here.</summary>
        /// <param name="slot">The time's id.</param>
        /// <param name="family">The name of the family that holds it, which the planner shows.</param>
        public void MarkTaken(Arg<string> slot, Arg<string> family) => Call(slot, family);

        /// <summary>Moves focus to the first time that can be booked.</summary>
        public void Focus() => Call();

        /// <summary>
        /// Raised when the booked times change, by the user or by a method. The planner names it its own
        /// way; as its input's commit event it is the event a form checks the field again on.
        /// </summary>
        public TypedEvent<VisitPlannerWeekChangedArgs> WeekChanged => Event<VisitPlannerWeekChangedArgs>();

        /// <inheritdoc />
        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: WeekChanged);

        /// <summary>Raised instead of booking when the week is full.</summary>
        public TypedEvent<VisitPlannerFullArgs> Full => Event<VisitPlannerFullArgs>();

        /// <summary>Raised before a time is booked; set <see cref="VisitPlannerBeforeBookArgs.Cancel"/> to refuse it.</summary>
        public TypedEvent<VisitPlannerBeforeBookArgs> BeforeBook => Event<VisitPlannerBeforeBookArgs>();

        /// <summary>Raised when the week's times are offered.</summary>
        public TypedEvent<VisitPlannerSlotsLoadedArgs> SlotsLoaded => Event<VisitPlannerSlotsLoadedArgs>();
    }

    /// <summary>A time the planner offers.</summary>
    public sealed class VisitSlot
    {
        /// <summary>The time's id, which the planner books and releases by.</summary>
        public string Id { get; set; } = "";

        /// <summary>What the planner shows for the time, for example <c>"Sat 11:00 AM"</c>.</summary>
        public string Label { get; set; } = "";

        /// <summary>The day the time falls on, for example <c>"Saturday"</c>.</summary>
        public string Day { get; set; } = "";

        /// <summary>Quiet hours: the community asks families not to visit then.</summary>
        public bool Quiet { get; set; }
    }

    /// <summary>Payload of <see cref="VisitPlanner.WeekChanged"/>.</summary>
    public class VisitPlannerWeekChangedArgs : JsObject
    {
        /// <summary>The booked time ids after the change.</summary>
        public ReadOnly<List<string>> Value => ReadOnly<List<string>>();

        /// <summary>The time just booked, when the change booked one.</summary>
        public ReadOnly<string> Added => ReadOnly<string>();

        /// <summary>The time just released, when the change released one.</summary>
        public ReadOnly<string> Removed => ReadOnly<string>();

        /// <summary>How many times are booked after the change.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();

        /// <summary>How many more visits the week allows after the change.</summary>
        public ReadOnly<int> Remaining => ReadOnly<int>();
    }

    /// <summary>Payload of <see cref="VisitPlanner.Full"/>; it carries a method as well as data.</summary>
    public class VisitPlannerFullArgs : JsObject
    {
        /// <summary>How many visits the week allows.</summary>
        public ReadOnly<int> Capacity => ReadOnly<int>();

        /// <summary>The time that could not be booked.</summary>
        public ReadOnly<string> Attempted => ReadOnly<string>();

        /// <summary>Puts the time that could not be booked on the waitlist; the planner shows it.</summary>
        public void Waitlist() => Call();
    }

    /// <summary>Payload of <see cref="VisitPlanner.BeforeBook"/>; the planner reads <see cref="Cancel"/> back.</summary>
    public class VisitPlannerBeforeBookArgs : JsObject
    {
        /// <summary>The time about to be booked; read into it with <c>args.Slot.Get(x =&gt; x.Quiet)</c>.</summary>
        public ReadOnly<VisitSlot> Slot => ReadOnly<VisitSlot>();

        /// <summary>Set to <see langword="true"/> to refuse the booking: <c>args.Cancel.Set(true)</c>.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="VisitPlanner.SlotsLoaded"/>.</summary>
    public class VisitPlannerSlotsLoadedArgs : JsObject
    {
        /// <summary>How many times are offered.</summary>
        public ReadOnly<int> Count => ReadOnly<int>();

        /// <summary>The wing the planner names in its heading.</summary>
        public ReadOnly<string> Wing => ReadOnly<string>();
    }

    /// <summary>Renders a <see cref="VisitPlanner"/> for a model-bound list of time ids.</summary>
    public static class VisitPlannerHtmlExtensions
    {
        /// <summary>
        /// Renders the planner's host with the framework's element ID and binding name, the week's
        /// capacity, and the times already booked; the app's bundle constructs the planner on it.
        /// </summary>
        /// <typeparam name="TModel">The view's model.</typeparam>
        /// <param name="field">The model-bound list of booked time ids the planner renders into.</param>
        /// <param name="capacity">How many visits the resident's week allows.</param>
        /// <returns>The rendered planner; wire its events with <c>.Reactive(evt =&gt; evt.WeekChanged, ...)</c>.</returns>
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
