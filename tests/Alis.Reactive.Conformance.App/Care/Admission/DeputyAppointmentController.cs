using Alis.Reactive.Conformance.World;
using Microsoft.AspNetCore.Mvc;

namespace Alis.Reactive.Conformance.App.Care;

public sealed class DeputyAppointmentController : Controller
{
    private const string PlainText = "text/plain";

    [HttpGet(CarePages.DeputyAppointment)]
    public IActionResult OfADeputy() => View("/Care/Admission/DeputyAppointment.cshtml", new DeputyAppointment());

    [HttpGet(CarePages.DeputyAppointmentOfANamelessDeputy)]
    public IActionResult OfANamelessDeputy() => View("/Care/Admission/DeputyNamelessAppointment.cshtml", new DeputyNamelessAppointment());

    [HttpPost(DeputyAppointmentEndpoints.Appointments)]
    public IActionResult Save([FromForm] DeputyAppointment appointment)
        => Content($"{appointment.Deputy?.Name}, {appointment.Deputy?.Phone}", PlainText);

    [HttpPost(DeputyAppointmentEndpoints.NamelessAppointments)]
    public IActionResult SaveNameless([FromForm] DeputyNamelessAppointment appointment)
        => Content($"{appointment.Deputy?.Name}, {appointment.Deputy?.Phone}", PlainText);
}
