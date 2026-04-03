namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Exposes the reactive events available on <see cref="NativeHiddenField"/>.
    /// </summary>
    public sealed class NativeHiddenFieldEvents
    {
        /// <summary>Gets the singleton event surface instance.</summary>
        public static readonly NativeHiddenFieldEvents Instance = new NativeHiddenFieldEvents();
        private NativeHiddenFieldEvents() { }

        /// <summary>Gets the hidden-field change event.</summary>
        public ReactiveEvent<NativeHiddenFieldChangeArgs> Changed =>
            new ReactiveEvent<NativeHiddenFieldChangeArgs>(
                "change", new NativeHiddenFieldChangeArgs());
    }
}
