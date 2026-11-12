using Alis.Reactive.Conformance.World;

namespace Alis.Reactive.Conformance.App.Refusals;

public static class ArrayLambdaEndpoints
{
    private const string Root = RefusalPages.Area + "array-lambdas/";

    public const string CensusResidents = Root + "census/residents";

    public const string DosesByWard = Root + "doses-by-ward/doses";

    public const string DosesByPriority = Root + "doses-by-priority/doses";

    public const string ChartedDoses = Root + "charted-doses/doses";

    public const string SuppliedDoses = Root + "supplied-doses/doses";

    public const string SealedDoses = Root + "sealed-doses/doses";

    public const string TaggedDoses = Root + "sealed-doses/tagged-doses";

    public const string PlannedDoses = Root + "planned-doses/doses";

    public const string ConvertedVisits = Root + "visits/converted-visits";

    public const string Visits = Root + "visits/visits";

    public const string Copays = Root + "copays/doses";
}
