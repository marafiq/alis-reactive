using Microsoft.AspNetCore.Mvc;
using ResidentIntake.Models;
using ResidentIntake.Validators;

namespace ResidentIntake.Controllers;

public class IntakeController : Controller
{
    public IActionResult Index() => View(new ResidentIntakeModel());

    [HttpGet]
    public IActionResult Facilities()
    {
        var facilities = new FacilitiesResponse
        {
            Facilities =
            [
                new("sunrise", "Sunrise Senior Living"),
                new("oakwood", "Oakwood Care Center"),
                new("maple", "Maple Grove Residence")
            ]
        };
        return Ok(facilities);
    }

    [HttpGet]
    public IActionResult CareLevels()
    {
        var levels = new CareLevelsResponse
        {
            Levels =
            [
                new("independent", "Independent Living"),
                new("assisted", "Assisted Living"),
                new("memory-care", "Memory Care")
            ]
        };
        return Ok(levels);
    }

    [HttpGet]
    public IActionResult Units([FromQuery] string? facilityId)
    {
        var units = (facilityId ?? "").ToLowerInvariant() switch
        {
            "sunrise" =>
            [
                new LookupItem("s-101", "Suite 101 — Garden View"),
                new LookupItem("s-102", "Suite 102 — Garden View"),
                new LookupItem("s-201", "Suite 201 — Courtyard"),
                new LookupItem("s-202", "Suite 202 — Courtyard")
            ],
            "oakwood" =>
            [
                new LookupItem("o-a1", "Wing A — Room 1"),
                new LookupItem("o-a2", "Wing A — Room 2"),
                new LookupItem("o-b1", "Wing B — Room 1")
            ],
            "maple" =>
            [
                new LookupItem("m-10", "Cottage 10"),
                new LookupItem("m-11", "Cottage 11"),
                new LookupItem("m-12", "Cottage 12"),
                new LookupItem("m-13", "Cottage 13"),
                new LookupItem("m-14", "Cottage 14")
            ],
            _ => new List<LookupItem>()
        };

        return Ok(new UnitsResponse { Units = units });
    }

    [HttpGet]
    public IActionResult FacilityInfo([FromQuery] string? facilityId)
    {
        var (name, address, phone, capacity) = (facilityId ?? "").ToLowerInvariant() switch
        {
            "sunrise" => ("Sunrise Senior Living", "123 Garden Lane, Springfield, IL 62701", "(555) 234-5678", 48),
            "oakwood" => ("Oakwood Care Center", "456 Oak Avenue, Riverside, CA 92501", "(555) 345-6789", 36),
            "maple"   => ("Maple Grove Residence", "789 Maple Drive, Madison, WI 53703", "(555) 456-7890", 60),
            _         => ("", "", "", 0)
        };

        if (string.IsNullOrEmpty(name))
            return Content("");

        ViewBag.FacilityName = name;
        ViewBag.Address = address;
        ViewBag.Phone = phone;
        ViewBag.Capacity = capacity;
        return PartialView("_FacilityInfo");
    }

    [HttpGet]
    public IActionResult FacilityRequirements([FromQuery] string? facilityId)
    {
        if (string.IsNullOrEmpty(facilityId))
            return Content("");

        return PartialView("_FacilityRequirements", new ResidentIntakeModel());
    }

    [HttpPost]
    public IActionResult Save([FromBody] ResidentIntakeModel? model)
    {
        if (model == null)
            return BadRequest(new
            {
                errors = new Dictionary<string, string[]>
                {
                    ["FirstName"] = ["Request body is required."]
                }
            });

        var validator = new IntakeValidator();
        var result = validator.Validate(model);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());
            return BadRequest(new { errors });
        }

        return Ok(new { message = "Intake saved successfully" });
    }

    [HttpGet]
    public IActionResult ConfirmationNumber()
    {
        // Simulate generating a confirmation number
        var number = $"RES-{DateTime.Now:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";
        return Ok(new ConfirmationResponse { Number = number });
    }

    [HttpGet]
    public IActionResult Summary([FromQuery] string? firstName, [FromQuery] string? lastName,
        [FromQuery] string? facilityId, [FromQuery] string? careLevel,
        [FromQuery] string? admissionDate, [FromQuery] decimal? monthlyRate)
    {
        ViewBag.FirstName = firstName ?? "—";
        ViewBag.LastName = lastName ?? "—";
        ViewBag.FacilityId = facilityId ?? "—";
        ViewBag.CareLevel = careLevel ?? "—";
        ViewBag.AdmissionDate = admissionDate ?? "—";
        ViewBag.MonthlyRate = monthlyRate?.ToString("C") ?? "—";
        return PartialView("_Summary");
    }
}
