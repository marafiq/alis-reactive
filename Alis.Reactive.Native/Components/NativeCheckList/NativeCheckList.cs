namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native checkbox-list component with <c>string[]</c> value semantics.
    /// </summary>
    /// <remarks>
    /// Use with <see cref="InputBoundField{TModel,TProp}"/> via the
    /// <c>.NativeCheckList()</c> factory to create a model-bound checkbox list with
    /// label, validation, and Reactive Plan event support. The container element holds
    /// the selected values as a <c>string[]</c>.
    /// </remarks>
    public sealed class NativeCheckList : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeCheckList(), "checklist");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
