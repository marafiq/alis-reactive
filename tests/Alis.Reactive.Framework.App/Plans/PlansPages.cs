
namespace Alis.Reactive.Framework.App.Plans;

public sealed class Booked
{
    public string Summary { get; set; } = "";
}

public sealed class ContactsPage
{
    public ContactCard NextOfKin { get; set; } = new();

    public ContactCard Alternate { get; set; } = new();
}

public sealed class ContactCard
{
    public string? Name { get; set; }
}

public sealed class KeyholdersPage
{
    public KeyholderCard Primary { get; set; } = new();
}

public sealed class KeyholderCard
{
    public string? Name { get; set; }

    public string? Relationship { get; set; }
}

public sealed class KeyholderRules : ReactiveValidator<KeyholderCard>
{
    public KeyholderRules()
    {
        RuleFor(x => x.Name).NotEmpty().WithMessage("Name the keyholder.");
    }
}

public sealed class NoticeboardPage;

public sealed class Notice;

public sealed class PlanRefusalsPage
{
    public PlanMistake Mistake { get; set; }
}

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
    ShowValidationErrorsOutsideARequest,
    ReadTheResponseInFinally,
    LoadTheResponseInFinally,
    RouteASuccessTwice,
    RouteEveryErrorTwice,
    RouteAStatusTwice,
    ChainTwoRequests,
    WriteAStepAfterItsCallbackReturned,
    RegisterAPluginTwice,
    HandOverARoundThatHoldsItself,
    HandOverARoundThatHoldsAComputation,
}

public static class RoundEvents
{
    public static readonly AppEvent<HandedRound> Handed = AppEvent.Declare<HandedRound>();
}

public sealed class HandedRound
{
    public string Ward { get; set; } = "";

    public HandedRound? Next { get; set; }

    public Func<string>? Describe { get; set; }
}

public sealed class NightLog : Plugin
{
    public NightLog()
        : base("night-log")
    {
    }
}

public sealed class SpacedNightLog : Plugin
{
    public SpacedNightLog()
        : base("night log")
    {
    }
}

public sealed class UnnamedNightLog : Plugin
{
    public UnnamedNightLog()
        : base("")
    {
    }
}
