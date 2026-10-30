using Alis.Reactive.SandboxApp.Areas.Sandbox.Models.Components.Brought;
using Alis.Reactive.SandboxApp.BroughtComponents;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.SandboxApp.Areas.Sandbox.Controllers.Components.Brought
{
    [Area("Sandbox")]
    [Route("Sandbox/Components/MedicationList")]
    public class MedicationListController : Controller
    {
        private const string ViewBase = "~/Areas/Sandbox/Views/Components/Brought/MedicationList/";

        [HttpGet("")]
        public IActionResult Index() => View(ViewBase + "Index.cshtml", new MedicationListModel());

        // Maple Grove Pharmacy delivered four of the five lines: no lidocaine patch, and its warfarin label
        // carries a trailing space. The empty delivery is what the pharmacy sends when nothing was ordered.
        [HttpGet("Delivery")]
        public IActionResult Delivery([FromQuery] string pharmacy)
        {
            if (pharmacy == "none") return Ok(new DeliveryResponse());

            var deliveredAt = new DateTime(2026, 6, 1, 16, 30, 0);
            return Ok(new DeliveryResponse
            {
                Lines =
                [
                    MedicationListModel.Line("Metoprolol 25 mg", units: 1, cost: 0.40m, hours: 12, MedicationRoute.Oral, hour: 8),
                    MedicationListModel.Line("Warfarin 2 mg ", units: 2, cost: 1.25m, hours: 24, MedicationRoute.Oral, hour: 18),
                    MedicationListModel.Line("Lorazepam 0.5 mg", units: 1, cost: 2.10m, hours: 8, MedicationRoute.Oral, hour: 8, controlled: true),
                    MedicationListModel.Line("Insulin glargine", units: 20, cost: 12.75m, hours: 24, MedicationRoute.Injection, hour: 7),
                ],
                Deliveries =
                [
                    new PharmacyDelivery
                    {
                        Pharmacy = "Maple Grove Pharmacy",
                        DeliveredAt = deliveredAt,
                        Lines = ["Metoprolol 25 mg", "Warfarin 2 mg", "Lorazepam 0.5 mg", "Insulin glargine"],
                    },
                ],
            });
        }
    }
}
