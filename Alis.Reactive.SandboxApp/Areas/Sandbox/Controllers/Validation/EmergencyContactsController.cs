using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Resident intake: emergency contacts are recorded only when the resident has them, and then
    /// every contact needs a phone number.
    /// </summary>
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
            {
                var errors = result.Errors
                    .GroupBy(error => error.PropertyName)
                    .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
                return BadRequest(new { errors });
            }

            return Ok(new EmergencyContactsConfirmation(ConfirmationFor(intake)));
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
