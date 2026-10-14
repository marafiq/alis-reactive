using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Family contacts: the front desk edits each resident's family contact in the grid and saves the
    /// edits together. Every page starts from the contacts on file; a save answers with the contacts as
    /// edited and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        [HttpGet("FamilyContacts")]
        public IActionResult FamilyContacts() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/FamilyContacts.cshtml",
                new FamilyContactsViewModel { Contacts = FamilyContactsOnFile() });

        [HttpPost("FamilyContactsSave")]
        public IActionResult FamilyContactsSave([FromBody] FamilyContactsSaveRequest? request)
        {
            var edited = request?.BatchChanges?.ChangedRecords ?? new List<FamilyContact>();

            var validator = new FamilyContactValidator();
            foreach (var contact in edited)
            {
                var result = validator.Validate(contact);
                if (!result.IsValid)
                    return BadRequest(new FamilyContactsSaved { Summary = $"{contact.ResidentName}: {result.Errors[0].ErrorMessage}" });
            }

            var byResident = edited.ToDictionary(contact => contact.ResidentId);
            var contacts = FamilyContactsOnFile()
                .Select(contact => byResident.TryGetValue(contact.ResidentId, out var edit) ? edit : contact)
                .ToList();
            return Ok(new FamilyContactsSaved
            {
                Contacts = contacts,
                Summary = edited.Count == 1 ? "Saved 1 family contact." : $"Saved {edited.Count} family contacts."
            });
        }

        private static List<FamilyContact> FamilyContactsOnFile() => new()
        {
            new FamilyContact { ResidentId = 7001, ResidentName = "Helen Park", ContactName = "Daniel O'Brien", Phone = "555-0142", Email = "daniel.obrien@example.com", PortalLink = "https://family.maplegrove.example/park", VisitsPerWeek = 3 },
            new FamilyContact { ResidentId = 7002, ResidentName = "Arthur Bell", ContactName = "Susan Bell", Phone = "555-0187", Email = "susan.bell@example.com", PortalLink = "https://family.maplegrove.example/bell", VisitsPerWeek = 1 },
            new FamilyContact { ResidentId = 7003, ResidentName = "Rosa Diaz", ContactName = "Marisol Diaz", Phone = "555-0119", Email = "marisol.diaz@example.com", PortalLink = "https://family.maplegrove.example/diaz", VisitsPerWeek = 5 }
        };
    }
}
