namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.PharmacyReview;

public class PharmacyReviewModel
{
    public string ResidentId { get; set; } = "";

    public string Medication { get; set; } = "";

    public bool HoldOrder { get; set; }
}

public sealed class PharmacyProfileRequest
{
    public string ResidentId { get; set; } = "";
}

public sealed record PharmacyProfile(string Summary);

public class PharmacyReviewOrder
{
    public string? Medication { get; set; }
}

public sealed class PharmacyReviewRequest : PharmacyReviewOrder
{
    public string ResidentId { get; set; } = "";
}

public sealed record PharmacyReviewAnswer(string Finding, int Severity, string Detail);

public sealed record PharmacyRefusal(string Code, string Reason);

public sealed class PhysicianPageRequest
{
    public string ResidentId { get; set; } = "";

    public string? Reason { get; set; }
}

public sealed record PhysicianPaged(string Message);

public sealed class FormularyLookup
{
    public string? Medication { get; set; }
}

public sealed record FormularyEntry(string Dispensed, string Form);
