using Alis.Reactive;

namespace Alis.Reactive.PlanTests.Support;

public sealed class Admission
{
    public string? Name { get; set; }

    public int Age { get; set; }

    public bool IsVeteran { get; set; }

    public CareLevel Level { get; set; }

    public List<string> Tags { get; set; } = new();

    public Contact NextOfKin { get; set; } = new();
    public decimal Deposit { get; set; }
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

public sealed class AdmissionAsText
{
    public string? Name { get; set; }

    public string? Age { get; set; }
}

public sealed class AdmissionRules : ReactiveValidator<Admission>
{
    public AdmissionRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident.");

        WhenOnServer(x => x.Tags.Any(tag => tag.Length > 3), () =>
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
    }
}

public sealed class CareLevelRules : ReactiveValidator<Admission>
{
    public CareLevelRules()
    {
        RuleFor(x => x.Level).IsInEnum();
    }
}

public sealed class DepositRules : ReactiveValidator<Admission>
{
    public DepositRules()
    {
        RuleFor(x => x.Deposit).PrecisionScale(5, 2, ignoreTrailingZeros: false);
    }
}

public sealed class NamedDepositRules : ReactiveValidator<Admission>
{
    public NamedDepositRules()
    {
        RuleFor(x => x.Deposit).PrecisionScale(5, 2, ignoreTrailingZeros: false).WithName("Deposit held");
    }
}

public sealed class DependentAdmissionRules : ReactiveValidator<Admission>
{
    public DependentAdmissionRules()
    {
        RuleFor(x => x.Name).NotEmpty().DependentRules(() =>
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
    }
}

public sealed class CustomAdmissionRules : ReactiveValidator<Admission>
{
    public CustomAdmissionRules()
    {
        RuleFor(x => x.Name).Custom((name, context) =>
        {
            if (name == "Nobody") context.AddFailure("Name a person.");
        });
    }
}

public sealed class StagedAdmissionRules : ReactiveValidator<Admission>
{
    public StagedAdmissionRules()
    {
        RuleFor(x => x.Name).MaximumLength(40);
        RuleSet("Enquire, Admit", () => RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident."));
        RuleSet("Admit", () => RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
        RuleSet("Enquire", () => RuleFor(x => x.Deposit).GreaterThan(0m));
    }
}

public sealed class UnreadableAdmissionRules : ReactiveValidator<Admission>
{
    public UnreadableAdmissionRules()
    {
        When(x => x.Tags.Any(tag => tag.Length > 3), () =>
            RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the resident's age."));
    }
}

public sealed class ContactRules : ReactiveValidator<Contact>
{
    public ContactRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the contact.");
    }
}

public sealed class MessageAfterChildRules : ReactiveValidator<Admission>
{
    public MessageAfterChildRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new ContactRules()).WithMessage("Give a next of kin.");
    }
}

public sealed class WaitingAdmissionRules : ReactiveValidator<Admission>
{
    public WaitingAdmissionRules()
    {
        RuleFor(x => x.Age).GreaterThan(0).WithMessage("Give the age.");
        WhenAsync((admission, cancellation) => Task.FromResult(admission.Age > 90), () =>
            RuleFor(x => x.Name).NotEmpty().WithMessage("Name the resident."));
    }
}

public sealed class ComputedMemberRules : ReactiveValidator<Admission>
{
    public ComputedMemberRules()
    {
        RuleFor(x => x.Name!.Trim()).NotEmpty().WithMessage("Name the resident.");
    }
}

public sealed class Desk : Component
{
    public override ComponentVendor Vendor => ComponentVendor.Native;

    public TypedSource<bool> IsFree(Arg<string> room) => MethodAsync<bool>(room);

    public TypedSource<bool> Reserve(Arg<RoomCheck> check) => MethodAsync<bool>(check);
}
