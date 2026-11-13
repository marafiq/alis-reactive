using System.Collections.Generic;
using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A list box: EJ2's ListBox, a list whose items a person selects.
/// </summary>
/// <remarks>EJ2 raises <see cref="Change"/> when a person selects or clears an item.</remarks>
/// <typeparam name="TValue">The list the values of the selected items make.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "ListBox")]
public sealed class FusionListBox<TValue> : FusionComponent
{
    /// <summary>Gets the values of the selected items.</summary>
    public Property<TValue> Value => Property<TValue>();

    /// <summary>Applies the changes written to the box's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Selects or clears the items named.</summary>
    /// <param name="items">The items' texts, or their values when <paramref name="isValue"/> is true.</param>
    /// <param name="state">True to select the items, false to clear them.</param>
    /// <param name="isValue">True when <paramref name="items"/> holds the items' values.</param>
    public void SelectItems(Arg<IReadOnlyList<string>> items, Arg<bool> state, Arg<bool> isValue) => Method([items, state, isValue]);

    /// <summary>Selects or clears every item.</summary>
    /// <param name="state">True to select every item, false to clear them.</param>
    public void SelectAll(Arg<bool> state) => Method([state]);

    /// <summary>Enables or disables the items named.</summary>
    /// <param name="items">The items' texts, or their values when <paramref name="isValue"/> is true.</param>
    /// <param name="enable">True to enable the items, false to disable them.</param>
    /// <param name="isValue">True when <paramref name="items"/> holds the items' values.</param>
    public void EnableItems(Arg<IReadOnlyList<string>> items, Arg<bool> enable, Arg<bool> isValue) => Method([items, enable, isValue]);

    /// <summary>Gets the event raised when a person selects or clears an item.</summary>
    public Event<ListBoxChangeEventArgs> Change => Event<ListBoxChangeEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "ListBoxChangeEventArgs")]
    public sealed class ListBoxChangeEventArgs : JsObject
    {
        /// <summary>Gets the values of the items selected after the change.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();
    }
}
