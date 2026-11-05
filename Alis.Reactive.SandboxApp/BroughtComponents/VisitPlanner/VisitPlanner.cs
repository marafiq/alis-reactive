using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Html;

namespace Alis.Reactive.SandboxApp.BroughtComponents
{
    public sealed class VisitPlanner : Component, IInputComponent
    {
        public static ComponentVendor VisitPlannerVendor { get; } = ComponentVendor.Define(
            "visitplanner",
            objectPath: "visitPlanner",
            subscribe: "on",
            unsubscribe: "off",
            payload: VendorEventPayload.Argument);

        public override ComponentVendor Vendor => VisitPlannerVendor;
        public Property<IReadOnlyList<string>> Value => Property<IReadOnlyList<string>>();

        public Property<int> Capacity => Property<int>();

        public Property<string> Wing => Property<string>();

        public Property<bool> ReadOnly => Property<bool>();

        public Property<IReadOnlyList<VisitSlot>> Slots => Property<IReadOnlyList<VisitSlot>>();

        public ReadOnly<IReadOnlyList<string>> OfferedDays => Slots.Each(slot => slot.Day);

        public VisitPlannerLabels Labels => Object<VisitPlannerLabels>();

        public ReadOnly<int> Count => ReadOnly<int>();

        public TypedSource<int> Remaining() => Method<int>();

        public TypedSource<bool> IsBooked(Arg<string> slot) => Method<bool>(slot);

        public TypedSource<string> FirstOpen() => Method<string>();

        public TypedSource<string> FirstBooked() => Method<string>();

        public void Select(Arg<string> slot) => Method(slot);

        public void Release(Arg<string> slot) => Method(slot);

        public void Clear() => Method();

        public void Book(Arg<string> slot, Arg<decimal> visitors, Arg<string> note) => Method(slot, visitors, note);

        public void PrepareSummary() => MethodAsync();

        public TypedSource<string> Summary() => MethodAsync<string>();

        public void MarkTaken(Arg<string> slot, Arg<string> family) => Method(slot, family);

        public void Focus() => Method();

        public Event<VisitPlannerWeekChangedArgs> WeekChanged => Event<VisitPlannerWeekChangedArgs>();

        ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: WeekChanged);

        public Event<VisitPlannerFullArgs> Full => Event<VisitPlannerFullArgs>();

        public Event<VisitPlannerBeforeBookArgs> BeforeBook => Event<VisitPlannerBeforeBookArgs>();

        public Event<VisitPlannerSlotsLoadedArgs> SlotsLoaded => Event<VisitPlannerSlotsLoadedArgs>();
    }

    public sealed class VisitPlannerLabels : JsObject
    {
        public Property<string> Empty => Property<string>();
    }

    public sealed class VisitSlot
    {
        public string Id { get; set; } = "";

        public string Label { get; set; } = "";

        public string Day { get; set; } = "";

        public bool Quiet { get; set; }
    }

    public sealed class VisitPlannerWeekChangedArgs : JsObject
    {
        public ReadOnly<List<string>> Value => ReadOnly<List<string>>();

        public ReadOnly<string> Added => ReadOnly<string>();

        public ReadOnly<string> Removed => ReadOnly<string>();

        public ReadOnly<int> Count => ReadOnly<int>();

        public ReadOnly<int> Remaining => ReadOnly<int>();
    }

    public sealed class VisitPlannerFullArgs : JsObject
    {
        public ReadOnly<int> Capacity => ReadOnly<int>();

        public ReadOnly<string> Attempted => ReadOnly<string>();

        public void Waitlist() => Method();
    }

    public sealed class VisitPlannerBeforeBookArgs : JsObject
    {
        public ReadOnly<VisitSlot> Slot => ReadOnly<VisitSlot>();

        public Property<bool> Cancel => Property<bool>();
    }

    public sealed class VisitPlannerSlotsLoadedArgs : JsObject
    {
        public ReadOnly<int> Count => ReadOnly<int>();

        public ReadOnly<string> Wing => ReadOnly<string>();
    }

    public static class VisitPlannerHtmlExtensions
    {
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
