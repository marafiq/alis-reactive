using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Conditions;

[Area("Sandbox")]
[Route("Sandbox/Conditions/AdmissionAssessment")]
public class AdmissionAssessmentController : Controller
{
    // ── GET Index ──────────────────────────────────────────────────────────────

    [HttpGet("")]
    [HttpGet("Index")]
    public IActionResult Index()
    {
        ViewBag.Diagnoses = new[] { "Alzheimer's", "Parkinson's", "Heart Disease", "Diabetes", "Stroke", "Other" };
        ViewBag.FallOptions = new[] { "None", "1-2 falls", "3+ falls" };
        ViewBag.MobilityAids = new[] { "None", "Cane", "Walker", "Wheelchair" };
        ViewBag.WanderFreqs = new[] { "Rarely", "Sometimes", "Frequently" };
        ViewBag.InsulinSchedules = new[] { "Morning", "Evening", "Both", "As Needed" };
        ViewBag.InjuryTypes = new[] { "Bruise", "Fracture", "Head Injury", "Other" };
        ViewBag.DiabetesTypes = new[] { "Type 1", "Type 2" };
        ViewBag.ServiceBranches = new[] { "Army", "Navy", "Air Force", "Marines", "Coast Guard" };

        return View("~/Areas/Sandbox/Views/Conditions/AdmissionAssessment/Index.cshtml", new HealthScreeningModel());
    }

