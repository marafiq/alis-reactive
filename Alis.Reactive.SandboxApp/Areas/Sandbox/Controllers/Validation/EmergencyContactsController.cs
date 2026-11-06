using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    [Area("Sandbox")]
    [Route("Sandbox/Validation/EmergencyContacts")]
    public class EmergencyContactsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var intake = new EmergencyContactsModel { Contacts = { new EmergencyContact(), new EmergencyContact() } };
            return View("~/Areas/Sandbox/Views/Validation/EmergencyContacts/Index.cshtml", intake);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] EmergencyContactsModel? intake)
        {
            if (intake == null) return BadRequest();

            var result = new EmergencyContactsValidator().Validate(intake);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new EmergencyContactsConfirmation(ConfirmationFor(intake)));
        }

        [HttpPost("SaveContacts")]
        public IActionResult SaveContacts([FromBody] EmergencyContactsSave? save)
        {
            if (save == null) return BadRequest();

            var result = new EmergencyContactsSaveValidator().Validate(save);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            var saved = save.HasContacts ? save.Contacts.Count : 0;
            return Ok(new EmergencyContactsConfirmation($"Emergency contacts saved: {saved}."));
        }

        private static string ConfirmationFor(EmergencyContactsModel intake)
        {
            var contacts = intake.HasContacts
                ? intake.Contacts.Where(contact => !string.IsNullOrWhiteSpace(contact.Phone)).ToList()
                : new List<EmergencyContact>();

            return contacts.Count == 0
                ? $"Intake saved for {intake.ResidentName} with no emergency contacts."
                : $"Intake saved for {intake.ResidentName} with {contacts.Count} emergency contact(s).";
        }
    }
}
