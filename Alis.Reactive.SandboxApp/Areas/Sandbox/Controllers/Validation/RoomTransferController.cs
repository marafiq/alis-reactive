using System.Globalization;
using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Validation
{
    /// <summary>
    /// Room transfers: the request and the urgent request run the same validator, asynchronously because
    /// housekeeping's calendar is asked. Housekeeping's own desk answers in plain text.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/Validation/RoomTransfer")]
    public class RoomTransferController : Controller
    {
        [HttpGet("")]
        public IActionResult Index() =>
            View("~/Areas/Sandbox/Views/Validation/RoomTransfer/Index.cshtml", new RoomTransferModel());

        [HttpPost("Request")]
        public Task<IActionResult> RequestTransfer([FromBody] RoomTransferModel? transfer) =>
            RequestAsync(transfer, "Transfer");

        [HttpPost("RequestUrgent")]
        public Task<IActionResult> RequestUrgentTransfer([FromBody] RoomTransferModel? transfer) =>
            RequestAsync(transfer, "Urgent transfer");

        /// <summary>Housekeeping's desk: closed on Sunday, and it says so in a note rather than a record.</summary>
        [HttpPost("Housekeeping")]
        public IActionResult Housekeeping([FromBody] RoomTransferModel? transfer)
        {
            if (transfer == null) return BadRequest();

            var day = transfer.TransferDate;
            if (day == null || day.Value.DayOfWeek == DayOfWeek.Sunday)
                return BadRequest("Housekeeping is closed on Sunday. Call the desk on Monday.");

            var dayText = day.Value.ToString("dddd, MMMM d", CultureInfo.InvariantCulture);
            return Ok(new RoomTransferConfirmation($"Housekeeping will prepare {transfer.ToRoom} on {dayText}."));
        }

        private async Task<IActionResult> RequestAsync(RoomTransferModel? transfer, string kind)
        {
            if (transfer == null) return BadRequest();

            var result = await new RoomTransferValidator().ValidateAsync(transfer);
            if (!result.IsValid)
                return BadRequest(new { errors = result.ErrorsByField });

            return Ok(new RoomTransferConfirmation($"{kind} of {transfer.ResidentName} to {transfer.ToRoom} requested."));
        }
    }
}
