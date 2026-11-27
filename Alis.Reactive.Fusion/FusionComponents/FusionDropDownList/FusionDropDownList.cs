using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.Elements;
using Alis.Reactive.InputsAndIds;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A drop-down list: EJ2's DropDownList, a list a person chooses one item from, which holds the value of the item
/// chosen.
/// </summary>
/// <remarks>
/// A list bound to a member is an input: a request container sends its <see cref="Value"/> and a check reads it. EJ2 raises
/// <see cref="Change"/> when a person chooses an item or the value changes.
/// </remarks>
/// <typeparam name="TValue">The type of the member the list binds, which admits null, since the list holds null until a choice.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "DropDownList")]
public sealed class FusionDropDownList<TValue> : FusionComponent, IInputComponent
{
    /// <summary>Gets the value the list holds, null until a person chooses an item.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Gets the text the list shows for the item chosen, null until a choice.</summary>
    public Property<string?> Text => Property<string?>();

    /// <summary>Gets the configured local catalogue rows or primitive choices.</summary>
    /// <remarks>Use this property for a local array. Call <see cref="DataBind"/> after a write so the list shows it.</remarks>
    /// <typeparam name="TItem">The local row or primitive choice type.</typeparam>
    /// <returns>The items.</returns>
    public Property<IReadOnlyList<TItem>> DataSource<TItem>() => Property<IReadOnlyList<TItem>>();

    /// <summary>Finds a catalogue row by its string value in the loaded list.</summary>
    /// <typeparam name="TItem">The type of a catalogue row.</typeparam>
    /// <param name="value">The value to look up.</param>
    /// <returns>The matching row, or null if the list has no match.</returns>
    public MethodResult<TItem?> GetDataByValue<TItem>(Arg<string> value)
        where TItem : class
        => Method<TItem?>([value]);

    /// <summary>Appends a catalogue item to the list.</summary>
    /// <typeparam name="TItem">The type of a catalogue item.</typeparam>
    /// <param name="item">The item to add.</param>
    public void AddItem<TItem>(Arg<TItem> item)
        where TItem : class
        => Method([item]);

    /// <summary>Inserts one catalogue item at a position in the list.</summary>
    /// <typeparam name="TItem">The type of a catalogue item.</typeparam>
    /// <param name="item">The item to add.</param>
    /// <param name="itemIndex">The zero-based position.</param>
    public void AddItem<TItem>(Arg<TItem> item, Arg<int> itemIndex)
        where TItem : class
        => Method([item, itemIndex]);

    /// <summary>Appends catalogue items to the list in their supplied order.</summary>
    /// <typeparam name="TItem">The type of a catalogue item.</typeparam>
    /// <param name="items">The items to add.</param>
    public void AddItems<TItem>(Arg<IReadOnlyList<TItem>> items) => Method([items], name: "addItem");

    /// <summary>Inserts catalogue items at a position in the list.</summary>
    /// <typeparam name="TItem">The type of a catalogue item.</typeparam>
    /// <param name="items">The items to add.</param>
    /// <param name="itemIndex">The zero-based position.</param>
    public void AddItems<TItem>(Arg<IReadOnlyList<TItem>> items, Arg<int> itemIndex) => Method([items, itemIndex], name: "addItem");

    /// <summary>Looks up a value in a text catalogue.</summary>
    /// <param name="value">The value to find.</param>
    /// <returns>The matching entry, or null if the loaded catalogue has no match.</returns>
    public MethodResult<string?> GetDataByValue(Arg<string> value) => Method<string?>([value]);

    /// <summary>Looks up a value in a numeric catalogue.</summary>
    /// <param name="value">The value to find.</param>
    /// <returns>The matching entry, or null if the loaded catalogue has no match.</returns>
    public MethodResult<double?> GetDataByValue(Arg<double> value) => Method<double?>([value]);

    /// <summary>Looks up a value in a Boolean catalogue.</summary>
    /// <param name="value">The value to find.</param>
    /// <returns>The matching entry, or null if the loaded catalogue has no match.</returns>
    public MethodResult<bool?> GetDataByValue(Arg<bool> value) => Method<bool?>([value]);

