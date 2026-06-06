namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed AIAssistView events available to Reactive Plans.
    /// </summary>
    public sealed class FusionAIAssistViewEvents
    {
        public static readonly FusionAIAssistViewEvents Instance = new FusionAIAssistViewEvents();
        private FusionAIAssistViewEvents() { }

        /// <summary>Fires before a submitted prompt starts its request.</summary>
        public TypedEvent<FusionAIAssistViewPromptRequestArgs> PromptRequest =>
            new TypedEvent<FusionAIAssistViewPromptRequestArgs>(
                "promptRequest", new FusionAIAssistViewPromptRequestArgs());

        /// <summary>Fires when the prompt text changes.</summary>
        public TypedEvent<FusionAIAssistViewPromptChangedArgs> PromptChanged =>
            new TypedEvent<FusionAIAssistViewPromptChangedArgs>(
                "promptChanged", new FusionAIAssistViewPromptChangedArgs());

        /// <summary>Fires when the user stops an active response.</summary>
        public TypedEvent<FusionAIAssistViewStopRespondingClickArgs> StopRespondingClick =>
            new TypedEvent<FusionAIAssistViewStopRespondingClickArgs>(
                "stopRespondingClick", new FusionAIAssistViewStopRespondingClickArgs());
    }
}
