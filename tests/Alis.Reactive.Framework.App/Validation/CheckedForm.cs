using System.Text.RegularExpressions;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// One field per check the validator offers, each starting at a value its rule takes and away from the rule's
/// bound, so a check made wrongly at its bound refuses only its own field.
/// </summary>
public sealed class CheckedForm
{
    public int? Count { get; set; } = 1;

    public string? Spare { get; set; }

    public string? Code { get; set; } = "abcde";

    public string? Pin { get; set; }

    public string? Nickname { get; set; } = "Ada";

    public string? Website { get; set; } = "https://visits.test";

    public string? Card { get; set; }

    public string? Letters { get; set; } = "abc";

    public string? Answer { get; set; } = "yes";

    public string? Other { get; set; } = "maybe";

    public string? Reason { get; set; } = "Visiting";

    public int Rooms { get; set; } = 1;

    public int Wing { get; set; } = 2;

    public int Floor { get; set; } = 3;

    public int Level { get; set; } = 3;

    public int Score { get; set; } = 5;

    public decimal? Budget { get; set; } = 100m;

    public DateTime Earlier { get; set; } = new(2026, 10, 1);

    public DateTime Later { get; set; } = new(2026, 10, 2);

    public string? Email { get; set; } = "ada@visits.test";

    public string? Short { get; set; } = "abc";

    public string? Lower { get; set; } = "abc";

    public int Stars { get; set; } = 3;

    public string? Original { get; set; } = "secret";

    public string? Repeat { get; set; } = "secret";

    public string? Former { get; set; } = "old";

    public string? Fresh { get; set; } = "new";

    public string? Purpose { get; set; } = "Visit";

    public int Guests { get; set; } = 2;

    public DateTime? Arriving { get; set; } = new(2026, 10, 1);

    public DateTime? Leaving { get; set; } = new(2026, 10, 3);

    public string? Badge { get; set; } = "A-17";

    public DateTime Visiting { get; set; } = new(2026, 10, 5);

    public DateTimeOffset Called { get; set; } = new(2026, 10, 5, 9, 0, 0, TimeSpan.Zero);

    public Contact Contact { get; set; } = new();
}

/// <summary>The contact the form holds, checked by its own validator.</summary>
public sealed class Contact
{
    public string? Phone { get; set; } = "555-0101";
}

/// <summary>A child validator whose one rule only the server makes.</summary>
public sealed class ContactRules : ReactiveValidator<Contact>
{
    public ContactRules()
    {
        RuleFor(x => x.Phone).Matches("^555-").ServerOnly().WithMessage("Use a 555 number.");
    }
}

/// <summary>Every check the validator offers, once each, written once and checked on both sides.</summary>
public sealed class CheckRules : ReactiveValidator<CheckedForm>
{
    public CheckRules()
    {
        RuleFor(x => x.Count).NotNull().WithMessage("Give a count.");
        RuleFor(x => x.Spare).Empty().WithMessage("Leave the spare field empty.");
        RuleFor(x => x.Code).MinimumLength(3).WithMessage("Use at least 3 characters.");
        RuleFor(x => x.Pin).Length(4).WithMessage("Use exactly 4 characters.");
        RuleFor(x => x.Nickname).Length(2, 5).WithMessage("Use 2 to 5 characters.");
        RuleFor(x => x.Website).Url().WithMessage("Give a web address.");
        RuleFor(x => x.Card).CreditCard().WithMessage("Give a card number.");
        RuleFor(x => x.Letters).Matches("^[a-z]+$", RegexOptions.IgnoreCase).WithMessage("Use letters only.");
        RuleFor(x => x.Answer).Equal("yes").WithMessage("Write yes.");
        RuleFor(x => x.Other).NotEqual("no").WithMessage("Anything but no.");
        RuleFor(x => x.Reason).NotEmpty().Unless(x => x.Answer == "yes").WithMessage("Give a reason unless the answer is yes.");
        RuleFor(x => x.Rooms).GreaterThan(0).WithMessage("More rooms than none.");
        RuleFor(x => x.Wing).GreaterThanOrEqualTo(1).WithMessage("Wing one at least.");
        RuleFor(x => x.Floor).LessThan(10).WithMessage("A floor below ten.");
        RuleFor(x => x.Level).LessThanOrEqualTo(9).WithMessage("Level nine at most.");
        RuleFor(x => x.Score).ExclusiveBetween(0, 10).WithMessage("Between 0 and 10, not either.");
        RuleFor(x => x.Budget).GreaterThan(0m).WithMessage("A budget above zero.");
        RuleFor(x => x.Later).GreaterThan(x => x.Earlier).WithMessage("Later comes after earlier.");
        RuleFor(x => x.Email).EmailAddress().WithMessage("Give an email address.");
        RuleFor(x => x.Short).MaximumLength(5).WithMessage("Five characters at most.");
        RuleFor(x => x.Lower).Matches("^[a-z]+$").WithMessage("Lower case only.");
        RuleFor(x => x.Stars).InclusiveBetween(1, 5).WithMessage("One to five stars.");
        RuleFor(x => x.Repeat).Equal(x => x.Original).WithMessage("Repeat the original.");
        RuleFor(x => x.Fresh).NotEqual(x => x.Former).WithMessage("Not the former one.");
        RuleFor(x => x.Purpose).NotEmpty().WithMessage("Give a purpose.");
        RuleFor(x => x.Guests).NotEmpty().WithMessage("At least one guest.");
        RuleFor(x => x.Leaving).GreaterThanOrEqualTo(x => x.Arriving).WithMessage("Leave on or after arriving.");
        RuleFor(x => x.Badge).Matches("^A-").ServerOnly().WithMessage("Badges start with A-.");
        RuleFor(x => x.Visiting).NotEmpty().WithMessage("Give the visiting day.");
        RuleFor(x => x.Called).NotEmpty().WithMessage("Give the time of the call.");
        RuleFor(x => x.Contact).SetValidator(new ContactRules());
    }
}
