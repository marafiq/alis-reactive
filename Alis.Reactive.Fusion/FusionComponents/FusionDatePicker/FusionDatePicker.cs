using System;
using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A date picker: EJ2's DatePicker, which holds the day a person chose.
/// </summary>
/// <remarks>
/// A picker bound to a member is an input: a request container sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="Change"/> reads the committed day.
/// </remarks>
/// <typeparam name="TValue">The type of the member the picker binds, a date that admits null, since the picker holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-calendars", "DatePicker")]
public sealed class FusionDatePicker<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the day the picker holds, null until a person chooses one.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets whether the date can be typed into the field.</summary>
    public Property<bool> AllowEdit => Property<bool>();

    /// <summary>Gets whether the editor accepts interaction.</summary>
    public Property<bool> Enabled => Property<bool>();

    /// <summary>Gets whether the current date is protected from editing and calendar selection.</summary>
    public Property<bool> Readonly => Property<bool>();

    /// <summary>Gets whether focusing the input opens its calendar.</summary>
    public Property<bool> OpenOnFocus => Property<bool>();

    /// <summary>Gets whether the picker corrects out-of-range dates to the configured range.</summary>
    public Property<bool> StrictMode => Property<bool>();

    /// <summary>Gets the earliest date available for selection.</summary>
    public Property<DateTime?> Min => Property<DateTime?>();

    /// <summary>Gets the latest date available for selection.</summary>
    public Property<DateTime?> Max => Property<DateTime?>();

    /// <summary>Gets the calendar level displayed on opening.</summary>
    public Property<FusionDatePickerView> Start => Property<FusionDatePickerView>();

    /// <summary>Gets the most detailed calendar level available for selection.</summary>
    public Property<FusionDatePickerView> Depth => Property<FusionDatePickerView>();

    /// <summary>Applies pending changes to the live date editor.</summary>
    public void DataBind() => Method();

    /// <summary>Opens the calendar when the date editor is available.</summary>
    public void Show() => Method();

    /// <summary>Opens the calendar with the browser interaction that requested it.</summary>
    /// <param name="interaction">A source of the original date interaction.</param>
    public void Show(TypedSource<BrowserEvent?> interaction) => Method([(Arg<string?>)(string?)null, (Arg<BrowserEvent?>)interaction]);

    /// <summary>Closes the open calendar unless its close listener holds it open.</summary>
    public void Hide() => Method();

    /// <summary>Closes the calendar with the browser interaction that requested it.</summary>
    /// <param name="interaction">A source of the original date interaction.</param>
    public void Hide(TypedSource<BrowserEvent?> interaction) => Method([(Arg<BrowserEvent?>)interaction]);

    /// <summary>Reads the calendar level after the popup has been created.</summary>
    /// <returns>The result supplied by the picker.</returns>
    public MethodResult<string?> CurrentView() => Method<string?>();

    /// <summary>Gets the event raised before the calendar opens.</summary>
    public Event<PopupEventArgs> Open => Event<PopupEventArgs>();

    /// <summary>Gets the event raised before the calendar closes.</summary>
    public Event<PopupEventArgs> Close => Event<PopupEventArgs>();

    /// <summary>Gets the event raised when the date editor receives focus.</summary>
    public Event<FocusEventArgs> Focus => Event<FocusEventArgs>();

    /// <summary>Gets the event raised when the date editor loses focus.</summary>
    public Event<BlurEventArgs> Blur => Event<BlurEventArgs>();

    /// <summary>Gets the calendar system used for display and selection.</summary>
    public Property<FusionDatePickerCalendar> CalendarMode => Property<FusionDatePickerCalendar>();

    /// <summary>Gets the CSS classes of the date editor and its popup.</summary>
    public Property<string?> CssClass => Property<string?>();

    /// <summary>Gets how weekday headings are abbreviated.</summary>
    public Property<FusionDatePickerHeader> DayHeaderFormat => Property<FusionDatePickerHeader>();

    /// <summary>Gets whether the field uses the date entry mask.</summary>
    public Property<bool> EnableMask => Property<bool>();

    /// <summary>Gets whether the editor saves the selected date on page exit.</summary>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the calendar opens in right-to-left order.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the first weekday of the calendar, counting Sunday as zero.</summary>
    public Property<int?> FirstDayOfWeek => Property<int?>();

    /// <summary>Gets the behavior of the floating date label.</summary>
    public Property<FusionDatePickerLabel> FloatLabelType => Property<FusionDatePickerLabel>();

    /// <summary>Gets the locale used to resolve calendar resources and date patterns.</summary>
    public Property<string> Locale => Property<string>();

    /// <summary>Gets the date entry hint.</summary>
    public Property<string?> Placeholder => Property<string?>();

    /// <summary>Gets the server offset applied when the editor renders, in hours.</summary>
    public Property<double?> ServerTimezoneOffset => Property<double?>();

    /// <summary>Gets whether the date editor offers its clear control.</summary>
    public Property<bool> ShowClearButton => Property<bool>();

    /// <summary>Gets whether the calendar includes the Today button.</summary>
    public Property<bool> ShowTodayButton => Property<bool>();

    /// <summary>Gets whether calendar rows display week numbers.</summary>
    public Property<bool> WeekNumber => Property<bool>();

    /// <summary>Gets the week-numbering convention.</summary>
    public Property<FusionDatePickerWeekRule> WeekRule => Property<FusionDatePickerWeekRule>();

    /// <summary>Gets the stacking order requested for the calendar popup.</summary>
    public Property<double> ZIndex => Property<double>();

    /// <summary>Gets the setter for a date pattern or the local default.</summary>
    public WriteOnly<string?> Format => WriteOnly<string?>();

    /// <summary>Gets the setter for a culture-resolved date pattern.</summary>
    public WriteOnly<FusionDateFormatPattern?> FormatPattern => WriteOnly<FusionDateFormatPattern?>(name: "format");

    /// <summary>Gets the setter for additional input date patterns.</summary>
    public WriteOnly<string[]?> InputFormats => WriteOnly<string[]?>();

    /// <summary>Gets the setter for additional culture-resolved input date patterns.</summary>
    public WriteOnly<FusionDateFormatPattern[]?> InputFormatPatterns => WriteOnly<FusionDateFormatPattern[]?>(name: "inputFormats");

    /// <summary>Gets the setter for the CSS width of the editor.</summary>
    public WriteOnly<string?> Width => WriteOnly<string?>();

    /// <summary>Gets the setter for the width of the editor in pixels.</summary>
    public WriteOnly<double?> WidthInPixels => WriteOnly<double?>(name: "width");

    /// <summary>Gets the setter for the date editor’s keyboard action bindings.</summary>
    public WriteOnly<IReadOnlyDictionary<FusionDatePickerKey, string>?> KeyConfigs => WriteOnly<IReadOnlyDictionary<FusionDatePickerKey, string>?>();

    /// <summary>Gets the original input used by the date editor.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Rebuilds the date editor using its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Releases the enhanced date editor and restores its original input.</summary>
    public void Destroy() => Method();

    /// <summary>Reads the recovery data for the date editor.</summary>
    /// <returns>The result supplied by the picker.</returns>
    public MethodResult<string> GetPersistData() => Method<string>();

    /// <summary>Shows the requested calendar view at the specified date.</summary>
    /// <param name="view">The calendar view to open.</param>
    /// <param name="date">The date to focus.</param>
    public void NavigateTo(Arg<FusionDatePickerView> view, Arg<DateTime> date) => Method([view, date]);

    /// <summary>Gets the native root returned by the date editor.</summary>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Gets the event raised when the calendar changes its displayed period.</summary>
    public Event<NavigationArgs> Navigated => Event<NavigationArgs>();

    /// <summary>Gets the event raised while a calendar day is prepared.</summary>
    public Event<DayCellArgs> RenderDayCell => Event<DayCellArgs>();

    /// <summary>Gets the event raised by the calendar’s clear control.</summary>
    public Event<ClearedArgs> Cleared => Event<ClearedArgs>();

    /// <summary>Gets the event raised when the date editor is destroyed.</summary>
    public Event<DestructionArgs> Destroyed => Event<DestructionArgs>();

    /// <summary>Gets the words used for individual empty date mask segments.</summary>
    public MaskWords MaskPlaceholder => LiveObject<MaskWords>();

    /// <summary>Gets the Hijri calendar service when the Islamic calendar is installed.</summary>
    public HijriCalendar IslamicModule => LiveObject<HijriCalendar>(optional: true);

    /// <summary>Reads the calendar and mask modules requested by the editor’s current settings.</summary>
    public LiveObjects<CalendarRequirement> RequiredModules() => ReturnedObjects<CalendarRequirement>();

    /// <summary>Moves the keyboard's focus to the picker.</summary>
    public void FocusIn() => Method();

    /// <summary>Takes the keyboard's focus away from the picker.</summary>
    public void FocusOut() => Method();

    /// <summary>Gets the event raised when the picker's day changes.</summary>
    public Event<ChangedEventArgs> Change => Event<ChangedEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-calendars", "ChangedEventArgs")]
    public sealed class ChangedEventArgs : JsObject
    {
        /// <summary>Gets the EJ2 notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the date input associated with the change.</summary>
        public ChildElement Element => LiveObject<ChildElement>(optional: true);

        /// <summary>Gets the original browser interaction when the change was initiated by a person.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true, nullable: true);

        /// <summary>Gets the original browser interaction as a source to pass to another calendar operation.</summary>
        public ReadOnly<BrowserEvent?> Interaction => ReadOnly<BrowserEvent?>(name: "event", optional: true);

        /// <summary>Gets the day the picker holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>(optional: true);

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>(optional: true);
    }

    /// <summary>The pending opening or closing of the date calendar.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "PopupObjectArgs")]
    public sealed class PopupEventArgs : JsObject
    {
        /// <summary>Gets whether the pending popup operation is canceled.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the live calendar popup for the pending operation.</summary>
        public CalendarPopup Popup => LiveObject<CalendarPopup>(optional: true);

        /// <summary>Gets the browser action for an interactive popup operation.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true, nullable: true);

        /// <summary>Prevents the pending popup operation in the current event handler.</summary>
        public void PreventDefault() => Method(optional: true);
    }

    /// <summary>The date editor receiving focus.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "FocusEventArgs")]
    public sealed class FocusEventArgs : JsObject
    {
        /// <summary>Gets the date editor that received focus.</summary>
        public FusionDatePicker<TValue> Model => LiveObject<FusionDatePicker<TValue>>(optional: true);
    }

    /// <summary>The date editor releasing focus.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "BlurEventArgs")]
    public sealed class BlurEventArgs : JsObject
    {
        /// <summary>Gets the date editor that lost focus.</summary>
        public FusionDatePicker<TValue> Model => LiveObject<FusionDatePicker<TValue>>(optional: true);
    }

    /// <summary>The browser interaction that prompted a date editor notification.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class BrowserEvent : JsObject
    {
        /// <summary>Gets the browser event type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>The displayed date and calendar level after a navigation.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "NavigatedEventArgs")]
    public sealed class NavigationArgs : JsObject
    {
        /// <summary>Gets the displayed date.</summary>
        public ReadOnly<DateTime> Date => ReadOnly<DateTime>(optional: true);

        /// <summary>Gets the navigation notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the displayed calendar level.</summary>
        public ReadOnly<string> View => ReadOnly<string>(optional: true);

        /// <summary>Gets the original browser interaction for manual navigation.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true, nullable: true);
    }

    /// <summary>The calendar day being prepared for selection.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "RenderDayCellEventArgs")]
    public sealed class DayCellArgs : JsObject
    {
        /// <summary>Gets the date represented by this cell.</summary>
        public ReadOnly<DateTime> Date => ReadOnly<DateTime>(optional: true);

        /// <summary>Gets the day-rendering notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the calendar level being rendered.</summary>
        public ReadOnly<string> CurrentView => ReadOnly<string>(optional: true);

        /// <summary>Gets whether this date is outside the editor’s configured range.</summary>
        public ReadOnly<bool> IsOutOfRange => ReadOnly<bool>(optional: true);

        /// <summary>Gets whether this cell must be unavailable for selection.</summary>
        public Property<bool> IsDisabled => Property<bool>(optional: true);

        /// <summary>Gets the rendered cell being prepared.</summary>
        public ChildElement Element => LiveObject<ChildElement>(optional: true);
    }

    /// <summary>The withdrawal of a date through the clear control.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "ClearedEventArgs")]
    public sealed class ClearedArgs : JsObject
    {
        /// <summary>Gets the browser gesture that cleared the date.</summary>
        public BrowserEvent OriginalEvent => LiveObject<BrowserEvent>(name: "event", optional: true);
    }

    /// <summary>The prompts used for empty date mask segments.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "MaskPlaceholderModel")]
    public sealed class MaskWords : JsObject
    {
        /// <summary>Gets the prompt used for the day segment.</summary>
        public Property<string> Day => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the month segment.</summary>
        public Property<string> Month => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the year segment.</summary>
        public Property<string> Year => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the hour segment.</summary>
        public Property<string> Hour => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the minute segment.</summary>
        public Property<string> Minute => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the second segment.</summary>
        public Property<string> Second => Property<string>(optional: true);

        /// <summary>Gets the prompt used for the weekday segment.</summary>
        public Property<string> DayOfTheWeek => Property<string>(optional: true);
    }

    /// <summary>The live date calendar popup supplied to its open and close listeners.</summary>
    [Mirrors("@syncfusion/ej2-popups", "Popup")]
    public sealed class CalendarPopup : JsObject
    {
        /// <summary>Gets the live element used to anchor this date calendar.</summary>
        public ChildElement Anchor => LiveObject<ChildElement>(name: "relateTo");

        /// <summary>Gets the popup container.</summary>
        public ChildElement Element => LiveObject<ChildElement>();

        /// <summary>Gets the calendar popup’s stacking order.</summary>
        public Property<double> ZIndex => Property<double>();

        /// <summary>Gets the calendar position relative to its current anchor.</summary>
        public PopupCoordinates Position => LiveObject<PopupCoordinates>();

        /// <summary>Gets the overflow policy for each edge of the calendar.</summary>
        public PopupCollision Collision => LiveObject<PopupCollision>();

        /// <summary>Reads the root element of the mounted calendar dialog.</summary>
        /// <returns>The element that owns this popup.</returns>
        public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

        /// <summary>Repositions the open calendar at its current anchor and applies its overflow policy.</summary>
        public void RefreshPosition() => Method();

        /// <summary>Gets the mounted date field’s anchor as a source for a call that takes a DOM element.</summary>
        public ReadOnly<ChildElement> AnchorTarget => ReadOnly<ChildElement>(name: "relateTo");

        /// <summary>Repositions the calendar and checks the supplied target’s fixed ancestors.</summary>
        /// <param name="target">A live element supplied by a browser object.</param>
        public void RefreshPosition(TypedSource<ChildElement> target) => Method([(Arg<ChildElement>)target]);

        /// <summary>Repositions the calendar at a live target with the chosen overflow behavior.</summary>
        /// <param name="target">A live element supplied by a browser object.</param>
        /// <param name="skipCollision">Whether to preserve positions that extend beyond the viewport.</param>
        public void RefreshPosition(TypedSource<ChildElement> target, Arg<bool> skipCollision) => Method([(Arg<ChildElement>)target, skipCollision]);

        /// <summary>Reads the scrolling element and document ancestors of a live calendar target.</summary>
        /// <param name="target">A live element supplied by a browser object.</param>
        /// <returns>The scrolling nodes for this target, from its nearest scrolling container outward.</returns>
        public LiveObjects<ScrollParent> GetScrollableParent(TypedSource<ChildElement> target) => ReturnedObjects<ScrollParent>([(Arg<ChildElement>)target]);

        /// <summary>Applies the overflow policy to the current calendar position.</summary>
        public void ResolveCollision() => Method();

        /// <summary>Starts following scroll events from the current anchor’s ancestors.</summary>
        public void WireScrollEvents() => Method();

        /// <summary>Stops following scroll events from the current anchor’s ancestors.</summary>
        public void UnwireScrollEvents() => Method();

        /// <summary>Gets the behavior when the position of a scrollable ancestor changes.</summary>
        public Property<FusionDatePickerScroll> ActionOnScroll => Property<FusionDatePickerScroll>();

        /// <summary>Gets whether the open calendar reads from right to left.</summary>
        public Property<bool> EnableRtl => Property<bool>();

        /// <summary>Gets the setter for the popup’s CSS width.</summary>
        public WriteOnly<string> Width => WriteOnly<string>();

        /// <summary>Gets the setter for the popup’s width in pixels.</summary>
        public WriteOnly<double> WidthInPixels => WriteOnly<double>(name: "width");

        /// <summary>Gets the setter for the popup’s CSS height.</summary>
        public WriteOnly<string> Height => WriteOnly<string>();

        /// <summary>Gets the setter for the popup’s height in pixels.</summary>
        public WriteOnly<double> HeightInPixels => WriteOnly<double>(name: "height");

        /// <summary>Applies pending changes to the calendar popup.</summary>
        public void DataBind() => Method();

        /// <summary>Gets the horizontal offset applied to an anchor-based position.</summary>
        public Property<double> OffsetX => Property<double>();

        /// <summary>Gets the vertical offset applied to an anchor-based position.</summary>
        public Property<double> OffsetY => Property<double>();

        /// <summary>Gets the reference used for aligning the calendar to its target.</summary>
        public Property<FusionDatePickerAnchor> TargetType => Property<FusionDatePickerAnchor>();
    }

    /// <summary>The final notification raised by the date editor.</summary>
    public sealed class DestructionArgs : JsObject
    {
        /// <summary>Gets the destruction notification name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the false cancellation flag supplied by this terminal notification.</summary>
        public ReadOnly<bool> Cancel => ReadOnly<bool>();
    }

    /// <summary>The Islamic calendar service installed by this date editor.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "Islamic")]
    public sealed class HijriCalendar : JsObject
    {
        /// <summary>Reads the name of the calendar service.</summary>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<string> GetModuleName() => Method<string>();

        /// <summary>Converts a Hijri referral date to its Gregorian calendar date.</summary>
        /// <param name="year">The year.</param>
        /// <param name="month">The month.</param>
        /// <param name="day">The day.</param>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<DateTime> ToGregorian(Arg<int> year, Arg<int> month, Arg<int> day) => Method<DateTime>([year, month, day]);

        /// <summary>Reads the Hijri year, month and day of a Gregorian date.</summary>
        /// <param name="date">The date to focus.</param>
        public HijriDate GetIslamicDate(Arg<DateTime> date) => ReturnedObject<HijriDate>([date]);

        /// <summary>Compares the two dates at month level using the calendar’s current view.</summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<int> IslamicCompareMonth(Arg<DateTime> start, Arg<DateTime> end) => Method<int>([start, end]);

        /// <summary>Compares the two dates at year level using the calendar’s current view.</summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<int> HijriCompareYear(Arg<DateTime> start, Arg<DateTime> end) => Method<int>([start, end]);

        /// <summary>Compares the two dates at decade level using the calendar’s current view.</summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<int> HijriCompareDecade(Arg<DateTime> start, Arg<DateTime> end) => Method<int>([start, end]);

        /// <summary>Compares dates using the calendar’s current view and the requested Hijri year interval.</summary>
        /// <param name="start">The start.</param>
        /// <param name="end">The end.</param>
        /// <param name="modifier">The modifier.</param>
        /// <returns>The result supplied by the picker.</returns>
        public MethodResult<int> IslamicCompare(Arg<DateTime> start, Arg<DateTime> end, Arg<int> modifier) => Method<int>([start, end, modifier]);

        /// <summary>Advances the open Islamic calendar by its current month, year or decade.</summary>
        public void IslamicNext() => Method();

        /// <summary>Moves the open Islamic calendar back by its current month, year or decade.</summary>
        public void IslamicPrevious() => Method();
    }

    /// <summary>The year, month and day of a Hijri referral.</summary>
    [Mirrors("@syncfusion/ej2-calendars", "IslamicDateArgs")]
    public sealed class HijriDate : JsObject
    {
        /// <summary>Gets the Hijri year.</summary>
        public ReadOnly<int> Year => ReadOnly<int>();

        /// <summary>Gets the Hijri month.</summary>
        public ReadOnly<int> Month => ReadOnly<int>();

        /// <summary>Gets the Hijri day of the month.</summary>
        public ReadOnly<int> Date => ReadOnly<int>();
    }

    /// <summary>A calendar module requested by this date editor.</summary>
    [Mirrors("@syncfusion/ej2-base", "ModuleDeclaration")]
    public sealed class CalendarRequirement : JsObject
    {
        /// <summary>Gets the requested module member.</summary>
        public ReadOnly<string> Member => ReadOnly<string>();

        /// <summary>Gets the optional module name.</summary>
        public ReadOnly<string> Name => ReadOnly<string>(optional: true);

        /// <summary>Gets the date editor passed to this module’s constructor.</summary>
        public LiveObjects<FusionDatePicker<TValue>> Args => LiveObjects<FusionDatePicker<TValue>>();
    }

    /// <summary>The horizontal and vertical position of the date calendar popup.</summary>
    [Mirrors("@syncfusion/ej2-popups", "PositionDataModel")]
    public sealed class PopupCoordinates : JsObject
    {
        /// <summary>Gets the setter for the popup’s left coordinate in pixels.</summary>
        public WriteOnly<double> LeftInPixels => WriteOnly<double>(name: "X", optional: true);

        /// <summary>Gets the setter for the popup’s top coordinate in pixels.</summary>
        public WriteOnly<double> TopInPixels => WriteOnly<double>(name: "Y", optional: true);

        /// <summary>Gets the setter for the named horizontal alignment.</summary>
        public WriteOnly<FusionDatePickerHorizontal> Horizontal => WriteOnly<FusionDatePickerHorizontal>(name: "X", optional: true);

        /// <summary>Gets the setter for the named vertical alignment.</summary>
        public WriteOnly<FusionDatePickerVertical> Vertical => WriteOnly<FusionDatePickerVertical>(name: "Y", optional: true);
    }

    /// <summary>The separate horizontal and vertical overflow policies of the calendar popup.</summary>
    [Mirrors("@syncfusion/ej2-popups", "CollisionAxis")]
    public sealed class PopupCollision : JsObject
    {
        /// <summary>Gets the policy used when the calendar extends across a horizontal edge.</summary>
        public Property<FusionDatePickerCollision> Horizontal => Property<FusionDatePickerCollision>(name: "X", optional: true);

        /// <summary>Gets the policy used when the calendar extends across a vertical edge.</summary>
        public Property<FusionDatePickerCollision> Vertical => Property<FusionDatePickerCollision>(name: "Y", optional: true);
    }

    /// <summary>A scrolling DOM node that contains the calendar’s anchor.</summary>
    [Mirrors("lib.dom", "Node")]
    public sealed class ScrollParent : JsObject
    {
        /// <summary>Gets the DOM node name of this scrolling ancestor.</summary>
        public ReadOnly<string> NodeName => ReadOnly<string>();

        /// <summary>Gets the DOM node type of this scrolling ancestor.</summary>
        public ReadOnly<int> NodeType => ReadOnly<int>();
    }
}
