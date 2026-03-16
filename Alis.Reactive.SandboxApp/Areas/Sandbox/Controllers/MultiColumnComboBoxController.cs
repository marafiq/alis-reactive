using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class MultiColumnComboBoxController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.Facilities = new List<FacilityItem>
            {
                new() { Value = "1", Text = "Sunrise Manor", City = "Seattle", Capacity = 120 },
                new() { Value = "2", Text = "Lakeside Care", City = "Portland", Capacity = 85 },
                new() { Value = "3", Text = "Meadow Ridge", City = "Bellevue", Capacity = 150 },
                new() { Value = "4", Text = "Harbor View", City = "Tacoma", Capacity = 95 }
            };
            return View(new MultiColumnComboBoxModel());
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