    /// <summary>Appends a numeric item.</summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(Arg<double> item) => Method([item]);

    /// <summary>Inserts a numeric item.</summary>
    /// <param name="item">The item to add.</param>
    /// <param name="itemIndex">The zero-based position.</param>
    public void AddItem(Arg<double> item, Arg<int> itemIndex) => Method([item, itemIndex]);

    /// <summary>Appends a Boolean item.</summary>
    /// <param name="item">The item to add.</param>
    public void AddItem(Arg<bool> item) => Method([item]);

    /// <summary>Inserts a Boolean item.</summary>
    /// <param name="item">The item to add.</param>
    /// <param name="itemIndex">The zero-based position.</param>
    public void AddItem(Arg<bool> item, Arg<int> itemIndex) => Method([item, itemIndex]);

    /// <summary>Disables an item identified by its string value.</summary>
    /// <param name="value">The string value of the item.</param>
    public void DisableItem(Arg<string> value) => Method([value]);

    /// <summary>Disables an item identified by its zero-based position.</summary>
    /// <param name="itemIndex">The zero-based position of the item.</param>
    public void DisableItem(Arg<int> itemIndex) => Method([itemIndex]);

    /// <summary>Disables a catalogue row selected by its mapped value.</summary>
    /// <typeparam name="TItem">The type of the row.</typeparam>
    /// <param name="item">The catalogue row.</param>
    public void DisableItem<TItem>(Arg<TItem> item)
        where TItem : class
        => Method([item]);

    /// <summary>Replaces the display and value mappings for this list's rows.</summary>
    /// <typeparam name="TItem">The catalogue row type.</typeparam>
    /// <returns>The property that accepts field mappings for these rows.</returns>
    public WriteOnly<FusionDropDownListFields<TItem>> Fields<TItem>() => WriteOnly<FusionDropDownListFields<TItem>>();

    /// <summary>Gets whether text searches ignore letter case.</summary>
    public Property<bool> IgnoreCase => Property<bool>();

    /// <summary>Gets whether text searches ignore accent marks.</summary>
    public Property<bool> IgnoreAccent => Property<bool>();

    /// <summary>Gets how the list matches the search text.</summary>
    public Property<FusionDropDownListMatchMode> FilterType => Property<FusionDropDownListMatchMode>();

    /// <summary>Gets the rendered list options in their display order.</summary>
    /// <returns>The iterable options, empty when the list contains none.</returns>
    public ItemCollection GetItems() => ReturnedObject<ItemCollection>();

    /// <summary>Applies a local filter while the search popup is active.</summary>
    /// <typeparam name="TItem">The type of a local entry.</typeparam>
    /// <param name="dataSource">The local entries.</param>
    public void Filter<TItem>(Arg<IReadOnlyList<TItem>> dataSource) => Method([dataSource]);

    /// <summary>Applies a local filter while the search popup is active.</summary>
    /// <typeparam name="TItem">The type of a local entry.</typeparam>
    /// <param name="dataSource">The local entries.</param>
    /// <param name="query">The live EJ2 Query applied to the local entries.</param>
    public void Filter<TItem>(Arg<IReadOnlyList<TItem>> dataSource, TypedSource<FusionDropDownListQuery> query) => Method([dataSource, (Arg<FusionDropDownListQuery>)query]);

    /// <summary>Applies a local filter while the search popup is active.</summary>
    /// <typeparam name="TItem">The type of a local entry.</typeparam>
    /// <param name="dataSource">The local entries.</param>
    /// <param name="query">The live EJ2 Query applied to the local entries.</param>
    /// <param name="fields">The display and value fields used for these entries.</param>
    public void Filter<TItem>(Arg<IReadOnlyList<TItem>> dataSource, TypedSource<FusionDropDownListQuery> query, Arg<FusionDropDownListFields<TItem>> fields) => Method([dataSource, (Arg<FusionDropDownListQuery>)query, fields]);

    /// <summary>Gets the zero-based index of the selected item, or null when unassigned.</summary>
    public Property<int?> Index => Property<int?>();

    /// <summary>Gets whether the list permits input and focus.</summary>
    public Property<bool> Enabled => Property<bool>();

