using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

/// <summary>
/// Server-side wizard — single page shell, step content loaded via Into().
///
/// Flow:
///   DomReady → POST LoadStep(1) → Into("step-container")
///   Next     → POST SaveStepX (JSON) → OnSuccess set ScreeningId → Chained POST LoadStep(X+1) → Into()
///   Previous → POST LoadStep(X-1) → Into()
///   Submit   → POST Submit (JSON) → validates all drafts server-side
///
/// Draft persistence: static ConcurrentDictionary per step, keyed by screeningId.
/// Edit scenario: LoadStep reads from draft, model carries saved values, SF components render them.
/// </summary>
[Area("Sandbox")]
[Route("Sandbox/Conditions/AdmissionWizard")]
public class AdmissionWizardController : Controller
{
    // ── Draft store (in-memory, keyed by screeningId) ────────────────────────

    private static readonly ConcurrentDictionary<string, Step1DemographicsModel> Step1Drafts = new();
    private static readonly ConcurrentDictionary<string, Step2ClinicalModel> Step2Drafts = new();
    private static readonly ConcurrentDictionary<string, Step3FunctionalModel> Step3Drafts = new();
    private static readonly ConcurrentDictionary<string, Step4ReviewModel> Step4Drafts = new();

    private static string NewScreeningId() => $"SCR-{DateTime.UtcNow:yyyyMMddHHmmssffff}";

    private static T GetDraft<T>(ConcurrentDictionary<string, T> store, string id) where T : new()
        => !string.IsNullOrEmpty(id) && store.TryGetValue(id, out var draft) ? draft : new T();

    private static string EnsureId(string? existing) =>
        string.IsNullOrEmpty(existing) ? NewScreeningId() : existing;

    private const string ViewBase = "~/Areas/Sandbox/Views/Conditions/AdmissionWizard/";

    // ── Data sources (shared by all steps) ───────────────────────────────────

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

    // ── GET Index — shell page, Step 1 loads on DomReady via Into() ──────────

    [HttpGet("")]
    public IActionResult Index([FromQuery] string? screeningId)
    {
        ViewBag.ScreeningId = screeningId ?? "";
        ViewBag.CurrentStep = 1;
        return View(ViewBase + "Index.cshtml", new Step1DemographicsModel());
    }

    // ── POST SaveStep1/2/3 — validate → save draft → return JSON ────────────

