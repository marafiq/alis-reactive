using Alis.Reactive.FluentValidator;

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

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class PartialsEcho
{
    public string Summary { get; set; } = "";
}
