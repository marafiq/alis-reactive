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
        /// <summary>The Syncfusion event name.</summary>
        public string? Name { get; set; }
        public FusionInPlaceEditorSubmitClickArgs() { }
    }
}
