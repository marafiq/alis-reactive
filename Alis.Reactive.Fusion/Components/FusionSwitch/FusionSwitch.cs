namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Fusion switch component for toggling a Boolean value.
    /// </summary>
    /// <remarks>
    /// Use as a type parameter in <c>p.Component&lt;FusionSwitch&gt;(m =&gt; m.ReceiveNotifications)</c>
    /// to access FusionSwitch-specific component operations and value reads.
    /// </remarks>
    public sealed class FusionSwitch : FusionComponent, IInputComponent
    {
        internal static InputComponentRegistrationProfile Registration { get; } =
            InputComponentRegistrationProfile.For(new FusionSwitch(), "switch");

        /// <inheritdoc />
        public string ValueMember => "checked";
    }
}
