using Alis.Reactive.Conformance.App.Components;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class MoveInPage
{
    public string DraftId { get; set; } = string.Empty;

    public int Version { get; set; }

    public DateTime Today { get; set; }

    public string ResidentId { get; set; } = string.Empty;

    public LocationInput Location { get; set; } = new();

    public CareInput Care { get; set; } = new();

    public ResidentInput Resident { get; set; } = new();

    public ContactSection Contacts { get; set; } = new();

    private PayerInput _payer = new();

    public PayerInput Payer
    {
        get => _payer;
        set
        {
            _payer = value;
            IncludesPayer = true;
        }
    }

    internal bool IncludesPayer { get; private set; }

    private MedicationSection _medications = new();

    public MedicationSection Medications
    {
        get => _medications;
        set
        {
            _medications = value;
            IncludesMedications = true;
        }
    }

    internal bool IncludesMedications { get; private set; }

    public ServiceInput Services { get; set; } = new();

    private DietInput _diet = new();

    public DietInput Diet
    {
        get => _diet;
        set
        {
            _diet = value;
            IncludesDiet = true;
        }
    }

    internal bool IncludesDiet { get; private set; }

    public DocumentSelection Documents { get; set; } = new();

    public ReviewInput Review { get; set; } = new();

    public string? Reference { get; set; }

    public int? AcceptedQuoteRevision { get; set; }

    public string CoordinatorId { get; set; } = string.Empty;

    public string? Referral { get; set; }

    public bool Rehearsal { get; set; }

    public Guid OperationId { get; set; }
}

public sealed class LocationInput
{
    public string CommunityId { get; set; } = string.Empty;

    public string BuildingId { get; set; } = string.Empty;

    public string ApartmentId { get; set; } = string.Empty;

    public string BedId { get; set; } = string.Empty;

    public int BedRevision { get; set; }
}

public sealed class CareInput
{
    public CareLevel Level { get; set; } = CareLevel.Independent;

    public IndependentCareInput? Independent { get; set; }

    public AssistedCareInput? Assisted { get; set; }

    public MemoryCareInput? Memory { get; set; }

    public SkilledCareInput? Skilled { get; set; }
}

public sealed class IndependentCareInput
{
    public bool EmergencyCallAcknowledged { get; set; }
}

public sealed class AssistedCareInput
{
    public string AssessmentCode { get; set; } = string.Empty;

    public DateTime AssessmentDate { get; set; }

    public decimal AssistanceHours { get; set; }

    public string MobilityNote { get; set; } = string.Empty;
}

public sealed class MemoryCareInput
{
    public string AssessmentCode { get; set; } = string.Empty;

    public bool SecureUnitConsent { get; set; }

    public string RepresentativeContactId { get; set; } = string.Empty;
}

public sealed class SkilledCareInput
{
    public string OrderReference { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }

    public string Prescriber { get; set; } = string.Empty;
}

public sealed class ResidentInput
{
    public ResidentIdentity Identity { get; set; } = new();

    public Address Address { get; set; } = new();

    public DateTime MoveInDate { get; set; }

    public DateTimeOffset ArrivalAt { get; set; }

    public List<string> Allergies { get; set; } = [];

    public string Notes { get; set; } = string.Empty;
}

public sealed class ResidentIdentity
{
    public string Name { get; set; } = string.Empty;

    public string? PreferredName { get; set; }

    public DateTime BirthDate { get; set; }

    public string Email { get; set; } = string.Empty;

    public string EmailConfirmation { get; set; } = string.Empty;
}

public sealed class Address
{
    public string Street { get; set; } = string.Empty;

    public string City { get; set; } = string.Empty;

    public string Region { get; set; } = string.Empty;

    public string PostalCode { get; set; } = string.Empty;

    public string CountryCode { get; set; } = string.Empty;
}

public sealed class ContactSection
{
    public List<ContactInput> Rows { get; set; } = [];
}

public sealed class ContactInput
{
    public string Id { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public ContactRelationship Relationship { get; set; }

    public string Email { get; set; } = string.Empty;

    public string Phone { get; set; } = string.Empty;

    public string AlternatePhone { get; set; } = string.Empty;

    public AuthorityRole Role { get; set; }

    public AuthorityInput? Authority { get; set; }
}

public sealed class AuthorityInput
{
    public string Reference { get; set; } = string.Empty;

    public DateTime StartsOn { get; set; }

    public DateTime? EndsOn { get; set; }

    public bool Consent { get; set; }
}

public sealed class PayerInput
{
    public PayerKind Kind { get; set; }

    public string ContactId { get; set; } = string.Empty;

    public CoverageInput Primary { get; set; } = new();

    public bool HasSecondary { get; set; }

    public SecondaryCoverageInput? Secondary { get; set; }

    public string PortalUrl { get; set; } = string.Empty;

    public string? VerificationId { get; set; }
}

public sealed class CoverageInput
{
    public string PolicyNumber { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public DateTime StartsOn { get; set; }

    public DateTime EndsOn { get; set; }

    public decimal ContributionPercent { get; set; }
}

public sealed class SecondaryCoverageInput
{
    public string PolicyNumber { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    public DateTime StartsOn { get; set; }

    public DateTime EndsOn { get; set; }

    public decimal ContributionPercent { get; set; }
}

public sealed class MedicationSection
{
    public List<MedicationInput> Rows { get; set; } = [];

    public decimal? MinimumRoundDose { get; set; }

    public List<decimal?> RoundDoses { get; set; } = [];
}

public sealed class MedicationInput
{
    public string Id { get; set; } = string.Empty;

    public string DrugCode { get; set; } = string.Empty;

    public decimal Dose { get; set; }

    public string Unit { get; set; } = string.Empty;

    public List<string> Schedule { get; set; } = [];

    public DateTime StartsOn { get; set; }

    public DateTime? EndsOn { get; set; }

    public string Instructions { get; set; } = string.Empty;
}

public sealed class ServiceInput
{
    public List<string> Codes { get; set; } = [];

    public List<ServiceVisit> Visits { get; set; } = [];

    public List<DateTime> PreferredDays { get; set; } = [];

    public List<string?> OptionalLabels { get; set; } = [];
}

public sealed class ServiceVisit
{
    public string Code { get; set; } = string.Empty;

    public int Count { get; set; }

    public bool Active { get; set; }
}

public sealed class DietInput
{
    internal DietaryNeeds UnavailableMeals { get; set; }
    internal DietaryNeeds? Referral { get; set; }
    internal string? ReferralRefusal { get; set; }

    public DietaryNeeds Needs { get; set; }

    public string KitchenNote { get; set; } = string.Empty;

    public int MealCount { get; set; }

    public bool Confirmed { get; set; }
}

public sealed class DocumentSelection
{
    public List<string> Ids { get; set; } = [];

    public List<DocumentKind> RequiredKinds { get; set; } = [];
}

public sealed class ReviewInput
{
    public string? Reference { get; set; }

    public int SelectedServiceCount { get; set; }

    public int VisitCount { get; set; }

    public bool Ready { get; set; }

    public Record? Result { get; set; }

    public DateTimeOffset? ReviewedAt { get; set; }
}
