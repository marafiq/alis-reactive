using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A schedule: EJ2's Schedule, a calendar of events in day, week, month and other views.
/// </summary>
/// <remarks>
/// A member that takes or hands events is typed by the view's event class, <c>TEvent</c>. An event's payload is read
/// with <c>Read</c>, and a handler writes true to its <c>Cancel</c> to stop the action.
/// </remarks>
[Mirrors("@syncfusion/ej2-schedule", "Schedule")]
public sealed class FusionSchedule : FusionComponent
{
    /// <summary>Gets the name of the view the schedule shows, such as Week or Month.</summary>
    public Property<string> CurrentView => Property<string>();

    /// <summary>Gets the date the schedule's view shows.</summary>
    public Property<DateTime> SelectedDate => Property<DateTime>();

    /// <summary>Gets the settings of the schedule's events, the events it shows among them.</summary>
    public EventSettingsModel EventSettings => LiveObject<EventSettingsModel>(nullable: true);

    /// <summary>Applies the changes written to the schedule's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Reads the events the schedule holds.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <returns>The events.</returns>
    public MethodResult<IReadOnlyList<TEvent>> GetEvents<TEvent>() => Method<IReadOnlyList<TEvent>>();

    /// <summary>Adds an event to the schedule.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <param name="data">The event.</param>
    public void AddEvent<TEvent>(Arg<TEvent> data) where TEvent : class => Method([data]);

    /// <summary>Saves an event of the schedule, found by its key.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <param name="data">The event.</param>
    /// <param name="currentAction">The action the save is, such as Save or EditOccurrence.</param>
    public void SaveEvent<TEvent>(Arg<TEvent> data, Arg<CurrentAction> currentAction) where TEvent : class => Method([data, currentAction]);

    /// <summary>Deletes an event of the schedule by its key.</summary>
    /// <typeparam name="TKey">The type of the events' key member, text or a number.</typeparam>
    /// <param name="id">The event's key.</param>
    /// <param name="currentAction">The action the delete is, such as Delete or DeleteOccurrence.</param>
    public void DeleteEvent<TKey>(Arg<TKey> id, Arg<CurrentAction> currentAction) => Method([id, currentAction]);

    /// <summary>Opens the schedule's editor for an action on an event.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <param name="data">The event the editor shows.</param>
    /// <param name="action">The action the editor opens for.</param>
    public void OpenEditor<TEvent>(Arg<TEvent> data, Arg<CurrentAction> action) where TEvent : class => Method([data, action]);

    /// <summary>Closes the schedule's editor.</summary>
    public void CloseEditor() => Method();

    /// <summary>Renders the schedule's events again.</summary>
    public void RefreshEvents() => Method();

    /// <summary>Prints the schedule.</summary>
    public void Print() => Method();

    /// <summary>Scrolls the schedule's view to a time of day.</summary>
    /// <param name="hour">The hour and minute, as text such as "09:00".</param>
    public void ScrollTo(Arg<string> hour) => Method([hour]);

    /// <summary>Reads the dates the schedule's view shows.</summary>
    /// <returns>The dates.</returns>
    public MethodResult<IReadOnlyList<DateTime>> GetCurrentViewDates() => Method<IReadOnlyList<DateTime>>();

    /// <summary>Closes the schedule's quick popup of an event or a cell.</summary>
    public void CloseQuickInfoPopup() => Method();

    /// <summary>Closes the tooltip of an event.</summary>
    public void CloseTooltip() => Method();

    /// <summary>Gets the event raised when a person clicks a cell of the schedule.</summary>
    public Event<CellClickEventArgs> CellClick => Event<CellClickEventArgs>();

    /// <summary>Gets the event raised when a person clicks an event of the schedule.</summary>
    public Event<EventClickArgs> EventClick => Event<EventClickArgs>();

    /// <summary>Gets the event raised as each action of the schedule begins, such as adding or saving an event.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <returns>The event.</returns>
    public Event<ActionEventArgs<TEvent>> ActionBegin<TEvent>() => Event<ActionEventArgs<TEvent>>();

