using Alis.Reactive.Fusion;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.ChipList
{
    public sealed class ChipListModel
    {
        public string? Destination { get; set; }

        public IReadOnlyList<OutingDestination> Destinations { get; init; } = [];

        public IReadOnlyList<OutingInterest> Interests { get; init; } = [];

        public IReadOnlyList<string> AskedToGo { get; init; } = [];

        public IReadOnlyList<string> Riders { get; init; } = [];

        public IReadOnlyList<string> WeekDays { get; init; } = [];
    }

    public sealed class OutingDestination
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class OutingInterest
    {
        public string Id { get; init; } = "";

        public string Name { get; init; } = "";
    }

    public sealed class DestinationQuery
    {
        public string? Destination { get; set; }
    }

    public sealed class DestinationOuting
    {
        public string[] Tags { get; init; } = [];

        public int Day { get; init; }
    }

    public sealed class InviteeQuery
    {
        public string[]? TagNames { get; set; }

        public string[]? Interests { get; set; }
    }

    public sealed class InviteeList
    {
        public string Message { get; init; } = "";
    }

    public sealed class LiftRiderList
    {
        public string[] Names { get; init; } = [];
    }

    public sealed class OutingPost
    {
        public string? Destination { get; set; }

        public string[]? Tags { get; set; }

        public int[]? RepeatDays { get; set; }
    }

    public sealed class OutingPosted
    {
        public string Message { get; init; } = "";
    }

    public sealed class OutingTags
    {
        public FusionSelectedChips Selection { get; set; } = new();
    }

    public sealed class OpenSeat
    {
        public string Rider { get; set; } = "";

        public FusionChipData NextInLine { get; set; } = new();
    }

    public static class OutingEvents
    {
        public static readonly AppEvent<OutingTags> Tagged = AppEvent.Declare<OutingTags>();

        public static readonly AppEvent<OpenSeat> SeatOpened = AppEvent.Declare<OpenSeat>();
    }
}
