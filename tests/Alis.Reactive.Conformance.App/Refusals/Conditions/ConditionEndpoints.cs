using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Refusals;

public static class ConditionEndpoints
{
    private const string Root = RefusalPages.Area + "conditions/";

    public const string DosesOfATextStrength = Root + "dose-strength/doses-written-as-text";

    public const string DosesOfAStrength = Root + "dose-strength/doses";

    public const string ResidentsAtConvertedAddresses = Root + "resident-address/residents-its-converter-writes";

    public const string ResidentsAtAddresses = Root + "resident-address/residents";

    public const string StatusesInTwoForms = Root + "dose-statuses/lists-in-two-forms";

    public const string StatusesInOneForm = Root + "dose-statuses/lists";

    public const string DosesGivenAtConvertedTimes = Root + "dose-given/doses-its-converter-writes";

    public const string DosesGivenAtTimes = Root + "dose-given/doses";
}
