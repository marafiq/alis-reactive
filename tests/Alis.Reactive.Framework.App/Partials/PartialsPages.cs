
namespace Alis.Reactive.Framework.App.Partials;

/// <summary>
/// The front desk's record of a visit: the page renders the visitor, and the next of kin is edited through
/// the editor template of its own model, rendered under the member's prefix (NextOfKin.Name).
/// </summary>
public sealed class DeskPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>The resident's next of kin: a model of its own, with an editor template (EditorTemplates/KinCard).</summary>
public sealed class KinCard
{
    public string? Name { get; set; }
}

/// <summary>The next of kin's rules: a name the browser checks is there and short enough for the file.</summary>
public sealed class KinRules : ReactiveValidator<KinCard>
{
    public KinRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the next of kin.");
        RuleFor(x => x.Name).MaximumLength(20).WithMessage("Keep the name to 20 letters.");
    }
}

/// <summary>The desk's rules, which check the next of kin by the next of kin's own.</summary>
public sealed class DeskRules : ReactiveValidator<DeskPage>
{
    public DeskRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

/// <summary>The desk's check of the next of kin against the resident's file: only the server holds the file.</summary>
public sealed class KinOnFileRules : ReactiveValidator<DeskPage>
{
    public KinOnFileRules()
    {
        RuleFor(x => x.NextOfKin.Name).Must(name => name is "Bo" or "Cy").WithMessage("That name is not on the resident's file.");
    }
}

/// <summary>A record a visitor signs in on, whichever class keeps it.</summary>
public interface ISignedIn
{
    string? Visitor { get; }
}

/// <summary>What every visit records, whichever desk takes it: the visitor, rendered by a partial of this class.</summary>
public abstract class VisitRecord : ISignedIn
{
    public string? Visitor { get; set; }
}

/// <summary>The lobby's sign-in: a visit record, with the resident visited.</summary>
public sealed class LobbyPage : VisitRecord
{
    public string? Resident { get; set; }
}

/// <summary>Rules written once for any record a visitor signs in on: they read the visitor through the interface.</summary>
public abstract class SignInRules<TRecord> : ReactiveValidator<TRecord> where TRecord : class, ISignedIn
{
    protected SignInRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor who signs in.");
    }
}

/// <summary>The lobby's rules: the sign-in rules, on the lobby's record.</summary>
public sealed class LobbyRules : SignInRules<LobbyPage>;

/// <summary>An entry on a resident's chart, whatever is charted: the reading, and the initials of who charted it.</summary>
public sealed class ChartEntry<TReading> where TReading : class, new()
{
    public string? Initials { get; set; }

    public TReading Reading { get; set; } = new();
}

/// <summary>A fluids reading charted at the bedside.</summary>
public sealed class FluidsReading
{
    public int? Millilitres { get; set; }
}

/// <summary>A visitor known by name, whatever else the visit records.</summary>
public interface INamedVisitor
{
    string? Visitor { get; }
}

/// <summary>A page that renders an input whose id cannot follow its member.</summary>
public sealed class PartialRefusalsPage : INamedVisitor
{
    public PartialMistake Mistake { get; set; }

    public string? Visitor { get; set; }
}

/// <summary>The ways an input can bind something with no class to scope its id.</summary>
public enum PartialMistake
{
    TypeAPartialToAnInterface,
    BindAnInputToTheModelItself,
}

/// <summary>
/// An admission: the page renders the resident, and the room is a member of the admission's own model that a
/// partial of the admission renders (RoomLine): with the page when the desk shows the room at once, or loaded
/// into the page later.
/// </summary>
public sealed class AdmissionPage
{
    public string? Resident { get; set; }

    public string? Room { get; set; }

    /// <summary>Whether the page renders the room's line with itself; otherwise the desk loads it when a room is chosen.</summary>
    public bool ShowsRoom { get; set; }
}

