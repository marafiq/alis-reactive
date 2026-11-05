using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
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

        private static string FamilyOf(string resident) => resident switch
        {
            "Helen Park" => "Grace Park",
            _ => "The family",
        };
    }
}
