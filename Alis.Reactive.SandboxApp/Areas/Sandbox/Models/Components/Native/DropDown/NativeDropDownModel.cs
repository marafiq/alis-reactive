
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class NativeDropDownModel
    {
        public string? ResidentId { get; set; }

        public string? Trip { get; set; }

        public string? Departure { get; set; }

        public string? Escort { get; set; }
    }

    public sealed class VanBooked
    {
        public string Message { get; set; } = "";
    }

    public sealed class VanBusy
    {
        public string NextFree { get; set; } = "";
    }

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
