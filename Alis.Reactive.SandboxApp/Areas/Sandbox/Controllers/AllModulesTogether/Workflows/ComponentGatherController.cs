using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Alis.Reactive.Native.Components;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.AllModulesTogether.Workflows
{
    [Area("Sandbox")]
    [Route("Sandbox/AllModulesTogether/ComponentGather")]
    public class ComponentGatherController : Controller
    {
        [HttpGet("")]
        [HttpGet("Index")]
        public IActionResult Index()
        {
            ViewBag.MobilityOptions = new SelectListItem[]
            {
                new SelectListItem("Ambulatory", "ambulatory"),
                new SelectListItem("Wheelchair", "wheelchair"),
                new SelectListItem("Bedbound", "bedbound"),
            };

            ViewBag.AllergyItems = new RadioButtonItem[]
            {
                new RadioButtonItem("Peanuts", "Peanuts"),
                new RadioButtonItem("Dairy", "Dairy"),
                new RadioButtonItem("Gluten", "Gluten"),
            };

            ViewBag.Facilities = new List<GatherFacilityItem>
            {
                new GatherFacilityItem { Value = "fac-1", Text = "Main Campus" },
                new GatherFacilityItem { Value = "fac-2", Text = "West Wing" },
                new GatherFacilityItem { Value = "fac-3", Text = "East Pavilion" },
            };

            ViewBag.Physicians = new List<GatherPhysicianItem>
            {
                new GatherPhysicianItem { Value = "Dr. Smith", Text = "Dr. Smith" },
                new GatherPhysicianItem { Value = "Dr. Jones", Text = "Dr. Jones" },
                new GatherPhysicianItem { Value = "Dr. Patel", Text = "Dr. Patel" },
            };

            ViewBag.InsuranceProviders = new List<GatherInsuranceItem>
            {
                new GatherInsuranceItem { Value = "blue-cross", Text = "Blue Cross", Category = "Private" },
                new GatherInsuranceItem { Value = "aetna", Text = "Aetna", Category = "Private" },
                new GatherInsuranceItem { Value = "medicare", Text = "Medicare", Category = "Government" },
            };

            ViewBag.DietaryItems = new List<GatherDietaryItem>
            {
                new GatherDietaryItem { Value = "vegetarian", Text = "Vegetarian" },
                new GatherDietaryItem { Value = "halal", Text = "Halal" },
                new GatherDietaryItem { Value = "low-sodium", Text = "Low Sodium" },
            };

            return View("~/Areas/Sandbox/Views/AllModulesTogether/ComponentGather/Index.cshtml", new ComponentGatherModel
            {
                ResidentId = "RES-1042",
                FormToken = "csrf-abc123",
                ResidentName = "Margaret Thompson",
                CareNotes = "Initial assessment completed.",
                HasAllergies = true,
                Allergies = new[] { "Peanuts" },
                MonthlyRate = 4250.00m,
                ReceiveNotifications = true
            });
        }

        [HttpPost("EchoJson")]
        public IActionResult EchoJson([FromBody] ComponentGatherModel? model)
        {
            if (model == null)
                return BadRequest(new { error = "Empty body" });
            return Ok(BuildEchoResponse(model));
        }

        [HttpPost("EchoFormData")]
        public IActionResult EchoFormData([FromForm] ComponentGatherModel? model)
        {
            if (model == null)
                return BadRequest(new { error = "Empty form" });
            return Ok(BuildEchoResponse(model));
        }

        private static object BuildEchoResponse(ComponentGatherModel m)
        {
            return new
            {
                residentId = m.ResidentId,
                formToken = m.FormToken,
                residentName = m.ResidentName,
                careNotes = m.CareNotes != null
                    ? (m.CareNotes.Length > 30 ? m.CareNotes.Substring(0, 30) + "..." : m.CareNotes)
                    : null,
                hasAllergies = m.HasAllergies,
                mobilityLevel = m.MobilityLevel,
                careLevel = m.CareLevel,
                allergies = m.Allergies,
                monthlyRate = m.MonthlyRate,
                facilityId = m.FacilityId,
                physicianName = m.PhysicianName,
                admissionDate = m.AdmissionDate?.ToString("yyyy-MM-dd"),
                medicationTime = m.MedicationTime?.ToString("HH:mm"),
                appointmentTime = m.AppointmentTime?.ToString("yyyy-MM-dd HH:mm"),
                stayStart = m.StayStart?.ToString("yyyy-MM-dd"),
                insuranceProvider = m.InsuranceProvider,
                phoneNumber = m.PhoneNumber,
                carePlan = m.CarePlan != null
                    ? (m.CarePlan.Length > 30 ? m.CarePlan.Substring(0, 30) + "..." : m.CarePlan)
                    : null,
                receiveNotifications = m.ReceiveNotifications,
                dietaryRestrictions = m.DietaryRestrictions,
                fieldCount = CountNonNull(m)
            };
        }

        private static int CountNonNull(ComponentGatherModel m)
        {
            Func<bool>[] checks =
            [
                () => m.ResidentId != null,
                () => m.FormToken != null,
                () => m.ResidentName != null,
                () => m.CareNotes != null,
                () => m.HasAllergies,
                () => m.MobilityLevel != null,
                () => m.CareLevel != null,
                () => m.Allergies?.Length > 0,
                () => m.MonthlyRate != 0,
                () => m.FacilityId != null,
                () => m.PhysicianName != null,
                () => m.AdmissionDate.HasValue,
                () => m.MedicationTime.HasValue,
                () => m.AppointmentTime.HasValue,
                () => m.StayStart.HasValue,
                () => m.InsuranceProvider != null,
                () => m.PhoneNumber != null,
                () => m.CarePlan != null,
                () => m.ReceiveNotifications,
                () => m.DietaryRestrictions?.Length > 0,
            ];
            return checks.Count(c => c());
        }
    }
}
