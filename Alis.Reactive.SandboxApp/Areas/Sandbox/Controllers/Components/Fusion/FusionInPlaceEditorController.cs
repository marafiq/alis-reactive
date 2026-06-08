using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionInPlaceEditor")]
    public class FusionInPlaceEditorController : Controller
    {
        private static readonly DateTime ServerErrorTrigger = new DateTime(1900, 1, 1);

        private static List<InPlaceEditorCareLevelOption> CareLevelsData() => new()
        {
            new InPlaceEditorCareLevelOption { Id = "independent",  Label = "Independent Living" },
            new InPlaceEditorCareLevelOption { Id = "assisted",     Label = "Assisted Living" },
            new InPlaceEditorCareLevelOption { Id = "memory-care",  Label = "Memory Care" },
            new InPlaceEditorCareLevelOption { Id = "skilled",      Label = "Skilled Nursing" }
        };

        private static List<AllergyOption> AllergiesData() => new()
        {
            new AllergyOption { Code = "none",       Name = "None" },
            new AllergyOption { Code = "penicillin", Name = "Penicillin" },
            new AllergyOption { Code = "latex",      Name = "Latex" },
            new AllergyOption { Code = "peanuts",    Name = "Peanuts" }
        };

        [HttpGet("")]
        public IActionResult Index()
        {
            ViewBag.CareLevels = CareLevelsData();
            ViewBag.Allergies  = AllergiesData();

            var profile = new ResidentProfile
            {
                Name          = "Eleanor Vance",
                CareLevelId   = "assisted",
                AdmissionDate = DateTime.Today.AddYears(-2),
                MonthlyRate   = 4500m,
                Nickname      = "Ellie",
                DateOfBirth   = new DateTime(1942, 6, 14),
                Allergies     = "penicillin"
            };

            return View("~/Areas/Sandbox/Views/Components/Fusion/FusionInPlaceEditor/Index.cshtml", profile);
        }

        [HttpGet("CareLevels")]
        public IActionResult GetCareLevels() => Ok(CareLevelsData());

        [HttpGet("Allergies")]
        public IActionResult GetAllergies() => Ok(AllergiesData());

        [HttpPost("UpdateProfile")]
        public IActionResult UpdateProfile([FromBody] ResidentProfile profile)
        {
            if (profile == null) return BadRequest(new InPlaceEditorCommitError { Message = "Profile required." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = profile.Name ?? "",
                Saved        = true
            });
        }

        [HttpPost("UpdateDateOfBirth")]
        public IActionResult UpdateDateOfBirth([FromBody] DateOfBirthQuickEdit body)
        {
            if (body?.Value == ServerErrorTrigger)
                return StatusCode(500, new InPlaceEditorCommitError { Message = "Server refused that date of birth." });
            if (body?.Value == null)
                return BadRequest(new InPlaceEditorCommitError { Message = "Date of birth required." });
            if (string.IsNullOrWhiteSpace(body.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{body.Value.Value:yyyy-MM-dd} (resident={body.ResidentId})",
                Saved = true
            });
        }

        [HttpPost("UpdateCareLevel")]
        public IActionResult UpdateCareLevel([FromBody] CareLevelQuickEdit body)
        {
            if (string.IsNullOrWhiteSpace(body?.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            var match = CareLevelsData().Find(c => c.Id == body.Value);
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{match?.Label ?? body.Value ?? ""} (resident={body.ResidentId})",
                Saved        = match != null
            });
        }

        [HttpPost("UpdateMonthlyRate")]
        public IActionResult UpdateMonthlyRate([FromBody] MonthlyRateQuickEdit? body)
        {
            if (body == null)
                return BadRequest(new { errors = new Dictionary<string, string[]> { ["Value"] = new[] { "Request body is required." } } });

            // Same FluentValidator used client-side. Server runs it; on failure returns the framework's standard
            // { errors: { fieldName: [msg] } } shape so .OnError(400, e => e.ValidationErrors(formId)) on the
            // client writes each message into its per-field validation slot. No custom rules, no Syncfusion channel.
            var result = new MonthlyRateQuickEditValidator().Validate(body);
            if (!result.IsValid)
            {
                var errors = new Dictionary<string, string[]>();
                foreach (var failure in result.Errors)
                {
                    if (errors.TryGetValue(failure.PropertyName, out var existing))
                    {
                        var extended = new string[existing.Length + 1];
                        existing.CopyTo(extended, 0);
                        extended[existing.Length] = failure.ErrorMessage;
                        errors[failure.PropertyName] = extended;
                    }
                    else
                    {
                        errors[failure.PropertyName] = new[] { failure.ErrorMessage };
                    }
                }
                return BadRequest(new { errors });
            }

            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{body.Value:C} (resident={body.ResidentId})",
                Saved = true
            });
        }

        [HttpPost("UpdateNickname")]
        public IActionResult UpdateNickname([FromBody] NicknameQuickEdit body)
        {
            if (string.Equals(body?.Value, "boom", StringComparison.OrdinalIgnoreCase))
                return StatusCode(500, new InPlaceEditorCommitError { Message = "Nickname rejected by server." });
            if (string.IsNullOrWhiteSpace(body?.Value))
                return BadRequest(new InPlaceEditorCommitError { Message = "Nickname required." });
            if (string.IsNullOrWhiteSpace(body.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{body.Value} (resident={body.ResidentId})",
                Saved = true
            });
        }

        [HttpPost("UpdateAllergies")]
        public IActionResult UpdateAllergies([FromBody] AllergiesQuickEdit body)
        {
            if (body?.Value == null || body.Value.Length == 0)
                return BadRequest(new InPlaceEditorCommitError { Message = "Select at least one allergy." });
            if (string.IsNullOrWhiteSpace(body.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{string.Join(", ", body.Value)} (resident={body.ResidentId})",
                Saved = true
            });
        }

        [HttpPost("UpdateLastAdmission")]
        public IActionResult UpdateLastAdmission([FromBody] LastAdmissionQuickEdit body)
        {
            if (body?.Value == null)
                return BadRequest(new InPlaceEditorCommitError { Message = "Admission timestamp required." });
            if (string.IsNullOrWhiteSpace(body.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{body.Value.Value:yyyy-MM-dd HH:mm} (resident={body.ResidentId})",
                Saved = true
            });
        }

        [HttpPost("UpdateMedicalRecordNumber")]
        public IActionResult UpdateMedicalRecordNumber([FromBody] MedicalRecordNumberQuickEdit body)
        {
            if (string.IsNullOrWhiteSpace(body?.Value))
                return BadRequest(new InPlaceEditorCommitError { Message = "Medical record number required." });
            if (string.IsNullOrWhiteSpace(body.ResidentId))
                return BadRequest(new InPlaceEditorCommitError { Message = "ResidentId missing from commit payload." });
            return Ok(new InPlaceEditorUpdateResponse
            {
                DisplayValue = $"{body.Value} (resident={body.ResidentId})",
                Saved = true
            });
        }
    }
}