    /// <summary>Gets the event raised as each action of the schedule completes.</summary>
    public Event<ActionEventArgs> ActionComplete => Event<ActionEventArgs>();

    /// <summary>Gets the event raised before the schedule moves to another date or view.</summary>
    public Event<NavigatingEventArgs> Navigating => Event<NavigatingEventArgs>();

    /// <summary>Gets the event raised before a popup of the schedule opens, its editor among them.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <returns>The event.</returns>
    public Event<PopupOpenEventArgs<TEvent>> PopupOpen<TEvent>() => Event<PopupOpenEventArgs<TEvent>>();

    /// <summary>Gets the event raised before a popup of the schedule closes.</summary>
    public Event<PopupCloseEventArgs> PopupClose => Event<PopupCloseEventArgs>();

    /// <summary>Gets the event raised once the events are bound to the schedule, which EJ2 raises with no payload.</summary>
    public Event DataBound => Event();

    /// <summary>Gets the event raised before each event of the schedule renders.</summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    /// <returns>The event.</returns>
    public Event<EventRenderedArgs<TEvent>> EventRendered<TEvent>() => Event<EventRenderedArgs<TEvent>>();

    /// <summary>
    /// The action a save, a delete or the editor is, written by its name.
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter<CurrentAction>))]
    public enum CurrentAction
    {
        /// <summary>Adds an event.</summary>
        [JsonStringEnumMemberName("Add")]
        Add,

        /// <summary>Saves an edited event.</summary>
        [JsonStringEnumMemberName("Save")]
        Save,

        /// <summary>Deletes an event.</summary>
        [JsonStringEnumMemberName("Delete")]
        Delete,

        /// <summary>Deletes one occurrence of a recurring event.</summary>
        [JsonStringEnumMemberName("DeleteOccurrence")]
        DeleteOccurrence,

        /// <summary>Deletes every occurrence of a recurring event.</summary>
        [JsonStringEnumMemberName("DeleteSeries")]
        DeleteSeries,

        /// <summary>Edits one occurrence of a recurring event.</summary>
        [JsonStringEnumMemberName("EditOccurrence")]
        EditOccurrence,

        /// <summary>Edits every occurrence of a recurring event.</summary>
        [JsonStringEnumMemberName("EditSeries")]
        EditSeries,

        /// <summary>Edits an occurrence of a recurring event and the occurrences after it.</summary>
        [JsonStringEnumMemberName("EditFollowingEvents")]
        EditFollowingEvents,

        /// <summary>Deletes an occurrence of a recurring event and the occurrences after it.</summary>
        [JsonStringEnumMemberName("DeleteFollowingEvents")]
        DeleteFollowingEvents,
    }

    /// <summary>
    /// The settings of the schedule's events.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "EventSettingsModel")]
    public sealed class EventSettingsModel : JsObject
    {
        /// <summary>Gets the events the schedule shows.</summary>
        /// <remarks>A write and then the schedule's <see cref="FusionSchedule.DataBind"/> replace the events it shows.</remarks>
        /// <typeparam name="TEvent">The class of the events.</typeparam>
        /// <returns>The events.</returns>
        public Property<IReadOnlyList<TEvent>> DataSource<TEvent>() => Property<IReadOnlyList<TEvent>>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="CellClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "CellClickEventArgs")]
    public sealed class CellClickEventArgs : JsObject
    {
        /// <summary>Gets the time the cell starts at.</summary>
        public ReadOnly<DateTime> StartTime => ReadOnly<DateTime>();

        /// <summary>Gets the time the cell ends at.</summary>
        public ReadOnly<DateTime> EndTime => ReadOnly<DateTime>();

        /// <summary>Gets whether the cell is an all-day cell.</summary>
        public ReadOnly<bool> IsAllDay => ReadOnly<bool>();

        /// <summary>Gets the index of the resource group the cell belongs to.</summary>
        public ReadOnly<int> GroupIndex => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="EventClick"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "EventClickArgs")]
    public sealed class EventClickArgs : JsObject
    {
        /// <summary>Gets whether the click is stopped; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="ActionBegin{TEvent}"/> receives.
    /// </summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    [Mirrors("@syncfusion/ej2-schedule", "ActionEventArgs")]
    public sealed class ActionEventArgs<TEvent> : JsObject
    {
        /// <summary>Gets the name of the action.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();

        /// <summary>Gets whether the action stops; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the events the action adds.</summary>
        public ReadOnly<IReadOnlyList<TEvent>> AddedRecords => ReadOnly<IReadOnlyList<TEvent>>(optional: true);

        /// <summary>Gets the events the action changes.</summary>
        public ReadOnly<IReadOnlyList<TEvent>> ChangedRecords => ReadOnly<IReadOnlyList<TEvent>>(optional: true);

        /// <summary>Gets the events the action deletes.</summary>
        public ReadOnly<IReadOnlyList<TEvent>> DeletedRecords => ReadOnly<IReadOnlyList<TEvent>>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="ActionComplete"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "ActionEventArgs")]
    public sealed class ActionEventArgs : JsObject
    {
        /// <summary>Gets the name of the action.</summary>
        public ReadOnly<string> RequestType => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="Navigating"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "NavigatingEventArgs")]
    public sealed class NavigatingEventArgs : JsObject
    {
        /// <summary>Gets what the schedule moves to, "date" or "view".</summary>
        public ReadOnly<string> Action => ReadOnly<string>();

        /// <summary>Gets whether the schedule stays where it is; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Gets the date the schedule showed before it moves.</summary>
        public ReadOnly<DateTime> PreviousDate => ReadOnly<DateTime>(optional: true);

        /// <summary>Gets the date the schedule shows once it moves.</summary>
        public ReadOnly<DateTime> CurrentDate => ReadOnly<DateTime>(optional: true);

        /// <summary>Gets the name of the view the schedule showed before it moves.</summary>
        public ReadOnly<string> PreviousView => ReadOnly<string>(optional: true);

        /// <summary>Gets the name of the view the schedule shows once it moves.</summary>
        public ReadOnly<string> CurrentView => ReadOnly<string>(optional: true);

        /// <summary>Gets the index of the view the schedule shows once it moves, from 0.</summary>
        public ReadOnly<int> ViewIndex => ReadOnly<int>(optional: true);
    }

    /// <summary>
    /// What a listener of <see cref="PopupOpen{TEvent}"/> receives.
    /// </summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    [Mirrors("@syncfusion/ej2-schedule", "PopupOpenEventArgs")]
    public sealed class PopupOpenEventArgs<TEvent> : JsObject
    {
        /// <summary>Gets the kind of popup that opens, such as Editor or QuickInfo.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets the event or the cell the popup is for.</summary>
        public ReadOnly<TEvent> Data => ReadOnly<TEvent>(optional: true);

        /// <summary>Gets whether the popup stays closed; write true to keep it closed.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="PopupClose"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-schedule", "PopupCloseEventArgs")]
    public sealed class PopupCloseEventArgs : JsObject
    {
        /// <summary>Gets the kind of popup that closes, such as Editor or QuickInfo.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>
    /// What a listener of <see cref="EventRendered{TEvent}"/> receives.
    /// </summary>
    /// <typeparam name="TEvent">The class of the events.</typeparam>
    [Mirrors("@syncfusion/ej2-schedule", "EventRenderedArgs")]
    public sealed class EventRenderedArgs<TEvent> : JsObject
    {
        /// <summary>Gets the event that renders.</summary>
        public ReadOnly<TEvent> Data => ReadOnly<TEvent>();

        /// <summary>Gets whether the event renders; write true to stop it.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Gets the kind of element that renders the event.</summary>
        public ReadOnly<string> Type => ReadOnly<string>(optional: true);
    }
}