    /// <summary>Gets whether the current selection is protected from user changes.</summary>
    public Property<bool> Readonly => Property<bool>();

    /// <summary>Gets the hint displayed when the selection is empty.</summary>
    public Property<string?> Placeholder => Property<string?>();

    /// <summary>Gets the HTML used for a catalogue option.</summary>
    /// <remarks>Supply an app-authored Fusion template or trusted markup. Setting null on a mounted list can retain the previous option markup.</remarks>
    public WriteOnly<string> ItemTemplate => WriteOnly<string>();

    /// <summary>Gets the HTML used for the selected item.</summary>
    /// <remarks>Supply an app-authored Fusion template or trusted markup. Setting null on a mounted list can retain the previous selected-value markup.</remarks>
    public WriteOnly<string> ValueTemplate => WriteOnly<string>();

    /// <summary>Gets the HTML used for a catalogue group heading.</summary>
    /// <remarks>Supply an app-authored Fusion template or trusted markup. EJ2 cannot clear an already-rendered group template with null.</remarks>
    public WriteOnly<string> GroupTemplate => WriteOnly<string>();

    /// <summary>Gets the HTML displayed at the top of the popup.</summary>
    /// <remarks>Supply an app-authored Fusion template or trusted markup. EJ2 cannot clear an already-rendered popup header with null.</remarks>
    public WriteOnly<string> HeaderTemplate => WriteOnly<string>();

    /// <summary>Gets the HTML displayed at the bottom of the popup.</summary>
    /// <remarks>Supply an app-authored Fusion template or trusted markup. EJ2 cannot clear an already-rendered popup footer with null.</remarks>
    public WriteOnly<string> FooterTemplate => WriteOnly<string>();

    /// <summary>Gets the list's original input element.</summary>
    public ChildElement Element => LiveObject<ChildElement>();

    /// <summary>Gets the original MVC input through the list's root-element method.</summary>
    /// <returns>The list's native input.</returns>
    public ChildElement GetRootElement() => ReturnedObject<ChildElement>();

    /// <summary>Closes the enhanced list and restores its original element.</summary>
    public void Destroy() => Method();

    /// <summary>Gets the notification raised after the enhanced list has been destroyed.</summary>
    public Event<DestructionEventArgs> Destroyed => Event<DestructionEventArgs>();

    /// <summary>Gets whether EJ2 persists the selected value on page exit.</summary>
    public Property<bool> EnablePersistence => Property<bool>();

    /// <summary>Gets whether the list lays out right to left.</summary>
    public Property<bool> EnableRtl => Property<bool>();

    /// <summary>Gets the CSS classes assigned to the list wrapper.</summary>
    public Property<string?> CssClass => Property<string?>();

    /// <summary>Gets whether the list offers a clear control.</summary>
    public Property<bool> ShowClearButton => Property<bool>();

    /// <summary>Gets the CSS width of the list.</summary>
    public WriteOnly<string> Width => WriteOnly<string>();

    /// <summary>Gets the width of the list in pixels.</summary>
    public WriteOnly<double> WidthInPixels => WriteOnly<double>(name: "width");

    /// <summary>Shows the loading indicator on the list.</summary>
    public void ShowSpinner() => Method();

    /// <summary>Hides the loading indicator on the list.</summary>
    public void HideSpinner() => Method();

    /// <summary>Renders the list again from its current settings.</summary>
    public void Refresh() => Method();

    /// <summary>Gets the event raised when the popup is about to open.</summary>
    public Event<PopupEventArgs> Open => Event<PopupEventArgs>();

    /// <summary>Gets the event raised before the list processes its data.</summary>
    public Event<ActionBeginEventArgs> ActionBegin => Event<ActionBeginEventArgs>();

    /// <summary>Gets the event raised after a local query returns its rows and before the list renders them.</summary>
    public Event<ActionCompleteEventArgs> ActionComplete => Event<ActionCompleteEventArgs>();

    /// <summary>Gets the event raised after the list binds its rows.</summary>
    public Event<DataBoundEventArgs> DataBound => Event<DataBoundEventArgs>();

