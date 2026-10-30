using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A guest with a name, whatever else the guest is.</summary>
public interface INamedGuest
{
    string? Name { get; }
}

/// <summary>A guest the app registers the generic rules for.</summary>
public sealed class RegisteredGuest : INamedGuest
{
    public string? Name { get; set; } = "Ada";

    public GuestAddress? Address { get; set; }
}

/// <summary>A guest no registration names the generic rules for.</summary>
public sealed class UnregisteredGuest : INamedGuest
{
    public string? Name { get; set; } = "Bea";
}

public sealed class GuestAddress
{
    public string? City { get; set; }
}

/// <summary>
/// Rules written once for any named guest. A generic validator is not found by scanning an assembly, so the
/// app registers each guest it checks: <c>validation.Add&lt;GuestNameRules&lt;RegisteredGuest&gt;&gt;()</c>.
/// </summary>
public sealed class GuestNameRules<TGuest> : ReactiveValidator<TGuest> where TGuest : class, INamedGuest
{
    public GuestNameRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the guest.");
    }
}

/// <summary>A rule that reads the guest's city with no guard for a guest who gave no address.</summary>
public sealed class UnguardedGuestRules : ReactiveValidator<RegisteredGuest>
{
    public UnguardedGuestRules()
    {
        RuleFor(x => x.Address!.City).Must(city => city != "Nowhere").WithMessage("Give a real city.");
    }
}

/// <summary>A rule that waits for the guest list before it decides whether the guest is expected.</summary>
public sealed class WaitingGuestRules : ReactiveValidator<RegisteredGuest>
{
    public WaitingGuestRules()
    {
        RuleFor(x => x.Name).MustAsync(async (name, cancellation) => { await Task.Yield(); return name != "Nobody"; })
            .WithMessage("That guest is not expected.");
    }
}
