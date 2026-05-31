using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Fusion.Stepper;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Concurrent;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/FusionStepper")]
    public class StepperController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Fusion/Stepper/";

        private static readonly ConcurrentDictionary<string, FusionStepperWizardIntakeModel> IntakeDrafts = new();
        private static readonly ConcurrentDictionary<string, FusionStepperWizardCareModel> CareDrafts = new();
        private static readonly ConcurrentDictionary<string, FusionStepperWizardContactModel> ContactDrafts = new();
        private static readonly ConcurrentDictionary<string, FusionStepperWizardReviewModel> ReviewDrafts = new();

        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            return View(
                ViewBase + "Index.cshtml",
                new FusionStepperModel());
        }

        [HttpGet("Wizard")]
        public IActionResult Wizard()
        {
            return View(
                ViewBase + "Wizard.cshtml",
                new FusionStepperWizardShellModel { WizardId = NewWizardId() });
        }

        [HttpPost("Wizard/LoadStep")]
        public IActionResult LoadWizardStep([FromBody] FusionStepperWizardLoadStepRequest request)
        {
            var wizardId = request.WizardId ?? NewWizardId();

            return request.Step switch
            {
                1 => StepPartial("_WizardIntakeStep.cshtml", GetDraft(IntakeDrafts, wizardId) with { WizardId = wizardId }),
                2 => StepPartial("_WizardCareStep.cshtml", BuildCareStep(wizardId)),
                3 => StepPartial("_WizardContactStep.cshtml", GetDraft(ContactDrafts, wizardId) with { WizardId = wizardId }),
                4 => StepPartial("_WizardReviewStep.cshtml", BuildReviewStep(wizardId)),
                _ => BadRequest("Invalid wizard step")
            };
        }

        [HttpPost("Wizard/SaveIntake")]
        public IActionResult SaveIntake([FromBody] FusionStepperWizardIntakeModel model)
        {
            model = model with { WizardId = EnsureWizardId(model.WizardId) };
            if (!TryValidate(new FusionStepperWizardIntakeValidator(), model, out var error)) return error;

            IntakeDrafts[model.WizardId] = model;
            return Ok(new FusionStepperWizardSaveResponse
            {
                WizardId = model.WizardId,
                Message = $"Intake saved for {model.ResidentName}"
            });
        }

        [HttpPost("Wizard/SaveCare")]
        public IActionResult SaveCare([FromBody] FusionStepperWizardCareModel model)
        {
            model = model with { WizardId = EnsureWizardId(model.WizardId) };
            if (!TryValidate(new FusionStepperWizardCareValidator(), model, out var error)) return error;

            CareDrafts[model.WizardId] = model;
            return Ok(new FusionStepperWizardSaveResponse
            {
                WizardId = model.WizardId,
                Message = $"Care plan saved for {model.CareLevel}"
            });
        }

        [HttpPost("Wizard/SaveContact")]
        public IActionResult SaveContact([FromBody] FusionStepperWizardContactModel model)
        {
            model = model with { WizardId = EnsureWizardId(model.WizardId) };
            if (!TryValidate(new FusionStepperWizardContactValidator(), model, out var error)) return error;

            ContactDrafts[model.WizardId] = model;
            return Ok(new FusionStepperWizardSaveResponse
            {
                WizardId = model.WizardId,
                Message = $"Contact saved for {model.ResponsibleParty}"
            });
        }

        [HttpPost("Wizard/Submit")]
        public IActionResult SubmitWizard([FromBody] FusionStepperWizardReviewModel model)
        {
            model = model with { WizardId = EnsureWizardId(model.WizardId) };

            var errors = new Dictionary<string, string[]>();
            CollectErrors(new FusionStepperWizardReviewValidator().Validate(model), errors);

            if (!IntakeDrafts.TryGetValue(model.WizardId, out var intake))
                errors["ResidentName"] = ["Complete intake before submitting."];

            if (!CareDrafts.TryGetValue(model.WizardId, out var care))
                errors["CareLevel"] = ["Complete care planning before submitting."];

            if (!ContactDrafts.TryGetValue(model.WizardId, out var contact))
                errors["ResponsibleParty"] = ["Complete contacts before submitting."];

            if (errors.Count > 0) return BadRequest(new { errors });

            ReviewDrafts[model.WizardId] = model;
            return Ok(new FusionStepperWizardSubmitResponse
            {
                WizardId = model.WizardId,
                Message = $"Admission packet submitted for {intake!.ResidentName}",
                CareSummary = $"{care!.CareLevel} / {care.PrimaryDiagnosis}",
                ContactSummary = $"{contact!.ResponsibleParty} ({contact.Phone})"
            });
        }

        private static string NewWizardId() => $"WIZ-{DateTime.UtcNow:yyyyMMddHHmmssffff}";

        private static string EnsureWizardId(string? wizardId) =>
            string.IsNullOrWhiteSpace(wizardId) ? NewWizardId() : wizardId;

        private static T GetDraft<T>(ConcurrentDictionary<string, T> store, string wizardId) where T : new() =>
            !string.IsNullOrEmpty(wizardId) && store.TryGetValue(wizardId, out var draft) ? draft : new T();

        private static IActionResult ValidationError(Dictionary<string, string[]> errors) =>
            new BadRequestObjectResult(new { errors });

        private IActionResult StepPartial<T>(string view, T model) =>
            PartialView(ViewBase + view, model);

        private FusionStepperWizardCareModel BuildCareStep(string wizardId)
        {
            var model = GetDraft(CareDrafts, wizardId) with { WizardId = wizardId };
            if (IntakeDrafts.TryGetValue(wizardId, out var intake) && string.IsNullOrWhiteSpace(model.ResidentName))
                model = model with { ResidentName = intake.ResidentName };
            return model;
        }

        private FusionStepperWizardReviewModel BuildReviewStep(string wizardId)
        {
            var intake = GetDraft(IntakeDrafts, wizardId);
            var care = GetDraft(CareDrafts, wizardId);
            var contact = GetDraft(ContactDrafts, wizardId);
            var review = GetDraft(ReviewDrafts, wizardId);

            return review with
            {
                WizardId = wizardId,
                ResidentName = intake.ResidentName,
                AdmissionType = intake.AdmissionType,
                CareLevel = care.CareLevel,
                PrimaryDiagnosis = care.PrimaryDiagnosis,
                ResponsibleParty = contact.ResponsibleParty,
                Phone = contact.Phone
            };
        }

        private static bool TryValidate<T>(IValidator<T> validator, T model, out IActionResult error)
        {
            var result = validator.Validate(model);
            if (result.IsValid)
            {
                error = null!;
                return true;
            }

            var errors = new Dictionary<string, string[]>();
            CollectErrors(result, errors);
            error = ValidationError(errors);
            return false;
        }

        private static void CollectErrors(FluentValidation.Results.ValidationResult result, Dictionary<string, string[]> errors)
        {
            foreach (var failure in result.Errors)
            {
                if (!errors.TryGetValue(failure.PropertyName, out var existing))
                    errors[failure.PropertyName] = [failure.ErrorMessage];
                else
                    errors[failure.PropertyName] = [.. existing, failure.ErrorMessage];
            }
        }
    }
}
