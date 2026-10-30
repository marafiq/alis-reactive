using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    /// <summary>
    /// Hospital transfer. The transfer desk answers by resident: Helen Park (1042) is accepted; Arthur
    /// Bell (1043) is already at the hospital; Rosa Diaz (1044) makes the desk fail with no answer; Walter
    /// Cho (1045) was discharged this morning; Frank Moore (1046) needs the ambulance dispatcher, who is
    /// offline. Mercy General has no bed tonight, which only the desk knows, so a transfer there is refused
    /// beside the hospital field. Once a transfer is accepted, the family desk tells the family where the
    /// resident is going, from the hospital's answer as the page forwards it.
    /// </summary>
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/HospitalTransfer")]
    public class HospitalTransferController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
        {
            var transfer = new HospitalTransferModel { Escorts = { new TransferEscort(), new TransferEscort() } };
            return View("~/Areas/Sandbox/Views/HttpPipeline/HospitalTransfer/Index.cshtml", transfer);
        }

        [HttpPost("Transfer")]
        public IActionResult Transfer([FromBody] HospitalTransferModel? transfer)
        {
            if (transfer == null) return BadRequest();

            // The same validator the page runs; its Must on the hospital runs here alone.
            var result = new HospitalTransferValidator().Validate(transfer);
            if (!result.IsValid) return BadRequest(new { errors = result.ErrorsByField });

            return transfer.ResidentId switch
            {
                "1042" => Ok(Accepted("1042", "Helen Park", transfer)),
                "1043" => Conflict(new TransferRefusal("Arthur Bell is already at St. Luke's since this morning.")),
                "1044" => StatusCode(StatusCodes.Status500InternalServerError),
                "1045" => NotFound(new TransferRefusal("Walter Cho was discharged this morning and is no longer on the floor.")),
                "1046" => StatusCode(StatusCodes.Status503ServiceUnavailable,
                    new TransferRefusal("The ambulance dispatcher is offline until 6 AM.")),
                _ => NotFound(new TransferRefusal("No resident on the floor matches this record number.")),
            };
        }

        [HttpPost("NotifyFamily")]
        public IActionResult NotifyFamily([FromBody] TransferFamilyNotice? notice)
        {
            if (notice?.Transfer is not { } transfer)
                return Ok(new FamilyNoticeSent("The family desk received no transfer details; nobody was told."));

            return Ok(new FamilyNoticeSent(
                $"{FamilyOf(transfer.Resident)} was told: {transfer.Resident} is going to {transfer.Receiving.Hospital}, " +
                $"ward {transfer.Receiving.Ward} (transfer {notice.TransferId ?? "unnumbered"})."));
        }

        // The desk's answer repeats everything it received, so a value lost between the page and the desk
        // shows up on the page.
        private static TransferAccepted Accepted(string residentId, string resident, HospitalTransferModel transfer)
        {
            var hospital = string.IsNullOrWhiteSpace(transfer.Destination?.Hospital)
                ? "a hospital to be named"
                : transfer.Destination.Hospital.Trim();
            var ward = string.IsNullOrWhiteSpace(transfer.Destination?.Ward) ? "" : $" ({transfer.Destination.Ward.Trim()} requested)";
            var travel = transfer.Ambulance ? "by ambulance" : "by wheelchair van";
            var escorts = transfer.Escorts
                .Select(escort => escort.Name?.Trim())
                .Where(name => !string.IsNullOrEmpty(name))
                .ToList();
            var escorted = escorts.Count == 0 ? "unescorted" : $"escorted by {string.Join(" and ", escorts)}";
            var packing = transfer.Ambulance ? new[] { "meds", "facesheet", "glasses" } : new[] { "meds", "facesheet" };

            return new TransferAccepted(
                $"T-{residentId}-7",
                resident,
                new TransferReceiving(hospital, "4B", "12"),
                packing,
                $"{resident} to {hospital}{ward}: {travel}, {escorted}. Reason: {transfer.Reason?.Trim()}");
        }

        // No static lookup table: a static collection in the sandbox is shared by every browser world.
        private static string FamilyOf(string resident) => resident switch
        {
            "Helen Park" => "Grace Park",
            _ => "The family",
        };
    }
}
