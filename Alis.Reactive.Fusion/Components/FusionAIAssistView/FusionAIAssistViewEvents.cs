namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// Typed AIAssistView events available to reactive plans.
    /// </summary>
    public sealed class FusionAIAssistViewEvents
    {
        public static readonly FusionAIAssistViewEvents Instance = new FusionAIAssistViewEvents();
        private FusionAIAssistViewEvents() { }

        public TypedEvent<FusionAIAssistViewPromptRequestArgs> PromptRequest =>
            new TypedEvent<FusionAIAssistViewPromptRequestArgs>(
                "promptRequest", new FusionAIAssistViewPromptRequestArgs());

        public TypedEvent<FusionAIAssistViewPromptChangedArgs> PromptChanged =>
            new TypedEvent<FusionAIAssistViewPromptChangedArgs>(
                "promptChanged", new FusionAIAssistViewPromptChangedArgs());

        public TypedEvent<FusionAIAssistViewStopRespondingClickArgs> StopRespondingClick =>
            new TypedEvent<FusionAIAssistViewStopRespondingClickArgs>(
                "stopRespondingClick", new FusionAIAssistViewStopRespondingClickArgs());
    }
}
