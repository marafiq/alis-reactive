using System.Collections.Concurrent;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Conditions.AdmissionAssessment;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/AdmissionAssessment")]
public class AdmissionAssessmentController : Controller
{
    // In-memory drafts are keyed by screeningId so each step can save independently.
    private static readonly ConcurrentDictionary<string, Step1DemographicsModel> Step1Drafts = new();
    private static readonly ConcurrentDictionary<string, Step2ClinicalModel> Step2Drafts = new();
    private static readonly ConcurrentDictionary<string, Step3FunctionalModel> Step3Drafts = new();
    private static readonly ConcurrentDictionary<string, Step4ReviewModel> Step4Drafts = new();

    private static string EnsureScreeningId(string? id) =>
        string.IsNullOrEmpty(id) ? $"SCR-{DateTime.UtcNow:yyyyMMddHHmmssffff}" : id;

    [HttpGet("")]
    public IActionResult Index([FromQuery] string? screeningId)
    {
        var id = screeningId ?? "";

        Step1Drafts.TryGetValue(id, out var step1);
        Step2Drafts.TryGetValue(id, out var step2);
        Step3Drafts.TryGetValue(id, out var step3);

        step1 ??= new Step1DemographicsModel();
        step2 ??= new Step2ClinicalModel();
        step3 ??= new Step3FunctionalModel();

        // Later step plans read Step 1 values through hidden fields in the Active Plan.
        step2.PrimaryDiagnosis = step1.PrimaryDiagnosis;
        step2.ResidentName = step1.ResidentName;
        step3.Age = step1.Age;
        step3.ResidentName = step1.ResidentName;

        var step4 = new Step4ReviewModel
        {
            ScreeningId = id,
            RiskTier = step1.RiskTier,
            CareUnit = step2.CareUnit,
            MonitoringLevel = step3.MonitoringLevel,
            Step1Saved = Step1Drafts.ContainsKey(id),
            Step2Saved = Step2Drafts.ContainsKey(id),
            Step3Saved = Step3Drafts.ContainsKey(id),
        };
        if (Step4Drafts.TryGetValue(id, out var step4Draft))
            step4.EmergencyContact = step4Draft.EmergencyContact;

        ViewBag.Diagnoses = new[] { "Alzheimer's", "Parkinson's", "Heart Disease", "Diabetes", "Stroke", "Other" };
        ViewBag.FallOptions = new[] { "None", "1-2 falls", "3+ falls" };
        ViewBag.MobilityAids = new[] { "None", "Cane", "Walker", "Wheelchair" };
        ViewBag.WanderFreqs = new[] { "Rarely", "Sometimes", "Frequently" };
        ViewBag.InsulinSchedules = new[] { "Morning", "Evening", "Both", "As Needed" };
        ViewBag.InjuryTypes = new[] { "Bruise", "Fracture", "Head Injury", "Other" };
        ViewBag.DiabetesTypes = new[] { "Type 1", "Type 2" };
        ViewBag.ServiceBranches = new[] { "Army", "Navy", "Air Force", "Marines", "Coast Guard" };
        ViewBag.ScreeningId = id;

        var model = new WizardShellModel
        {
            ScreeningId = id,
            Step1 = step1,
            Step2 = step2,
            Step3 = step3,
            Step4 = step4,
        };

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionAssessment/Index.cshtml", model);
    }

