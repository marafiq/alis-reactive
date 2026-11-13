using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;
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

public sealed class DeputyAppointment
{
    public Deputy? Deputy { get; set; }
}

public sealed class Deputy
{
    public string Name { get; set; } = "Unnamed";

    public string? Phone { get; set; }
}

public sealed class DeputyRules : ReactiveValidator<Deputy>
{
    public DeputyRules() => RuleFor(deputy => deputy.Name).NotEmpty().WithMessage("Name the deputy.");
}

public sealed class DeputyAppointmentRules : ReactiveValidator<DeputyAppointment>
{
    public DeputyAppointmentRules() => RuleFor(appointment => appointment.Deputy!).SetValidator(new DeputyRules());
}

public sealed class DeputyNamelessAppointment
{
    public DeputyNameless? Deputy { get; set; }
}

public sealed class DeputyNameless
{
    public string Name { get; set; } = "";

    public string? Phone { get; set; }
}

public sealed class DeputyNamelessRules : ReactiveValidator<DeputyNameless>
{
    public DeputyNamelessRules() => RuleFor(deputy => deputy.Name).NotEmpty().WithMessage("Name the deputy.");
}

public sealed class DeputyNamelessAppointmentRules : ReactiveValidator<DeputyNamelessAppointment>
{
    public DeputyNamelessAppointmentRules() => RuleFor(appointment => appointment.Deputy!).SetValidator(new DeputyNamelessRules());
}

public static class DeputyAppointmentEndpoints
{
    public const string Appointments = CarePages.DeputyAppointment + "/appointments";

    public const string NamelessAppointments = CarePages.DeputyAppointmentOfANamelessDeputy + "/appointments";
}
