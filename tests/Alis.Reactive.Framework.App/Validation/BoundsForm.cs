using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// Numbers bounded every way a comparison is written: by a value or by another member, the checked member and
/// the other one each nullable or not. Base and MaybeBase are 5; every field starts inside its bound.
/// </summary>
public sealed class BoundsForm
{
    public int Base { get; set; } = 5;

    public int? MaybeBase { get; set; } = 5;

    public int? MaybeAtLeastTwo { get; set; } = 3;

    public int? MaybeBelowFive { get; set; } = 3;

    public int? MaybeAtMostFive { get; set; } = 3;

    public int AtLeastBase { get; set; } = 6;

    public int BelowBase { get; set; } = 4;

    public int AtMostBase { get; set; } = 4;

    public int? MaybeAboveBase { get; set; } = 6;

    public int? MaybeAtLeastBase { get; set; } = 6;

    public int? MaybeBelowBase { get; set; } = 4;

    public int? MaybeAtMostBase { get; set; } = 4;

    public int? MaybeAboveMaybeBase { get; set; } = 6;

    public int? MaybeBelowMaybeBase { get; set; } = 4;

    public int? MaybeAtMostMaybeBase { get; set; } = 4;

    public int AboveMaybeBase { get; set; } = 6;

    public int AtLeastMaybeBase { get; set; } = 6;

    public int BelowMaybeBase { get; set; } = 4;

    public int AtMostMaybeBase { get; set; } = 4;

    public int? MaybeOneToFive { get; set; } = 3;

    public int? MaybeInsideTen { get; set; } = 5;
}

/// <summary>One rule per comparison overload a page has not used elsewhere.</summary>
public sealed class BoundsRules : ReactiveValidator<BoundsForm>
{
    public BoundsRules()
    {
        RuleFor(x => x.MaybeAtLeastTwo).GreaterThanOrEqualTo(2).WithMessage("Out of bounds: MaybeAtLeastTwo.");
        RuleFor(x => x.MaybeBelowFive).LessThan(5).WithMessage("Out of bounds: MaybeBelowFive.");
        RuleFor(x => x.MaybeAtMostFive).LessThanOrEqualTo(5).WithMessage("Out of bounds: MaybeAtMostFive.");
        RuleFor(x => x.AtLeastBase).GreaterThanOrEqualTo(x => x.Base).WithMessage("Out of bounds: AtLeastBase.");
        RuleFor(x => x.BelowBase).LessThan(x => x.Base).WithMessage("Out of bounds: BelowBase.");
        RuleFor(x => x.AtMostBase).LessThanOrEqualTo(x => x.Base).WithMessage("Out of bounds: AtMostBase.");
        RuleFor(x => x.MaybeAboveBase).GreaterThan(x => x.Base).WithMessage("Out of bounds: MaybeAboveBase.");
        RuleFor(x => x.MaybeAtLeastBase).GreaterThanOrEqualTo(x => x.Base).WithMessage("Out of bounds: MaybeAtLeastBase.");
        RuleFor(x => x.MaybeBelowBase).LessThan(x => x.Base).WithMessage("Out of bounds: MaybeBelowBase.");
        RuleFor(x => x.MaybeAtMostBase).LessThanOrEqualTo(x => x.Base).WithMessage("Out of bounds: MaybeAtMostBase.");
        RuleFor(x => x.MaybeAboveMaybeBase).GreaterThan(x => x.MaybeBase).WithMessage("Out of bounds: MaybeAboveMaybeBase.");
        RuleFor(x => x.MaybeBelowMaybeBase).LessThan(x => x.MaybeBase).WithMessage("Out of bounds: MaybeBelowMaybeBase.");
        RuleFor(x => x.MaybeAtMostMaybeBase).LessThanOrEqualTo(x => x.MaybeBase).WithMessage("Out of bounds: MaybeAtMostMaybeBase.");
        RuleFor(x => x.AboveMaybeBase).GreaterThan(x => x.MaybeBase).WithMessage("Out of bounds: AboveMaybeBase.");
        RuleFor(x => x.AtLeastMaybeBase).GreaterThanOrEqualTo(x => x.MaybeBase).WithMessage("Out of bounds: AtLeastMaybeBase.");
        RuleFor(x => x.BelowMaybeBase).LessThan(x => x.MaybeBase).WithMessage("Out of bounds: BelowMaybeBase.");
        RuleFor(x => x.AtMostMaybeBase).LessThanOrEqualTo(x => x.MaybeBase).WithMessage("Out of bounds: AtMostMaybeBase.");
        RuleFor(x => x.MaybeOneToFive).InclusiveBetween(1, 5).WithMessage("Out of bounds: MaybeOneToFive.");
        RuleFor(x => x.MaybeInsideTen).ExclusiveBetween(0, 10).WithMessage("Out of bounds: MaybeInsideTen.");
    }
}
