namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoveInWorld
{
    public WorldClock Clock { get; set; } = new();

    public List<Community> Communities { get; set; } = [];

    public List<CareLevelDefinition> CareLevels { get; set; } = [];

    public List<ResidentState> Residents { get; set; } = [];

    public List<DraftState> Drafts { get; set; } = [];

    public List<PolicyState> Policies { get; set; } = [];

    public List<DrugDefinition> Drugs { get; set; } = [];

    public List<ServiceDefinition> Services { get; set; } = [];

    public List<CoordinatorState> Coordinators { get; set; } = [];

    public List<DocumentState> Documents { get; set; } = [];

    public List<PaymentState> Payments { get; set; } = [];

    public List<AdmissionState> Admissions { get; set; } = [];

    public List<WaitlistState> Waitlist { get; set; } = [];

    public List<ReviewState> Reviews { get; set; } = [];

    public List<CoordinatorMessage> Messages { get; set; } = [];

    public List<AcknowledgementState> Acknowledgements { get; set; } = [];

    public List<HistoryEntry> History { get; set; } = [];

    public ServerRules Rules { get; set; } = new();
}

public sealed class WorldClock
{
    public DateTime Today { get; set; }

    public DateTimeOffset Now { get; set; }

    public string Culture { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = string.Empty;
}

public sealed class Community
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Site { get; set; } = string.Empty;

    public string TimeZoneId { get; set; } = string.Empty;

    public List<Building> Buildings { get; set; } = [];
}

public sealed class Building
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<Apartment> Apartments { get; set; } = [];
}

public sealed class Apartment
{
    public string Id { get; set; } = string.Empty;

    public string Number { get; set; } = string.Empty;

    public List<CareLevel> CareLevels { get; set; } = [];

    public List<Bed> Beds { get; set; } = [];
}

public sealed class Bed
{
    public string Id { get; set; } = string.Empty;

    public string Label { get; set; } = string.Empty;

    public int Revision { get; set; }

    public BedState State { get; set; }

    public BedHolder? Holder { get; set; }

    public BedHold? Hold { get; set; }

    public bool NeedsWater { get; set; }
}

public sealed class BedHolder
{
    public string ResidentId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string? AdmissionId { get; set; }
}

public sealed class BedHold
{
    public string CoordinatorId { get; set; } = string.Empty;

    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class CareLevelDefinition
{
    public CareLevel Level { get; set; }

    public string Name { get; set; } = string.Empty;

    public decimal MonthlyBasePrice { get; set; }
}

public sealed class ResidentState
{
    public string Id { get; set; } = string.Empty;

    public ResidentIdentity Identity { get; set; } = new();

    public Address Address { get; set; } = new();

    public List<string> Allergies { get; set; } = [];

    public string? ActiveAdmissionId { get; set; }

    public bool Restricted { get; set; }
}

public sealed class DraftState
{
    public string Id { get; set; } = string.Empty;

    public int Version { get; set; }

    public DraftStatus Status { get; set; }

    public MoveInPage Data { get; set; } = new();

    public string? PayerVerificationId { get; set; }

    public string? PharmacyClearanceId { get; set; }

    public QuoteState? Quote { get; set; }

    public int? AcceptedQuoteRevision { get; set; }

    public string? DepositReceiptId { get; set; }

    public string SavedBy { get; set; } = string.Empty;

    public DateTimeOffset SavedAt { get; set; }
}

public sealed class DrugDefinition
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Unit { get; set; } = string.Empty;

    public decimal MaximumDose { get; set; }

    public string? ConflictingAllergy { get; set; }

    public string? AlternativeCode { get; set; }
}

public sealed class ServiceDefinition
{
    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal MonthlyPrice { get; set; }

    public int Revision { get; set; }

    public bool Available { get; set; }

    public int Capacity { get; set; }
}

public sealed class QuoteState
{
    public string Id { get; set; } = string.Empty;

    public int Revision { get; set; }

    public CareLevel CareLevel { get; set; }

    public PayerKind PayerKind { get; set; }

    public QuotePricing Pricing { get; set; } = new();

    public DateTimeOffset ExpiresAt { get; set; }
}

public sealed class QuotePricing
{
    public decimal Base { get; set; }

    public List<PriceLine> Lines { get; set; } = [];

    public decimal MonthlyTotal { get; set; }

