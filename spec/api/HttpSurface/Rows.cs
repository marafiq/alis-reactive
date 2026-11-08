using System;

namespace Alis.Reactive.Hosting;

public sealed class Rows<TModel, TRow>
{
    private Rows()
    {
    }
}

public static class PlanRows
{
    public static Rows<TModel, TRow> Rows<TModel, TRow>(this ReactivePlan<TModel> plan, string rowsId) => throw new NotSupportedException();
}

public sealed class PlanVerbs<TModel>
{
    internal PlanVerbs()
    {
    }

    public Rows<TModel, TRow> Rows<TRow>(string rowsId) => throw new NotSupportedException();
}
