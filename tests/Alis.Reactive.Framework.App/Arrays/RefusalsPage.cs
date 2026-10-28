namespace Alis.Reactive.Framework.App.Arrays;

/// <summary>A page whose plan makes one mistake an array operation cannot run in the browser, so it is refused when it renders.</summary>
public sealed class RefusalsPage
{
    public Mistake Mistake { get; set; }
}

/// <summary>The mistakes an array lambda can make.</summary>
public enum Mistake
{
    OrderByARecord,
    CallAMethodWithNoJavaScriptForm,
    ComputeAProjection,
    TestTextAgainstTheItem,
    TestTextAgainstACharacter,
    CompareWithoutTheItem,
    CastToAFlag,
    ReadAMemberWithNoJavaScriptForm,
    ReadADayOfTheWeekAsAValue,
    CastToAWholeNumber,
    CastToANarrowerNumber,
}
