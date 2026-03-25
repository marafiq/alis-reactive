using System.Collections.Concurrent;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Server-side wizard — each step is a full page load.
/// Next = POST save → redirect to next step. Previous = GET with screeningId.
/// Edit scenario: server loads model from draft, SF components render with saved values.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Conditions/AdmissionWizard")]
public class AdmissionWizardController : Controller
{
    private static readonly ConcurrentDictionary<string, Step1DemographicsModel> Step1Drafts = new();
    private static readonly ConcurrentDictionary<string, Step2ClinicalModel> Step2Drafts = new();
    private static readonly ConcurrentDictionary<string, Step3FunctionalModel> Step3Drafts = new();
    private static readonly ConcurrentDictionary<string, Step4ReviewModel> Step4Drafts = new();

    private static string NewScreeningId() => $"SCR-{DateTime.UtcNow:yyyyMMddHHmmssffff}";

    private void SetDataSources()
    {
        ViewBag.Diagnoses = new[] { "Alzheimer's", "Parkinson's", "Heart Disease", "Diabetes", "Stroke", "Other" };
        ViewBag.FallOptions = new[] { "None", "1-2 falls", "3+ falls" };
        ViewBag.MobilityAids = new[] { "None", "Cane", "Walker", "Wheelchair" };
        ViewBag.WanderFreqs = new[] { "Rarely", "Sometimes", "Frequently" };
        ViewBag.InsulinSchedules = new[] { "Morning", "Evening", "Both", "As Needed" };
        ViewBag.InjuryTypes = new[] { "Bruise", "Fracture", "Head Injury", "Other" };
        ViewBag.DiabetesTypes = new[] { "Type 1", "Type 2" };
        ViewBag.ServiceBranches = new[] { "Army", "Navy", "Air Force", "Marines", "Coast Guard" };
    }

    // ── GET /Index → redirect to Step1 ──────────────────────────────────────

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index([FromQuery] string? screeningId)
        => RedirectToAction(nameof(Step1), new { screeningId });

    // ── Step 1: Demographics ────────────────────────────────────────────────

    [HttpGet("Step1")]
    public IActionResult Step1([FromQuery] string? screeningId)
    {
        SetDataSources();
        ViewBag.ScreeningId = screeningId ?? "";
        ViewBag.CurrentStep = 1;

        var model = !string.IsNullOrEmpty(screeningId) && Step1Drafts.TryGetValue(screeningId, out var draft)
            ? draft
            : new Step1DemographicsModel();

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionWizard/Step1.cshtml", model);
    }

    [HttpPost("SaveStep1")]
    public IActionResult SaveStep1([FromForm] Step1DemographicsModel model, [FromForm] string? screeningId)
    {
        var id = string.IsNullOrEmpty(screeningId) ? NewScreeningId() : screeningId;
        Step1Drafts[id] = model;
        return RedirectToAction(nameof(Step2), new { screeningId = id });
    }

    // ── Step 2: Clinical ────────────────────────────────────────────────────

    [HttpGet("Step2")]
    public IActionResult Step2([FromQuery] string screeningId)
    {
        SetDataSources();
        ViewBag.ScreeningId = screeningId;
        ViewBag.CurrentStep = 2;

        Step1Drafts.TryGetValue(screeningId, out var step1);

        var model = Step2Drafts.TryGetValue(screeningId, out var draft)
            ? draft
            : new Step2ClinicalModel();

        // Cross-step data from Step 1
        model.ScreeningId = screeningId;
        model.PrimaryDiagnosis = step1?.PrimaryDiagnosis ?? "";
        model.ResidentName = step1?.ResidentName ?? "";

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionWizard/Step2.cshtml", model);
    }

