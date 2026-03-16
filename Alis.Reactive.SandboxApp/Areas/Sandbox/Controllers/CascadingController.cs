using System;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class CascadingController : Controller
    {
        public IActionResult Index()
        {
            // Complex objects with camelCase properties for SF DataSource
            ViewBag.Countries = new object[]
            {
                new { value = "US", text = "United States", continent = "North America" },
                new { value = "UK", text = "United Kingdom", continent = "Europe" },
                new { value = "CA", text = "Canada", continent = "North America" },
                new { value = "AU", text = "Australia", continent = "Oceania" }
            };
            return View(new CascadingModel());
        }

        [HttpGet]
        public IActionResult Cities([FromQuery] string? Country)
        {
            // Return cities for the selected country — camelCase JSON (ASP.NET Core default)
            var cities = (Country ?? "").ToUpperInvariant() switch
            {
                "US" => new object[]
                {
                    new { value = "SEA", text = "Seattle", state = "WA", population = 750000 },
                    new { value = "NYC", text = "New York", state = "NY", population = 8300000 },
                    new { value = "CHI", text = "Chicago", state = "IL", population = 2700000 }
                },
                "UK" => new object[]
                {
                    new { value = "LON", text = "London", state = "England", population = 9000000 },
                    new { value = "MAN", text = "Manchester", state = "England", population = 550000 }
                },
                "CA" => new object[]
                {
                    new { value = "TOR", text = "Toronto", state = "Ontario", population = 2800000 },
                    new { value = "VAN", text = "Vancouver", state = "BC", population = 675000 }
                },
                "AU" => new object[]
                {
                    new { value = "SYD", text = "Sydney", state = "NSW", population = 5300000 },
                    new { value = "MEL", text = "Melbourne", state = "VIC", population = 5100000 }
                },
                _ => Array.Empty<object>()
            };
            return Ok(new { cities, country = Country, count = cities.Length });
        }

        [HttpPost]
        public IActionResult Save([FromBody] CascadingModel model)
        {
            return Ok(new
            {
                receivedCountry = model.Country ?? "(empty)",
                receivedCity = model.City ?? "(empty)",
                message = $"Saved: {model.City} in {model.Country}"
            });
        }
    }
}
