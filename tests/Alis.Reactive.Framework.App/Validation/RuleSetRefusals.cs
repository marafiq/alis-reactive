namespace Alis.Reactive.Framework.App.Validation;

/// <summary>
/// The mistakes a validation integration can make in building its browser rules by hand (BrowserRuleSet,
/// BrowserRule), each refused where the page that describes the rules renders.
/// </summary>
public enum RuleSetMistake
{
    NameARuleForACallOnAMember,
    DeclareItemsOfAMemberThatHoldsNone,
    BoundARangeByTwoKindsOfValue,
}

/// <summary>A page whose rules are built by hand, wrongly, the way its mistake asks.</summary>
public sealed class RuleSetRefusalsPage
{
    public RuleSetMistake Mistake { get; set; }
}

/// <summary>The condition with no browser form an escort request's rules sit under.</summary>
public enum EscortUnreadable
{
    LengthOfText,
    OrderAgainstNothing,
    TextEquals,
}
