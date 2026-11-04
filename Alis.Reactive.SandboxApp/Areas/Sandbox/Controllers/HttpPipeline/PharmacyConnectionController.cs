using Alis.Reactive.SandboxApp.Areas.Sandbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.HttpPipeline
{
    [Area("Sandbox")]
    [Route("Sandbox/HttpPipeline/PharmacyConnection")]
    public class PharmacyConnectionController : Controller
    {
        [HttpGet("")]
        public IActionResult Index()
            => View("~/Areas/Sandbox/Views/HttpPipeline/PharmacyConnection/Index.cshtml", new PharmacyConnectionModel());

        [HttpGet("TestOrder/{pharmacyId}")]
        public IActionResult TestOrder(string pharmacyId) => pharmacyId switch
        {
            "riverside" => Ok(new PharmacyHandshake(200, "Riverside Pharmacy accepted test order 4471.")),
            "northgate" => Ok(new PharmacyHandshake(503, "Northgate Pharmacy is down for maintenance until 6 AM.")),
            _ => NotFound(),
        };
    }
}
