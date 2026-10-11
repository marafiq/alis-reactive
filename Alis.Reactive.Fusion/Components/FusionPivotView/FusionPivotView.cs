using System;
#if NET48
using System.Web.Mvc;
#else
using Microsoft.AspNetCore.Mvc.Rendering;
#endif
using Syncfusion.EJ2;
using Syncfusion.EJ2.PivotView;

namespace Alis.Reactive.Fusion.Components
{
    /// <summary>
    /// The Syncfusion PivotView, declared by the names its JavaScript API uses.
    /// </summary>
    /// <remarks>
    /// Render it with <c>Html.FusionPivotView(plan, "care-pivot", b =&gt; ...)</c> and target it with
    /// <c>p.Component&lt;FusionPivotView&gt;("care-pivot")</c>; every declared member works with <c>Set</c>,
    /// <c>Call</c> and <c>Get</c>, for example <c>.Set(c =&gt; c.DataSource, json.Read(x =&gt; x.Rows)).Call(c =&gt; c.Refresh)</c>.
    /// </remarks>
    public sealed class FusionPivotView : FusionComponent
    {
        /// <summary>The shown view: <c>"Table"</c> or <c>"Chart"</c>.</summary>
        public ComponentProperty<string> CurrentView => Property<string>("currentView");

        /// <summary>The rows the pivot summarizes; call <see cref="Refresh"/> after writing it.</summary>
        public ComponentProperty<object> DataSource => Property<object>("dataSourceSettings.dataSource");

        /// <summary>Re-renders the pivot from its data source.</summary>
        public ComponentMethod Refresh => Method("refresh");

        /// <summary>The layout as JSON; the argument says whether to leave the data source out.</summary>
        public ComponentFunction<bool, string> GetPersistData => Function<bool, string>("getPersistData");

        /// <summary>Restores a layout from its JSON.</summary>
        public ComponentMethod<string> LoadPersistData => Method<string>("loadPersistData");

        /// <summary>Opens the conditional formatting dialog.</summary>
        public ComponentMethod ShowConditionalFormattingDialog => Method("showConditionalFormattingDialog");

        /// <summary>Opens the calculated field dialog.</summary>
        public ComponentMethod CreateCalculatedFieldDialog => Method("createCalculatedFieldDialog");

        /// <summary>Raised after the pivot renders its data.</summary>
        public TypedEvent<FusionPivotViewDataBoundArgs> DataBound =>
            Event<FusionPivotViewDataBoundArgs>("dataBound");

        /// <summary>Raised when the user selects a cell.</summary>
        public TypedEvent<FusionPivotViewCellSelectingArgs> CellSelecting =>
            Event<FusionPivotViewCellSelectingArgs>("cellSelecting");
    }

    /// <summary>Payload of <see cref="FusionPivotView.DataBound"/>; carries no data.</summary>
    public class FusionPivotViewDataBoundArgs
    {
    }

    /// <summary>Payload of <see cref="FusionPivotView.CellSelecting"/>.</summary>
    public class FusionPivotViewCellSelectingArgs
    {
        /// <summary>The selected cell.</summary>
        public FusionPivotViewCellData Data { get; set; } = new FusionPivotViewCellData();
    }

    /// <summary>A pivot cell as the <see cref="FusionPivotView.CellSelecting"/> payload reports it.</summary>
    public class FusionPivotViewCellData
    {
        /// <summary>The axis the cell belongs to.</summary>
        public string Axis { get; set; } = "";

        /// <summary>The cell's field text.</summary>
        public string ActualText { get; set; } = "";

        /// <summary>The cell text as shown.</summary>
        public string FormattedText { get; set; } = "";

        /// <summary>The cell value as shown.</summary>
        public decimal Value { get; set; }

        /// <summary>The cell value before formatting.</summary>
        public decimal ActualValue { get; set; }

        /// <summary>Row index of the cell.</summary>
        public int RowIndex { get; set; }

        /// <summary>Column index of the cell.</summary>
        public int ColIndex { get; set; }

        /// <summary>Row headers of the cell.</summary>
        public string RowHeaders { get; set; } = "";

        /// <summary>Column headers of the cell.</summary>
        public string ColumnHeaders { get; set; } = "";

        /// <summary>Whether the cell is a subtotal.</summary>
        public bool IsSum { get; set; }

        /// <summary>Whether the cell is a grand total.</summary>
        public bool IsGrandSum { get; set; }
    }

    /// <summary>
    /// Factory extension for rendering Syncfusion PivotView with reactive event support.
    /// </summary>
    public static class FusionPivotViewHtmlExtensions
    {
        public static RenderedComponent<global::Alis.Reactive.Fusion.Components.FusionPivotView, TModel> FusionPivotView<TModel>(
#if NET48
            this HtmlHelper<TModel> html,
#else
            this IHtmlHelper<TModel> html,
#endif
            ReactivePlan<TModel> plan,
            string elementId,
            Action<PivotViewBuilder> build)
            where TModel : class
        {
            var builder = html.EJS().PivotView(elementId);
            build(builder);

            return plan.Render<global::Alis.Reactive.Fusion.Components.FusionPivotView>(elementId, builder.Render());
        }
    }
}
