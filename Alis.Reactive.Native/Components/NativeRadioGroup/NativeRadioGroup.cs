namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native radio-group component backed by a hidden input value.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeRadioGroup()</c> factory to create a model-bound radio group with
    /// label, validation, and Reactive Plan event support. A hidden input holds the
    /// selected value for form submission and component reads.
    /// </remarks>
    public sealed class NativeRadioGroup : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeRadioGroup(), "radiogroup");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