    [HttpPost("SaveStep1")]
    public async Task<IActionResult> SaveStep1([FromBody] Step1DemographicsModel model)
    {
        await Task.Delay(200);
        var id = EnsureScreeningId(null);
        Step1Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = "Step 1 saved" });
    }

    [HttpPost("SaveStep2")]
    public async Task<IActionResult> SaveStep2([FromBody] Step2ClinicalModel model)
    {
        await Task.Delay(200);

        var ts = DateTime.UtcNow.ToString("yyyyMMddHHmmss");
        if (model.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's" && model.CognitiveScore > 0)
            model.CognitiveAssessmentId = $"COG-{ts}";
        if (model.PrimaryDiagnosis == "Heart Disease" && model.SystolicBP > 0)
            model.CardiacAssessmentId = $"CAR-{ts}";
        if (model.PrimaryDiagnosis == "Diabetes" && !string.IsNullOrEmpty(model.DiabetesType))
            model.DiabetesAssessmentId = $"DIA-{ts}";

        var id = model.ScreeningId;
        if (!string.IsNullOrEmpty(id))
            Step2Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = "Step 2 saved" });
    }

    [HttpPost("SaveStep3")]
    public async Task<IActionResult> SaveStep3([FromBody] Step3FunctionalModel model)
    {
        await Task.Delay(200);
        var id = model.ScreeningId;
        if (!string.IsNullOrEmpty(id))
            Step3Drafts[id] = model;
        return Ok(new SaveStepResponse { ScreeningId = id, Message = "Step 3 saved" });
    }

    [HttpGet("SearchPhysicians")]
    public async Task<IActionResult> SearchPhysicians([FromQuery] string? q)
    {
        await Task.Delay(300);

        var physicians = new List<AssessmentPhysicianItem>
        {
            new() { Text = "Dr. Sarah Chen", Value = "Dr. Sarah Chen", Specialty = "Geriatrics" },
            new() { Text = "Dr. James Wilson", Value = "Dr. James Wilson", Specialty = "Cardiology" },
            new() { Text = "Dr. Emily Park", Value = "Dr. Emily Park", Specialty = "Neurology" },
            new() { Text = "Dr. Michael Torres", Value = "Dr. Michael Torres", Specialty = "Internal Medicine" },
            new() { Text = "Dr. Lisa Wang", Value = "Dr. Lisa Wang", Specialty = "Endocrinology" },
            new() { Text = "Dr. Robert Kim", Value = "Dr. Robert Kim", Specialty = "Pain Management" },
        };

        var filtered = string.IsNullOrEmpty(q)
            ? physicians
            : physicians.Where(p => p.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(new PhysicianSearchResponse { Physicians = filtered });
    }

    [HttpPost("VerifyVeteran")]
    public async Task<IActionResult> VerifyVeteran([FromBody] VerifyVeteranRequest? request)
    {
        await Task.Delay(600);

        var eligible = request?.VaId?.StartsWith("VA-", StringComparison.OrdinalIgnoreCase) == true;

        return Ok(new VerifyVaResponse
        {
            Message = eligible
                ? $"VA eligibility confirmed for {request?.VaId}"
                : $"VA ID '{request?.VaId}' not recognized — must start with VA-",
            Eligible = eligible
        });
    }

    [HttpPost("AlertElopement")]
    public async Task<IActionResult> AlertElopement([FromBody] AlertElopementRequest? request)
    {
        await Task.Delay(400);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Elopement risk flagged for {request?.ResidentName}",
            Urgency = "high"
        });
    }

    [HttpPost("AlertHypertension")]
    public async Task<IActionResult> AlertHypertension([FromBody] AlertHypertensionRequest? request)
    {
        await Task.Delay(400);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Hypertension flagged: {request?.SystolicBP}mmHg — cardiology referral",
            Urgency = "moderate"
        });
    }

    [HttpPost("AlertUncontrolled")]
    public async Task<IActionResult> AlertUncontrolled([FromBody] AlertUncontrolledRequest? request)
    {
        await Task.Delay(400);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Uncontrolled {request?.DiabetesType} diabetes: A1C {request?.A1cLevel} — endocrinology referral",
            Urgency = "high"
        });
    }

    [HttpPost("AlertNeuro")]
    public async Task<IActionResult> AlertNeuro([FromBody] AlertNeuroRequest? request)
    {
        await Task.Delay(400);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Neuro consult ordered for {request?.ResidentName} — {request?.InjuryType}",
            Urgency = "immediate"
        });
    }

    [HttpPost("AlertPain")]
    public async Task<IActionResult> AlertPain([FromBody] AlertPainRequest? request)
    {
        await Task.Delay(400);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Pain management required: level {request?.PainLevel} at {request?.PainLocation}",
            Urgency = "immediate"
        });
    }

    [HttpPost("RequestRoomSetup")]
    public async Task<IActionResult> RequestRoomSetup([FromBody] RequestRoomSetupRequest? request)
    {
        await Task.Delay(500);
        return Ok(new ScreeningAlertResponse
        {
            Message = $"Accessible room scheduled for {request?.ResidentName}",
            Urgency = "routine"
        });
    }

    [HttpPost("Submit")]
    public async Task<IActionResult> Submit([FromBody] Step4ReviewModel model)
    {
        await Task.Delay(1000);

        var errors = new Dictionary<string, string[]>();
        var id = model.ScreeningId;

        CollectErrors(new Step4Validator().Validate(model), errors);

        if (string.IsNullOrEmpty(id))
            return BadRequest(new { errors });

        if (!Step1Drafts.TryGetValue(id, out var step1))
        {
            errors["Step1"] = ["Step 1 (Demographics) must be saved before submission"];
        }
        else
        {
            CollectErrors(new Step1Validator().Validate(step1), errors);
        }

        Step2Drafts.TryGetValue(id, out var step2);
        if (step2 != null)
            CollectErrors(new Step2Validator().Validate(step2), errors);

        if (step1 != null)
        {
            if (step1.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's"
                && string.IsNullOrEmpty(step2?.CognitiveAssessmentId))
                errors["CognitiveAssessmentId"] = ["Cognitive assessment must be saved"];

            if (step1.PrimaryDiagnosis == "Heart Disease"
                && string.IsNullOrEmpty(step2?.CardiacAssessmentId))
                errors["CardiacAssessmentId"] = ["Cardiac assessment must be saved"];

            if (step1.PrimaryDiagnosis == "Diabetes"
                && string.IsNullOrEmpty(step2?.DiabetesAssessmentId))
                errors["DiabetesAssessmentId"] = ["Diabetes assessment must be saved"];
        }

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
            Alerts = model.AlertsSent
        });
    }

    private static void CollectErrors(FluentValidation.Results.ValidationResult result,
        Dictionary<string, string[]> errors)
    {
        foreach (var failure in result.Errors)
        {
            var key = failure.PropertyName;
            if (!errors.ContainsKey(key))
                errors[key] = [failure.ErrorMessage];
            else
                errors[key] = [..errors[key], failure.ErrorMessage];
        }
    }

}