/// <summary>The admission's rules: a room the browser checks is chosen, and one only the server knows is closed.</summary>
public sealed class AdmissionRules : ReactiveValidator<AdmissionPage>
{
    public AdmissionRules()
    {
        RuleFor(x => x.Room).NotEmpty().WithMessage("Choose the resident's room.");
        RuleFor(x => x.Room).Must(room => room != "13").WithMessage("Room 13 is closed for repairs.");
    }
}

/// <summary>
/// A visitor's sitting with a resident: the page renders the visitor, and the next of kin is edited through the
/// member's editor template (KinCard); only the server checks the sitting.
/// </summary>
public sealed class SittingPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>
/// A room handed over to a keyholder: the page renders the room, and the keyholder's line is the editor template
/// of its own model, which carries the handover's save.
/// </summary>
public sealed class HandoverPage
{
    public string? Room { get; set; }

    public KeyholderLine Keyholder { get; set; } = new();
}

/// <summary>A keyholder: the line renders the name; how they are related is the server's to ask for.</summary>
public sealed class KeyholderLine
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }
}

/// <summary>The keyholder line's rules: a name the browser checks is given.</summary>
public sealed class KeyholderLineRules : ReactiveValidator<KeyholderLine>
{
    public KeyholderLineRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the keyholder.");
    }
}

/// <summary>
/// The escort for a resident's appointment: a driver and an aide, each a line of one model rendered through its
/// editor template, each checked against the roster by a request of its own line.
/// </summary>
public sealed class EscortPage
{
    public EscortLine Driver { get; set; } = new();

    public EscortLine Aide { get; set; } = new();
}

/// <summary>One escort: the line renders the name; which shift they cover is the server's to ask for.</summary>
public sealed class EscortLine
{
    public string? Name { get; set; }

    public string? Shift { get; set; }
}

/// <summary>
/// The concierge's record of a visit: the page renders the visitor, and the next of kin's name is rendered by the
/// community's person line, a partial of a model of its own (PersonName) rendered under the member's prefix, so its
/// input binds as NextOfKin.Name though no class of the page's model declares it.
/// </summary>
public sealed class ConciergePage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>A person's name, as the community's person line (PersonLine) renders it for whichever member holds one.</summary>
public sealed class PersonName
{
    public string? Name { get; set; }
}

/// <summary>The concierge's rules, which check the next of kin by the next of kin's own.</summary>
public sealed class ConciergeRules : ReactiveValidator<ConciergePage>
{
    public ConciergeRules()
    {
        RuleFor(x => x.NextOfKin).SetValidator(new KinRules());
    }
}

/// <summary>What a desk notes of a next of kin, whichever page sends it.</summary>
public sealed class KinNote
{
    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>
/// A desk midway through moving from the next of kin's own card to the community's person line: for now the page
/// renders both, so two inputs bind NextOfKin.Name.
/// </summary>
public sealed class ChangeoverPage
{
    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>
/// A visit and a sitting booked on one page, each in a form of its own: the visit's next of kin through the next
/// of kin's own card, the sitting's through the community's person line, both binding NextOfKin.Name.
/// </summary>
public sealed class BookingsPage
{
    public string? Visitor { get; set; }

    public KinCard NextOfKin { get; set; } = new();
}

/// <summary>The visit's rules: a visitor who brings a next of kin is named.</summary>
public sealed class BookingRules : ReactiveValidator<BookingsPage>
{
    public BookingRules()
    {
        RuleFor(x => x.Visitor).NotEmpty().WithMessage("Name the visitor who brings the next of kin.")
            .When(x => !string.IsNullOrEmpty(x.NextOfKin.Name));
    }
}

/// <summary>
/// A desk that corrects a resident's room and the family's phone on one page, each through a partial of a model of
/// its own (RoomCorrection, PhoneCorrection) whose plan is its own; both inputs bind Value.
/// </summary>
public sealed class CorrectionsPage
{
}

/// <summary>A correction of the resident's room.</summary>
public sealed class RoomCorrection
{
    public string? Value { get; set; }
}

/// <summary>A correction of the family's phone.</summary>
public sealed class PhoneCorrection
{
    public string? Value { get; set; }
}

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class PartialsEcho
{
    public string Summary { get; set; } = "";
}