    // ── GET SearchPhysicians ───────────────────────────────────────────────────

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
            new() { Text = "Dr. Lisa Wang", Value = "Dr. Lisa Wang", Specialty = "Endocrinology" },
            new() { Text = "Dr. Robert Kim", Value = "Dr. Robert Kim", Specialty = "Pain Management" },
        };

        var filtered = string.IsNullOrEmpty(q)
            ? physicians
            : physicians.Where(p => p.Text.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();

        return Ok(new PhysicianSearchResponse { Physicians = filtered });
    }

    // ── POST VerifyVeteran ─────────────────────────────────────────────────────

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

    // ── POST Alert endpoints (5) ──────────────────────────────────────────────

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

    // ── POST RequestRoomSetup ─────────────────────────────────────────────────

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

    // ── POST Save section endpoints (3) ───────────────────────────────────────

    [HttpPost("SaveCognitive")]
    public async Task<IActionResult> SaveCognitive([FromBody] HealthScreeningModel model)
    {
        await Task.Delay(800);

        return Ok(new SaveSectionResponse
        {
            Id = $"COG-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Message = "Cognitive assessment saved"
        });
    }

    [HttpPost("SaveCardiac")]
    public async Task<IActionResult> SaveCardiac([FromBody] HealthScreeningModel model)
    {
        await Task.Delay(600);

        return Ok(new SaveSectionResponse
        {
            Id = $"CAR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Message = "Cardiac assessment saved"
        });
    }

    [HttpPost("SaveDiabetes")]
    public async Task<IActionResult> SaveDiabetes([FromBody] HealthScreeningModel model)
    {
        await Task.Delay(700);

        return Ok(new SaveSectionResponse
        {
            Id = $"DIA-{DateTime.UtcNow:yyyyMMddHHmmss}",
            Message = "Diabetes assessment saved"
        });
    }

    // ── POST Submit ───────────────────────────────────────────────────────────

    [HttpPost("Submit")]
    public async Task<IActionResult> Submit([FromBody] HealthScreeningModel model)
    {
        await Task.Delay(1000);

        var errors = new Dictionary<string, string>();

        // Always required
        if (string.IsNullOrEmpty(model.ResidentName)) errors["ResidentName"] = "Resident name is required";
        if (model.Age <= 0) errors["Age"] = "Age is required";
        if (string.IsNullOrEmpty(model.PrimaryDiagnosis)) errors["PrimaryDiagnosis"] = "Primary diagnosis is required";
        if (string.IsNullOrEmpty(model.EmergencyContact)) errors["EmergencyContact"] = "Emergency contact is required";

        // Veteran conditional
        if (model.IsVeteran && string.IsNullOrEmpty(model.VaId))
            errors["VaId"] = "VA ID required for veterans";

        // Cognitive conditional
        if (model.PrimaryDiagnosis is "Alzheimer's" or "Parkinson's")
        {
            if (model.CognitiveScore <= 0) errors["CognitiveScore"] = "Cognitive score required for neurological diagnosis";
            if (string.IsNullOrEmpty(model.CognitiveAssessmentId)) errors["CognitiveAssessmentId"] = "Cognitive assessment must be saved before submission";
            if (model.Wanders && string.IsNullOrEmpty(model.WanderFrequency))
                errors["WanderFrequency"] = "Wander frequency required when wandering reported";
        }

        // Cardiac conditional
        if (model.PrimaryDiagnosis == "Heart Disease")
        {
            if (model.SystolicBP <= 0) errors["SystolicBP"] = "Blood pressure required for cardiac diagnosis";
            if (string.IsNullOrEmpty(model.CardiacAssessmentId)) errors["CardiacAssessmentId"] = "Cardiac assessment must be saved before submission";
            if (model.HasPacemaker && string.IsNullOrEmpty(model.PacemakerModel))
                errors["PacemakerModel"] = "Pacemaker model required when pacemaker reported";
        }

        // Diabetes conditional
        if (model.PrimaryDiagnosis == "Diabetes")
        {
            if (string.IsNullOrEmpty(model.DiabetesType)) errors["DiabetesType"] = "Diabetes type required";
            if (model.A1cLevel <= 0) errors["A1cLevel"] = "A1C level required for diabetes diagnosis";
            if (string.IsNullOrEmpty(model.DiabetesAssessmentId)) errors["DiabetesAssessmentId"] = "Diabetes assessment must be saved before submission";
            if (model.InsulinDependent && string.IsNullOrEmpty(model.InsulinSchedule))
                errors["InsulinSchedule"] = "Insulin schedule required when insulin dependent";
        }

        // Falls conditional
        if (model.FallHistory is "1-2 falls" or "3+ falls")
        {
            if (model.CausedInjury && string.IsNullOrEmpty(model.InjuryType))
                errors["InjuryType"] = "Injury type required when injury reported";
        }

        // Pain conditional
        if (model.TakesPainMedication)
        {
            if (model.PainLevel <= 0) errors["PainLevel"] = "Pain level required when pain medication reported";
            if (model.PainLevel > 7 && string.IsNullOrEmpty(model.PainLocation))
                errors["PainLocation"] = "Pain location required for severe pain (level > 7)";
        }

        if (errors.Count > 0) return BadRequest(new { errors });

        // Compute care plan
        var careUnit = model.PrimaryDiagnosis switch
        {
            "Alzheimer's" or "Parkinson's" when model.CognitiveScore < 15 => "Memory Care",
            "Alzheimer's" or "Parkinson's" when model.CognitiveScore < 25 => "Assisted Living with Memory Support",
            _ => "Standard Assisted Living"
        };

        var monitoring = "Standard";
        if (model.TakesBloodThinners) monitoring = "Enhanced";
        if (model.FallRiskScore >= 7 && model.Age >= 80) monitoring = "Continuous";

        var alerts = new List<string>();
        if (model.Wanders && model.WanderFrequency == "Frequently") alerts.Add("elopementRisk");
        if (model.SystolicBP > 140) alerts.Add("hypertension");
        if (model.A1cLevel > 9) alerts.Add("uncontrolledDiabetes");
        if (model.PainLevel > 7) alerts.Add("painManagement");

        return Ok(new SubmitScreeningResponse
        {
            ScreeningId = $"SCR-{DateTime.UtcNow:yyyyMMddHHmmss}",
            CareUnit = careUnit,
            MonitoringLevel = monitoring,
            Message = $"Assessment complete for {model.ResidentName}",
            Alerts = alerts
        });
    }

    // ── Request DTOs ──────────────────────────────────────────────────────────

    public class VerifyVeteranRequest
    {
        public string VaId { get; set; } = "";
        public string ServiceBranch { get; set; } = "";
    }

    public class AlertElopementRequest
    {
        public string ResidentName { get; set; } = "";
        public string WanderFrequency { get; set; } = "";
    }

    public class AlertHypertensionRequest
    {
        public decimal SystolicBP { get; set; }
        public string ResidentName { get; set; } = "";
    }

    public class AlertUncontrolledRequest
    {
        public decimal A1cLevel { get; set; }
        public string DiabetesType { get; set; } = "";
    }

    public class AlertNeuroRequest
    {
        public string InjuryType { get; set; } = "";
        public string ResidentName { get; set; } = "";
    }

    public class AlertPainRequest
    {
        public decimal PainLevel { get; set; }
        public string PainLocation { get; set; } = "";
    }

    public class RequestRoomSetupRequest
    {
        public string MobilityAid { get; set; } = "";
        public string ResidentName { get; set; } = "";
    }
}
