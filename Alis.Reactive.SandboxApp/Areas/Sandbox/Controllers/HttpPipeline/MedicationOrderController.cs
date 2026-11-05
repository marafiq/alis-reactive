using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/MedicationOrder")]
    public class MedicationOrderController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var order = new MedicationOrderModel { Lines = { new MedicationOrderLine(), new MedicationOrderLine() } };
            return View("~/Areas/Sandbox/Views/HttpPipeline/MedicationOrder/Index.cshtml", order);
        }

        [HttpPost("Save")]
        public IActionResult Save([FromBody] MedicationOrderModel? order)
        {
            if (order == null) return BadRequest();

            return Ok(new MedicationOrderConfirmation(ConfirmationFor(order)));
        }

        private static string ConfirmationFor(MedicationOrderModel order)
        {
            var lines = order.Lines
                .Where(line => !string.IsNullOrWhiteSpace(line.Sku))
                .Select(line => $"{line.Sku} × {line.Quantity.ToString(CultureInfo.InvariantCulture)}")
                .ToList();

            return lines.Count == 0
                ? $"Order saved for {order.ResidentName} with no medication lines."
                : $"Order saved for {order.ResidentName}: {string.Join(", ", lines)}.";
        }
    }
}
