using System;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

/// <summary>
/// Exercises component-read conditions (<c>p.When(comp.Value&lt;T&gt;()).Op(...)</c>)
/// across every shape kind the framework supports: string, number, nullable number,
/// boolean, nullable date, and array-of-string.
///
/// Every operator/shape pair that depends on shape convert correctness has a
/// dedicated button scenario. Component-read (not event-args and not
/// condition-inside-event-handler) is the focus — those paths are already
/// exercised by existing sandbox views.
/// </summary>
public class ConditionCoverageModel
{
    /// <summary>Dropdown value — exercises eq / neq on Shape.String.</summary>
    public string CareLevel { get; set; } = "Standard";

    /// <summary>Text input — exercises contains / starts-with / ends-with / matches / min-length.</summary>
    public string ResidentName { get; set; } = "Jane Doe";

    /// <summary>Rich-text body — exercises is-empty / not-empty on strings.</summary>
    public string Notes { get; set; } = "Initial notes";

    /// <summary>Numeric — exercises gt / gte / lt / lte / between / in on Shape.Number.</summary>
    public decimal HeartRate { get; set; } = 72m;

    /// <summary>Nullable numeric — exercises is-null / not-null on Nullable(Number).</summary>
    public decimal? Dosage { get; set; } = null;

    /// <summary>Boolean — exercises truthy / falsy / eq on Shape.Boolean.</summary>
    public bool AcceptedTerms { get; set; } = false;

    /// <summary>Nullable date — exercises gt / lt / between / is-null / not-null / eq on Nullable(Date).</summary>
    public DateTime? AdmissionDate { get; set; } = null;

    /// <summary>Peer date for cross-component comparison (admission &lt; discharge).</summary>
    public DateTime? DischargeDate { get; set; } = null;

    /// <summary>Array of strings — exercises is-empty / not-empty / array-contains.</summary>
    public string[] Allergies { get; set; } = Array.Empty<string>();
}
