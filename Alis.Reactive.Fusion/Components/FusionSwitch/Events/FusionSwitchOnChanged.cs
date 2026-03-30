namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when a <see cref="FusionSwitch"/> state changes.
    /// </summary>
    /// <remarks>
    /// Access properties in conditions: <c>p.When(args, x =&gt; x.Checked).Truthy()</c>.
    /// </remarks>
    public class FusionSwitchChangeArgs
    {
        /// <summary>Gets or sets the switch's checked state after the change.</summary>
        public bool Checked { get; set; }

        /// <summary>Gets or sets whether the change was triggered by user interaction.</summary>
        public bool IsInteracted { get; set; }

        /// <summary>
        /// Creates a new instance. Framework-internal: instances are created by the event descriptor.
        /// </summary>
        public FusionSwitchChangeArgs() { }
    }
}
