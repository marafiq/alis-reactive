using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.PlaywrightTests.Harness;

public static class Hosts
{
    public const string Net10Name = HostNames.Net10;

    public static readonly string[] Net10 = [Net10Name];

    public const string NumbersAsTextName = HostNames.NumbersAsText;

    public static readonly string[] NumbersAsText = [NumbersAsTextName];

    public const string DateConverterName = HostNames.DateConverter;

    public static readonly string[] DateConverter = [DateConverterName];

    public const string SnakeCaseName = HostNames.SnakeCase;

    public static readonly string[] SnakeCase = [SnakeCaseName];

    public const string PascalCaseName = HostNames.PascalCase;

    public static readonly string[] PascalCase = [PascalCaseName];

    public const string EnumsAsNumbersName = HostNames.EnumsAsNumbers;

    public static readonly string[] EnumsAsNumbers = [EnumsAsNumbersName];
}
