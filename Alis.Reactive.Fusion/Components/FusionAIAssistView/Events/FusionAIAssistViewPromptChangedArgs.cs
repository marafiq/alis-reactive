namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Carries the current and previous prompt text when the prompt changes.
    /// </summary>
    public sealed class FusionAIAssistViewPromptChangedArgs
    {
        /// <summary>Prompt text after the change.</summary>
        public string Value { get; set; } = "";

        /// <summary>Prompt text before the change.</summary>
        public string PreviousValue { get; set; } = "";
    }
}
