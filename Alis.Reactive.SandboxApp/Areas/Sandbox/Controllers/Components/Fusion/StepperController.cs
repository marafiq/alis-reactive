using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionStepper")]
    public class StepperController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Fusion/Stepper/";

        private AdmissionDrafts Drafts => new(HttpContext.Session);

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                ViewBase + "Index.cshtml",
                new FusionStepperModel());
        }

        /// <summary>A new admission; its steps load one at a time from <see cref="LoadWizardStep"/>.</summary>
        [HttpGet("Wizard")]
        public IActionResult Wizard() =>
            View(ViewBase + "Wizard.cshtml", new AdmissionWizardModel { AdmissionId = Guid.NewGuid().ToString("N") });

        /// <summary>One step of the admission, holding what was last saved for it.</summary>
        [HttpPost("Wizard/LoadStep")]
        public IActionResult LoadWizardStep([FromBody] AdmissionStepRequest request)
        {
            var id = request.AdmissionId;
            if (id.Length == 0) return BadRequest();

            var drafts = Drafts;
            return request.Step switch
            {
                0 => StepPartial("_WizardIntakeStep.cshtml", drafts.Load<AdmissionIntake>(id) with { AdmissionId = id }),
                1 => StepPartial("_WizardCareStep.cshtml", drafts.Load<AdmissionCare>(id) with
                {
                    AdmissionId = id,
                    ResidentName = drafts.Load<AdmissionIntake>(id).ResidentName ?? ""
                }),
                2 => StepPartial("_WizardContactStep.cshtml", drafts.Load<AdmissionContacts>(id) with { AdmissionId = id }),
                3 => StepPartial("_WizardReviewStep.cshtml", Review(drafts, id)),
                _ => BadRequest()
            };
        }

        [HttpPost("Wizard/SaveIntake")]
        public IActionResult SaveIntake([FromBody] AdmissionIntake intake)
        {
            var result = new AdmissionIntakeValidator().Validate(intake);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            Drafts.Save(intake.AdmissionId, intake);
            return Ok(new AdmissionStepSaved { Message = $"Saved {intake.ResidentName}'s intake." });
        }

        [HttpPost("Wizard/SaveCare")]
        public IActionResult SaveCare([FromBody] AdmissionCare care)
        {
            var result = new AdmissionCareValidator().Validate(care);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var drafts = Drafts;
            drafts.Save(care.AdmissionId, care);
            var resident = drafts.Load<AdmissionIntake>(care.AdmissionId).ResidentName;
            return Ok(new AdmissionStepSaved { Message = $"Saved {resident}'s care plan: {care.CareLevel}." });
        }

        [HttpPost("Wizard/SaveContact")]
        public IActionResult SaveContact([FromBody] AdmissionContacts contacts)
        {
            var drafts = Drafts;
            var resident = drafts.Load<AdmissionIntake>(contacts.AdmissionId).ResidentName;
            var errors = new Dictionary<string, string[]>(new AdmissionContactsValidator().Validate(contacts).ErrorsByField);

            // The responsible party is someone other than the resident the intake names.
            if (resident is not null && string.Equals(contacts.ResponsibleParty, resident, StringComparison.OrdinalIgnoreCase))
                errors[nameof(AdmissionContacts.ResponsibleParty)] = [$"{resident} is the resident. Name someone else as the responsible party."];

            if (errors.Count > 0) return BadRequest(new { errors });

            drafts.Save(contacts.AdmissionId, contacts);
            return Ok(new AdmissionStepSaved { Message = $"Saved {contacts.ResponsibleParty} as {resident}'s responsible party." });
        }

        [HttpPost("Wizard/Submit")]
        public IActionResult SubmitWizard([FromBody] AdmissionReview review)
        {
            var result = new AdmissionReviewValidator().Validate(review);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            var admission = Review(Drafts, review.AdmissionId);
            return Ok(new AdmissionStepSaved
            {
                Message = $"Submitted {admission.ResidentName}'s admission for {admission.CareLevel}. {review.AdmissionCoordinator} is the coordinator."
            });
        }

        private IActionResult StepPartial<T>(string view, T model) =>
            PartialView(ViewBase + view, model);

        private static AdmissionReview Review(AdmissionDrafts drafts, string admissionId)
        {
            var intake = drafts.Load<AdmissionIntake>(admissionId);
            var care = drafts.Load<AdmissionCare>(admissionId);
            var contacts = drafts.Load<AdmissionContacts>(admissionId);

            return new AdmissionReview
            {
                AdmissionId = admissionId,
                ResidentName = intake.ResidentName ?? "",
                AdmissionType = intake.AdmissionType ?? "",
                CareLevel = care.CareLevel ?? "",
                PrimaryDiagnosis = care.PrimaryDiagnosis ?? "",
                ResponsibleParty = contacts.ResponsibleParty ?? "",
                Phone = contacts.Phone ?? ""
            };
        }
    }
}
