using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Validation;

/// <summary>A notice whose fields are checked with the messages the validator words itself, or builds.</summary>
public sealed class Notice
{
    public int? Rooms { get; set; } = 1;

    public string? FirstName { get; set; } = "Ada";

    public string? Spare { get; set; }

    public string? Nickname { get; set; } = "Ada";

    public string? Pin { get; set; }

    public string? Code { get; set; } = "abcde";

    public string? Short { get; set; } = "abc";

    public string? Email { get; set; } = "ada@visits.test";

    public string? Letters { get; set; } = "abc";

    public string? Card { get; set; }

    public string? Website { get; set; } = "https://visits.test";

    public string? Answer { get; set; } = "yes";

    public string? Other { get; set; } = "maybe";

    public int Wing { get; set; } = 2;

    public int Floor { get; set; } = 2;

    public int Level { get; set; } = 3;

    public int Bay { get; set; } = 3;

    public int Stars { get; set; } = 3;

    public int Score { get; set; } = 5;

    public string? Repeat { get; set; } = "secret";

    public string? Motto { get; set; } = "Kind";

    public string? Tagline { get; set; } = "Hi";

    public int Rating { get; set; } = 3;

    public string? Host { get; set; } = "Bea";

    public string? Room { get; set; } = "A-1";

    public string? Bed { get; set; } = "B-2";

    public string? Pass { get; set; } = "P-1";

    public string? Locker { get; set; } = "L-1";

    public string? Original { get; set; } = "secret";

    public NoticeAddress Address { get; set; } = new();

    public NoticeContact Contact { get; set; } = new();

    public string? Escort { get; set; } = "Cy";

    public string? Parking { get; set; } = "P-1";
}

public sealed class NoticeAddress
{
    public string? City { get; set; } = "Dover";
}

public sealed class NoticeContact
{
    public string? Phone { get; set; } = "555-0101";
}

/// <summary>Each check with the message the validator words for it, and the messages only the server can give.</summary>
public sealed class NoticeRules : ReactiveValidator<Notice>
{
    public NoticeRules()
    {
        RuleFor(x => x.Rooms).NotNull();
        RuleFor(x => x.FirstName).NotEmpty();
        RuleFor(x => x.Spare).Empty();
        RuleFor(x => x.Nickname).Length(2, 5);
        RuleFor(x => x.Pin).Length(4);
        RuleFor(x => x.Code).MinimumLength(3);
        RuleFor(x => x.Short).MaximumLength(5);
        RuleFor(x => x.Email).EmailAddress();
        RuleFor(x => x.Letters).Matches("^[a-z]+$");
        RuleFor(x => x.Card).CreditCard();
        RuleFor(x => x.Website).Url();
        RuleFor(x => x.Answer).Equal("yes");
        RuleFor(x => x.Other).NotEqual("no");
        RuleFor(x => x.Wing).GreaterThan(0);
        RuleFor(x => x.Floor).GreaterThanOrEqualTo(1);
        RuleFor(x => x.Level).LessThan(10);
        RuleFor(x => x.Bay).LessThanOrEqualTo(9);
        RuleFor(x => x.Stars).InclusiveBetween(1, 5);
        RuleFor(x => x.Score).ExclusiveBetween(0, 10);
        RuleFor(x => x.Repeat).Equal(x => x.Original);
        RuleFor(x => x.Address.City).NotEmpty();
        RuleFor(x => x.Motto).MaximumLength(8).WithMessage("Keep '{PropertyName}' under {MaxLength:000} characters.");
        RuleFor(x => x.Contact).ChildRules(contact => contact.RuleFor(x => x.Phone).NotEmpty());
        RuleFor(x => x.Tagline).MaximumLength(5).ServerOnly();
        RuleFor(x => x.Rating).InclusiveBetween(1, 5).ServerOnly();
        RuleFor(x => x.Host).NotEmpty().WithMessage(x => $"{x.FirstName} needs a host.");
        RuleFor(x => x.Room).Must(room => room != "Z-0");
        RuleFor(x => x.Bed).Must((notice, bed) => bed != notice.Room).WithMessage("The bed cannot share the room's name.");
        RuleFor(x => x.Pass).NotEqual("P-9").WithMessage("Pass {PropertyValue} is withdrawn.");
        RuleFor(x => x.Locker).MustAsync(async (locker, cancellation) => { await Task.Yield(); return locker != "L-0"; }).WithMessage("That locker is taken.");
        RuleFor(x => x.Escort).NotEmpty().WhenOnServer(x => x.Wing > 5).WithMessage("The far wings need an escort.");
        RuleFor(x => x.Parking).NotEmpty().UnlessOnServer(x => x.Wing > 5).WithMessage("The near wings need parking.");
    }
}
