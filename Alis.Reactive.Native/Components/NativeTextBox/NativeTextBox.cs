namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML <c>&lt;input type="text"&gt;</c> component for model-bound text entry.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeTextBox()</c> factory to create a model-bound text input with
    /// label, validation, and Reactive Plan event support.
    /// </remarks>
    public sealed class NativeTextBox : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeTextBox(), "textbox");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