    /// <summary>Looks up a catalogue row by its numeric value.</summary>
    /// <typeparam name="TItem">The type of a catalogue row.</typeparam>
    /// <param name="value">The value to find.</param>
    /// <returns>The matching row, or null when the loaded catalogue has no match.</returns>
    public MethodResult<TItem?> GetDataByValue<TItem>(Arg<double> value)
        where TItem : class
        => Method<TItem?>([value]);

    /// <summary>Looks up a catalogue row by its Boolean value.</summary>
    /// <typeparam name="TItem">The type of a catalogue row.</typeparam>
    /// <param name="value">The value to find.</param>
    /// <returns>The matching row, or null when the loaded catalogue has no match.</returns>
    public MethodResult<TItem?> GetDataByValue<TItem>(Arg<bool> value)
        where TItem : class
        => Method<TItem?>([value]);

    /// <summary>Supplies a function whose string result EJ2 renders as HTML for the option.</summary>
    public Callback<TemplateArgs, string> ItemTemplateCallback => Callback<TemplateArgs, string>(name: "itemTemplate");

    /// <summary>Supplies a function whose string result EJ2 renders as HTML for the selected value.</summary>
    public Callback<TemplateArgs, string> ValueTemplateCallback => Callback<TemplateArgs, string>(name: "valueTemplate");

    /// <summary>Gets whether the popup provides a search field.</summary>
    public Property<bool> AllowFiltering => Property<bool>();

    /// <summary>Gets the hint displayed in the search field.</summary>
    public Property<string?> FilterBarPlaceholder => Property<string?>();

    /// <summary>Gets the order in which catalogue entries appear.</summary>
    public Property<FusionDropDownListOrder> SortOrder => Property<FusionDropDownListOrder>();

    /// <summary>Gets the display mode of the floating label.</summary>
    public Property<FusionDropDownListLabelMode> FloatLabelType => Property<FusionDropDownListLabelMode>();

    /// <summary>Gets the culture used for built-in messages.</summary>
    public Property<string> Locale => Property<string>();

    /// <summary>Gets the heading template function, whose app-authored string result EJ2 renders as HTML.</summary>
    public Callback<PopupTemplateArgs, string> HeaderTemplateCallback => Callback<PopupTemplateArgs, string>(name: "headerTemplate");

    /// <summary>Gets the footer template function, whose app-authored string result EJ2 renders as HTML.</summary>
    public Callback<PopupTemplateArgs, string> FooterTemplateCallback => Callback<PopupTemplateArgs, string>(name: "footerTemplate");

    /// <summary>Gets the empty-list template function, whose app-authored string result EJ2 renders as HTML.</summary>
    public Callback<PopupTemplateArgs, string> NoRecordsTemplateCallback => Callback<PopupTemplateArgs, string>(name: "noRecordsTemplate");

    /// <summary>Gets the group template whose app-authored string result EJ2 renders as HTML.</summary>
    public Callback<TemplateArgs, string> GroupTemplateCallback => Callback<TemplateArgs, string>(name: "groupTemplate");

    /// <summary>Gets whether EJ2 renders a window of the local catalogue at a time.</summary>
    /// <remarks>Choose the mode before opening a fresh list and refresh its rendering after the write. To leave virtual mode, render a fresh list. Disabling it on the same instance retains the virtual module and can prevent the popup from opening.</remarks>
    public Property<bool> EnableVirtualization => Property<bool>();

    /// <summary>Gets whether staff can resize the popup using its handle.</summary>
    /// <remarks>Choose the resize policy before opening the popup. EJ2 creates the handle when it opens.</remarks>
    public Property<bool> AllowResize => Property<bool>();

    /// <summary>Gets the event that starts a popup resize.</summary>
    public Event<ResizeEventArgs> ResizeStart => Event<ResizeEventArgs>();

    /// <summary>Gets the event delivered as the user moves the resize handle.</summary>
    public Event<ResizeEventArgs> Resizing => Event<ResizeEventArgs>();

    /// <summary>Gets the event delivered when the user releases the resize handle.</summary>
    public Event<ResizeEventArgs> ResizeStop => Event<ResizeEventArgs>();

