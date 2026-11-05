
namespace Alis.Reactive.Framework.App.Partials;

public sealed class DeskPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

public sealed class KinCard
{
    public string? Name { get; set; }
}

public sealed class KinRules : ReactiveValidator<KinCard>
{
    public KinRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the next of kin.");
        RuleFor(x => x.Name).MaximumLength(20).WithMessage("Keep the name to 20 letters.");
    }
}

public sealed class DeskRules : ReactiveValidator<DeskPage>
{
    public DeskRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

public sealed class KinOnFileRules : ReactiveValidator<DeskPage>
{
    public KinOnFileRules()
    {
        RuleFor(x => x.NextOfKin.Name).Must(name => name is "Bo" or "Cy").WithMessage("That name is not on the resident's file.");
    }
}

public interface ISignedIn
{
    string? Visitor { get; }
}

public abstract class VisitRecord : ISignedIn
{
    public string? Visitor { get; set; }
}

public sealed class LobbyPage : VisitRecord
{
    public string? Resident { get; set; }
}

public abstract class SignInRules<TRecord> : ReactiveValidator<TRecord> where TRecord : class, ISignedIn
{
    protected SignInRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor who signs in.");
    }
}

public sealed class LobbyRules : SignInRules<LobbyPage>;

public sealed class ChartEntry<TReading> where TReading : class, new()
{
    public string? Initials { get; set; }

    public TReading Reading { get; set; } = new();
}

public sealed class FluidsReading
{
    public int? Millilitres { get; set; }
}

public interface INamedVisitor
{
    string? Visitor { get; }
}

public sealed class PartialRefusalsPage : INamedVisitor
{
    public PartialMistake Mistake { get; set; }

    public string? Visitor { get; set; }
}

public enum PartialMistake
{
    TypeAPartialToAnInterface,
    BindAnInputToTheModelItself,
}

public sealed class AdmissionPage
{
    public string? Resident { get; set; }

    public string? Room { get; set; }

    public bool ShowsRoom { get; set; }
}

public sealed class AdmissionRules : ReactiveValidator<AdmissionPage>
{
    public AdmissionRules()
    {
        RuleFor(x => x.Room).NotEmpty().WithMessage("Choose the resident's room.");
        RuleFor(x => x.Room).Must(room => room != "13").WithMessage("Room 13 is closed for repairs.");
    }
}

public sealed class SittingPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

public sealed class HandoverPage
{
    public string? Room { get; set; }

    public KeyholderLine Keyholder { get; set; } = new();
}

public sealed class KeyholderLine
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }
}

public sealed class KeyholderLineRules : ReactiveValidator<KeyholderLine>
{
    public KeyholderLineRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the keyholder.");
    }
}

public sealed class EscortPage
{
    public EscortLine Driver { get; set; } = new();

    public EscortLine Aide { get; set; } = new();
}

public sealed class EscortLine
{
    public string? Name { get; set; }

    public string? Shift { get; set; }
}

public sealed class ConciergePage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

public sealed class PersonName
{
    public string? Name { get; set; }
}

public sealed class ConciergeRules : ReactiveValidator<ConciergePage>
{
    public ConciergeRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

public sealed class KinNote
{
    public KinCard NextOfKin { get; set; } = new();
}

public sealed class ChangeoverPage
{
    public KinCard NextOfKin { get; set; } = new();
}

public sealed class BookingsPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

public sealed class BookingRules : ReactiveValidator<BookingsPage>
{
    public BookingRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor who brings the next of kin.")
            .When(x => !string.IsNullOrEmpty(x.NextOfKin.Name));
    }
}

public sealed class CorrectionsPage
{
}

public sealed class RoomCorrection
{
    public string? Value { get; set; }
}

public sealed class PhoneCorrection
{
    public string? Value { get; set; }
}

public sealed class PartialsEcho
{
    public string Summary { get; set; } = "";
}
