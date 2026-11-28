using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// An accordion: EJ2's Accordion, a stack of items whose panels a person expands and collapses.
/// </summary>
/// <remarks>EJ2 raises <see cref="Expanded"/> after an item's panel expands or collapses.</remarks>
[Mirrors("@syncfusion/ej2-navigations", "Accordion")]
public sealed class FusionAccordion : FusionComponent
{
    /// <summary>Expands or collapses the item at a position.</summary>
    /// <param name="isExpand">True to expand the item, false to collapse it.</param>
    /// <param name="index">The item's position, from 0.</param>
    public void ExpandItem(Arg<bool> isExpand, Arg<int> index) => Method([isExpand, index]);

    /// <summary>Enables or disables the item at a position.</summary>
    /// <param name="index">The item's position, from 0.</param>
    /// <param name="isEnable">True to enable the item, false to disable it.</param>
    public void EnableItem(Arg<int> index, Arg<bool> isEnable) => Method([index, isEnable]);

    /// <summary>Gets the event raised after an item's panel expands or collapses.</summary>
    public Event<ExpandedEventArgs> Expanded => Event<ExpandedEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Expanded"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "ExpandedEventArgs")]
    public sealed class ExpandedEventArgs : JsObject
    {
        /// <summary>Gets the position of the item that expanded or collapsed, from 0.</summary>
        public ReadOnly<int> Index => ReadOnly<int>(optional: true);

        /// <summary>Gets whether the item's panel is expanded.</summary>
        public ReadOnly<bool> IsExpanded => ReadOnly<bool>(optional: true);
    }
}
