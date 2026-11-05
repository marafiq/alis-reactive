namespace Alis.Reactive.Framework.App.Inputs;

public static class Ward
{
    public sealed class Room
    {
        public int Bed { get; set; }

        public string? Note { get; set; }

        public RoomContact Contact { get; set; } = new();
    }

    public sealed class RoomContact
    {
        public string? Phone { get; set; }
    }
}

public sealed class RoomEcho
{
    public string Summary { get; set; } = "";
}
