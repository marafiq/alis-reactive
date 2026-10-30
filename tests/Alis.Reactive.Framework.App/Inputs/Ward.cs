namespace Alis.Reactive.Framework.App.Inputs;

/// <summary>A ward, whose rooms and their contacts are models declared inside it: nested types, whose full names have a <c>+</c>.</summary>
public static class Ward
{
    /// <summary>A room on the ward: a member of a value type, one of text, and one inside another class.</summary>
    public sealed class Room
    {
        public int Bed { get; set; }

        public string? Note { get; set; }

        public RoomContact Contact { get; set; } = new();
    }

    /// <summary>Who answers for a room: the class that declares the phone a room's contact is reached on.</summary>
    public sealed class RoomContact
    {
        public string? Phone { get; set; }
    }
}

/// <summary>What the endpoint bound, in its own words.</summary>
public sealed class RoomEcho
{
    public string Summary { get; set; } = "";
}