    /// <summary>Specifies the popup width as a CSS length or percentage of the field.</summary>
    public WriteOnly<string> PopupWidth => WriteOnly<string>();

    /// <summary>Specifies the popup width in pixels.</summary>
    public WriteOnly<double> PopupWidthInPixels => WriteOnly<double>(name: "popupWidth");

    /// <summary>Specifies the popup height as a CSS length.</summary>
    public WriteOnly<string> PopupHeight => WriteOnly<string>();

    /// <summary>Specifies the popup height in pixels.</summary>
    public WriteOnly<double> PopupHeightInPixels => WriteOnly<double>(name: "popupHeight");

    /// <summary>Gets the popup's stacking level among overlapping page elements.</summary>
    public Property<int> ZIndex => Property<int>();

    /// <summary>Gets the app-authored HTML or selector shown when the directory is empty.</summary>
    /// <remarks>Setting null restores EJ2’s localized empty-list message.</remarks>
    public WriteOnly<string?> NoRecordsTemplate => WriteOnly<string?>();

    /// <summary>Applies the changes written to the list's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Moves the keyboard's focus to the list.</summary>
    public void FocusIn() => Method();

    /// <summary>Moves the keyboard's focus out of the list, if the list holds it.</summary>
    public void FocusOut() => Method();

    /// <summary>Opens the list's popup.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the list's popup.</summary>
    public void HidePopup() => Method();

    /// <summary>Clears the list's value.</summary>
    public void Clear() => Method();

    /// <summary>Gets the event raised when a person chooses an item, or when the list's value changes.</summary>
    public Event<ChangeEventArgs> Change => Event<ChangeEventArgs>();

    /// <summary>Gets the event raised when the list takes the keyboard's focus.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Focus => Event();

    /// <summary>Gets the event raised when the keyboard's focus leaves the list.</summary>
    /// <remarks>Its listener receives no member a view reads.</remarks>
    public Event Blur => Event();

    /// <summary>Gets the event raised as the user enters a search in the popup.</summary>
    public Event<FilteringEventArgs> Filtering => Event<FilteringEventArgs>();

    /// <summary>Gets the event raised before a user selection is committed.</summary>
    public Event<SelectEventArgs> Select => Event<SelectEventArgs>();

    /// <summary>Gets the event raised before the list builds its popup.</summary>
    public Event<BeforeOpenEventArgs> BeforeOpen => Event<BeforeOpenEventArgs>();

