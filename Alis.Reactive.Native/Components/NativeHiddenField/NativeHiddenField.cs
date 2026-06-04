namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Reactive Plan-registered native HTML <c>&lt;input type="hidden"&gt;</c> component.
    /// </summary>
    /// <remarks>
    /// The component type constrains hidden-field operations and allows gather to
    /// include the hidden input through the input component catalog.
    /// </remarks>
    public sealed class NativeHiddenField : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeHiddenField(), "hiddenfield");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
