namespace Alis.Reactive.Framework.App.Validation;

public enum RuleSetMistake
{
    NameARuleForACallOnAMember,
    RuleTheItemsOfAMemberThatHoldsNone,
    BoundARangeByTwoKindsOfValue,
}

public sealed class RuleSetRefusalsPage
{
    public RuleSetMistake Mistake { get; set; }
}

public enum EscortUnreadable
{
    TrimmedText,
    OrderAgainstNothing,
    TextEquals,
}
