using Microsoft.AspNetCore.Mvc;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class NumericTextBoxController : Controller
    {
        public IActionResult Index()
        {
            return View(new NumericTextBoxModel
            {
                Amount = 0,
                Temperature = 0,
                Quantity = 1
            });
        }

        [HttpPost]
        public IActionResult Echo([FromBody] Dictionary<string, object> data)
        {
            return Ok(data);
        }
    }
}