    [HttpPost("SaveStep1")]
    public IActionResult SaveStep1([FromBody] Step1DemographicsModel model)
    {
        if (!TryValidate(new Step1Validator(), model, out var error)) return error;

        var id = EnsureId(model.ScreeningId);
        model.ScreeningId = id;
        Step1Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = $"Step 1 saved for {model.ResidentName}" });
    }

    [HttpPost("SaveStep2")]
    public IActionResult SaveStep2([FromBody] Step2ClinicalModel model)
    {
        if (!TryValidate(new Step2Validator(), model, out var error)) return error;

        var id = EnsureId(model.ScreeningId);
        model.ScreeningId = id;

        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        if (model.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's" && model.CognitiveScore > 0)
            model.CognitiveAssessmentId = $"COG-{ts}";
        if (model.PrimaryDiagnosis == "Heart Disease" && model.SystolicBP > 0)
            model.CardiacAssessmentId = $"CAR-{ts}";
        if (model.PrimaryDiagnosis == "Diabetes" && !string.IsNullOrEmpty(model.DiabetesType))
            model.DiabetesAssessmentId = $"DIA-{ts}";

        Step2Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = $"Step 2 saved — {model.PrimaryDiagnosis}" });
    }

    [HttpPost("SaveStep3")]
    public IActionResult SaveStep3([FromBody] Step3FunctionalModel model)
    {
        if (!TryValidate(new Step3Validator(), model, out var error)) return error;

        var id = EnsureId(model.ScreeningId);
        model.ScreeningId = id;
        Step3Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = "Step 3 saved" });
    }

    // ── POST LoadStep — loads step partial from draft (Next chained / Previous) ─

    [HttpPost("LoadStep")]
    public IActionResult LoadStep([FromBody] LoadStepRequest request)
    {
        SetDataSources();
        var id = request.ScreeningId ?? "";
        ViewBag.ScreeningId = id;
        ViewBag.CurrentStep = request.Step;

        return request.Step switch
        {
            1 => StepPartial("_Step1Content.cshtml", GetDraft(Step1Drafts, id)),
            2 => StepPartial("_Step2Content.cshtml", BuildStep2Model(id)),
            3 => StepPartial("_Step3Content.cshtml", BuildStep3Model(id)),
            4 => StepPartial("_Step4Content.cshtml", BuildStep4Model(id)),
            _ => BadRequest("Invalid step")
        };
    }

    private IActionResult StepPartial<T>(string view, T model) =>
        PartialView(ViewBase + view, model);

    private Step2ClinicalModel BuildStep2Model(string id)
    {
        var step1 = GetDraft(Step1Drafts, id);
        var model = GetDraft(Step2Drafts, id);
        model.ScreeningId = id;
        model.PrimaryDiagnosis = step1.PrimaryDiagnosis;
        model.ResidentName = step1.ResidentName;
        return model;
    }

    private Step3FunctionalModel BuildStep3Model(string id)
    {
        var step1 = GetDraft(Step1Drafts, id);
        var model = GetDraft(Step3Drafts, id);
        model.ScreeningId = id;
        model.Age = step1.Age;
        model.ResidentName = step1.ResidentName;
        return model;
    }

    private Step4ReviewModel BuildStep4Model(string id)
    {
        var step1 = GetDraft(Step1Drafts, id);
        var step2 = GetDraft(Step2Drafts, id);
        var step3 = GetDraft(Step3Drafts, id);
        var model = GetDraft(Step4Drafts, id);
        model.ScreeningId = id;
        model.RiskTier = step1.RiskTier;
        model.CareUnit = step2.CareUnit;
        model.MonitoringLevel = step3.MonitoringLevel;
        model.Step1Saved = !string.IsNullOrEmpty(id) && Step1Drafts.ContainsKey(id);
        model.Step2Saved = !string.IsNullOrEmpty(id) && Step2Drafts.ContainsKey(id);
        model.Step3Saved = !string.IsNullOrEmpty(id) && Step3Drafts.ContainsKey(id);
        return model;
    }

    // ── POST Submit — validates all drafts, returns care plan ────────────────

    [HttpPost("Submit")]
    public async Task<IActionResult> Submit([FromBody] Step4ReviewModel model)
    {
        await Task.Delay(500);

        var errors = new Dictionary<string, string[]>();
        CollectErrors(new Step4Validator().Validate(model), errors);

        var id = model.ScreeningId;
        if (string.IsNullOrEmpty(id))
            return BadRequest(new { errors });

        if (Step1Drafts.TryGetValue(id, out var step1))
            CollectErrors(new Step1Validator().Validate(step1), errors);
        else
            errors["Step1"] = ["Complete Step 1 before submitting"];

        if (Step2Drafts.TryGetValue(id, out var step2))
            CollectErrors(new Step2Validator().Validate(step2), errors);

        if (Step3Drafts.TryGetValue(id, out var step3))
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

    // ── Alert + search endpoints ─────────────────────────────────────────────

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
        return Ok(new PhysicianSearchResponse
        {
            Physicians = string.IsNullOrEmpty(q)
                ? all
                : all.Where(p => p.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList()
        });
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

    // ── Helpers ──────────────────────────────────────────────────────────────

    public class LoadStepRequest
    {
        public string ScreeningId { get; set; } = "";
        public int Step { get; set; }
    }

    private bool TryValidate<T>(FluentValidation.IValidator<T> validator, T model, out IActionResult error)
    {
        var result = validator.Validate(model);
        if (result.IsValid)
        {
            error = null!;
            return true;
        }

        var errors = new Dictionary<string, string[]>();
        CollectErrors(result, errors);
        error = BadRequest(new { errors });
        return false;
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
