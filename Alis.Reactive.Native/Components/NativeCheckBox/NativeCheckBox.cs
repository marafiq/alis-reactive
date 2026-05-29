namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// A native HTML checkbox (<c>&lt;input type="checkbox"&gt;</c>).
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeCheckBox()</c> factory to create a model-bound checkbox with
    /// label, validation, and reactive event support.
    /// </remarks>
    public sealed class NativeCheckBox : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeCheckBox(), "checkbox");

        /// <inheritdoc />
        public string ValueMember => "checked";
    }
}
