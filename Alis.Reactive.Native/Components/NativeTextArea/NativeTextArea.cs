namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML <c>&lt;textarea&gt;</c> component for model-bound multi-line text.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeTextArea()</c> factory to create a model-bound textarea with
    /// label, validation, and Reactive Plan event support.
    /// </remarks>
    public sealed class NativeTextArea : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeTextArea(), "textarea");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
