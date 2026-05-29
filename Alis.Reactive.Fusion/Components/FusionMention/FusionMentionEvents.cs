namespace Alis.Reactive.Fusion.Components
{
    public sealed class FusionMentionEvents
    {
        public static readonly FusionMentionEvents Instance = new FusionMentionEvents();
        private FusionMentionEvents() { }

        public TypedEvent<FusionMentionChangedArgs> Changed =>
            new TypedEvent<FusionMentionChangedArgs>(
                "change", new FusionMentionChangedArgs());

        public TypedEvent<FusionMentionPopupArgs> Opened =>
            new TypedEvent<FusionMentionPopupArgs>(
                "opened", new FusionMentionPopupArgs());

        public TypedEvent<FusionMentionPopupArgs> Closed =>
            new TypedEvent<FusionMentionPopupArgs>(
                "closed", new FusionMentionPopupArgs());
    }
}
