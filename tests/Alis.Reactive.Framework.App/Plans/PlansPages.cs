using Alis.Reactive.FluentValidator;

namespace Alis.Reactive.Framework.App.Plans;

/// <summary>What an endpoint bound, in its own words.</summary>
public sealed class Booked
{
    public string Summary { get; set; } = "";
}

/// <summary>The contacts of a move-in: two cards of one model, each starting a plan of its own and saved on its own.</summary>
public sealed class ContactsPage
{
    public ContactCard NextOfKin { get; set; } = new();

    public ContactCard Alternate { get; set; } = new();
}

/// <summary>One contact: a card rendered for one member of the contacts.</summary>
public sealed class ContactCard
{
    public string? Name { get; set; }
}

/// <summary>The keyholders of a room: a card for the primary keyholder, saved by the card's own plan and checked by the server.</summary>
public sealed class KeyholdersPage
{
    public KeyholderCard Primary { get; set; } = new();
}

/// <summary>A keyholder: the card renders the name; how they are related is the server's to ask for.</summary>
public sealed class KeyholderCard
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }
}

/// <summary>The keyholder card's rule: a name the browser can check is given.</summary>
public sealed class KeyholderRules : ReactiveValidator<KeyholderCard>
{
    public KeyholderRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the keyholder.");
    }
}

/// <summary>A noticeboard with a slot a notice is pinned into.</summary>
public sealed class NoticeboardPage;

/// <summary>A notice pinned to the board: a partial that starts two plans of its model, the notice's and its share button's.</summary>
public sealed class Notice;

/// <summary>A page that makes one mistake a plan cannot take, so it is refused when it renders.</summary>
public sealed class PlanRefusalsPage
{
    public PlanMistake Mistake { get; set; }
}

/// <summary>The mistakes a page can make in what it names and in the behavior it writes.</summary>
public enum PlanMistake
{
    RenderAComponentWithNoId,
    NameAPluginWithASpace,
    GiveAPluginNoName,
    GiveAHeaderNoName,
    NameARoutePlaceholderWithADash,
    RouteAStatusHttpDoesNotHave,
    ReadAnotherPlansComponent,
    ReadAResponseOutsideItsRoutes,
    ReadTheResponseInFinally,
    LoadTheResponseInFinally,
    RouteASuccessTwice,
    RouteEveryErrorTwice,
    RouteAStatusTwice,
    ChainTwoRequests,
    WriteAStepAfterItsCallbackReturned,
    RegisterAPluginTwice,
}

/// <summary>A night log the page registers as a plugin.</summary>
public sealed class NightLog : Plugin
{
    public NightLog()
        : base("night-log")
    {
    }
}

/// <summary>A night log the page registers as a plugin under a name with a space in it.</summary>
public sealed class SpacedNightLog : Plugin
{
    public SpacedNightLog()
        : base("night log")
    {
    }
}

/// <summary>A night log the page registers as a plugin under no name.</summary>
public sealed class UnnamedNightLog : Plugin
{
    public UnnamedNightLog()
        : base("")
    {
    }
}