    public decimal Deposit { get; set; }

    public AssistanceTerms Assistance { get; set; } = new();
}

public sealed class PriceLine
{
    public string ServiceCode { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public int Revision { get; set; }
}

public sealed class AssistanceTerms
{
    internal static AssistanceTerms For(MoveInWorld world, DraftState draft, PayerKind payer, decimal total)
    {
        var coverage = draft.Data.Payer.Primary;
        var policy = world.Policies.Find(p => p.Number == coverage.PolicyNumber && p.ResidentId == draft.Data.ResidentId && p.Active && !p.Blocked);
        if (payer != PayerKind.Insurance || policy is null) return new AssistanceTerms();
        return new AssistanceTerms
        {
            Required = true,
            Reference = "AST-" + draft.Id[1..],
            Contribution = Math.Min(policy.MonthlyLimit, decimal.Round(total * coverage.ContributionPercent / 100m, 2)),
        };
    }

    public bool Required { get; set; }

    public string Reference { get; set; } = string.Empty;

    public decimal Contribution { get; set; }

    public bool Accepted { get; set; }
}

public sealed class PolicyState
{
    public string Number { get; set; } = string.Empty;

    public string ResidentId { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public bool Active { get; set; }

    public string AuthorisedContactId { get; set; } = string.Empty;

    public decimal MonthlyLimit { get; set; }

    public bool Blocked { get; set; }
}

public sealed class ReviewState
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public ReviewKind Kind { get; set; }

    public int DraftVersion { get; set; }

    public bool Passed { get; set; }

    public string Reference { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}

public sealed class DocumentState
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public DocumentKind Kind { get; set; }

    public string Name { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long Size { get; set; }

    public long LastModified { get; set; }

    public byte[] Bytes { get; set; } = [];

    public int Revision { get; set; }

    public DocumentScan Scan { get; set; }

    public string UploadedBy { get; set; } = string.Empty;
}

public sealed class PaymentState
{
    public string ReceiptId { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public Guid OperationId { get; set; }

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public string LastFour { get; set; } = string.Empty;

    public PaymentStatus Status { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}

public sealed class AdmissionState
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public string ResidentId { get; set; } = string.Empty;

    public string BedId { get; set; } = string.Empty;

    public int QuoteRevision { get; set; }

    public decimal MonthlyTotal { get; set; }

    public string DepositReceiptId { get; set; } = string.Empty;

    public ArrivalState Arrival { get; set; } = new();
}

public sealed class ArrivalState
{
    public DateTimeOffset At { get; set; }

    public bool ChecklistRequired { get; set; }

    public string ChecklistId { get; set; } = string.Empty;

    public bool Completed { get; set; }
}

public sealed class WaitlistState
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public CareLevel CareLevel { get; set; }

    public string ResidentId { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public int Position { get; set; }
}

public sealed class CoordinatorState
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public List<string> CommunityIds { get; set; } = [];

    public bool Online { get; set; }

    public bool CanAdmit { get; set; }

    public bool CanTakeDeposit { get; set; }
}

public sealed class CoordinatorMessage
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public string CoordinatorId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;

    public int Sequence { get; set; }

    public DateTimeOffset SentAt { get; set; }
}

public sealed class AcknowledgementState
{
    public string Id { get; set; } = string.Empty;

    public string DraftId { get; set; } = string.Empty;

    public string Kind { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string CoordinatorId { get; set; } = string.Empty;

    public string Text { get; set; } = string.Empty;
}

public sealed class HistoryEntry
{
    public int Sequence { get; set; }

    public string DraftId { get; set; } = string.Empty;

    public Guid OperationId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string Actor { get; set; } = string.Empty;

    public DateTimeOffset At { get; set; }

    public string ResultReference { get; set; } = string.Empty;
}

public sealed class ServerRules
{
    public List<string> ClosedBuildingIds { get; set; } = [];

    public List<string> RevokedAuthorityReferences { get; set; } = [];

    public List<string> BlockedPolicyNumbers { get; set; } = [];

    public List<string> QuarantinedFileContents { get; set; } = [];

    public List<string> DeclinedCardNumbers { get; set; } = [];

    public bool RoomReviewLocked { get; set; }

    public bool DocumentUploadLocked { get; set; }

    public int MinimumAdmissionAge { get; set; }

    public int MaximumContacts { get; set; }
}
