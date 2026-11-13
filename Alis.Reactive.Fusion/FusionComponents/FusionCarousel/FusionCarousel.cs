using System.Text.Json.Serialization;
using Alis.Reactive.Declarations;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>
/// A carousel: EJ2's Carousel, which shows one slide of its items at a time.
/// </summary>
/// <remarks>
/// EJ2 raises <see cref="SlideChanging"/> before it moves to another slide, and a handler writes true to the
/// payload's <c>Cancel</c> to keep the slide shown; it raises <see cref="SlideChanged"/> once the move is done.
/// </remarks>
[Mirrors("@syncfusion/ej2-navigations", "Carousel")]
public sealed class FusionCarousel : FusionComponent
{
    /// <summary>Gets the position of the slide the carousel shows, from 0.</summary>
    public Property<int> SelectedIndex => Property<int>();

    /// <summary>Moves the carousel to the next slide.</summary>
    public void Next() => Method();

    /// <summary>Moves the carousel to the previous slide.</summary>
    public void Prev() => Method();

    /// <summary>Gets the event raised before the carousel moves to another slide.</summary>
    public Event<SlideChangingEventArgs> SlideChanging => Event<SlideChangingEventArgs>();

    /// <summary>Gets the event raised after the carousel moved to another slide.</summary>
    public Event<SlideChangedEventArgs> SlideChanged => Event<SlideChangedEventArgs>();

    /// <summary>
    /// The way the carousel moves between slides, written by its name.
    /// </summary>
    [JsonConverter(typeof(CarouselSlideDirectionConverter))]
    public enum CarouselSlideDirection
    {
        /// <summary>Toward the previous slide.</summary>
        [JsonStringEnumMemberName("Previous")]
        Previous,

        /// <summary>Toward the next slide.</summary>
        [JsonStringEnumMemberName("Next")]
        Next,
    }

    /// <summary>
    /// What a listener of <see cref="SlideChanging"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "SlideChangingEventArgs")]
    public sealed class SlideChangingEventArgs : JsObject
    {
        /// <summary>Gets the position of the slide shown before the move, from 0.</summary>
        public ReadOnly<int> CurrentIndex => ReadOnly<int>();

        /// <summary>Gets the position of the slide the carousel moves to, from 0.</summary>
        public ReadOnly<int> NextIndex => ReadOnly<int>();

        /// <summary>Gets whether a person swiped the carousel to move it.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();

        /// <summary>Gets the way the carousel moves.</summary>
        public ReadOnly<CarouselSlideDirection> SlideDirection => ReadOnly<CarouselSlideDirection>();

        /// <summary>Gets whether the carousel keeps the slide shown; write true to keep it.</summary>
        public Property<bool> Cancel => Property<bool>();
    }

    /// <summary>
    /// What a listener of <see cref="SlideChanged"/> receives.
    /// </summary>
    [Mirrors("@syncfusion/ej2-navigations", "SlideChangedEventArgs")]
    public sealed class SlideChangedEventArgs : JsObject
    {
        /// <summary>Gets the position of the slide the carousel shows now, from 0.</summary>
        public ReadOnly<int> CurrentIndex => ReadOnly<int>();

        /// <summary>Gets the position of the slide shown before the move, from 0.</summary>
        public ReadOnly<int> PreviousIndex => ReadOnly<int>();

        /// <summary>Gets whether a person swiped the carousel to move it.</summary>
        public ReadOnly<bool> IsSwiped => ReadOnly<bool>();

        /// <summary>Gets the way the carousel moved.</summary>
        public ReadOnly<CarouselSlideDirection> SlideDirection => ReadOnly<CarouselSlideDirection>();
    }
}
