using Alis.Reactive.Conformance.World;
using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

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