    /// <summary>Gets the event raised when the popup is about to close.</summary>
    public Event<PopupEventArgs> Close => Event<PopupEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: Change);

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "ChangeEventArgs")]
    public sealed class ChangeEventArgs : JsObject
    {
        /// <summary>Gets the value the list holds after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets the selected option, or no element after clearing.</summary>
        public ListOption Item => LiveObject<ListOption>(nullable: true);

        /// <summary>Gets the preceding option, or no element on the first selection.</summary>
        public ListOption PreviousItem => LiveObject<ListOption>(nullable: true);

        /// <summary>Gets the browser event supplied as e, absent for an assignment from code.</summary>
        public BrowserAction E => LiveObject<BrowserAction>(nullable: true);

        /// <summary>Gets the browser event responsible for a user selection, absent for an assignment from code.</summary>
        public BrowserAction OriginalEvent => LiveObject<BrowserAction>(name: "event", nullable: true);

        /// <summary>Gets the selected catalogue row, or null after clearing the selection.</summary>
        /// <remarks>A primitive catalogue supplies an object with text and value of that primitive type.</remarks>
        /// <typeparam name="TItem">The row or primitive choice envelope.</typeparam>
        /// <returns>The selected row.</returns>
        public ReadOnly<TItem?> ItemData<TItem>()
            where TItem : class
            => ReadOnly<TItem?>();

        /// <summary>Gets the previously selected row, or null before the first selection.</summary>
        /// <remarks>A primitive catalogue supplies an object with text and value of that primitive type.</remarks>
        /// <typeparam name="TItem">The row or primitive choice envelope.</typeparam>
        /// <returns>The previous row.</returns>
        public ReadOnly<TItem?> PreviousItemData<TItem>()
            where TItem : class
            => ReadOnly<TItem?>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>What a listener receives before a user selection is applied.</summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "SelectEventArgs")]
    public sealed class SelectEventArgs : JsObject
    {
        /// <summary>Gets the actual list element the user is selecting.</summary>
        public ReadOnly<ListOption> Item => ReadOnly<ListOption>();

        /// <summary>Gets whether a user interaction originated this selection.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Gets the original browser event for this selection, when present.</summary>
        public BrowserAction E => LiveObject<BrowserAction>(nullable: true);

        /// <summary>Gets whether to prevent this selection. Set it during the selection handler.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);

        /// <summary>Gets the catalogue entry the user is trying to select.</summary>
        /// <remarks>A primitive catalogue supplies an object with text and value of that primitive type.</remarks>
        /// <typeparam name="TItem">The row or primitive choice envelope.</typeparam>
        /// <returns>The candidate row.</returns>
        public ReadOnly<TItem?> ItemData<TItem>()
            where TItem : class
            => ReadOnly<TItem?>();
    }

    /// <summary>What a listener receives before the list creates its popup.</summary>
    public sealed class BeforeOpenEventArgs : JsObject
    {
        /// <summary>Gets whether opening is canceled. Set it before the handler waits.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>What a listener receives when the popup changes state.</summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "PopupEventArgs")]
    public sealed class PopupEventArgs : JsObject
    {
        /// <summary>Gets the popup involved in this transition.</summary>
        public PopupSurface Popup => LiveObject<PopupSurface>();

        /// <summary>Gets whether to cancel this popup transition. Set it before the handler waits.</summary>
        public Property<bool> Cancel => Property<bool>(optional: true);
    }

    /// <summary>The live EJ2 popup supplied when the list opens or closes.</summary>
    [Mirrors("@syncfusion/ej2-popups", "Popup")]
    public sealed class PopupSurface : JsObject
    {
        /// <summary>Gets the native popup container.</summary>
        public PopupElement Element => LiveObject<PopupElement>();

        /// <summary>Gets the native root of this mounted popup.</summary>
        /// <returns>The popup's native root element.</returns>
        public PopupElement GetRootElement() => ReturnedObject<PopupElement>();
    }

    /// <summary>The mounted popup element and its actual layout dimensions.</summary>
    [Mirrors("lib.dom", "HTMLElement")]
    public sealed class PopupElement : JsObject
    {
        /// <summary>Gets the laid-out border-box width in pixels.</summary>
        public ReadOnly<int> OffsetWidth => ReadOnly<int>();

        /// <summary>Gets the laid-out border-box height in pixels.</summary>
        public ReadOnly<int> OffsetHeight => ReadOnly<int>();
    }

    /// <summary>The native mouse or touch event that drives a popup resize.</summary>
    public sealed class ResizeEventArgs : JsObject
    {
        /// <summary>Gets the original mouse or touch event type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets the viewport x coordinate on a mouse event.</summary>
        /// <remarks>A touch event carries its positions on its touches collection.</remarks>
        public ReadOnly<double?> ClientX => ReadOnly<double?>(optional: true);
    }

    /// <summary>What a listener receives while a user searches the list.</summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "FilteringEventArgs")]
    public sealed class FilteringEventArgs : JsObject
    {
        /// <summary>Gets the search text entered in the popup.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets whether the list skips its default filter.</summary>
        public Property<bool> PreventDefaultAction => Property<bool>();

        /// <summary>Gets whether filtering and replacement of its results are canceled.</summary>
        public Property<bool> Cancel => Property<bool>();

        /// <summary>Shows the supplied local matches.</summary>
        /// <typeparam name="TItem">The type of each matching entry.</typeparam>
        /// <param name="dataSource">The matching entries.</param>
        public void UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource) => Method([dataSource]);

        /// <summary>Replaces the search matches using the supplied live query.</summary>
        /// <typeparam name="TItem">The type of a local entry.</typeparam>
        /// <param name="dataSource">The local entries.</param>
        /// <param name="query">The live EJ2 Query applied to the local entries.</param>
        public void UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource, TypedSource<FusionDropDownListQuery> query) => Method([dataSource, (Arg<FusionDropDownListQuery>)query]);

        /// <summary>Replaces the search matches using the supplied live query.</summary>
        /// <typeparam name="TItem">The type of a local entry.</typeparam>
        /// <param name="dataSource">The local entries.</param>
        /// <param name="query">The live EJ2 Query applied to the local entries.</param>
        /// <param name="fields">The display and value fields used for these entries.</param>
        public void UpdateData<TItem>(Arg<IReadOnlyList<TItem>> dataSource, TypedSource<FusionDropDownListQuery> query, Arg<FusionDropDownListFields<TItem>> fields) => Method([dataSource, (Arg<FusionDropDownListQuery>)query, fields]);
    }

    /// <summary>The notification for data submitted for local processing.</summary>
    public sealed class ActionBeginEventArgs : JsObject
    {
        /// <summary>Gets the configured data source when the list uses a local row array.</summary>
        /// <typeparam name="TItem">The local row type.</typeparam>
        /// <returns>The entries for a local array catalogue.</returns>
        public ReadOnly<IReadOnlyList<TItem>> Data<TItem>() => ReadOnly<IReadOnlyList<TItem>>();

        /// <summary>Gets whether this operation is canceled before its handler waits.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>The notification for returned local catalogue rows.</summary>
    public sealed class ActionCompleteEventArgs : JsObject
    {
        /// <summary>Gets the result rows after the query on the local catalogue.</summary>
        /// <typeparam name="TItem">The local row type.</typeparam>
        /// <returns>The entries for a local array catalogue.</returns>
        public ReadOnly<IReadOnlyList<TItem>> Result<TItem>() => ReadOnly<IReadOnlyList<TItem>>();

        /// <summary>Gets whether this operation is canceled before its handler waits.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>The notification for rows bound to the list.</summary>
    public sealed class DataBoundEventArgs : JsObject
    {
        /// <summary>Gets the local data rows EJ2 supplied to the rendered list.</summary>
        /// <typeparam name="TItem">The local row type.</typeparam>
        /// <returns>The entries for a local array catalogue.</returns>
        public ReadOnly<IReadOnlyList<TItem>> Items<TItem>() => ReadOnly<IReadOnlyList<TItem>>();
    }

    /// <summary>The arguments supplied to an option or selected-value template.</summary>
    public sealed class TemplateArgs : JsObject
    {
        /// <summary>Gets the row being rendered.</summary>
        /// <typeparam name="TItem">The configured type of the row or group being rendered.</typeparam>
        /// <returns>The first argument EJ2 supplied.</returns>
        public ReadOnly<TItem> Row<TItem>() => Argument<TItem>(0);
    }

    /// <summary>The metadata supplied to a popup heading, footer or empty-list template.</summary>
    public sealed class PopupTemplateArgs : JsObject
    {
        /// <summary>Gets the name that EJ2 passes to the template function.</summary>
        public ReadOnly<string> TemplateName => Argument<string>(2);
    }

    /// <summary>The iterable collection returned by the list when its options are requested.</summary>
    public sealed class ItemCollection : JsObject
    {
        /// <summary>Gets the options in their displayed order.</summary>
        public LiveObjects<ListOption> Items => LiveIterator<ListOption>();
    }

    /// <summary>A rendered option from this list.</summary>
    [Mirrors("lib.dom", "HTMLLIElement")]
    public sealed class ListOption : JsObject
    {
        /// <summary>Gets the text shown for the option.</summary>
        public ReadOnly<string> TextContent => ReadOnly<string>();
    }

    /// <summary>The original browser event that changed an assignment.</summary>
    [Mirrors("lib.dom", "Event")]
    public sealed class BrowserAction : JsObject
    {
        /// <summary>Gets the browser event type.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();
    }

    /// <summary>The list's terminal notification.</summary>
    public sealed class DestructionEventArgs : JsObject
    {
        /// <summary>Gets the name of the terminal notification.</summary>
        public ReadOnly<string> Name => ReadOnly<string>();

        /// <summary>Gets the cancellation flag reported with destruction.</summary>
        public ReadOnly<bool> Cancel => ReadOnly<bool>();
    }
}
