namespace Alis.Reactive.Fusion.Components
{
    public sealed class FusionSmartTextAreaEvents
    {
        public static readonly FusionSmartTextAreaEvents Instance = new FusionSmartTextAreaEvents();
        private FusionSmartTextAreaEvents() { }

        public TypedEvent<FusionSmartTextAreaBeforeSuggestionInsertArgs> BeforeSuggestionInsert =>
            new TypedEvent<FusionSmartTextAreaBeforeSuggestionInsertArgs>(
                "beforeSuggestionInsert", new FusionSmartTextAreaBeforeSuggestionInsertArgs());

        public TypedEvent<FusionSmartTextAreaAfterSuggestionInsertArgs> AfterSuggestionInsert =>
            new TypedEvent<FusionSmartTextAreaAfterSuggestionInsertArgs>(
                "afterSuggestionInsert", new FusionSmartTextAreaAfterSuggestionInsertArgs());
    }
}
