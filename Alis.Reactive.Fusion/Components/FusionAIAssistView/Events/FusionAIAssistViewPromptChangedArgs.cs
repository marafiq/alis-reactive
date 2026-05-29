namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Payload for AIAssistView promptChanged.
    /// </summary>
    public sealed class FusionAIAssistViewPromptChangedArgs
    {
        public string Value { get; set; } = "";
        public string PreviousValue { get; set; } = "";
    }
}
