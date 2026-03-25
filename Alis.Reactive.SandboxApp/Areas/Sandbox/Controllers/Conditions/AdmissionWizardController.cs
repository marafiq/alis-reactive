using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Server-side wizard — single page, step content loaded via Into().
/// Next = Post save → validate → OnSuccess(s => s.Into("step-container")) → server returns next step partial HTML.
/// Previous = Post → server returns previous step partial HTML.
/// Edit scenario: server loads model from draft on every step load.
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

    private const string ViewBase = "~/Areas/Sandbox/Views/Conditions/AdmissionWizard/";

    // ── GET /Index → full page with Step 1 loaded ───────────────────────────

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index([FromQuery] string? screeningId)
    {
        SetDataSources();
        var id = screeningId ?? "";
        ViewBag.ScreeningId = id;
        ViewBag.CurrentStep = 1;

        var model = !string.IsNullOrEmpty(id) && Step1Drafts.TryGetValue(id, out var draft)
            ? draft
            : new Step1DemographicsModel();

        return View(ViewBase + "Index.cshtml", model);
    }

    // ── POST SaveStep1 → saves draft, returns Step 2 partial HTML ───────────

    [HttpPost("SaveStep1")]
    public IActionResult SaveStep1([FromBody] Step1DemographicsModel model)
    {
        var errors = new Dictionary<string, string[]>();
        CollectErrors(new Step1Validator().Validate(model), errors);
        if (errors.Count > 0) return BadRequest(new { errors });

        var id = NewScreeningId();
        Step1Drafts[id] = model;

        // Return Step 2 partial
        SetDataSources();
        ViewBag.ScreeningId = id;
        ViewBag.CurrentStep = 2;

        var step2 = new Step2ClinicalModel
        {
            ScreeningId = id,
            PrimaryDiagnosis = model.PrimaryDiagnosis,
            ResidentName = model.ResidentName,
        };

        return PartialView(ViewBase + "_Step2Content.cshtml", step2);
    }

    // ── POST SaveStep2 → saves draft, returns Step 3 partial HTML ───────────

    [HttpPost("SaveStep2")]
    public IActionResult SaveStep2([FromBody] Step2ClinicalModel model)
    {
        var errors = new Dictionary<string, string[]>();
        CollectErrors(new Step2Validator().Validate(model), errors);
        if (errors.Count > 0) return BadRequest(new { errors });

        var screeningId = model.ScreeningId;
        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        if (model.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's" && model.CognitiveScore > 0)
            model.CognitiveAssessmentId = $"COG-{ts}";
        if (model.PrimaryDiagnosis == "Heart Disease" && model.SystolicBP > 0)
            model.CardiacAssessmentId = $"CAR-{ts}";
        if (model.PrimaryDiagnosis == "Diabetes" && !string.IsNullOrEmpty(model.DiabetesType))
            model.DiabetesAssessmentId = $"DIA-{ts}";
        Step2Drafts[screeningId] = model;

        // Return Step 3 partial
        SetDataSources();
        ViewBag.ScreeningId = screeningId;
        ViewBag.CurrentStep = 3;

        Step1Drafts.TryGetValue(screeningId, out var step1);
        var step3 = Step3Drafts.TryGetValue(screeningId, out var s3Draft)
            ? s3Draft
            : new Step3FunctionalModel();
        step3.ScreeningId = screeningId;
        step3.Age = step1?.Age ?? 0;
        step3.ResidentName = step1?.ResidentName ?? "";

        return PartialView(ViewBase + "_Step3Content.cshtml", step3);
    }

    // ── POST SaveStep3 → saves draft, returns Step 4 partial HTML ───────────

    [HttpPost("SaveStep3")]
    public IActionResult SaveStep3([FromBody] Step3FunctionalModel model)
    {
        var errors = new Dictionary<string, string[]>();
        CollectErrors(new Step3Validator().Validate(model), errors);
        if (errors.Count > 0) return BadRequest(new { errors });

        var screeningId = model.ScreeningId;
        Step3Drafts[screeningId] = model;

        // Return Step 4 partial
        ViewBag.ScreeningId = screeningId;
        ViewBag.CurrentStep = 4;

        Step1Drafts.TryGetValue(screeningId, out var step1);
        Step2Drafts.TryGetValue(screeningId, out var step2);

        var step4 = Step4Drafts.TryGetValue(screeningId, out var s4Draft)
            ? s4Draft
            : new Step4ReviewModel();
        step4.ScreeningId = screeningId;
        step4.RiskTier = step1?.RiskTier ?? "";
        step4.CareUnit = step2?.CareUnit ?? "";
        step4.MonitoringLevel = model.MonitoringLevel;
        step4.Step1Saved = Step1Drafts.ContainsKey(screeningId);
        step4.Step2Saved = Step2Drafts.ContainsKey(screeningId);
        step4.Step3Saved = true;

        return PartialView(ViewBase + "_Step4Content.cshtml", step4);
    }

    // ── POST LoadStep (Previous navigation) ─────────────────────────────────

    [HttpPost("LoadStep")]
    public IActionResult LoadStep([FromBody] LoadStepRequest request)
    {
        SetDataSources();
        ViewBag.ScreeningId = request.ScreeningId;
        ViewBag.CurrentStep = request.Step;

        return request.Step switch
        {
            1 => PartialView(ViewBase + "_Step1Content.cshtml",
                Step1Drafts.TryGetValue(request.ScreeningId, out var s1) ? s1 : new Step1DemographicsModel()),
            2 => LoadStep2Partial(request.ScreeningId),
            3 => LoadStep3Partial(request.ScreeningId),
            _ => BadRequest("Invalid step")
        };
    }

    private IActionResult LoadStep2Partial(string screeningId)
    {
        Step1Drafts.TryGetValue(screeningId, out var step1);
        var model = Step2Drafts.TryGetValue(screeningId, out var draft)
            ? draft : new Step2ClinicalModel();
        model.ScreeningId = screeningId;
        model.PrimaryDiagnosis = step1?.PrimaryDiagnosis ?? "";
        model.ResidentName = step1?.ResidentName ?? "";
        return PartialView(ViewBase + "_Step2Content.cshtml", model);
    }

    private IActionResult LoadStep3Partial(string screeningId)
    {
        Step1Drafts.TryGetValue(screeningId, out var step1);
        var model = Step3Drafts.TryGetValue(screeningId, out var draft)
            ? draft : new Step3FunctionalModel();
        model.ScreeningId = screeningId;
        model.Age = step1?.Age ?? 0;
        model.ResidentName = step1?.ResidentName ?? "";
        return PartialView(ViewBase + "_Step3Content.cshtml", model);
    }

    // ── POST Submit ─────────────────────────────────────────────────────────

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
        if (step2 != null) CollectErrors(new Step2Validator().Validate(step2), errors);

        Step3Drafts.TryGetValue(id, out var step3);
        if (step3 != null) CollectErrors(new Step3Validator().Validate(step3), errors);

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
            ScreeningId = id, CareUnit = careUnit, MonitoringLevel = monitoring,
            Message = $"Assessment complete for {step1?.ResidentName}",
        });
    }

    // ── Alert + search endpoints (unchanged) ────────────────────────────────

    [HttpGet("SearchPhysicians")]
    public async Task<IActionResult> SearchPhysicians([FromQuery] string? q)
    {
        await Task.Delay(300);
        var all = new List<PhysicianItem>
        {
            new() { Text = "Dr. Sarah Chen", Value = "Dr. Sarah Chen", Specialty = "Geriatrics" },
            new() { Text = "Dr. James Wilson", Value = "Dr. James Wilson", Specialty = "Cardiology" },
            new() { Text = "Dr. Emily Park", Value = "Dr. Emily Park", Specialty = "Neurology" },
            new() { Text = "Dr. Michael Torres", Value = "Dr. Michael Torres", Specialty = "Internal Medicine" },
        };
        var filtered = string.IsNullOrEmpty(q) ? all : all.Where(p => p.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
        return Ok(new PhysicianSearchResponse { Physicians = filtered });
    }

    [HttpPost("AlertElopement")]
    public async Task<IActionResult> AlertElopement([FromBody] AdmissionAssessmentController.AlertElopementRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Elopement risk flagged for {r?.ResidentName}", Urgency = "high" }); }

    [HttpPost("AlertHypertension")]
    public async Task<IActionResult> AlertHypertension([FromBody] AdmissionAssessmentController.AlertHypertensionRequest? r)
    { await Task.Delay(400); return Ok(new ScreeningAlertResponse { Message = $"Hypertension: {r?.SystolicBP}mmHg", Urgency = "moderate" }); }

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

    // ── DTOs + Helpers ──────────────────────────────────────────────────────

    public class LoadStepRequest
    {
        public string ScreeningId { get; set; } = "";
        public int Step { get; set; }
    }

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
