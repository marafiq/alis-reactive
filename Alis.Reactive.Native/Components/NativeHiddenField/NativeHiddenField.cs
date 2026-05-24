namespace Alis.Reactive.Native.Components
{
    /// <summary>
    /// Native HTML &lt;input type="hidden"&gt; element.
    /// Phantom type — constrains which vertical slice extensions are available.
    /// Participates in input component onboarding catalog for gather (IncludeAll picks it up).
    /// </summary>
    public sealed class NativeHiddenField : NativeComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new NativeHiddenField(), "hiddenfield");

        /// <inheritdoc />
        public string ValueMember => "value";
    }
}
