using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.AppLevel.PharmacyDelivery;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.AppLevel
{
    /// <summary>
    /// The evening pharmacy delivery: the page's drawer, toast, confirm and loader, and a link that marks
    /// the delivery with who received it. URL: /Sandbox/Components/PharmacyDelivery.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Components/PharmacyDelivery")]
    public class PharmacyDeliveryController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/AppLevel/PharmacyDelivery/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new PharmacyDeliveryModel());

        // The delivery slip, loaded into the drawer.
        [HttpGet("DeliverySlip")]
        public IActionResult DeliverySlip() => PartialView(ViewBase + "_DeliverySlip.cshtml", new DeliverySlipModel());

        [HttpGet("Manifest")]
        public IActionResult Manifest() => Ok(new ManifestSummary("12 items on the manifest, 4 of them controlled."));

        [HttpPost("ReturnItem")]
        public IActionResult ReturnItem() => Ok(new ReturnResult("Damaged item returned to the pharmacy."));

        // The link's own address, opened in a window of its own (a Shift click): the slip, not a change.
        [HttpGet("MarkDelivered")]
        public IActionResult OpenDelivery() =>
            Content("The delivery slip is open. Mark it delivered from the delivery page.", "text/plain");

        [HttpPost("MarkDelivered")]
        public IActionResult MarkDelivered([FromBody] DeliveryReceipt? receipt)
        {
            if (receipt == null || string.IsNullOrWhiteSpace(receipt.ReceivedBy)) return BadRequest();
            return Ok(new DeliveryMarked($"Delivery marked; received by {receipt.ReceivedBy}."));
        }
    }
}
