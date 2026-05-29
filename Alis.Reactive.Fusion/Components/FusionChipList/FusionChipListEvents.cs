namespace Alis.Reactive.Fusion.Components
{
    public sealed class FusionChipListEvents
    {
        public static readonly FusionChipListEvents Instance = new FusionChipListEvents();
        private FusionChipListEvents() { }

        public TypedEvent<FusionChipListClickArgs> Clicked =>
            new TypedEvent<FusionChipListClickArgs>(
                "click", new FusionChipListClickArgs());

        public TypedEvent<FusionChipListDeletedArgs> Deleted =>
            new TypedEvent<FusionChipListDeletedArgs>(
                "deleted", new FusionChipListDeletedArgs());
    }
}
