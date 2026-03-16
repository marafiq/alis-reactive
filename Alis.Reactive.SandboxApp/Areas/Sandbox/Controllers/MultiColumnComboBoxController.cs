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
            ViewBag.Facilities = new List<Dictionary<string, object>>
            {
                new() { ["value"] = "1", ["text"] = "Sunrise Manor", ["city"] = "Seattle", ["capacity"] = 120 },
                new() { ["value"] = "2", ["text"] = "Lakeside Care", ["city"] = "Portland", ["capacity"] = 85 },
                new() { ["value"] = "3", ["text"] = "Meadow Ridge", ["city"] = "Bellevue", ["capacity"] = 150 },
                new() { ["value"] = "4", ["text"] = "Harbor View", ["city"] = "Tacoma", ["capacity"] = 95 }
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
