using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers
{
    [Area("Sandbox")]
    public class IdGeneratorController : Controller
    {
        public IActionResult Index()
        {
            return View(new IdGeneratorModel
            {
                Name = "Test",
                Amount = 42.5m,
                Status = "active",
                Address = new IdGeneratorAddress { City = "Seattle", PostalCode = 98101 }
            });
        }

        [HttpPost]
        public IActionResult SaveJson([FromBody] IdGeneratorModel? model)
        {
            if (model == null) return BadRequest(new { error = "null body" });

            return Ok(new
            {
                summary = $"Name={model.Name}, Amount={model.Amount}, Status={model.Status}, City={model.Address?.City}, Zip={model.Address?.PostalCode}"
            });
        }

        [HttpPost]
        public IActionResult SaveForm([FromForm] IdGeneratorModel? model)
        {
            if (model == null) return BadRequest(new { error = "null body" });

            return Ok(new
            {
                summary = $"Name={model.Name}, Amount={model.Amount}, Status={model.Status}, City={model.Address?.City}, Zip={model.Address?.PostalCode}"
            });
        }
    }
}
