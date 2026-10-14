using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.HiddenField
{
    /// <summary>
    /// One room check on a night round: the room the nurse checked and how she found the resident, sent
    /// with what the page keeps out of sight: the wing, when the round started, and the resident in
    /// that room.
    /// </summary>
    public sealed class NightRoomCheck
    {
        /// <summary>The wing the round covers, written by the server when the page opens.</summary>
        public string Wing { get; set; } = "";

        /// <summary>When the round started, as the server answered the start of the round.</summary>
        public string? RoundStartedAt { get; set; }

        /// <summary>The resident in the chosen room, as the server answered the choice of room.</summary>
        public string? ResidentId { get; set; }

        public string? Room { get; set; }

        public string? Finding { get; set; }
    }

    /// <summary>The rooms on the round and what the nurse can find in each.</summary>
    public static class NightRound
    {
        public static readonly string[] Rooms = { "Maple 110", "Maple 112", "Maple 114" };

        public static readonly string[] Findings = { "Asleep", "Awake and settled", "Up and needs help" };
    }

    /// <summary>The start of a night round, for one wing.</summary>
    public sealed class NightRoundStart
    {
        public string Wing { get; set; } = "";
    }

    /// <summary>The server's answer to the start of a round.</summary>
    public sealed class NightRoundStarted
    {
        public string StartedAt { get; set; } = "";

        public string Announcement { get; set; } = "";
    }

    /// <summary>A room on the wing.</summary>
    public sealed class RoomOccupantQuery
    {
        public string Room { get; set; } = "";
    }

    /// <summary>The resident who sleeps in a room.</summary>
    public sealed class RoomOccupant
    {
        public string ResidentId { get; set; } = "";

        public string ResidentName { get; set; } = "";
    }

    /// <summary>A resident whose night care the nurse looks up.</summary>
    public sealed class NightCareQuery
    {
        public string ResidentId { get; set; } = "";
    }

    /// <summary>What the care plan asks of the night shift for one resident.</summary>
    public sealed class NightCareNote
    {
        public string Note { get; set; } = "";
    }

    /// <summary>What the server says once a room check is recorded.</summary>
    public sealed class RoomCheckRecorded
    {
        public string Summary { get; set; } = "";
    }

    public sealed class NightRoomCheckValidator : ReactiveValidator<NightRoomCheck>
    {
        public NightRoomCheckValidator()
        {
            RuleFor(x => x.Room).NotEmpty().WithMessage("Choose the room you checked.");
            RuleFor(x => x.ResidentId).NotEmpty().WithMessage("The check names no resident yet: the room you choose names who sleeps there.");
            RuleFor(x => x.Finding).NotEmpty().WithMessage("Say how you found the resident.");
        }
    }
}
