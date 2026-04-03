namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;input type="hidden"&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Participates in RegisteredComponents for gather (IncludeAll picks it up).
    /// </summary>
    public sealed class NativeHiddenField : NativeComponent, IInputComponent
    {
        /// <inheritdoc />
        public string ValueMemberPath => "value";
    }
}
