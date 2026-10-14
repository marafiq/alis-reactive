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
    /// Render it with <c>Html.FusionSchedule(plan, "shift-schedule", b =&gt; ...)</c>, reach it with
    /// <c>var schedule = p.Component&lt;FusionSchedule&gt;("shift-schedule");</c> and use its members directly:
    /// <c>schedule.ScrollTo("09:00");</c>. Event data is typed by your event class:
    /// <c>schedule.GetEvents&lt;Shift&gt;()</c> reads the events as <c>Shift[]</c>,
    /// <c>.Reactive(evt =&gt; evt.EventRendered&lt;Shift&gt;(), (args, p) =&gt; ...)</c> hands each rendered
    /// <c>Shift</c> over as <c>args.Data</c>, and <c>.Reactive(evt =&gt; evt.ActionBegin&lt;Shift&gt;(), ...)</c>
    /// hands the shifts an action adds, saves or deletes over as <c>args.AddedRecords</c>,
    /// <c>args.ChangedRecords</c> and <c>args.DeletedRecords</c>.
    /// </remarks>
    public sealed class FusionSchedule : FusionComponent
    {
        /// <summary>The shown view, for example <c>"Week"</c>.</summary>
        public Prop<string> CurrentView => Prop<string>();

        /// <summary>The date the view shows.</summary>
        public Prop<DateTime> SelectedDate => Prop<DateTime>();

        /// <summary>The events and how they are read; call <see cref="DataBind"/> after writing them.</summary>
        public FusionScheduleEventSettings EventSettings => Object<FusionScheduleEventSettings>();

        /// <summary>Applies pending property changes to the rendered component.</summary>
        public void DataBind() => Call();

        /// <summary>The events, typed by your event class.</summary>
        /// <typeparam name="TEvent">Your event class.</typeparam>
        public TypedSource<TEvent[]> GetEvents<TEvent>() where TEvent : class => Invoke<TEvent[]>();

        /// <summary>Adds an event.</summary>
        /// <typeparam name="TEvent">Your event class, or an array of it.</typeparam>
        /// <param name="data">The event to add, or an array of events.</param>
        public void AddEvent<TEvent>(Arg<TEvent> data) where TEvent : class => Call(data);

        /// <summary>Saves an edited event.</summary>
        /// <typeparam name="TEvent">Your event class.</typeparam>
        /// <param name="data">The edited event.</param>
        /// <param name="currentAction">
        /// For a recurring event, whether the edit applies to one occurrence
        /// (<see cref="ScheduleAction.EditOccurrence"/>) or the whole series (<see cref="ScheduleAction.EditSeries"/>).
        /// </param>
        public void SaveEvent<TEvent>(Arg<TEvent> data, Arg<ScheduleAction> currentAction) where TEvent : class => Call(data, currentAction);

        /// <summary>Deletes an event, given the event or its key.</summary>
        /// <typeparam name="TKey">The type of the event's key, or your event class when you pass the event.</typeparam>
        /// <param name="id">The event's key, or the event itself.</param>
        /// <param name="currentAction">
        /// What to delete: <see cref="ScheduleAction.Delete"/>, <see cref="ScheduleAction.DeleteOccurrence"/> or
        /// <see cref="ScheduleAction.DeleteSeries"/>.
        /// </param>
        public void DeleteEvent<TKey>(Arg<TKey> id, Arg<ScheduleAction> currentAction) => Call(id, currentAction);

        /// <summary>Opens the event editor for an event.</summary>
        /// <typeparam name="TEvent">Your event class.</typeparam>
        /// <param name="data">The event to edit, or the cell data a new event starts from.</param>
        /// <param name="action">
        /// What the editor opens for: <see cref="ScheduleAction.Add"/>, <see cref="ScheduleAction.Save"/>,
        /// <see cref="ScheduleAction.EditOccurrence"/> or <see cref="ScheduleAction.EditSeries"/>.
        /// </param>
        public void OpenEditor<TEvent>(Arg<TEvent> data, Arg<ScheduleAction> action) where TEvent : class => Call(data, action);

        /// <summary>Closes the event editor.</summary>
        public void CloseEditor() => Call();

        /// <summary>Re-renders the events.</summary>
        public void RefreshEvents() => Call();

        /// <summary>Prints the schedule.</summary>
        public void Print() => Call();

        /// <summary>Scrolls to an hour, for example <c>"09:00"</c>.</summary>
        /// <param name="hour">The hour and minute to scroll to, in 24-hour form.</param>
        public void ScrollTo(Arg<string> hour) => Call(hour);

        /// <summary>Raised when the user clicks a cell.</summary>
        public TypedEvent<FusionScheduleCellClickArgs> CellClick =>
            Event<FusionScheduleCellClickArgs>();

        /// <summary>Raised when the user clicks an event.</summary>
        public TypedEvent<FusionScheduleEventClickArgs> EventClick =>
            Event<FusionScheduleEventClickArgs>();

        /// <summary>
        /// Raised before an action runs: a move to another date or view, or an event added, saved or deleted.
        /// </summary>
        /// <typeparam name="TEvent">Your event class; the payload's records read the events the action carries as it.</typeparam>
        public TypedEvent<FusionScheduleActionBeginArgs<TEvent>> ActionBegin<TEvent>() where TEvent : class =>
            Event<FusionScheduleActionBeginArgs<TEvent>>();

        /// <summary>Raised after an action completes.</summary>
        public TypedEvent<FusionScheduleActionCompleteArgs> ActionComplete =>
            Event<FusionScheduleActionCompleteArgs>();

        /// <summary>Raised before the view or date changes.</summary>
        public TypedEvent<FusionScheduleNavigatingArgs> Navigating =>
            Event<FusionScheduleNavigatingArgs>();

        /// <summary>Raised before a popup opens: an event's details, the event editor, a delete alert, ...</summary>
        /// <typeparam name="TEvent">Your event class; the payload's <c>Data</c> reads the event the popup is for as it.</typeparam>
        public TypedEvent<FusionSchedulePopupOpenArgs<TEvent>> PopupOpen<TEvent>() where TEvent : class =>
            Event<FusionSchedulePopupOpenArgs<TEvent>>();

        /// <summary>Raised before a popup closes.</summary>
        public TypedEvent<FusionSchedulePopupCloseArgs> PopupClose =>
            Event<FusionSchedulePopupCloseArgs>();

        /// <summary>Raised after the schedule binds its events.</summary>
        public TypedEvent<FusionScheduleDataBoundArgs> DataBound =>
            Event<FusionScheduleDataBoundArgs>();

        /// <summary>Raised after each event renders.</summary>
        /// <typeparam name="TEvent">Your event class; the payload's <c>Data</c> reads the rendered event as it.</typeparam>
        public TypedEvent<FusionScheduleEventRenderedArgs<TEvent>> EventRendered<TEvent>() where TEvent : class, new() =>
            Event<FusionScheduleEventRenderedArgs<TEvent>>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.CellClick"/>.</summary>
    public class FusionScheduleCellClickArgs : JsObject
    {
        /// <summary>Start of the clicked cell.</summary>
        public ReadOnly<DateTime> StartTime => ReadOnly<DateTime>();

        /// <summary>End of the clicked cell.</summary>
        public ReadOnly<DateTime> EndTime => ReadOnly<DateTime>();

        /// <summary>Resource group of the clicked cell.</summary>
        public ReadOnly<int> GroupIndex => ReadOnly<int>();

        /// <summary>Whether the cell is in the all-day row.</summary>
        public ReadOnly<bool> IsAllDay => ReadOnly<bool>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.EventClick"/>.</summary>
    public class FusionScheduleEventClickArgs : JsObject
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.ActionBegin{TEvent}"/>.</summary>
    /// <remarks>
    /// An event action hands its events over as arrays, whichever way it started (the schedule's editor or delete
    /// alert, or <c>AddEvent</c>, <c>SaveEvent</c> and <c>DeleteEvent</c>): an added event is in
    /// <see cref="AddedRecords"/>, a saved one in <see cref="ChangedRecords"/> and a deleted one in
    /// <see cref="DeletedRecords"/>. A recurring event differs: deleting one occurrence, or it and those after it,
    /// hands them over in <see cref="ChangedRecords"/>, and saving the whole series lists the occurrences it drops
    /// in <see cref="DeletedRecords"/>. A move to another date or view carries no records, which
    /// <c>p.From(args.ChangedRecords)</c> reads as an empty list. The payload's <c>data</c> is not declared: it
    /// changes shape with the action (the added events, the one event the editor saves, occurrence and parent
    /// pairs for a recurring event).
    /// </remarks>
    /// <typeparam name="TEvent">Your event class.</typeparam>
    public class FusionScheduleActionBeginArgs<TEvent> : JsObject
        where TEvent : class
    {
        /// <summary>
        /// The kind of action: <c>"dateNavigate"</c>, <c>"viewNavigate"</c>, <c>"eventCreate"</c>,
        /// <c>"eventChange"</c>, <c>"eventRemove"</c>, ...
        /// </summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The events being added.</summary>
        public ReadOnly<TEvent[]> AddedRecords => ReadOnly<TEvent[]>();

        /// <summary>The events being saved, each with the values it is saved with.</summary>
        public ReadOnly<TEvent[]> ChangedRecords => ReadOnly<TEvent[]>();

        /// <summary>The events being deleted.</summary>
        public ReadOnly<TEvent[]> DeletedRecords => ReadOnly<TEvent[]>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.ActionComplete"/>.</summary>
    public class FusionScheduleActionCompleteArgs : JsObject
    {
        /// <summary>The kind of action.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.Navigating"/>.</summary>
    public class FusionScheduleNavigatingArgs : JsObject
    {
        /// <summary><c>"date"</c> or <c>"view"</c>.</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>The date the schedule moves to; for a view switch, the date it shows.</summary>
        public ReadOnly<DateTime> CurrentDate => ReadOnly<DateTime>();

        /// <summary>The date it shows now; only a date move carries it.</summary>
        public ReadOnly<DateTime> PreviousDate => ReadOnly<DateTime>();

        /// <summary>The view it moves to.</summary>
        public ReadOnly<string> CurrentView => ReadOnly<string>();

        /// <summary>The view it shows now.</summary>
        public ReadOnly<string> PreviousView => ReadOnly<string>();

        /// <summary>Index of the view it moves to.</summary>
        public ReadOnly<int> ViewIndex => ReadOnly<int>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.PopupOpen{TEvent}"/>.</summary>
    /// <typeparam name="TEvent">Your event class.</typeparam>
    public class FusionSchedulePopupOpenArgs<TEvent> : JsObject
        where TEvent : class
    {
        /// <summary>The popup: <c>"QuickInfo"</c>, <c>"Editor"</c>, <c>"DeleteAlert"</c>, ...</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>
        /// The event the popup is for. A popup for an empty cell holds only the cell's start, end and resource, so the
        /// event's own members, its key among them, read as unset.
        /// </summary>
        public ReadOnly<TEvent> Data => ReadOnly<TEvent>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.PopupClose"/>.</summary>
    public class FusionSchedulePopupCloseArgs : JsObject
    {
        /// <summary>The popup that closes.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>Payload of <see cref="FusionSchedule.DataBound"/>; carries no data.</summary>
    public class FusionScheduleDataBoundArgs : JsObject
    {
    }

    /// <summary>Payload of <see cref="FusionSchedule.EventRendered{TEvent}"/>.</summary>
    /// <typeparam name="TEvent">Your event class.</typeparam>
    public class FusionScheduleEventRenderedArgs<TEvent> : JsObject
        where TEvent : class, new()
    {
        /// <summary>Set to <see langword="true"/> with <c>args.Cancel.Set(true)</c> to stop the action.</summary>
        public Prop<bool> Cancel => Prop<bool>();

        /// <summary>The rendered event.</summary>
        public ReadOnly<TEvent> Data => ReadOnly<TEvent>();

        /// <summary>Where the event renders.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>The schedule's <c>eventSettings</c>: its events.</summary>
    public sealed class FusionScheduleEventSettings : JsObject
    {
        /// <summary>The events.</summary>
        public Prop<object> DataSource => Prop<object>();
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
        /// <typeparam name="TModel">The view's model type.</typeparam>
        /// <param name="html">The view's HTML helper.</param>
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
