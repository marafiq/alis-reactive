using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using Alis.Reactive.Net48.SmokeTest.Models;
using Alis.Reactive.Net48.SmokeTest.Validators;

namespace Alis.Reactive.Net48.SmokeTest.Controllers
{
    public class IntakeController : Controller
    {
        public ActionResult Index()
        {
            var model = new ResidentIntakeModel();

            // Server-side lookup data for native <select> elements
            ViewBag.Facilities = new List<LookupItem>
            {
                new LookupItem("sunrise", "Sunrise Senior Living"),
                new LookupItem("oakwood", "Oakwood Care Center"),
                new LookupItem("maple", "Maple Grove Residence")
            };
            ViewBag.CareLevels = new List<LookupItem>
            {
                new LookupItem("independent", "Independent Living"),
                new LookupItem("assisted", "Assisted Living"),
                new LookupItem("memory-care", "Memory Care")
            };

            return View(model);
        }

        [HttpGet]
        public ActionResult Units(string facilityId)
        {
            var units = GetUnitsForFacility(facilityId ?? "");
            return PartialView("_UnitOptions", units);
        }

        [HttpGet]
        public ActionResult FacilityInfo(string facilityId)
        {
            string name, address, phone;
            int capacity;

            switch ((facilityId ?? "").ToLowerInvariant())
            {
                case "sunrise":
                    name = "Sunrise Senior Living";
                    address = "123 Garden Lane, Springfield, IL 62701";
                    phone = "(555) 234-5678";
                    capacity = 48;
                    break;
                case "oakwood":
                    name = "Oakwood Care Center";
                    address = "456 Oak Avenue, Riverside, CA 92501";
                    phone = "(555) 345-6789";
                    capacity = 36;
                    break;
                case "maple":
                    name = "Maple Grove Residence";
                    address = "789 Maple Drive, Madison, WI 53703";
                    phone = "(555) 456-7890";
                    capacity = 60;
                    break;
                default:
                    return Content("");
            }

            ViewBag.FacilityName = name;
            ViewBag.Address = address;
            ViewBag.Phone = phone;
            ViewBag.Capacity = capacity;
            return PartialView("_FacilityInfo");
        }

        [HttpGet]
        public ActionResult FacilityRequirements(string facilityId)
        {
            if (string.IsNullOrEmpty(facilityId))
                return Content("");

            return PartialView("_FacilityRequirements", new ResidentIntakeModel());
        }

        [HttpPost]
        public ActionResult Save(ResidentIntakeModel model)
        {
            if (model == null)
            {
                Response.StatusCode = 400;
                return Json(new { errors = new Dictionary<string, string[]> { { "FirstName", new[] { "Request body is required." } } } },
                    JsonRequestBehavior.AllowGet);
            }

            var validator = new IntakeValidator();
            var result = validator.Validate(model);

            if (!result.IsValid)
            {
                var errors = result.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                Response.StatusCode = 400;
                return Json(new { errors }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { message = "Intake saved successfully" }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult ConfirmationNumber()
        {
            var rng = new Random();
            var number = string.Format("RES-{0:yyyyMMdd}-{1}", DateTime.Now, rng.Next(1000, 9999));
            return Json(new { number }, JsonRequestBehavior.AllowGet);
        }

        [HttpGet]
        public ActionResult Summary(string firstName, string lastName, string facilityId,
            string careLevel, string admissionDate, string monthlyRate)
        {
            ViewBag.FirstName = firstName ?? "\u2014";
            ViewBag.LastName = lastName ?? "\u2014";
            ViewBag.FacilityId = facilityId ?? "\u2014";
            ViewBag.CareLevel = careLevel ?? "\u2014";
            ViewBag.AdmissionDate = admissionDate ?? "\u2014";
            ViewBag.MonthlyRate = monthlyRate ?? "\u2014";
            return PartialView("_Summary");
        }

        private static List<LookupItem> GetUnitsForFacility(string facilityId)
        {
            switch (facilityId.ToLowerInvariant())
            {
                case "sunrise":
                    return new List<LookupItem>
                    {
                        new LookupItem("s-101", "Suite 101 \u2014 Garden View"),
                        new LookupItem("s-102", "Suite 102 \u2014 Garden View"),
                        new LookupItem("s-201", "Suite 201 \u2014 Courtyard"),
                        new LookupItem("s-202", "Suite 202 \u2014 Courtyard")
                    };
                case "oakwood":
                    return new List<LookupItem>
                    {
                        new LookupItem("o-a1", "Wing A \u2014 Room 1"),
                        new LookupItem("o-a2", "Wing A \u2014 Room 2"),
                        new LookupItem("o-b1", "Wing B \u2014 Room 1")
                    };
                case "maple":
                    return new List<LookupItem>
                    {
                        new LookupItem("m-10", "Cottage 10"),
                        new LookupItem("m-11", "Cottage 11"),
                        new LookupItem("m-12", "Cottage 12"),
                        new LookupItem("m-13", "Cottage 13"),
                        new LookupItem("m-14", "Cottage 14")
                    };
                default:
                    return new List<LookupItem>();
            }
        }
    }
}
