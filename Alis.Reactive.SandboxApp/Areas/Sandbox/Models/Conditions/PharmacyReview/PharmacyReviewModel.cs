namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.PharmacyReview;

/// <summary>
/// A physician's new medication order, as the charge nurse enters it and checks it with the pharmacy
/// before it goes on the medication administration record (MAR).
/// </summary>
public class PharmacyReviewModel
{
    /// <summary>The resident the order is for; empty until the nurse chooses one.</summary>
    public string ResidentId { get; set; } = "";

    /// <summary>The medication as the nurse types it: a generic or a brand name.</summary>
    public string Medication { get; set; } = "";

    /// <summary>Whether the order is held off the MAR until the hold is lifted.</summary>
    public bool HoldOrder { get; set; }
}

/// <summary>The pharmacy's profile request <c>Residents/{residentId}/Profile</c> binds: the resident in the path.</summary>
public sealed class PharmacyProfileRequest
{
    public string ResidentId { get; set; } = "";
}

/// <summary>What the pharmacy has on file for a resident: current medications and allergies.</summary>
public sealed record PharmacyProfile(string Summary);

/// <summary>The order the pharmacy reviews: the medication, in the body.</summary>
public class PharmacyReviewOrder
{
    /// <summary>The medication; a cleared field arrives as null.</summary>
    public string? Medication { get; set; }
}

/// <summary>The review request <c>Residents/{residentId}/Review</c> binds: the resident in the path.</summary>
public sealed class PharmacyReviewRequest : PharmacyReviewOrder
{
    public string ResidentId { get; set; } = "";
}

/// <summary>
/// The pharmacy's review: what it found ("interaction", "duplicate" or "clear"), how severe it is from
/// 0 to 10, and what it said.
/// </summary>
public sealed record PharmacyReviewAnswer(string Finding, int Severity, string Detail);

/// <summary>Why the pharmacy would not review the order: a code ("allergy", "prescription") and its reason.</summary>
public sealed record PharmacyRefusal(string Code, string Reason);

/// <summary>The page to the resident's physician: who it is about and why.</summary>
public sealed class PhysicianPageRequest
{
    public string ResidentId { get; set; } = "";

    /// <summary>The pharmacy's words; a cleared value arrives as null.</summary>
    public string? Reason { get; set; }
}

/// <summary>The paging service's answer.</summary>
public sealed record PhysicianPaged(string Message);

/// <summary>The formulary lookup, by the name the nurse typed, in the query string.</summary>
public sealed class FormularyLookup
{
    /// <summary>The typed name; an empty field arrives as null.</summary>
    public string? Medication { get; set; }
}

/// <summary>What the pharmacy dispenses for a name on its formulary: the generic and its form.</summary>
public sealed record FormularyEntry(string Dispensed, string Form);
