#if NET48
using System;
using System.Collections.Generic;
using System.Web.Mvc;
using Alis.Reactive.RenderingAndDelivery;
using Syncfusion.EJ2;
using Syncfusion.EJ2.QueryBuilder;

namespace Alis.Reactive.Fusion.FusionComponents;

/// <summary>Renders the EJ2 query builder in a view.</summary>
public static class FusionQueryBuilderHtml
{
    /// <summary>Renders a filter editor for the supplied rows under the view’s chosen id.</summary>
    /// <param name="html">The view’s HTML helper.</param>
    /// <param name="plan">The view’s plan.</param>
    /// <param name="elementId">The id of the editor.</param>
    /// <param name="rows">The local data source, with its fields named as the application writes them.</param>
    /// <param name="build">Configures the editor’s columns, initial rules and options.</param>
    /// <returns>The rendered query builder.</returns>
    public static Rendered<FusionQueryBuilder<TRow>, TModel> FusionQueryBuilder<TModel, TRow>(this HtmlHelper<TModel> html, ReactivePlan<TModel> plan, string elementId, IReadOnlyList<TRow> rows, Action<QueryBuilderBuilder> build)
    {
        if (plan is null) throw new ArgumentNullException(nameof(plan));
        if (elementId is null) throw new ArgumentNullException(nameof(elementId));
        if (rows is null) throw new ArgumentNullException(nameof(rows));
        if (build is null) throw new ArgumentNullException(nameof(build));
        var editor = html.EJS().QueryBuilder(elementId);
        build(editor);
        editor.DataSource(FusionData.AsWritten(plan.Json(rows)));
        return plan.Render<FusionQueryBuilder<TRow>>(elementId, editor.Render());
    }
}
#endif
