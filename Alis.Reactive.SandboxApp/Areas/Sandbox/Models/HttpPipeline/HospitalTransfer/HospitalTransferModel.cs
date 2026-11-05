
namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    public class HospitalTransferModel
    {
        public string? ResidentId { get; set; }

        public string? Reason { get; set; }

        public bool Ambulance { get; set; }

        public TransferDestination Destination { get; set; } = new();

        public List<TransferEscort> Escorts { get; set; } = new();

        public string[]? Packing { get; set; }
    }

    public class TransferDestination
    {
        public string? Hospital { get; set; }
        public string? Ward { get; set; }
    }

    public class TransferEscort
    {
        public string? Name { get; set; }
    }

    public class HospitalTransferValidator : ReactiveValidator<HospitalTransferModel>
    {
        public HospitalTransferValidator()
        {
            RuleFor(x => x.ResidentId).NotEmpty().WithMessage("Choose the resident who is going.");
            RuleFor(x => x.Reason).NotEmpty().WithMessage("Say why the resident is going to the hospital.");
            RuleFor(x => x.Destination.Hospital)
                .NotEmpty().WithMessage("Name the hospital.")
                .Must(hospital => !string.Equals(hospital?.Trim(), "Mercy General", StringComparison.OrdinalIgnoreCase))
                .WithMessage("Mercy General has no bed tonight; name another hospital.");
        }
    }

    public sealed record TransferAccepted(string TransferId, string Resident, TransferReceiving Receiving, string[] Packing, string Summary);

    public sealed record TransferReceiving(string Hospital, string Ward, string Bed);

    public sealed record TransferRefusal(string Message);

    public sealed class TransferFamilyNotice
    {
        public string? TransferId { get; set; }
        public TransferAccepted? Transfer { get; set; }
    }

    public sealed record FamilyNoticeSent(string Message);
}
