using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Fusion
{
    /// <summary>
    /// Garden outing sign-up: the activities director seats residents who asked to go on Thursday's
    /// botanical garden outing and books the bus. Every page starts from the sign-up sheet; a booking
    /// answers with whom it booked and keeps nothing.
    /// </summary>
    public partial class GridController
    {
        private const int GardenOutingBusSeats = 4;

        [HttpGet("RosterSelection"), ActionName("RosterSelection")]
        public IActionResult GardenOutingPage() =>
            View(
                "~/Areas/Sandbox/Views/Components/Fusion/Grid/RosterSelection.cshtml",
                new RosterSelectionModel
                {
                    BusSeats = GardenOutingBusSeats,
                    // The bus was full when Rosa Diaz asked to go on September's outing.
                    TurnedAwayResidentId = 7105,
                    TurnedAwayResidentName = "Rosa Diaz"
                });

        [HttpGet("RosterSelectionSignUps")]
        public IActionResult RosterSelectionSignUps() => Ok(GardenOutingSignUps());

        [HttpPost("RosterSelectionBook")]
        public IActionResult RosterSelectionBook([FromBody] GardenOutingBooking? booking)
        {
            var riders = booking?.Riders ?? new List<GardenOutingSignUp>();
            if (riders.Count == 0)
                return BadRequest(new GardenOutingBooked { Summary = "Seat at least one resident before booking the bus." });

            var overBy = riders.Count - GardenOutingBusSeats;
            if (overBy > 0)
                return BadRequest(new GardenOutingBooked
                {
                    Summary = $"The bus seats {GardenOutingBusSeats}; take {GardenOutingResidents(overBy)} off before booking."
                });

            return Ok(new GardenOutingBooked
            {
                Summary = $"Booked {GardenOutingResidents(riders.Count)} for Thursday's garden outing: " +
                          $"{GardenOutingNames(riders.Select(rider => rider.ResidentName).ToList())}."
            });
        }

        private static string GardenOutingResidents(int count) => count == 1 ? "1 resident" : $"{count} residents";

        // "Helen Park", "Helen Park and Arthur Bell", "Helen Park, Arthur Bell and Walter Kim".
        private static string GardenOutingNames(List<string> names) =>
            names.Count == 1 ? names[0] : $"{string.Join(", ", names.Take(names.Count - 1))} and {names[^1]}";

        // The residents who asked to go, in the order they signed up.
        private static List<GardenOutingSignUp> GardenOutingSignUps() => new()
        {
            new GardenOutingSignUp { ResidentId = 7101, ResidentName = "Helen Park", Wing = "North", Mobility = "Walks", SignedUp = "Mon 9:05" },
            new GardenOutingSignUp { ResidentId = 7102, ResidentName = "Arthur Bell", Wing = "North", Mobility = "Walker", SignedUp = "Mon 9:40" },
            new GardenOutingSignUp { ResidentId = 7103, ResidentName = "Walter Kim", Wing = "East", Mobility = "Walks", SignedUp = "Mon 11:15" },
            new GardenOutingSignUp { ResidentId = 7104, ResidentName = "Irene Morgan", Wing = "West", Mobility = "Wheelchair", SignedUp = "Tue 8:30" },
            new GardenOutingSignUp { ResidentId = 7105, ResidentName = "Rosa Diaz", Wing = "East", Mobility = "Walker", SignedUp = "Tue 10:20" },
            new GardenOutingSignUp { ResidentId = 7106, ResidentName = "Grace Bennett", Wing = "West", Mobility = "Walks", SignedUp = "Wed 2:10" }
        };
    }
}
