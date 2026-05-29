namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for AIAssistView stopRespondingClick.
    /// </summary>
    public sealed class FusionAIAssistViewStopRespondingClickArgs
    {
        public string Prompt { get; set; } = "";
        public int DataIndex { get; set; }
    }
}
