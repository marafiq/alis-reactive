namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoveInPageAddress
{
    public string DraftId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public string Referral { get; set; } = string.Empty;

    public bool ReviewOnly { get; set; }

    public string Culture { get; set; } = string.Empty;
}

public sealed class LocationQuery
{
    public string DraftId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public string BuildingId { get; set; } = string.Empty;

    public string ApartmentId { get; set; } = string.Empty;

    public CareLevel Level { get; set; }

    public string Culture { get; set; } = string.Empty;
}

public sealed class SectionQuery
{
    public string DraftId { get; set; } = string.Empty;

    public SectionKind Section { get; set; }

    public string? ContactId { get; set; }

    public CareLevel Level { get; set; }

    public PayerKind PayerKind { get; set; }

    public string Culture { get; set; } = string.Empty;
}

public sealed class ContactEdit
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public ContactSection Contacts { get; set; } = new();

    public ContactInput? NewContact { get; set; }

    public string? RemoveId { get; set; }
}

public sealed class MedicationEdit
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public MedicationSection Medications { get; set; } = new();

    public MedicationInput? NewMedication { get; set; }

    public string? RemoveId { get; set; }
}

public sealed class AllocationRequest
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public LocationInput Location { get; set; } = new();

    public CareLevel Level { get; set; }

    public string ResidentId { get; set; } = string.Empty;

    public bool Ready { get; set; }

    public string? Reference { get; set; }
}

public sealed class PayerVerificationPage
{
    public string DraftId { get; set; } = string.Empty;

    public string ResidentId { get; set; } = string.Empty;

    public string ContactId { get; set; } = string.Empty;

    public CoverageInput Policy { get; set; } = new();

    public string? Reference { get; set; }

    public bool Consent { get; set; }
}

public sealed class PharmacyRequest
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public string ResidentId { get; set; } = string.Empty;

    public List<string> Allergies { get; set; } = [];

    public MedicationSection Medications { get; set; } = new();

    public ReviewInput Review { get; set; } = new();
}

public sealed class QuoteRequest
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public CareLevel CareLevel { get; set; }

    public PayerKind PayerKind { get; set; }

    public ServiceInput Services { get; set; } = new();

    public ReviewInput Review { get; set; } = new();
}

public sealed class QuoteAcceptance
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public string QuoteId { get; set; } = string.Empty;

    public int Revision { get; set; }

    public bool AcceptAssistance { get; set; }
}

public sealed class WaitlistPage
{
    public string DraftId { get; set; } = string.Empty;

    public string ResidentId { get; set; } = string.Empty;

    public string CommunityId { get; set; } = string.Empty;

    public CareLevel CareLevel { get; set; }

    public string Reason { get; set; } = string.Empty;

    public bool AcceptTerms { get; set; }
}

public sealed class DocumentsPage
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public DocumentKind Kind { get; set; }

    public string Caption { get; set; } = string.Empty;

    public List<IFormFile> Files { get; set; } = [];

    public IFormFile? AuthorityFile { get; set; }

    public bool IncludeAuthority { get; set; }
}

public sealed class SignatureRequest
{
    public string DraftId { get; set; } = string.Empty;

    public string DocumentId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;

    public long Size { get; set; }

    public long LastModified { get; set; }
}

public sealed class DepositPage
{
    public string DraftId { get; set; } = string.Empty;

    public Guid OperationId { get; set; }

    public string CardNumber { get; set; } = string.Empty;

    public string Cardholder { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public string Currency { get; set; } = string.Empty;

    public bool Consent { get; set; }
}

public sealed class DietRequest
{
    public string DraftId { get; set; } = string.Empty;

    public DietInput Diet { get; set; } = new();

    public List<DocumentKind> Kinds { get; set; } = [];

    public List<string?> OptionalLabels { get; set; } = [];
}

public sealed class RoomReviewRequest
{
    public string DraftId { get; set; } = string.Empty;

    public string Room { get; set; } = string.Empty;

    public string Reference { get; set; } = string.Empty;

    public string CoordinatorId { get; set; } = string.Empty;
}

public sealed class MobilityRequest
{
    public string DraftId { get; set; } = string.Empty;

    public double Ratio { get; set; }

    public string Description { get; set; } = string.Empty;

    public string? PreviousDescription { get; set; }
}

public sealed class ReceptionPage
{
    public string DraftId { get; set; } = string.Empty;

