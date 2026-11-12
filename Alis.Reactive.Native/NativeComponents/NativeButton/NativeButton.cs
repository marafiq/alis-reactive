using Alis.Reactive.Declarations;

namespace Alis.Reactive.Native.NativeComponents;

/// <summary>
/// A button: an HTML <c>button</c> under the id the view gives it.
/// </summary>
/// <remarks>
/// A button is no input: no field renders it and it binds no member. A handler of <see cref="Click"/> reads the
/// pointer's event, the listener's own argument.
/// </remarks>
[Mirrors("lib.dom", "HTMLButtonElement")]
public sealed class NativeButton : NativeComponent
{
    /// <summary>Gets the button's text.</summary>
    public Property<string> TextContent => Property<string>();

    /// <summary>Gets the event the button raises when it is clicked.</summary>
    public Event<Clicked> Click => Event<Clicked>();

    /// <summary>Moves the keyboard's focus to the button.</summary>
    public void Focus() => Method();

    /// <summary>
    /// What a listener of <see cref="Click"/> receives: the browser's pointer event.
    /// </summary>
    [Mirrors("lib.dom", "PointerEvent")]
    public sealed class Clicked : JsObject
    {
        /// <summary>Gets the event's name, <c>click</c>.</summary>
        public ReadOnly<string> Type => ReadOnly<string>();

        /// <summary>Gets whether the browser raised the event; one a script raised reads false.</summary>
        public ReadOnly<bool> IsTrusted => ReadOnly<bool>();

        /// <summary>Gets the time the event was raised, in milliseconds since the page's time origin.</summary>
        public ReadOnly<double> TimeStamp => ReadOnly<double>();

        /// <summary>Gets the click count, when it is not zero.</summary>
        public ReadOnly<int> Detail => ReadOnly<int>();

        /// <summary>Gets whether the Alt key was pressed when the event occurred.</summary>
        public ReadOnly<bool> AltKey => ReadOnly<bool>();

        /// <summary>Gets whether the Ctrl key was pressed when the event occurred.</summary>
        public ReadOnly<bool> CtrlKey => ReadOnly<bool>();

        /// <summary>Gets whether the Shift key was pressed when the event occurred.</summary>
        public ReadOnly<bool> ShiftKey => ReadOnly<bool>();

        /// <summary>Gets whether the Meta key was pressed when the event occurred.</summary>
        public ReadOnly<bool> MetaKey => ReadOnly<bool>();

        /// <summary>Gets which button was pressed or released to raise the event.</summary>
        public ReadOnly<int> Button => ReadOnly<int>();

        /// <summary>Gets which buttons were pressed when the event was raised.</summary>
        public ReadOnly<int> Buttons => ReadOnly<int>();

        /// <summary>Gets the horizontal coordinate of the event within the viewport.</summary>
        public ReadOnly<double> ClientX => ReadOnly<double>();

        /// <summary>Gets the vertical coordinate of the event within the viewport.</summary>
        public ReadOnly<double> ClientY => ReadOnly<double>();

        /// <summary>Gets the horizontal coordinate of the pointer in screen coordinates.</summary>
        public ReadOnly<double> ScreenX => ReadOnly<double>();

        /// <summary>Gets the vertical coordinate of the pointer in screen coordinates.</summary>
        public ReadOnly<double> ScreenY => ReadOnly<double>();

        /// <summary>Gets the horizontal coordinate of the event from the left edge of the whole document.</summary>
        public ReadOnly<double> PageX => ReadOnly<double>();

        /// <summary>Gets the vertical coordinate of the event from the top edge of the whole document.</summary>
        public ReadOnly<double> PageY => ReadOnly<double>();

        /// <summary>Gets the horizontal offset of the pointer from the padding edge of the event's target.</summary>
        public ReadOnly<double> OffsetX => ReadOnly<double>();

        /// <summary>Gets the vertical offset of the pointer from the padding edge of the event's target.</summary>
        public ReadOnly<double> OffsetY => ReadOnly<double>();

        /// <summary>Gets the horizontal coordinate of the event from the current layer.</summary>
        public ReadOnly<int> LayerX => ReadOnly<int>();

        /// <summary>Gets the vertical coordinate of the event from the current layer.</summary>
        public ReadOnly<int> LayerY => ReadOnly<int>();

        /// <summary>Gets the pointer's horizontal movement since the move event before it.</summary>
        public ReadOnly<double> MovementX => ReadOnly<double>();

        /// <summary>Gets the pointer's vertical movement since the move event before it.</summary>
        public ReadOnly<double> MovementY => ReadOnly<double>();

        /// <summary>Gets the horizontal coordinate of the event within the viewport, as <see cref="ClientX"/> does.</summary>
        public ReadOnly<double> X => ReadOnly<double>();

        /// <summary>Gets the vertical coordinate of the event within the viewport, as <see cref="ClientY"/> does.</summary>
        public ReadOnly<double> Y => ReadOnly<double>();

        /// <summary>Gets the identifier of the pointer that raised the event.</summary>
        public ReadOnly<int> PointerId => ReadOnly<int>();

        /// <summary>Gets the type of device that raised the event: mouse, pen or touch.</summary>
        public ReadOnly<string> PointerType => ReadOnly<string>();

        /// <summary>Gets whether the pointer that raised the event is the primary pointer.</summary>
        public ReadOnly<bool> IsPrimary => ReadOnly<bool>();

        /// <summary>Gets the width of the pointer's contact, in CSS pixels.</summary>
        public ReadOnly<double> Width => ReadOnly<double>();

        /// <summary>Gets the height of the pointer's contact, in CSS pixels.</summary>
        public ReadOnly<double> Height => ReadOnly<double>();

        /// <summary>Gets the normalized pressure of the pointer.</summary>
        public ReadOnly<double> Pressure => ReadOnly<double>();

        /// <summary>Gets the normalized tangential pressure of the pointer.</summary>
        public ReadOnly<double> TangentialPressure => ReadOnly<double>();

        /// <summary>Gets the angle, in degrees, between the pointer's Y-Z plane and the screen.</summary>
        public ReadOnly<int> TiltX => ReadOnly<int>();

        /// <summary>Gets the angle, in degrees, between the pointer's X-Z plane and the screen.</summary>
        public ReadOnly<int> TiltY => ReadOnly<int>();

        /// <summary>Gets the clockwise rotation of the pointer around its major axis, in degrees.</summary>
        public ReadOnly<int> Twist => ReadOnly<int>();

        /// <summary>Gets the angle between the pointer's axis and the plane of the screen.</summary>
        public ReadOnly<double> AltitudeAngle => ReadOnly<double>();

        /// <summary>Gets the angle between the Y-Z plane and the plane that holds the pointer's axis and the Y axis.</summary>
        public ReadOnly<double> AzimuthAngle => ReadOnly<double>();

        /// <summary>Gets the identifier of the pointing device that raised the event.</summary>
        public ReadOnly<int> PersistentDeviceId => ReadOnly<int>();
    }
}
