namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when the user clicks the Cancel button in a <see cref="FusionInPlaceEditor"/>.
    /// </summary>
    /// <remarks>
    /// Fires after <c>endEdit(action="cancel")</c>. Editor has already closed and value reverted by the time this fires.
    /// </remarks>
    public class FusionInPlaceEditorCancelClickArgs
    {
        /// <summary>The SF event name.</summary>
        public string? Name { get; set; }

        /// <summary>Creates an event payload instance for descriptor wiring.</summary>
        public FusionInPlaceEditorCancelClickArgs() { }
    }
}