    public string ResidentId { get; set; } = string.Empty;

    public string VisitorName { get; set; } = string.Empty;

    public ContactRelationship Relationship { get; set; }

    public bool Consent { get; set; }

    public string Desk { get; set; } = string.Empty;

    public int Copies { get; set; }
}

public sealed record SaveReply(SaveReceipt Receipt, ReceiptReview Review);

public sealed record SaveReceipt(string DraftId, int Version, string Reference, PersonLabel SavedBy, DateTimeOffset SavedAt);

public sealed record PersonLabel(string Id, string Name);

public sealed record ReceiptReview(bool Required, string Reference);

public sealed record AllocationReply(AllocationDecision Allocation);

public sealed record AllocationDecision(string Status, BedSummary Bed, InstructionReference Instructions);

public sealed record BedSummary(string Id, ApartmentSummary Apartment, int Revision, bool Available);

public sealed record ApartmentSummary(string Id, string Number, BuildingSummary Building);

public sealed record BuildingSummary(string Id, string Name, CommunitySummary Community);

public sealed record CommunitySummary(string Id, string Name);

public sealed record InstructionReference(bool Required, string Id);

public sealed record VerificationReply(VerifiedPolicy Policy);

public sealed record VerifiedPolicy(string Number, VerificationDecision Verification);

public sealed record VerificationDecision(string Id, bool CanProceed, InstructionReference Details);

public sealed record PharmacyReply(PharmacyReview Review);

public sealed record PharmacyReview(string Id, PharmacyClearance Clearance);

public sealed record PharmacyClearance(bool Passed, bool InstructionsRequired, InstructionReference Instructions);

public sealed record QuoteReply(QuoteState Quote);

public sealed record AdmissionReply(AdmissionState Admission);

public sealed record DocumentReply(DocumentAdmission Admission);

public sealed record DocumentAdmission(string DraftId, DocumentBatch Documents);

public sealed record DocumentBatch(List<DocumentMetadata> Accepted, InstructionReference Receipt);

public sealed record DocumentMetadata(string Id, string Name, string Type, long Size, int Revision, long LastModified);

public sealed record PaymentReply(DepositResult Deposit);

public sealed record DepositResult(PaymentState Receipt, DepositBalance Balance);

public sealed record DepositBalance(decimal Due, bool Satisfied);

public sealed record AvailabilityReply(CommunitySummary Community, int Revision, List<BedSummary> Beds);

public sealed record BedChanged(string CommunityId, BedSummary Bed, int PriceRevision);

public sealed record CoordinatorLineReply(CoordinatorLine Line);

public sealed record CoordinatorLine(int Sequence, List<PersonLabel> Coordinators, List<CoordinatorMessage> Messages);

public sealed record BedConflict(BedConflictDetail Conflict);

public sealed record BedConflictDetail(ConflictingBed Bed, WaitlistOffer Waitlist);

public sealed record ConflictingBed(string Id, PersonLabel Holder, ConflictHold Hold);

public sealed record ConflictHold(DateTimeOffset ExpiresAt);

public sealed record WaitlistOffer(bool Allowed, string TermsId);

public sealed record VersionConflict(VersionConflictDetail Conflict);

public sealed record VersionConflictDetail(ConflictingDraft Draft);

public sealed record ConflictingDraft(string Id, int Revision, PersonLabel SavedBy, InstructionReference Comparison);

public sealed record PayerDependency(PayerDependencyDetail Dependency);

public sealed record PayerDependencyDetail(DependentPayer Payer);

public sealed record DependentPayer(string ContactId, string PolicyNumber, InstructionReference Explanation);

public sealed record PharmacyConflict(PharmacyConflictDetail Conflict);

public sealed record PharmacyConflictDetail(string MedicationId, AllergyConflict Allergy);

public sealed record AllergyConflict(string Name, ConflictResolution Resolution);

public sealed record ConflictResolution(bool HasAlternatives, string Id);

public sealed record ErrorEnvelope(ErrorDetail Error);

public sealed record ErrorDetail(string Code, string Message, RecoveryAdvice Recovery);

public sealed record RecoveryAdvice(bool Allowed, string Reference);

public sealed record UploadPolicyReply(UploadPolicy Upload);

public sealed record UploadPolicy(bool Allowed, long MaximumBytes, List<string> Types, int Revision);
