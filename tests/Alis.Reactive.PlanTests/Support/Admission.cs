using Alis.Reactive;
using Alis.Reactive.Components;
using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.PlanTests.Support;

// The one page the plans are authored for: a resident's admission, with the members every grammar
// production needs, a text, a number, a yes or no, a choice and a list.
public sealed class Admission
{
    public string? Name { get; set; }

    public int Age { get; set; }

    public bool IsVeteran { get; set; }

    public CareLevel Level { get; set; }

    public List<string> Tags { get; set; } = new();

    public Contact NextOfKin { get; set; } = new();
}

public enum CareLevel
{
    Independent,
    Assisted,
    Memory,
}

public sealed class Contact
{
    public string? Name { get; set; }

    public string? Phone { get; set; }
}

public sealed class AdmissionSaved
{
    public string Confirmation { get; set; } = "";
}

public sealed class ResidentQuery
{
    public int Id { get; set; }

    public string? Wing { get; set; }
}

public sealed class RoomCheck
{
    public string Room { get; set; } = "";
}

public static class AdmissionEvents
{
    public static readonly AppEvent Opened = AppEvent.Declare();

    public static readonly AppEvent<RoomCheck> RoomChecked = AppEvent.Declare<RoomCheck>();

    public static readonly AppEvent<Admission> Admitted = AppEvent.Declare<Admission>();
}

/// <summary>What an endpoint binds when it reads the age as text: the form's number does not fit it.</summary>
public sealed class AdmissionAsText
{
    public string? Name { get; set; }

    public string? Age { get; set; }
}

/// <summary>The rules every admission is checked by, written once and run on both sides.</summary>
public sealed class AdmissionRules : ReactiveValidator<Admission>
{
    public AdmissionRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");

        // Only the server reads the tags through a query: declared so, the rule under it stays there.
        WhenOnServer(x => x.Tags.Any(tag => tag.Length > 3), () =>
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
    }
}

/// <summary>The same rules with the query written as a plain When, which the browser cannot read.</summary>
public sealed class UnreadableAdmissionRules : ReactiveValidator<Admission>
{
    public UnreadableAdmissionRules()
    {
        When(x => x.Tags.Any(tag => tag.Length > 3), () =>
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
    }
}

/// <summary>The rules of a next of kin, written once for every validator that holds one.</summary>
public sealed class ContactRules : ReactiveValidator<Contact>
{
    public ContactRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
    }
}

/// <summary>A message after a child validator has no check of its own to name.</summary>
public sealed class MessageAfterChildRules : ReactiveValidator<Admission>
{
    public MessageAfterChildRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new ContactRules()).WithMessage("Give a next of kin.");
    }
}

/// <summary>A rule on a value computed from a member has no member to report under.</summary>
public sealed class ComputedMemberRules : ReactiveValidator<Admission>
{
    public ComputedMemberRules()
    {
        RuleFor(x => x.Name!.Trim()).NotEmpty().WithMessage("Name the resident.");
    }
}

/// <summary>The front desk's object on the page: a method that answers after asking the server.</summary>
public sealed class Desk : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public TypedSource<bool> IsFree(Arg<string> room) => MethodAsync<bool>(room);

    public TypedSource<bool> Reserve(Arg<RoomCheck> check) => MethodAsync<bool>(check);
}
