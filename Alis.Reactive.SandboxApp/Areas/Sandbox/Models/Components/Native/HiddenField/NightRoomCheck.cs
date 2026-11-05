
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.HiddenField
{
    public sealed class NightRoomCheck
    {
        public string Wing { get; set; } = "";

        public string? RoundStartedAt { get; set; }

        public string? ResidentId { get; set; }

        public string? Room { get; set; }

        public string? Finding { get; set; }
    }

    public static class NightRound
    {
        public static readonly string[] Rooms = { "Maple 110", "Maple 112", "Maple 114" };

        public static readonly string[] Findings = { "Asleep", "Awake and settled", "Up and needs help" };
    }

    public sealed class NightRoundStart
    {
        public string Wing { get; set; } = "";
    }

    public sealed class NightRoundStarted
    {
        public string StartedAt { get; set; } = "";

        public string Announcement { get; set; } = "";
    }

    public sealed class RoomOccupantQuery
    {
        public string Room { get; set; } = "";
    }

    public sealed class RoomOccupant
    {
        public string ResidentId { get; set; } = "";

        public string ResidentName { get; set; } = "";
    }

    public sealed class NightCareQuery
    {
        public string ResidentId { get; set; } = "";
    }

    public sealed class NightCareNote
    {
        public string Note { get; set; } = "";
    }

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
