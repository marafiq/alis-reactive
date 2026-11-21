using System.Text.Json.Serialization;

namespace Alis.Reactive.Conformance.App.Care;

[JsonConverter(typeof(JsonStringEnumConverter<CareLevel>))]
public enum CareLevel
{
    Independent = 1,
    Assisted = 2,
    Memory = 3,
    Skilled = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter<BedState>))]
public enum BedState
{
    Available,
    Held,
    Occupied,
    Closed,
}

[JsonConverter(typeof(JsonStringEnumConverter<DraftStatus>))]
public enum DraftStatus
{
    Open,
    Submitted,
    Discarded,
}

[JsonConverter(typeof(JsonStringEnumConverter<ContactRelationship>))]
public enum ContactRelationship
{
    Daughter,
    Son,
    Spouse,
    Friend,
    Professional,
}

[JsonConverter(typeof(JsonStringEnumConverter<AuthorityRole>))]
public enum AuthorityRole
{
    Ordinary,
    PowerOfAttorney,
    Guardian,
}

[JsonConverter(typeof(JsonStringEnumConverter<PayerKind>))]
public enum PayerKind
{
    Private,
    Insurance,
    Benefits,
}

[JsonConverter(typeof(JsonStringEnumConverter<DocumentKind>))]
public enum DocumentKind
{
    Consent,
    MedicationList,
    Authority,
    Signature,
}

[JsonConverter(typeof(JsonStringEnumConverter<DocumentScan>))]
public enum DocumentScan
{
    Accepted,
    Quarantined,
}

[JsonConverter(typeof(JsonStringEnumConverter<PaymentStatus>))]
public enum PaymentStatus
{
    Authorised,
    Declined,
    Voided,
}

[JsonConverter(typeof(JsonStringEnumConverter<ReviewKind>))]
public enum ReviewKind
{
    Allocation,
    Payer,
    Pharmacy,
    Services,
    Room,
    Mobility,
    Identity,
}

[Flags]
[JsonConverter(typeof(JsonStringEnumConverter<DietaryNeeds>))]
public enum DietaryNeeds
{
    None = 0,
    Vegetarian = 1,
    LowSodium = 2,
    Pureed = 4,
}

[JsonConverter(typeof(JsonStringEnumConverter<SectionKind>))]
public enum SectionKind
{
    Buildings,
    Apartments,
    Beds,
    IndependentCare,
    AssistedCare,
    MemoryCare,
    SkilledCare,
    Contacts,
    Authority,
    PrimaryPayer,
    SecondaryPayer,
    Medications,
    AssistanceTerms,
    AllocationOwner,
    PayerOwner,
}
