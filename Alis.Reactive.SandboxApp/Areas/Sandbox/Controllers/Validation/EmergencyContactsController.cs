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
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new EmergencyContactsConfirmation(ConfirmationFor(intake)));
        }

        /// <summary>Saves the contacts alone: each contact is checked, the rest of the intake is not.</summary>
        [HttpPost("SaveContacts")]
        public IActionResult SaveContacts([FromBody] EmergencyContactsModel? intake)
        {
            if (intake == null) return BadRequest();

            var contacts = intake.HasContacts ? intake.Contacts : new List<EmergencyContact>();
            var contactValidator = new EmergencyContactValidator();
            var errors = contacts
                .SelectMany((contact, index) => contactValidator.Validate(contact).Errors
                    .Select(error => (Name: $"Contacts[{index}].{error.PropertyName}", error.ErrorMessage)))
                .GroupBy(error => error.Name)
                .ToDictionary(group => group.Key, group => group.Select(error => error.ErrorMessage).ToArray());
            if (errors.Count > 0) return BadRequest(new { errors });

            return Ok(new EmergencyContactsConfirmation($"Emergency contacts saved: {contacts.Count}."));
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
