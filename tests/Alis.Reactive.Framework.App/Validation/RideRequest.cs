using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// A ride to an appointment: the resident's record, the pick-up time, the wing to collect from and the
/// clinic's directions. None of them is text to the server: a record number, a time of day, a letter, a link.
/// </summary>
public sealed class RideRequest
{
    public Guid? Resident { get; set; } = new("3f2c5d1e-8a4b-4c6d-9e7f-1a2b3c4d5e6f");

    public TimeSpan? PickUp { get; set; } = new(14, 30, 0);

    public char? Wing { get; set; } = 'B';

    public Uri? Directions { get; set; } = new("https://clinic.test/directions");
}

/// <summary>The rules every ride is booked by: each detail given.</summary>
public sealed class RideRequestRules : ReactiveValidator<RideRequest>
{
    public RideRequestRules()
    {
        RuleFor(x => x.Resident).NotEmpty().WithMessage("Give the resident's record number.");
        RuleFor(x => x.PickUp).NotEmpty().WithMessage("Give the pick-up time.");
        RuleFor(x => x.Wing).NotEmpty().WithMessage("Give the wing to collect from.");
        RuleFor(x => x.Directions).NotEmpty().WithMessage("Paste the clinic's directions link.");
    }
}
