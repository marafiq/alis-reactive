using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Shift start. The community's gateway forwards a check to the partner its X-Partner header names:
    /// the pharmacy answers, the lab has nothing pending and says so with a 404, the kitchen answers. The
    /// Maple wing's handover sheet is written; Cedar's is not. The kitchen takes its orders as form posts
    /// and answers in its own words; its crackers line runs on the old system, whose answer the page
    /// cannot read. The board is signed for a wing with no answer to read.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/ShiftStart")]
    public class ShiftStartController : Controller
    {
        private const string ViewFolder = "~/Areas/Sandbox/Views/HttpPipeline/ShiftStart/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewFolder + "Index.cshtml", new ShiftStartModel());

        [HttpGet("Partners")]
        public IActionResult Partners([FromHeader(Name = "X-Partner")] string? partner) => partner switch
        {
            "pharmacy" => Ok(new PartnerAnswer("Riverside Pharmacy: the night cart is stocked.")),
            "lab" => NotFound(new PartnerAnswer("Northgate Lab: no results are pending for this floor.")),
            "kitchen" => Ok(new PartnerAnswer("Kitchen: warm milk and crackers until 2 AM.")),
            _ => BadRequest(new PartnerAnswer("Name the partner the gateway forwards to.")),
        };

        [HttpGet("Sheet")]
        public IActionResult Sheet(string? wing)
        {
            if (string.IsNullOrWhiteSpace(wing))
                return BadRequest(new SheetMessage("Choose a wing to open its handover sheet."));

            return wing switch
            {
                "Maple" => PartialView(ViewFolder + "_HandoverSheet.cshtml", new HandoverSheet("Maple", new[]
                {
                    "Helen Park: refused dinner; offered a supplement at 7 PM.",
                    "Arthur Bell: new walker delivered; needs a fitting check.",
                    "Rosa Diaz: family visiting tomorrow at 10 AM.",
                })),
                _ => NotFound(new SheetMessage($"{wing} wing's handover sheet is not written yet.")),
            };
        }

        [HttpPost("Order")]
        public IActionResult Order([FromForm] ShiftStartModel order)
        {
            if (order.Snack == "Crackers")
                return Content("{\"message\": \"Crackers", "application/json");
            if (string.IsNullOrWhiteSpace(order.SnackCount))
                return BadRequest(new KitchenAnswer("Say how many."));

            var note = string.IsNullOrWhiteSpace(order.FloorNote) ? " (no floor note)" : $" with the note: {order.FloorNote}";
            return Ok(new KitchenAnswer($"The kitchen will send {order.SnackCount.Trim()} {order.Snack} to the floor{note}."));
        }

        [HttpPost("Sign")]
        public IActionResult Sign([FromBody] BoardSignature? signature) =>
            string.IsNullOrWhiteSpace(signature?.Wing) ? BadRequest() : NoContent();
    }
}
