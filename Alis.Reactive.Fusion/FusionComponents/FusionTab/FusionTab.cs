using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A tab strip: EJ2's Tab, which shows one of its items' content at a time.
/// </summary>
/// <remarks>EJ2 raises <see cref="Selected"/> after an item is selected, by a person or by <see cref="Select"/>.</remarks>
[Mirrors("@syncfusion/ej2-navigations", "Tab")]
public sealed class FusionTab : FusionComponent
{
    /// <summary>Gets the index of the selected item, from 0.</summary>
    public Property<int> SelectedItem => Property<int>();

    /// <summary>Selects the item at an index.</summary>
    /// <param name="index">The index of the item, from 0.</param>
    public void Select(Arg<int> index) => Method([index]);

    /// <summary>Hides or shows the item at an index.</summary>
    /// <param name="index">The index of the item, from 0.</param>
    /// <param name="value">True to hide the item, false to show it.</param>
    public void HideTab(Arg<int> index, Arg<bool> value) => Method([index, value]);

    /// <summary>Gets the event raised after an item is selected.</summary>
    public Event<SelectEventArgs> Selected => Event<SelectEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Selected"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "SelectEventArgs")]
    public sealed class SelectEventArgs : JsObject
    {
        /// <summary>Gets the index of the item selected, from 0.</summary>
        public ReadOnly<int> SelectedIndex => ReadOnly<int>();

        /// <summary>Gets the index of the item selected before, from 0.</summary>
        public ReadOnly<int> PreviousIndex => ReadOnly<int>();

        /// <summary>Gets whether a person selected the item by a swipe.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();
    }
}
