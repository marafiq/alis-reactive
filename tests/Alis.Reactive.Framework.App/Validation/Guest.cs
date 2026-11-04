
namespace Alis.Reactive.Framework.App.Validation;

public interface INamedGuest
{
    string? Name { get; }
}

public sealed class RegisteredGuest : INamedGuest
{
    public string? Name { get; set; } = "Ada";

    public GuestAddress? Address { get; set; }
}

public sealed class UnregisteredGuest : INamedGuest
{
    public string? Name { get; set; } = "Bea";
}

public sealed class GuestAddress
{
    public string? City { get; set; }
}

public sealed class GuestNameRules<TGuest> : ReactiveValidator<TGuest> where TGuest : class, INamedGuest
{
    public GuestNameRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the guest.");
    }
}

public sealed class UnguardedGuestRules : ReactiveValidator<RegisteredGuest>
{
    public UnguardedGuestRules()
    {
        RuleFor(x => x.Address!.City).Must(city => city != "Nowhere").WithMessage("Give a real city.");
    }
}

public sealed class WaitingGuestRules : ReactiveValidator<RegisteredGuest>
{
    public WaitingGuestRules()
    {
        RuleFor(x => x.Name).MustAsync(async (name, cancellation) => { await Task.Yield(); return name != "Nobody"; })
            .WithMessage("That guest is not expected.");
    }
}
