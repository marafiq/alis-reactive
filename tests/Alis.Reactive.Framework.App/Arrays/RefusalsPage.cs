namespace Alis.Reactive.Framework.App.Arrays;

public sealed class RefusalsPage
{
    public Mistake Mistake { get; set; }
}

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
    CompareALengthOfTime,
    OrderByALengthOfTime,
}
