using Alis.Reactive.Declarations;
using Alis.Reactive.InputsAndIds;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A rating: EJ2's Rating, a row of symbols a person clicks to give a score.
/// </summary>
/// <remarks>
/// A rating bound to a member is an input: a form sends its <see cref="Value"/> and a check reads it. A handler of
/// <see cref="ValueChanged"/> reads the committed score.
/// </remarks>
[Mirrors("@syncfusion/ej2-inputs", "Rating")]
public sealed class FusionRating : FusionComponent, IInputComponent
{
    /// <summary>Gets the score the rating holds, 0 until a person gives one.</summary>
    public Property<double> Value => Property<double>();

    /// <summary>Applies the changes written to the rating's properties now.</summary>
    public void DataBind() => Method();

    /// <summary>Sets the score back to the rating's minimum.</summary>
    public void Reset() => Method();

    /// <summary>Gets the event raised when the rating's score changes.</summary>
    public Event<RatingChangedEventArgs> ValueChanged => Event<RatingChangedEventArgs>();

    ComponentInput IInputComponent.Input => ComponentInput.Of(Value, committedBy: ValueChanged);

    /// <summary>
    /// What a listener of <see cref="ValueChanged"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-inputs", "RatingChangedEventArgs")]
    public sealed class RatingChangedEventArgs : JsObject
    {
        /// <summary>Gets the score the rating holds after the change.</summary>
        public ReadOnly<double> Value => ReadOnly<double>();

        /// <summary>Gets the score the rating held before the change.</summary>
        public ReadOnly<double> PreviousValue => ReadOnly<double>();

        /// <summary>Gets whether a person made the change; a write reads false.</summary>
        public ReadOnly<bool> IsInteracted => ReadOnly<bool>();
    }
}