    [HttpPost("SaveStep2")]
    public IActionResult SaveStep2([FromForm] Step2ClinicalModel model, [FromForm] string screeningId)
    {
        model.ScreeningId = screeningId;

        // Generate assessment IDs
        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        if (model.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's" && model.CognitiveScore > 0)
            model.CognitiveAssessmentId = $"COG-{ts}";
        if (model.PrimaryDiagnosis == "Heart Disease" && model.SystolicBP > 0)
            model.CardiacAssessmentId = $"CAR-{ts}";
        if (model.PrimaryDiagnosis == "Diabetes" && !string.IsNullOrEmpty(model.DiabetesType))
            model.DiabetesAssessmentId = $"DIA-{ts}";

        Step2Drafts[screeningId] = model;
        return RedirectToAction(nameof(Step3), new { screeningId });
    }

    // ── Step 3: Functional ──────────────────────────────────────────────────

    [HttpGet("Step3")]
    public IActionResult Step3([FromQuery] string screeningId)
    {
        SetDataSources();
        ViewBag.ScreeningId = screeningId;
        ViewBag.CurrentStep = 3;

        Step1Drafts.TryGetValue(screeningId, out var step1);

        var model = Step3Drafts.TryGetValue(screeningId, out var draft)
            ? draft
            : new Step3FunctionalModel();

        model.ScreeningId = screeningId;
        model.Age = step1?.Age ?? 0;
        model.ResidentName = step1?.ResidentName ?? "";

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionWizard/Step3.cshtml", model);
    }

    [HttpPost("SaveStep3")]
    public IActionResult SaveStep3([FromForm] Step3FunctionalModel model, [FromForm] string screeningId)
    {
        model.ScreeningId = screeningId;
        Step3Drafts[screeningId] = model;
        return RedirectToAction(nameof(Step4), new { screeningId });
    }

    // ── Step 4: Review & Submit ─────────────────────────────────────────────

    [HttpGet("Step4")]
    public IActionResult Step4([FromQuery] string screeningId)
    {
        ViewBag.ScreeningId = screeningId;
        ViewBag.CurrentStep = 4;

        Step1Drafts.TryGetValue(screeningId, out var step1);
        Step2Drafts.TryGetValue(screeningId, out var step2);
        Step3Drafts.TryGetValue(screeningId, out var step3);

        var model = Step4Drafts.TryGetValue(screeningId, out var draft)
            ? draft
            : new Step4ReviewModel();

        model.ScreeningId = screeningId;
        model.RiskTier = step1?.RiskTier ?? "";
        model.CareUnit = step2?.CareUnit ?? "";
        model.MonitoringLevel = step3?.MonitoringLevel ?? "";
        model.Step1Saved = Step1Drafts.ContainsKey(screeningId);
        model.Step2Saved = Step2Drafts.ContainsKey(screeningId);
        model.Step3Saved = Step3Drafts.ContainsKey(screeningId);

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionWizard/Step4.cshtml", model);
    }

    [HttpPost("Submit")]
    public async Task<IActionResult> Submit([FromBody] Step4ReviewModel model)
    {
        await Task.Delay(500);

        var errors = new Dictionary<string, string[]>();
        CollectErrors(new Step4Validator().Validate(model), errors);

        var id = model.ScreeningId;
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { errors });

        if (!Step1Drafts.TryGetValue(id, out var step1))
            errors["Step1"] = ["Complete Step 1 before submitting"];
        else
            CollectErrors(new Step1Validator().Validate(step1), errors);

        Step2Drafts.TryGetValue(id, out var step2);
        if (step2 != null)
            CollectErrors(new Step2Validator().Validate(step2), errors);

        Step3Drafts.TryGetValue(id, out var step3);
        if (step3 != null)
            CollectErrors(new Step3Validator().Validate(step3), errors);

        if (errors.Count > 0) return BadRequest(new { errors });

        var careUnit = step1?.PrimaryDiagnosis switch
        {
            "Alzheimer's" or "Parkinson's" when (step2?.CognitiveScore ?? 0) < 15 => "Memory Care",
            "Alzheimer's" or "Parkinson's" when (step2?.CognitiveScore ?? 0) < 25 => "Assisted Living with Memory Support",
            _ => "Standard Assisted Living"
        };

        var monitoring = "Standard";
        if (step3?.TakesBloodThinners == true) monitoring = "Enhanced";
        if ((step3?.FallRiskScore ?? 0) >= 7 && (step1?.Age ?? 0) >= 80) monitoring = "Continuous";

        return Ok(new SubmitScreeningResponse
        {
            ScreeningId = id,
            CareUnit = careUnit,
            MonitoringLevel = monitoring,
            Message = $"Assessment complete for {step1?.ResidentName}",
        });
    }

