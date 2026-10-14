using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Native.ArrayOps;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Native
{
    /// <summary>
    /// Wing census: the director's morning stand-up reads each wing's census from this morning's records
    /// and ticks the programs running today. URL: /Sandbox/Components/ArrayOps.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/ArrayOps")]
    public class ArrayOpsController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Components/Native/ArrayOps/Index.cshtml", new WingCensusModel());

        /// <summary>
        /// Maple wing. Each count has a resident on its line: Margaret Hale's Morse score is 45, Rose
        /// Delgado's 24, Evelyn Hart is 90 and her review is in 7 days. Walter Brooks, in hospital, has the
        /// wing's highest fall risk and the soonest review, and the first resident in residence on the
        /// list, Harold Kim, has neither.
        /// </summary>
        [HttpGet("Maple")]
        public IActionResult MapleWing() => Ok(new WingCensusResponse
        {
            Wing = "Maple wing",
            Residents = new[]
            {
                Resident("Harold Kim",    CensusStatus.InResidence, age: 84, morse: 30, doses: 7, charges: 5025.00m, hours: 8,     review: 9),
                Resident("Walter Brooks", CensusStatus.InHospital,  age: 93, morse: 70, doses: 4, charges: 4800.00m, hours: 14,    review: 1),
                Resident("Margaret Hale", CensusStatus.InResidence, age: 88, morse: 45, doses: 6, charges: 5200.25m, hours: 10.5,  review: 3),
                Resident("Robert Miles",  CensusStatus.OnLeave,     age: 79, morse: 20, doses: 3, charges: 3900.00m, hours: 4,     review: 12),
                Resident("Evelyn Hart",   CensusStatus.InResidence, age: 90, morse: 50, doses: 5, charges: 6100.50m, hours: 14.25, review: 7),
                Resident("Rose Delgado",  CensusStatus.InResidence, age: 91, morse: 24, doses: 4, charges: 4450.75m, hours: 6.75,  review: 5),
                Resident("Agnes Moore",   CensusStatus.OnLeave,     age: 96, morse: 55, doses: 2, charges: 4300.00m, hours: 3,     review: 2),
                Resident("Ida Bell",      CensusStatus.InResidence, age: 76, morse: 15, doses: 3, charges: 3875.25m, hours: 2.75,  review: 20),
            },
        });

        /// <summary>Birch wing: nobody in hospital this morning; George Webb is 90.</summary>
        [HttpGet("Birch")]
        public IActionResult BirchWing() => Ok(new WingCensusResponse
        {
            Wing = "Birch wing",
            Residents = new[]
            {
                Resident("Clara Nguyen",  CensusStatus.InResidence, age: 82, morse: 35, doses: 4, charges: 4600.00m, hours: 5.5,  review: 4),
                Resident("Frank Ortiz",   CensusStatus.OnLeave,     age: 87, morse: 40, doses: 3, charges: 4100.25m, hours: 3.25, review: 10),
                Resident("George Webb",   CensusStatus.InResidence, age: 90, morse: 60, doses: 6, charges: 5800.50m, hours: 12.5, review: 8),
                Resident("Lillian Price", CensusStatus.InResidence, age: 94, morse: 25, doses: 5, charges: 5150.00m, hours: 9,    review: 6),
                Resident("Nora Fields",   CensusStatus.InResidence, age: 79, morse: 10, doses: 2, charges: 3700.75m, hours: 1.75, review: 15),
            },
        });

        private static CensusResident Resident(
            string name, string status, int age, int morse, int doses, decimal charges, double hours, int review) =>
            new CensusResident
            {
                Name = name,
                Status = status,
                Age = age,
                MorseScore = morse,
                MorningDoses = doses,
                MonthlyCharges = charges,
                CareHours = hours,
                DaysToReview = review,
            };
    }
}
