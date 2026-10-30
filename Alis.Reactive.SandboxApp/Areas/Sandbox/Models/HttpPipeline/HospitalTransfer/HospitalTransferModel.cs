using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models
{
    /// <summary>
    /// Hospital transfer: the nurse sends a resident to the hospital with the reason, how the resident
    /// travels, where they go and who escorts them; the hospital answers where the resident is received
    /// and what to pack, and the family is then told from that answer.
    /// </summary>
    public class HospitalTransferModel
    {
        /// <summary>The record number of the resident who goes; null until the nurse chooses one.</summary>
        public string? ResidentId { get; set; }

        /// <summary>Why the resident goes.</summary>
        public string? Reason { get; set; }

        /// <summary>Whether the resident travels by ambulance; otherwise by wheelchair van.</summary>
        public bool Ambulance { get; set; }

        /// <summary>Where the resident goes.</summary>
        public TransferDestination Destination { get; set; } = new();

        /// <summary>Who escorts the resident, one staff member per line.</summary>
        public List<TransferEscort> Escorts { get; set; } = new();

        /// <summary>What goes with the resident; the hospital ticks what it needs.</summary>
        public string[]? Packing { get; set; }
    }

    /// <summary>The hospital and the ward the nurse asks for.</summary>
    public class TransferDestination
    {
        public string? Hospital { get; set; }
        public string? Ward { get; set; }
    }

    /// <summary>One escort line.</summary>
    public class TransferEscort
    {
        public string? Name { get; set; }
    }

    /// <summary>
    /// What the page checks before a transfer is sent: who goes, why, and to which hospital. Which hospital
    /// has a bed tonight only the transfer desk knows: a Must the browser cannot run.
    /// </summary>
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

    /// <summary>
    /// The hospital's answer: the transfer's number, the resident, where they are received, what to pack,
    /// and the transfer as the desk understood it.
    /// </summary>
    public sealed record TransferAccepted(string TransferId, string Resident, TransferReceiving Receiving, string[] Packing, string Summary);

    /// <summary>Where the hospital receives the resident.</summary>
    public sealed record TransferReceiving(string Hospital, string Ward, string Bed);

    /// <summary>Why the transfer desk refused the transfer.</summary>
    public sealed record TransferRefusal(string Message);

    /// <summary>The body <c>POST NotifyFamily</c> binds: the transfer's number and the hospital's whole answer.</summary>
    public sealed class TransferFamilyNotice
    {
        public string? TransferId { get; set; }
        public TransferAccepted? Transfer { get; set; }
    }

    /// <summary>The family desk's answer once the family is told.</summary>
    public sealed record FamilyNoticeSent(string Message);
}
