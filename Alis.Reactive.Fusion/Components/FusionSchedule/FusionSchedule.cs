using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.Schedule;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion Schedule, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionSchedule(plan, "shift-schedule", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionSchedule&gt;("shift-schedule")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>. Event data is typed by your event class: <c>.Get(c =&gt; c.GetEvents&lt;Shift&gt;())</c>,
    /// <c>.Reactive(evt =&gt; evt.EventRendered&lt;Shift&gt;(), ...)</c>.
    /// </remarks>
    public sealed class FusionSchedule : FusionComponent
    {
        /// <summary>The shown view, for example <c>"Week"</c>.</summary>
        public ComponentProperty<string> CurrentView => Property<string>("currentView");

        /// <summary>The date the view shows.</summary>
        public ComponentProperty<DateTime> SelectedDate => Property<DateTime>("selectedDate");

        /// <summary>The events; call <see cref="DataBind"/> after writing them.</summary>
        public ComponentProperty<object> EventDataSource => Property<object>("eventSettings.dataSource");

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public ComponentMethod DataBind => Method("dataBind");

        /// <summary>The events, typed by your event class.</summary>
        public ComponentFunction<TEvent[]> GetEvents<TEvent>() where TEvent : class =>
            Function<TEvent[]>("getEvents");

        /// <summary>Adds an event.</summary>
        public ComponentMethod<object> AddEvent => Method<object>("addEvent");

        /// <summary>Saves an edited event.</summary>
        public ComponentMethod<TEvent, ScheduleAction> SaveEvent<TEvent>() where TEvent : class =>
            Method<TEvent, ScheduleAction>("saveEvent");

        /// <summary>Deletes an event, given the event or its key.</summary>
        public ComponentMethod<TKey, ScheduleAction> DeleteEvent<TKey>() =>
            Method<TKey, ScheduleAction>("deleteEvent");

        /// <summary>Opens the event editor for an event.</summary>
        public ComponentMethod<TEvent, ScheduleAction> OpenEditor<TEvent>() where TEvent : class =>
            Method<TEvent, ScheduleAction>("openEditor");

        /// <summary>Closes the event editor.</summary>
        public ComponentMethod CloseEditor => Method("closeEditor");

        /// <summary>Re-renders the events.</summary>
        public ComponentMethod RefreshEvents => Method("refreshEvents");

        /// <summary>Prints the schedule.</summary>
        public ComponentMethod Print => Method("print");

        /// <summary>Scrolls to an hour, for example <c>"09:00"</c>.</summary>
        public ComponentMethod<string> ScrollTo => Method<string>("scrollTo");

        /// <summary>Raised when the user clicks a cell.</summary>
        public TypedEvent<FusionScheduleCellClickArgs> CellClick =>
            Event<FusionScheduleCellClickArgs>("cellClick");

        /// <summary>Raised when the user clicks an event.</summary>
        public TypedEvent<FusionScheduleEventClickArgs> EventClick =>
            Event<FusionScheduleEventClickArgs>("eventClick");

        /// <summary>Raised before an action runs.</summary>
        public TypedEvent<FusionScheduleActionBeginArgs> ActionBegin =>
            Event<FusionScheduleActionBeginArgs>("actionBegin");

        /// <summary>Raised after an action completes.</summary>
        public TypedEvent<FusionScheduleActionCompleteArgs> ActionComplete =>
            Event<FusionScheduleActionCompleteArgs>("actionComplete");

        /// <summary>Raised before the view or date changes.</summary>
        public TypedEvent<FusionScheduleNavigatingArgs> Navigating =>
            Event<FusionScheduleNavigatingArgs>("navigating");

        /// <summary>Raised before a popup opens.</summary>
        public TypedEvent<FusionSchedulePopupOpenArgs> PopupOpen =>
            Event<FusionSchedulePopupOpenArgs>("popupOpen");

        /// <summary>Raised before a popup closes.</summary>
        public TypedEvent<FusionSchedulePopupCloseArgs> PopupClose =>
            Event<FusionSchedulePopupCloseArgs>("popupClose");

        /// <summary>Raised after the schedule binds its events.</summary>
        public TypedEvent<FusionScheduleDataBoundArgs> DataBound =>
            Event<FusionScheduleDataBoundArgs>("dataBound");

        /// <summary>Raised after each event renders.</summary>
        public TypedEvent<FusionScheduleEventRenderedArgs<TEvent>> EventRendered<TEvent>() where TEvent : class, new() =>
            Event<FusionScheduleEventRenderedArgs<TEvent>>("eventRendered");
    }

    /// <summary>Payload of <see cref="FusionSchedule.CellClick"/>.</summary>
    public class FusionScheduleCellClickArgs
    {
        /// <summary>Start of the clicked cell.</summary>
        public DateTime StartTime { get; set; }

        /// <summary>End of the clicked cell.</summary>
        public DateTime EndTime { get; set; }

        /// <summary>Resource group of the clicked cell.</summary>
        public int GroupIndex { get; set; }

        /// <summary>Whether the cell is in the all-day row.</summary>
        public bool IsAllDay { get; set; }
    }

    /// <summary>Payload of <see cref="FusionSchedule.EventClick"/>.</summary>
    public class FusionScheduleEventClickArgs
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionSchedule.ActionBegin"/>.</summary>
    public class FusionScheduleActionBeginArgs
    {
        /// <summary>The kind of action.</summary>
        public string RequestType { get; set; } = "";

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionSchedule.ActionComplete"/>.</summary>
    public class FusionScheduleActionCompleteArgs
    {
        /// <summary>The kind of action.</summary>
        public string RequestType { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionSchedule.Navigating"/>.</summary>
    public class FusionScheduleNavigatingArgs
    {
        /// <summary><c>"date"</c> or <c>"view"</c>.</summary>
        public string Action { get; set; } = "";

        /// <summary>The date the schedule moves to.</summary>
        public string CurrentDate { get; set; } = "";

        /// <summary>The date it shows now.</summary>
        public string PreviousDate { get; set; } = "";

        /// <summary>The view it moves to.</summary>
        public string CurrentView { get; set; } = "";

        /// <summary>The view it shows now.</summary>
        public string PreviousView { get; set; } = "";

        /// <summary>Index of the view it moves to.</summary>
        public int ViewIndex { get; set; }

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }
    }

    /// <summary>Payload of <see cref="FusionSchedule.PopupOpen"/>.</summary>
    public class FusionSchedulePopupOpenArgs
    {
        /// <summary>The popup: <c>"QuickInfo"</c>, <c>"Editor"</c>, ...</summary>
        public string Type { get; set; } = "";

        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>The event or cell the popup is for.</summary>
        public FusionSchedulePopupData Data { get; set; } = new FusionSchedulePopupData();
    }

    /// <summary>The event a popup is for, as <see cref="FusionSchedulePopupOpenArgs"/> reports it.</summary>
    public class FusionSchedulePopupData
    {
        /// <summary>The event ID; 0 for an empty cell.</summary>
        public int Id { get; set; }

        /// <summary>The event subject.</summary>
        public string Subject { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionSchedule.PopupClose"/>.</summary>
    public class FusionSchedulePopupCloseArgs
    {
        /// <summary>The popup that closes.</summary>
        public string Type { get; set; } = "";
    }

    /// <summary>Payload of <see cref="FusionSchedule.DataBound"/>; carries no data.</summary>
    public class FusionScheduleDataBoundArgs
    {
    }

    /// <summary>Payload of <c>EventRendered</c>.</summary>
    public class FusionScheduleEventRenderedArgs<TEvent>
        where TEvent : class, new()
    {
        /// <summary>Set to <see langword="true"/> with <c>p.Payload(args).Set(a =&gt; a.Cancel, true)</c> to stop the action.</summary>
        public bool Cancel { get; set; }

        /// <summary>The rendered event.</summary>
        public TEvent Data { get; set; } = new TEvent();

        /// <summary>Where the event renders.</summary>
        public string Type { get; set; } = "";
    }

    /// <summary>
    /// Editor action for schedule event operations. Mirrors the Syncfusion EJ2 Schedule
    /// <c>CurrentAction</c> values one-to-one; each name serializes to its EJ2 string.
    /// </summary>
    public enum ScheduleAction
    {
        /// <summary>Add a new event.</summary>
        Add,
        /// <summary>Save an edited event.</summary>
        Save,
        /// <summary>Delete an event.</summary>
        Delete,
        /// <summary>Delete a single occurrence of a recurring series.</summary>
        DeleteOccurrence,
        /// <summary>Delete an entire recurring series.</summary>
        DeleteSeries,
        /// <summary>Edit a single occurrence of a recurring series.</summary>
        EditOccurrence,
        /// <summary>Edit an entire recurring series.</summary>
        EditSeries,
        /// <summary>Edit this and following occurrences.</summary>
        EditFollowingEvents,
        /// <summary>Delete this and following occurrences.</summary>
        DeleteFollowingEvents,
    }

    /// <summary>
    /// Renders the component for a plan, ready for <c>.Reactive(...)</c>.
    /// </summary>
    public static class FusionScheduleHtmlExtensions
    {
        /// <summary>
        /// Creates a Syncfusion Schedule and carries its controlled component ID into the Reactive Plan.
        /// </summary>
        /// <param name="plan">Reactive Plan receiving schedule event wiring.</param>
        /// <param name="elementId">Controlled component ID shared by markup and Reactive Plan behavior.</param>
        /// <param name="build">Configures the component before rendering.</param>
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionSchedule, TModel> FusionSchedule<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<ScheduleBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().Schedule(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionSchedule>(elementId, builder.Render());
        }
    }
}
