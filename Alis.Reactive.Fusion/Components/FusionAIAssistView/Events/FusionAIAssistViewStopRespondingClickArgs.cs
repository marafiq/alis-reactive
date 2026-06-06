namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries the prompt and response index when the user stops a response.
    /// </summary>
    public sealed class FusionAIAssistViewStopRespondingClickArgs
    {
        /// <summary>Prompt text associated with the stopped response.</summary>
        public string Prompt { get; set; } = "";

        /// <summary>Index of the response item being stopped.</summary>
        public int DataIndex { get; set; }
    }
}
