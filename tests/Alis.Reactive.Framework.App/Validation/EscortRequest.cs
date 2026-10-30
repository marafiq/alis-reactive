using System.Text.RegularExpressions;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// An escort request: what the desk is told (guests, wing, floor, overnight, the visit's purpose) and one
/// field per form a rule's condition can take, each needed only while its condition holds. Each field starts
/// filled, so a field is refused only when a test empties it. A condition that reads a member of text or of a
/// nullable tests the member first, as the server's C# must; the browser reads both tests.
/// </summary>
public sealed class EscortRequest
{
    public int Guests { get; set; } = 1;

    public bool Overnight { get; set; }

    public string? Wing { get; set; }

    public int? Floor { get; set; }

    public EscortVisit Visit { get; set; } = new();

    public string? ForTwo { get; set; } = "yes";

    public string? NotForTwo { get; set; } = "yes";

    public string? ForMore { get; set; } = "yes";

    public string? ForTwoOrMore { get; set; } = "yes";

    public string? ForFewer { get; set; } = "yes";

    public string? ForTwoOrFewer { get; set; } = "yes";

    public string? TwoBelow { get; set; } = "yes";

    public string? TwoAtMost { get; set; } = "yes";

    public string? TwoAbove { get; set; } = "yes";

    public string? TwoAtLeast { get; set; } = "yes";

    public string? OverLimit { get; set; } = "yes";

    public string? NoWing { get; set; } = "yes";

    public string? AnyWing { get; set; } = "yes";

    public string? BlankWing { get; set; } = "yes";

    public string? GivenWing { get; set; } = "yes";

    public string? AnnexWing { get; set; } = "yes";

    public string? EastFirst { get; set; } = "yes";

    public string? AnnexLast { get; set; } = "yes";

    public string? PoleWing { get; set; } = "yes";

    public string? KnownWing { get; set; } = "yes";

    public string? OtherWing { get; set; } = "yes";

    public string? ListedWing { get; set; } = "yes";

    public string? KnownFloor { get; set; } = "yes";

    public string? HighFloor { get; set; } = "yes";

    public string? ByNight { get; set; } = "yes";

    public string? ByDay { get; set; } = "yes";

    public string? NotMany { get; set; } = "yes";

    public string? TourPurpose { get; set; } = "yes";

    public string? ManyByNight { get; set; } = "yes";

    public string? ManyOrByNight { get; set; } = "yes";

    public List<string>? Tags { get; set; }

    public string? NoAnnex { get; set; } = "yes";

    public string? EastNamedFirst { get; set; } = "yes";

    public string? GuestOfNote { get; set; } = "yes";

    /// <summary>Who comes with the visitor: checked by its own validator, while the visit is overnight.</summary>
    public EscortCompanion Companion { get; set; } = new();
}

/// <summary>A companion of the visitor: what kind, how long they stay, and a field per rule its validator writes under a condition.</summary>
public sealed class EscortCompanion
{
    public string? Kind { get; set; }

    public string? Stay { get; set; }

    public string? Name { get; set; } = "yes";

    public string? Room { get; set; } = "yes";

    public string? Phone { get; set; } = "yes";

    public string? Badge { get; set; } = "yes";

    public string? Pass { get; set; } = "yes";
}

/// <summary>A companion's rules: one always, the others each under a condition of its own over the companion's members.</summary>
public sealed class EscortCompanionRules : ReactiveValidator<EscortCompanion>
{
    public EscortCompanionRules()
    {
        RuleFor(c => c.Name).NotEmpty().WithMessage("Name the companion.");
        RuleFor(c => c.Room).NotEmpty().When(c => c.Stay == "overnight").WithMessage("Give the companion's room for the night.");
        RuleFor(c => c.Phone).NotEmpty().When(c => c.Kind == "carer" && c.Stay == "overnight").WithMessage("Give the night carer's phone.");
        RuleFor(c => c.Badge).NotEmpty().When(c => c.Kind == "carer" || c.Kind == "nurse").WithMessage("Give the companion's badge.");
        RuleFor(c => c.Pass).NotEmpty().When(c => !(c.Kind == "family")).WithMessage("Give a pass to a companion who is not family.");
    }
}

/// <summary>The visit the escort is for.</summary>
public sealed class EscortVisit
{
    public string? Purpose { get; set; }
}

/// <summary>Each field is needed while its condition holds; every condition has a browser form.</summary>
public sealed class EscortRules : ReactiveValidator<EscortRequest>
{
    private static readonly string[] KnownWings = { "East", "West" };
    private static readonly List<string?> ListedWings = new() { "West", "Garden" };
    private readonly int GuestLimit = 4;

