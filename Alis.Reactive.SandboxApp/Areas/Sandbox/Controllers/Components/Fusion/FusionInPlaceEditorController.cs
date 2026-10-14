using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.InPlaceEditor;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionInPlaceEditor")]
    public class FusionInPlaceEditorController : Controller
    {
        private const string ResidentId = "resident-42";

        // Another Maple Grove resident's medical record number: a face sheet cannot take it.
        private const string WalterGrantsRecordNumber = "WGT4321";

        private static readonly CultureInfo English = CultureInfo.GetCultureInfo("en-US");

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.ResidentId = ResidentId;
            return View("~/Areas/Sandbox/Views/Components/Fusion/FusionInPlaceEditor/Index.cshtml", new ResidentProfileModel
            {
                Name = "Eleanor Vance",
                AdmissionDate = DateTime.Today.AddMonths(-1),
                PreferredName = "Ellie"
            });
        }

        [HttpPost("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] ResidentProfileModel profile)
        {
            var result = new ResidentProfileModelValidator().Validate(profile);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var preferredName = string.IsNullOrWhiteSpace(profile.PreferredName)
                ? "She has no preferred name."
                : $"Preferred name: {profile.PreferredName}.";
            return Ok(new ProfileSaved { Message = $"Saved {profile.Name}'s profile. {preferredName}" });
        }

        [HttpPost("UpdateDateOfBirth")]
        public IActionResult UpdateDateOfBirth([FromBody] DateOfBirthCorrection correction)
        {
            var result = new DateOfBirthCorrectionValidator().Validate(correction);
            if (!result.IsValid || correction.ResidentId != ResidentId)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new CorrectionSaved
            {
                Message = $"Date of birth saved: {correction.Value!.Value.ToString("MMMM d, yyyy", English)}."
            });
        }

        [HttpPost("UpdateCareLevel")]
        public IActionResult UpdateCareLevel([FromBody] CareLevelCorrection correction)
        {
            var result = new CareLevelCorrectionValidator().Validate(correction);
            var level = CareLevelChoice.All.FirstOrDefault(choice => choice.Name == correction.Value);
            if (!result.IsValid || level == null || correction.ResidentId != ResidentId)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new CorrectionSaved
            {
                Message = $"Care level saved: {level.Name}, {level.MonthlyRate.ToString("C0", English)} a month."
            });
        }

        // The editor's own commit data carries the new rate as text under "value".
        [HttpPost("UpdateMonthlyRate")]
        public IActionResult UpdateMonthlyRate([FromBody] MonthlyRateChange change)
        {
            var sent = change.Change != null && change.Change.TryGetValue("value", out var value) ? value?.ToString() : null;
            var rate = decimal.TryParse(sent, NumberStyles.Number, CultureInfo.InvariantCulture, out var parsed) ? parsed : 0m;
            var result = new MonthlyRateCorrectionValidator().Validate(new MonthlyRateCorrection
            {
                ResidentId = change.ResidentId,
                Value = rate,
                CustomRate = true
            });
            if (!result.IsValid || change.ResidentId != ResidentId)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new MonthlyRateSaved
            {
                Rate = rate.ToString(CultureInfo.InvariantCulture),
                Message = $"Monthly rate saved: {rate.ToString("C0", English)}."
            });
        }

        [HttpPost("UpdateLastHospitalStay")]
        public IActionResult UpdateLastHospitalStay([FromBody] LastHospitalStayCorrection correction)
        {
            var result = new LastHospitalStayCorrectionValidator().Validate(correction);
            if (!result.IsValid || correction.ResidentId != ResidentId)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new CorrectionSaved
            {
                Message = $"Last hospital stay saved: {correction.Value!.Value.ToString("MMMM d, yyyy 'at' h:mm tt", English)}."
            });
        }

        [HttpPost("UpdateMedicalRecordNumber")]
        public IActionResult UpdateMedicalRecordNumber([FromBody] MedicalRecordNumberCorrection correction)
        {
            var result = new MedicalRecordNumberCorrectionValidator().Validate(correction);
            if (!result.IsValid || correction.ResidentId != ResidentId)
                return BadRequest(new { errors = result.ErrorsByField });
            if (correction.Value == WalterGrantsRecordNumber)
                return Conflict(new CorrectionRefused { Message = $"{WalterGrantsRecordNumber} is Walter Grant's medical record number." });

            return Ok(new CorrectionSaved { Message = $"Medical record number saved: {correction.Value}." });
        }
    }
}
