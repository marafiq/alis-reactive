namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML <c>&lt;select&gt;</c> component for model-bound dropdowns.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeDropDown()</c> factory to create a model-bound dropdown with
    /// label, validation, and Reactive Plan event support.
    /// </remarks>
    public sealed class NativeDropDown : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeDropDown(), "dropdown");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
