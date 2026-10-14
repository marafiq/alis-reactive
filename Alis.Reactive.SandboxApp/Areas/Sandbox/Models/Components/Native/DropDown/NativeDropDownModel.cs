using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// A booking of the community van: the resident who rides, what the trip is for, when the van leaves,
    /// and who goes with the resident.
    /// </summary>
    public class NativeDropDownModel
    {
        public string? ResidentId { get; set; }

        public string? Trip { get; set; }

        public string? Departure { get; set; }

        public string? Escort { get; set; }
    }

    /// <summary>The front desk's answer once the van is booked.</summary>
    public sealed class VanBooked
    {
        public string Message { get; set; } = "";
    }

    /// <summary>The answer when the van is already out at the time asked for: the next time it is free.</summary>
    public sealed class VanBusy
    {
        public string NextFree { get; set; } = "";
    }

    /// <summary>What a van booking needs: who rides, what for, and when.</summary>
    public sealed class VanBookingValidator : ReactiveValidator<NativeDropDownModel>
    {
        public VanBookingValidator()
        {
            RuleFor(x => x.ResidentId).NotEmpty().WithMessage("Choose the resident.");
            RuleFor(x => x.Trip).NotEmpty().WithMessage("Choose what the trip is for.");
            RuleFor(x => x.Departure).NotEmpty().WithMessage("Choose when the van leaves.");
        }
    }
}
