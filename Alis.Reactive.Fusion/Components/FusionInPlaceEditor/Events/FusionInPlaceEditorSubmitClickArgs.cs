namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Event payload delivered when the user clicks Save or presses Enter in a <see cref="FusionInPlaceEditor"/>.
    /// </summary>
    /// <remarks>
    /// Fires on every user save intent, including blocked saves where validation has cancelled the commit.
    /// Not a commit-success signal: hook <c>ActionSuccess</c> for that. Args carry only <c>name</c>.
    /// </remarks>
    public class FusionInPlaceEditorSubmitClickArgs
    {
        /// <summary>The SF event name.</summary>
        public string? Name { get; set; }

        /// <summary>Creates an event payload instance for descriptor wiring.</summary>
        public FusionInPlaceEditorSubmitClickArgs() { }
    }
}