    public EscortRules()
    {
        RuleFor(x => x.ForTwo).NotEmpty().When(x => x.Guests == 2).WithMessage("Needed for exactly two guests.");
        RuleFor(x => x.NotForTwo).NotEmpty().When(x => x.Guests != 2).WithMessage("Needed for other than two guests.");
        RuleFor(x => x.ForMore).NotEmpty().When(x => x.Guests > 2).WithMessage("Needed for more than two guests.");
        RuleFor(x => x.ForTwoOrMore).NotEmpty().When(x => x.Guests >= 2).WithMessage("Needed for two guests or more.");
        RuleFor(x => x.ForFewer).NotEmpty().When(x => x.Guests < 2).WithMessage("Needed for fewer than two guests.");
        RuleFor(x => x.ForTwoOrFewer).NotEmpty().When(x => x.Guests <= 2).WithMessage("Needed for two guests or fewer.");
        RuleFor(x => x.TwoBelow).NotEmpty().When(x => 2 < x.Guests).WithMessage("Needed when two is below the guests.");
        RuleFor(x => x.TwoAtMost).NotEmpty().When(x => 2 <= x.Guests).WithMessage("Needed when two is at most the guests.");
        RuleFor(x => x.TwoAbove).NotEmpty().When(x => 2 > x.Guests).WithMessage("Needed when two is above the guests.");
        RuleFor(x => x.TwoAtLeast).NotEmpty().When(x => 2 >= x.Guests).WithMessage("Needed when two is at least the guests.");
        RuleFor(x => x.OverLimit).NotEmpty().When(x => x.Guests > GuestLimit).WithMessage("Needed for more guests than the limit.");
        RuleFor(x => x.NoWing).NotEmpty().When(x => x.Wing == null).WithMessage("Needed when no wing is named.");
        RuleFor(x => x.AnyWing).NotEmpty().When(x => x.Wing != null).WithMessage("Needed when a wing is named.");
        RuleFor(x => x.BlankWing).NotEmpty().When(x => string.IsNullOrEmpty(x.Wing)).WithMessage("Needed when the wing is left blank.");
        RuleFor(x => x.GivenWing).NotEmpty().When(x => !string.IsNullOrEmpty(x.Wing)).WithMessage("Needed when the wing is given.");
        RuleFor(x => x.AnnexWing).NotEmpty().When(x => x.Wing != null && x.Wing.Contains("Annex")).WithMessage("Needed when the wing is an annex.");
        RuleFor(x => x.EastFirst).NotEmpty().When(x => x.Wing != null && x.Wing.StartsWith("East")).WithMessage("Needed when the wing starts with East.");
        RuleFor(x => x.AnnexLast).NotEmpty().When(x => x.Wing != null && x.Wing.EndsWith("Annex")).WithMessage("Needed when the wing ends with Annex.");
        RuleFor(x => x.PoleWing).NotEmpty().When(x => x.Wing != null && Regex.IsMatch(x.Wing, "^[NS]")).WithMessage("Needed when the wing is north or south.");
        RuleFor(x => x.KnownWing).NotEmpty().When(x => KnownWings.Contains(x.Wing)).WithMessage("Needed when the wing is a known one.");
        RuleFor(x => x.OtherWing).NotEmpty().When(x => !KnownWings.Contains(x.Wing)).WithMessage("Needed when the wing is another one.");
        RuleFor(x => x.ListedWing).NotEmpty().When(x => ListedWings.Contains(x.Wing)).WithMessage("Needed when the wing is a listed one.");
        RuleFor(x => x.KnownFloor).NotEmpty().When(x => x.Floor.HasValue).WithMessage("Needed when the floor is known.");
        RuleFor(x => x.HighFloor).NotEmpty().When(x => x.Floor.HasValue && x.Floor.Value > 2).WithMessage("Needed when the floor is above two.");
        RuleFor(x => x.ByNight).NotEmpty().When(x => x.Overnight).WithMessage("Needed for an overnight visit.");
        RuleFor(x => x.ByDay).NotEmpty().When(x => !x.Overnight).WithMessage("Needed for a day visit.");
        RuleFor(x => x.NotMany).NotEmpty().When(x => !(x.Guests > 2)).WithMessage("Needed unless there are more than two guests.");
        RuleFor(x => x.TourPurpose).NotEmpty().When(x => x.Visit.Purpose == "Tour").WithMessage("Needed for a tour.");
        RuleFor(x => x.ManyByNight).NotEmpty().When(x => x.Guests > 2 && x.Overnight).WithMessage("Needed for many guests overnight.");
        RuleFor(x => x.ManyOrByNight).NotEmpty().When(x => x.Guests > 2 || x.Overnight).WithMessage("Needed for many guests or overnight.");
        RuleFor(x => x.NoAnnex).NotEmpty().When(x => x.Wing != null && !x.Wing.Contains("Annex")).WithMessage("Needed when the wing is no annex.");
        RuleFor(x => x.EastNamedFirst).NotEmpty().When(x => "East" == x.Wing).WithMessage("Needed when East is the wing.");
        RuleFor(x => x.GuestOfNote).NotEmpty().When(x => x.Tags != null && x.Tags.Contains("vip")).WithMessage("Needed for a guest of note.");
        RuleFor(x => x.Companion).SetValidator(new EscortCompanionRules()).When(x => x.Overnight);
    }
}

/// <summary>A rule under an ordering against nothing, which no value passes and the browser has no form of.</summary>
public sealed class NothingOrderedEscortRules : ReactiveValidator<EscortRequest>
{
    private static readonly int? NoGuestLimit = null;

    public NothingOrderedEscortRules()
    {
        RuleFor(x => x.ForMore).NotEmpty().When(x => x.Guests > NoGuestLimit).WithMessage("Needed above no limit.");
    }
}

/// <summary>A rule under a text method the browser has no form of (Equals), and not declared the server's.</summary>
public sealed class TextEqualsEscortRules : ReactiveValidator<EscortRequest>
{
    public TextEqualsEscortRules()
    {
        RuleFor(x => x.ForMore).NotEmpty().When(x => x.Wing != null && x.Wing.Equals("East")).WithMessage("Needed for the east wing.");
    }
}

/// <summary>A rule under a condition the browser cannot read (a member of text, its length) and not declared the server's.</summary>
public sealed class UnreadableEscortRules : ReactiveValidator<EscortRequest>
{
    public UnreadableEscortRules()
    {
        RuleFor(x => x.ForMore).NotEmpty().When(x => x.Wing != null && x.Wing.Length > 3).WithMessage("Needed for a long wing name.");
    }
}
