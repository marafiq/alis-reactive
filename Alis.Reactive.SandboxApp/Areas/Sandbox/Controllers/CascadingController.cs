using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class CascadingController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Countries = new List<CountryItem>
            {
                new() { Value = "US", Text = "United States", Continent = "North America" },
                new() { Value = "UK", Text = "United Kingdom", Continent = "Europe" },
                new() { Value = "CA", Text = "Canada", Continent = "North America" },
                new() { Value = "AU", Text = "Australia", Continent = "Oceania" }
            };
            return View(new CascadingModel());
        }

        [HttpGet]
        public IActionResult Cities([FromQuery] string? Country)
        {
            var cities = (Country ?? "").ToUpperInvariant() switch
            {
                "US" => new List<CityItem>
                {
                    new() { Value = "SEA", Text = "Seattle", State = "WA", Population = 750000 },
                    new() { Value = "NYC", Text = "New York", State = "NY", Population = 8300000 },
                    new() { Value = "CHI", Text = "Chicago", State = "IL", Population = 2700000 }
                },
                "UK" => new List<CityItem>
                {
                    new() { Value = "LON", Text = "London", State = "England", Population = 9000000 },
                    new() { Value = "MAN", Text = "Manchester", State = "England", Population = 550000 }
                },
                "CA" => new List<CityItem>
                {
                    new() { Value = "TOR", Text = "Toronto", State = "Ontario", Population = 2800000 },
                    new() { Value = "VAN", Text = "Vancouver", State = "BC", Population = 675000 }
                },
                "AU" => new List<CityItem>
                {
                    new() { Value = "SYD", Text = "Sydney", State = "NSW", Population = 5300000 },
                    new() { Value = "MEL", Text = "Melbourne", State = "VIC", Population = 5100000 }
                },
                _ => new List<CityItem>()
            };
            return Ok(new { cities, country = Country, count = cities.Count });
        }

        [HttpPost]
        public IActionResult Save([FromBody] CascadingModel model)
        {
            return Ok(new CascadingSaveResponse
            {
                ReceivedCountry = model.Country ?? "(empty)",
                ReceivedCity = model.City ?? "(empty)",
                Message = $"Saved: {model.City} in {model.Country}"
            });
        }
    }
}
