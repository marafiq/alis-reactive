using System.Collections.Generic;

namespace Alis.Reactive.DriftDetection.Tests.Infrastructure;

/// <summary>
/// Senior living domain model for drift detection tests.
/// Properties exercise all coercion types: string, number, boolean, date.
/// </summary>
public class ResidentModel
{
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? CareLevel { get; set; }
    public List<string>? CareTags { get; set; }
    public decimal? MonthlyRate { get; set; }
    public bool IsVeteran { get; set; }
    public DateTime? AdmissionDate { get; set; }
    public string? PhysicianName { get; set; }
    public string? VeteranId { get; set; }
}
