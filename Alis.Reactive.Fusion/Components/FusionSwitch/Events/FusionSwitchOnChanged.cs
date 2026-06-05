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
        /// <summary>Switch checked state after the change.</summary>
        public bool Checked { get; set; }

        /// <summary>Whether user interaction triggered the change.</summary>
        public bool IsInteracted { get; set; }
    }
}
