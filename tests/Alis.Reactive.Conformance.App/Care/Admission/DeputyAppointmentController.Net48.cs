using System.Web.Mvc;
using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DeputyAppointmentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet]
    [Address(CarePages.DeputyAppointment)]
    public ActionResult OfADeputy() => View("/Care/Admission/DeputyAppointment.cshtml", new DeputyAppointment());

    [HttpGet]
    [Address(CarePages.DeputyAppointmentOfANamelessDeputy)]
    public ActionResult OfANamelessDeputy() => View("/Care/Admission/DeputyNamelessAppointment.cshtml", new DeputyNamelessAppointment());

    [HttpPost]
    [Address(DeputyAppointmentEndpoints.Appointments)]
    public ActionResult Save(DeputyAppointment appointment)
        => Content($"{appointment.Deputy?.Name}, {appointment.Deputy?.Phone}", PlainText);

    [HttpPost]
    [Address(DeputyAppointmentEndpoints.NamelessAppointments)]
    public ActionResult SaveNameless(DeputyNamelessAppointment appointment)
        => Content($"{appointment.Deputy?.Name}, {appointment.Deputy?.Phone}", PlainText);
}