    // ── Alert endpoints (reuse from AdmissionAssessmentController pattern) ──

    [HttpGet("SearchPhysicians")]
    public async Task<IActionResult> SearchPhysicians([FromQuery] string? q)
    {
        await Task.Delay(300);
        var physicians = new List<PhysicianItem>
        {
            new() { Text = "Dr. Sarah Chen", Value = "Dr. Sarah Chen", Specialty = "Geriatrics" },
            new() { Text = "Dr. James Wilson", Value = "Dr. James Wilson", Specialty = "Cardiology" },
            new() { Text = "Dr. Emily Park", Value = "Dr. Emily Park", Specialty = "Neurology" },
            new() { Text = "Dr. Michael Torres", Value = "Dr. Michael Torres", Specialty = "Internal Medicine" },
        };
        var filtered = string.IsNullOrEmpty(q)
            ? physicians
            : physicians.Where(p => p.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(new PhysicianSearchResponse { Physicians = filtered });
    }

    [HttpPost("AlertElopement")]
    public async Task<IActionResult> AlertElopement([FromBody] AdmissionAssessmentController.AlertElopementRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Elopement risk flagged for {r?.ResidentName}", Urgency = "high" }); }

    [HttpPost("AlertHypertension")]
    public async Task<IActionResult> AlertHypertension([FromBody] AdmissionAssessmentController.AlertHypertensionRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Hypertension flagged: {r?.SystolicBP}mmHg", Urgency = "moderate" }); }

    [HttpPost("AlertUncontrolled")]
    public async Task<IActionResult> AlertUncontrolled([FromBody] AdmissionAssessmentController.AlertUncontrolledRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Uncontrolled diabetes: A1C {r?.A1cLevel}", Urgency = "high" }); }

    [HttpPost("AlertNeuro")]
    public async Task<IActionResult> AlertNeuro([FromBody] AdmissionAssessmentController.AlertNeuroRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Neuro consult for {r?.ResidentName}", Urgency = "immediate" }); }

    [HttpPost("AlertPain")]
    public async Task<IActionResult> AlertPain([FromBody] AdmissionAssessmentController.AlertPainRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Pain alert: level {r?.PainLevel}", Urgency = "immediate" }); }

    [HttpPost("RequestRoomSetup")]
    public async Task<IActionResult> RequestRoomSetup([FromBody] AdmissionAssessmentController.RequestRoomSetupRequest? r)
    { await Task.Delay(500); return Ok(new ScreeningAlertResponse { Message = $"Room setup for {r?.ResidentName}", Urgency = "routine" }); }

    [HttpPost("VerifyVeteran")]
    public async Task<IActionResult> VerifyVeteran([FromBody] AdmissionAssessmentController.VerifyVeteranRequest? r)
    { await Task.Delay(600); return Ok(new VerifyVaResponse { Message = $"VA: {r?.VaId}", Eligible = r?.VaId?.StartsWith("VA-") == true }); }

    private static void CollectErrors(FluentValidation.Results.ValidationResult result, Dictionary<string, string[]> errors)
    {
        foreach (var f in result.Errors)
        {
            if (!errors.ContainsKey(f.PropertyName))
                errors[f.PropertyName] = [f.ErrorMessage];
            else
                errors[f.PropertyName] = [..errors[f.PropertyName], f.ErrorMessage];
        }
    }
}
