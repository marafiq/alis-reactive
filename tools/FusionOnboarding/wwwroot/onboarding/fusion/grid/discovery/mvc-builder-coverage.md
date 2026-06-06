# FusionGrid MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Grid`
MVC builder: `GridBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/33.2.10/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 173 |
| JS members with matching builder method | 164 |
| JS members without matching builder method | 320 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `AdaptiveUIMode` | `Syncfusion.EJ2.Grids.AdaptiveMode` |
| `Aggregates` | `System.Collections.Generic.List{Syncfusion.EJ2.Grids.GridAggregate}` |
| `AllowExcelExport` | `System.Boolean` |
| `AllowFiltering` | `System.Boolean` |
| `AllowGrouping` | `System.Boolean` |
| `AllowKeyboard` | `System.Boolean` |
| `AllowMultiSorting` | `System.Boolean` |
| `AllowPaging` | `System.Boolean` |
| `AllowPdfExport` | `System.Boolean` |
| `AllowReordering` | `System.Boolean` |
| `AllowResizing` | `System.Boolean` |
| `AllowRowDragAndDrop` | `System.Boolean` |
| `AllowSelection` | `System.Boolean` |
| `AllowSorting` | `System.Boolean` |
| `AllowTextWrap` | `System.Boolean` |
| `AutoFit` | `System.Boolean` |
| `BatchAdd` | `System.String` |
| `BatchCancel` | `System.String` |
| `BatchDelete` | `System.String` |
| `BeforeAutoFill` | `System.String` |
| `BeforeBatchAdd` | `System.String` |
| `BeforeBatchDelete` | `System.String` |
| `BeforeBatchSave` | `System.String` |
| `BeforeCopy` | `System.String` |
| `BeforeCustomFilterOpen` | `System.String` |
| `BeforeDataBound` | `System.String` |
| `BeforeDetailTemplateDetach` | `System.String` |
| `BeforeExcelExport` | `System.String` |
| `BeforeOpenAdaptiveDialog` | `System.String` |
| `BeforeOpenColumnChooser` | `System.String` |
| `BeforePaste` | `System.String` |
| `BeforePdfExport` | `System.String` |
| `BeforePrint` | `System.String` |
| `BeginEdit` | `System.String` |
| `CellDeselected` | `System.String` |
| `CellDeselecting` | `System.String` |
| `CellEdit` | `System.String` |
| `CellSave` | `System.String` |
| `CellSaved` | `System.String` |
| `CellSelected` | `System.String` |
| `CellSelecting` | `System.String` |
| `CheckBoxChange` | `System.String` |
| `ChildGrid` | `System.Object` |
| `ClipMode` | `Syncfusion.EJ2.Grids.ClipMode` |
| `ColumnChooserSettings` | `Syncfusion.EJ2.Grids.GridColumnChooserSettings` |
| `ColumnDataStateChange` | `System.String` |
| `ColumnDeselected` | `System.String` |
| `ColumnDeselecting` | `System.String` |
| `ColumnDrag` | `System.String` |
| `ColumnDragStart` | `System.String` |
| `ColumnDrop` | `System.String` |
| `ColumnMenuClick` | `System.String` |
| `ColumnMenuClose` | `System.String` |
| `ColumnMenuItems` | `System.Object` |
| `ColumnMenuOpen` | `System.String` |
| `ColumnQueryMode` | `Syncfusion.EJ2.Grids.ColumnQueryModeType` |
| `Columns` | `System.Object` |
| `Columns` | `System.Collections.Generic.List{Syncfusion.EJ2.Grids.GridColumn}` |
| `Columns` | `System.String[]` |
| `ColumnSelected` | `System.String` |
| `ColumnSelecting` | `System.String` |
| `CommandClick` | `System.String` |
| `ContextMenuClick` | `System.String` |
| `ContextMenuClose` | `System.String` |
| `ContextMenuItems` | `System.Object` |
| `ContextMenuOpen` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `CurrentAction` | `System.Object` |
| `CurrentViewData` | `System.Object` |
| `DataBound` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSourceChanged` | `System.String` |
| `DataStateChange` | `System.String` |
| `Destroyed` | `System.String` |
| `DetailCollapse` | `System.String` |
| `DetailDataBound` | `System.String` |
| `DetailExpand` | `System.String` |
| `DetailTemplate` | `System.String` |
| `EditSettings` | `Syncfusion.EJ2.Grids.GridEditSettings` |
| `Ej2StatePersistenceVersion` | `System.String` |
| `EmptyRecordTemplate` | `System.String` |
| `EnableAdaptiveUI` | `System.Boolean` |
| `EnableAltRow` | `System.Boolean` |
| `EnableAutoFill` | `System.Boolean` |
| `EnableColumnSpan` | `System.Boolean` |
| `EnableColumnVirtualization` | `System.Boolean` |
| `EnableHeaderFocus` | `System.Boolean` |
| `EnableHover` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnableImmutableMode` | `System.Boolean` |
| `EnableInfiniteScrolling` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRowSpan` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableStickyHeader` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `EnableVirtualMaskRow` | `System.Boolean` |
| `ExcelAggregateQueryCellInfo` | `System.String` |
| `ExcelExportComplete` | `System.String` |
| `ExcelHeaderQueryCellInfo` | `System.String` |
| `ExcelQueryCellInfo` | `System.String` |
| `ExportDetailDataBound` | `System.String` |
| `ExportDetailTemplate` | `System.String` |
| `ExportGrids` | `System.String[]` |
| `ExportGroupCaption` | `System.String` |
| `FilterSettings` | `Syncfusion.EJ2.Grids.GridFilterSettings` |
| `FrozenColumns` | `System.Double` |
| `FrozenRows` | `System.Double` |
| `GridLines` | `Syncfusion.EJ2.Grids.GridLine` |
| `GroupSettings` | `Syncfusion.EJ2.Grids.GridGroupSettings` |
| `HeaderCellInfo` | `System.String` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HierarchyPrintMode` | `Syncfusion.EJ2.Grids.HierarchyGridPrintMode` |
| `HtmlAttributes` | `System.Object` |
| `InfiniteScrollSettings` | `Syncfusion.EJ2.Grids.GridInfiniteScrollSettings` |
| `IsRowPinned` | `System.Object` |
| `IsRowSelectable` | `System.Object` |
| `IsRowSelectable` | `System.String` |
| `KeyPressed` | `System.String` |
| `LazyLoadGroupCollapse` | `System.String` |
| `LazyLoadGroupExpand` | `System.String` |
| `Load` | `System.String` |
| `LoadingIndicator` | `Syncfusion.EJ2.Grids.GridLoadingIndicator` |
| `Locale` | `System.String` |
| `PagerTemplate` | `System.String` |
| `PageSettings` | `Syncfusion.EJ2.Grids.GridPageSettings` |
| `ParentDetails` | `System.Object` |
| `PdfAggregateQueryCellInfo` | `System.String` |
| `PdfExportComplete` | `System.String` |
| `PdfHeaderQueryCellInfo` | `System.String` |
| `PdfQueryCellInfo` | `System.String` |
| `PrintComplete` | `System.String` |
| `PrintMode` | `Syncfusion.EJ2.Grids.PrintMode` |
| `Query` | `System.String` |
| `QueryCellInfo` | `System.String` |
| `QueryString` | `System.String` |
| `RecordClick` | `System.String` |
| `RecordDoubleClick` | `System.String` |
| `ResizeSettings` | `Syncfusion.EJ2.Grids.GridResizeSettings` |
| `ResizeStart` | `System.String` |
| `ResizeStop` | `System.String` |
| `Resizing` | `System.String` |
| `RowDataBound` | `System.String` |
| `RowDeselected` | `System.String` |
| `RowDeselecting` | `System.String` |
| `RowDrag` | `System.String` |
| `RowDragStart` | `System.String` |
| `RowDragStartHelper` | `System.String` |
| `RowDrop` | `System.String` |
| `RowDropSettings` | `Syncfusion.EJ2.Grids.GridRowDropSettings` |
| `RowHeight` | `System.Double` |
| `RowRenderingMode` | `Syncfusion.EJ2.Grids.RowRenderingDirection` |
| `RowSelected` | `System.String` |
| `RowSelecting` | `System.String` |
| `RowTemplate` | `System.String` |
| `SearchSettings` | `Syncfusion.EJ2.Grids.GridSearchSettings` |
| `SelectedRowIndex` | `System.Double` |
| `SelectionSettings` | `Syncfusion.EJ2.Grids.GridSelectionSettings` |
| `ShowColumnChooser` | `System.Boolean` |
| `ShowColumnMenu` | `System.Boolean` |
| `ShowHider` | `System.Object` |
| `SortSettings` | `Syncfusion.EJ2.Grids.GridSortSettings` |
| `TextWrapSettings` | `Syncfusion.EJ2.Grids.GridTextWrapSettings` |
| `Toolbar` | `System.Object` |
| `ToolbarClick` | `System.String` |
| `ToolbarTemplate` | `System.String` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `adaptiveDlgTarget` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `adaptiveUIMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `addListener` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addMovableRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addNewRowFocus` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `addRecord` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `addShimmerEffect` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `aggregateModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `aggregates` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowExcelExport` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowFiltering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowGrouping` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowKeyboard` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowMultiSorting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowPaging` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowPdfExport` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowReordering` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowResizing` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowRowDragAndDrop` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowSelection` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowSorting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowTextWrap` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `applyBiggerTheme` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `applyTextWrap` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `ariaService` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `asyncTimeOut` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `autoFit` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `autoFitColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `batchAdd` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `batchAsyncUpdate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `batchCancel` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `batchDelete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `batchUpdate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `beforeAutoFill` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeBatchAdd` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeBatchDelete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeBatchSave` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeCopy` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeCustomFilterOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeDataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeDetailTemplateDetach` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeExcelExport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpenAdaptiveDialog` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeOpenColumnChooser` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforePaste` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforePdfExport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforePrint` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beginEdit` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `calculatePageSizeByParentHeight` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cellDeselected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellDeselecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellEdit` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellSave` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellSaved` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellSelected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellSelecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `changeDataSource` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `checkAllRows` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `checkBoxChange` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `childGrid` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clearCellSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearFiltering` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearGridActions` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearGrouping` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearRowSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clearSorting` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clipboardModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `clipMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `closeEdit` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `columnChooserModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `columnChooserSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `columnDataStateChange` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnDeselected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnDeselecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnDrag` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnDragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnDrop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnMenuClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnMenuClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnMenuItems` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `columnMenuModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `columnMenuOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnQueryMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `columns` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `columnSelected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `columnSelecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `commandClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `commandDelIndex` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `commonQuery` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `contentModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `contextMenuClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `contextMenuClose` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `contextMenuItems` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `contextMenuModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `contextMenuOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `copy` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `createColumnchooser` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `createTooltip` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `csvExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `currentAction` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `currentViewData` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dataReady` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `dataSource` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataSourceChanged` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dataStateChange` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `defaultChartLocale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `deleteRecord` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `deleteRow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroyTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `detailCollapse` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `detailCollapseAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `detailDataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `detailExpand` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `detailExpandAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `detailRowModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `detailTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `disableRowDD` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `disableSelectedRecords` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `editCell` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `editModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `editSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ej2StatePersistenceVersion` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `emptyRecordTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableAdaptiveUI` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableAltRow` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableAutoFill` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableColumnSpan` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableColumnVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableDeepCompare` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `enableHeaderFocus` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHover` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableImmutableMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableInfiniteScrolling` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableRowSpan` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableStickyHeader` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableToolbarItems` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `enableVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableVirtualMaskRow` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `endEdit` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `ensureModuleInjected` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `excelAggregateQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `excelExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `excelExportComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `excelExportModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `excelHeaderQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `excelQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `exportDetailDataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `exportDetailTemplate` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `exportGrid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `exportGrids` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `exportGroupCaption` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `extendRequiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterByColumn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `filterOperators` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `filterSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focusModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `freezeRefresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `frozenColumns` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `frozenRows` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getAllDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getAllFrozenDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getAllFrozenRightDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getAllMovableDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getBatchChanges` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getCellFromIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnByField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnByUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnChooserFooterTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnChooserHeaderTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnChooserTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnFieldNames` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnHeaderByField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnHeaderByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnHeaderByUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnIndexByField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnIndexByUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumnIndexesInView` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getContentTable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getCurrentViewRecords` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getCurrentVisibleColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getDataModule` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getDetailTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getEditFooterTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getEditHeaderTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getEditTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getEmptyRecordTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFilteredRecords` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFilterUIInfo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFooterContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFooterContentTable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getForeignKeyColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenHeaderTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenLeftColumnHeaderByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenLeftColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenLeftColumnsCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenLeftContentTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenLeftCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenMode` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightCellFromIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightColumnHeaderByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightColumnsCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightContentTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightHeader` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightHeaderTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightRowByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRightRowsObject` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getFrozenRowByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeaderContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeaderTable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeight` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHiddenColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getIndentCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getLocaleConstants` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMediaColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableCellFromIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableColumnHeaderByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableColumnsCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableContentTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableDataRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableHeaderTbody` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableRowByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMovableRowsObject` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getNormalizedColumnIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getPager` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getPinnedRowObjectByKey` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getPreviousRowData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getPrimaryKeyFieldNames` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getQuery` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowElementByUID` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowHeight` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowIndexByPrimaryKey` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowInfo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowObjectFromUID` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowsObject` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedColumnsUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedRecords` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedRowCellIndexes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedRowIndexes` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSelectedRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getStackedColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getStackedHeaderColumnByHeaderText` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getSummaryValues` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getTablesCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getUidByColumnField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getVisibleColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getVisibleFrozenColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getVisibleFrozenLeftCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getVisibleFrozenRightCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getVisibleMovableCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `goToPage` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `grabColumnByFieldFromAllCols` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `grabColumnByUidFromAllCols` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `gridLines` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `groupCollapseAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `groupColumn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `groupExpandAll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `groupModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `groupSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `headerCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `headerModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hideScroll` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hideSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hierarchyPrintMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `infiniteScrollModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `infiniteScrollSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `invokedFromMedia` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isAddNewRow` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isAutoFitColumns` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isAutoGen` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isAutoGenerateColumns` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isCheckBoxSelection` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isCollapseStateEnabled` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isContextMenuOpen` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isDetail` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isEdit` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isExportGrid` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isFrozenGrid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isInitialLoad` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isLastCellPrimaryKey` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `islazyloadRequest` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isManualRefresh` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isPersistSelection` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isPreventScrollEvent` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isRemote` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isRowDragable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isRowPinned` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isRowSelectable` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isSelectedRowIndexUpdating` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isSpan` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isVirtualAdaptive` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isWidgetsDestroyed` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `keyboardModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `keyPressed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `lazyLoadGroupCollapse` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `lazyLoadGroupExpand` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `lazyLoadRender` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `leftrightColumnWidth` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `load` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `loadingIndicator` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `localeObj` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lockcolPositionCount` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `log` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `mediaQueryUpdate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `mergePersistGridData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `openColumnChooser` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pageRequireRefresh` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pagerModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pagerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pageSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pageTemplateChange` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `parentDetails` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `partialSelectedIndexes` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `partialSelectedRecords` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pdfAggregateQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `pdfExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pdfExportComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `pdfExportModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pdfHeaderQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `pdfQueryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `pinnedTopRecords` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pinnedTopRowModels` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pinRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `preventAdjustColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `preventAutoFit` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `prevPageMoving` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `print` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `printComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `printGridParent` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `printMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `printModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `queryCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `queryString` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `recalcIndentWidth` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `recordClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `recordDoubleClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `recordsCount` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `refresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshDataSource` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshGroupCaptionFooterTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshHeader` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshReactColumnTemplateByUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshReactHeaderTemplateByUid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshReactTemplateTD` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeFilteredColsByField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeListener` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeMaskRow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeSortColumn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeTextWrap` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `renderModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `renderTemplates` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reorderColumnByIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reorderColumnByModel` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reorderColumnByTargetIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reorderColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `reorderModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `reorderRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `requireTemplateRef` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `resetFilterDlgPosition` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resetIndentWidth` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizeModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `resizeSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `resizeStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizeStop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `resizing` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDeselected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDeselecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDrag` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDragAndDropModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `rowDragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDragStartHelper` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDrop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowDropSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `rowHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `rowRenderingMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `rowSelected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowSelecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `rowTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `sanitize` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `saveCell` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `scrollModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `scrollPosition` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `search` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `searchModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `searchSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectCell` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectCells` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectCellsByRange` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectedRowIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectionModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `selectionSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `selectRow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectRowsByRange` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `selectVirtualRowOnAdd` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `serverCsvExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `serverExcelExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `serverPdfExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `serviceLocator` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `setCellValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setColumnIndexesInView` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setForeignKeyData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setFrozenCount` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridContentTable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridHeaderContent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridHeaderTable` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridPager` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setHeaderText` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setInjectedModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setProperties` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setRowData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showAdaptiveFilterDialog` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showAdaptiveSortDialog` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showColumnChooser` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showColumnMenu` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showHider` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showMaskRow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showResponsiveCustomColumnChooser` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showResponsiveCustomFilter` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showResponsiveCustomSort` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortColumn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `sortSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `startEdit` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `tableIndex` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `tapEvent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `textWrapSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toolbar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toolbarClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `toolbarModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `toolbarTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `totalDataRecordsCount` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `translateX` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `ungroupColumn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `unpinRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `unwireEvents` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateCell` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateDefaultCursor` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateExternalMessage` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateMediaColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateRow` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateRowValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateVisibleExpandCollapseRows` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `valueFormatterService` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `vcRows` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `vRows` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `widthService` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `wireEvents` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
