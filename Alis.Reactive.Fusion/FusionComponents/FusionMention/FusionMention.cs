using Alis.Reactive.Declarations;
using Alis.Reactive.ValuesAndSources;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A mention: EJ2's Mention, which suggests items when a person types its trigger character in an editable element
/// and writes the one chosen there.
/// </summary>
/// <remarks>EJ2 raises <see cref="Change"/> when a person chooses an item of the popup and the mention writes it.</remarks>
/// <typeparam name="TValue">The type of the items' value member.</typeparam>
[Mirrors("@syncfusion/ej2-dropdowns", "Mention")]
public sealed class FusionMention<TValue> : FusionComponent
{
    /// <summary>Filters the items by a text and shows them in the popup at a position.</summary>
    /// <param name="text">The text the items are filtered by.</param>
    /// <param name="positionX">The popup's horizontal position, in pixels.</param>
    /// <param name="positionY">The popup's vertical position, in pixels.</param>
    public void Search(Arg<string> text, Arg<double> positionX, Arg<double> positionY) => Method([text, positionX, positionY]);

    /// <summary>Opens the popup of items.</summary>
    public void ShowPopup() => Method();

    /// <summary>Closes the popup of items, if it is open.</summary>
    public void HidePopup() => Method();

    /// <summary>Gets the event raised when a person chooses an item of the popup and the mention writes it.</summary>
    public Event<MentionChangeEventArgs> Change => Event<MentionChangeEventArgs>();

    /// <summary>Gets the event raised after the popup opens.</summary>
    public Event<PopupEventArgs> Opened => Event<PopupEventArgs>();

    /// <summary>Gets the event raised after the popup closes.</summary>
    public Event<PopupEventArgs> Closed => Event<PopupEventArgs>();

    /// <summary>
    /// What a listener of <see cref="Change"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "MentionChangeEventArgs")]
    public sealed class MentionChangeEventArgs : JsObject
    {
        /// <summary>Gets the value of the item chosen.</summary>
        public ReadOnly<TValue> Value => ReadOnly<TValue>();

        /// <summary>Gets whether a person made the choice.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="Opened"/> or <see cref="Closed"/> receives, of which the declaration reads no member.
    /// </summary>
    [Mirrors("@syncfusion/ej2-dropdowns", "PopupEventArgs")]
    public sealed class PopupEventArgs : JsObject;
}
