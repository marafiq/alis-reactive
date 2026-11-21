using Alis.Reactive.Validation;

namespace Alis.Reactive.Conformance.App.Care;

public sealed partial class MoveInRules
{
    partial void TheLocation() => RuleFor(moveIn => moveIn.Location).SetValidator(new LocationRules());
}

public sealed class LocationRules : ReactiveValidator<LocationInput>
{
    public LocationRules()
    {
        RuleFor(location => location.CommunityId).NotEmpty().WithName("Community").WithMessage("Choose a community.");
        RuleFor(location => location.BuildingId).NotEmpty().WithName("Building").WithMessage("Choose a building.");
        RuleFor(location => location.ApartmentId).NotEmpty().WithName("Apartment").WithMessage("Choose an apartment.");
        RuleFor(location => location.BedId).NotEmpty().WithName("Bed").WithMessage("Choose a bed.");
    }
}
