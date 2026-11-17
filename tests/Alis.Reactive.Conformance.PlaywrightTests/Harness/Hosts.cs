using Alis.Reactive.Conformance.World;
using NUnit.Framework;
using NUnit.Framework.Internal;

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

    public const string EnumNamesAloneName = HostNames.EnumNamesAlone;

    public static readonly string[] EnumNamesAlone = [EnumNamesAloneName];

    public const string JsonContractName = HostNames.JsonContract;

    public static readonly string[] JsonContract = [JsonContractName];

    public const string NoRequestsName = HostNames.NoRequests;

    public static readonly string[] NoRequests = [NoRequestsName];

    public const string NoAntiforgeryChoiceName = HostNames.NoAntiforgeryChoice;

    public static readonly string[] NoAntiforgeryChoice = [NoAntiforgeryChoiceName];

    public const string SiteHeaderName = HostNames.SiteHeader;

    public static readonly string[] SiteHeader = [SiteHeaderName];

    public const string AntiforgeryFieldName = HostNames.AntiforgeryField;

    public static readonly string[] AntiforgeryField = [AntiforgeryFieldName];

    public const string NoShowValidationErrorsName = HostNames.NoShowValidationErrors;

    public static readonly string[] NoShowValidationErrors = [NoShowValidationErrorsName];

    public const string ClassNamesName = HostNames.ClassNames;

    public static readonly string[] ClassNames = [ClassNamesName];

    public const string DefaultHooksName = HostNames.DefaultHooks;

    public static readonly string[] DefaultHooks = [DefaultHooksName];

    public const string TracingName = HostNames.Tracing;

    public static readonly string[] Tracing = [TracingName];

    public const string TracingSinkThrowsName = HostNames.TracingSinkThrows;

    public static readonly string[] TracingSinkThrows = [TracingSinkThrowsName];

    public const string RealtimeNoOfferName = HostNames.RealtimeNoOffer;

    public static readonly string[] RealtimeNoOffer = [RealtimeNoOfferName];

    public const string NoReconnectComponentName = HostNames.NoReconnectComponent;

    public static readonly string[] NoReconnectComponent = [NoReconnectComponentName];

    public const string NoReconnectOfferName = HostNames.NoReconnectOffer;

    public static readonly string[] NoReconnectOffer = [NoReconnectOfferName];

    public const string Net48Name = HostNames.Net48;

    public static readonly string[] Net48 = [Net48Name];

    public const string ConsumerNet10Name = HostNames.ConsumerNet10;

    public static readonly string[] ConsumerNet10 = [ConsumerNet10Name];

    public const string ConsumerNet48Name = HostNames.ConsumerNet48;

    public static readonly string[] ConsumerNet48 = [ConsumerNet48Name];

    public static readonly TestFixtureData[] Net10AndNet48 = [Under(Net10Name), Under(Net48Name)];

    private static TestFixtureData Under(string host)
    {
        var instance = new TestFixtureData(host);
        instance.Properties.Add(PropertyNames.Category, host);
        return instance;
    }
}
