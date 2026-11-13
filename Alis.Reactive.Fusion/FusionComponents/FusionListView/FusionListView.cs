using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A list view: EJ2's ListView, a list of items a person selects or checks.
/// </summary>
/// <remarks>
/// <see cref="SelectItem{TItem}"/> and <see cref="UnselectItem{TItem}"/> take an item as the list holds it: its text
/// in a list of text, its data in a list of objects.
/// </remarks>
[Mirrors("@syncfusion/ej2-lists", "ListView")]
public sealed class FusionListView : FusionComponent
{
    /// <summary>Selects an item of the list.</summary>
    /// <typeparam name="TItem">The type of the list's items.</typeparam>
    /// <param name="item">The item, as the list holds it.</param>
    public void SelectItem<TItem>(Arg<TItem> item) => Method([item]);

    /// <summary>Clears the selection of an item of the list.</summary>
    /// <typeparam name="TItem">The type of the list's items.</typeparam>
    /// <param name="item">The item, as the list holds it.</param>
    public void UnselectItem<TItem>(Arg<TItem> item) => Method([item]);

    /// <summary>Checks every item of a list that shows check boxes.</summary>
    public void CheckAllItems() => Method();

    /// <summary>Clears the check of every item of a list that shows check boxes.</summary>
    public void UncheckAllItems() => Method();

    /// <summary>Gets the event raised when an item is selected.</summary>
    public Event<SelectEventArgs> Select => Event<SelectEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Select"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-lists", "SelectEventArgs")]
    public sealed class SelectEventArgs : JsObject
    {
        /// <summary>Gets the text of the item selected.</summary>
        public ReadOnly<string> Text => ReadOnly<string>();

        /// <summary>Gets the index of the item selected in the list.</summary>
        public ReadOnly<int> Index => ReadOnly<int>();

        /// <summary>Gets whether a person selected the item; a call reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();

        /// <summary>Gets whether the item is checked, in a list that shows check boxes.</summary>
        public ReadOnly<bool> IsChecked => ReadOnly<bool>(optional: true);

        /// <summary>Gets whether the selection stays as it was; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>();
    }
}
