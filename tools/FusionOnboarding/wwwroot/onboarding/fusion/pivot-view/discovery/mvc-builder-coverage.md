# FusionPivotView MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `PivotView`
MVC builder: `PivotViewBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 94 |
| JS members with matching builder method | 88 |
| JS members without matching builder method | 158 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `AfterServiceInvoke` | `System.String` |
| `AggregateCellInfo` | `System.String` |
| `AggregateMenuOpen` | `System.String` |
| `AggregateTypes` | `System.Object` |
| `AllowCalculatedField` | `System.Boolean` |
| `AllowConditionalFormatting` | `System.Boolean` |
| `AllowDataCompression` | `System.Boolean` |
| `AllowDeferLayoutUpdate` | `System.Boolean` |
| `AllowDrillThrough` | `System.Boolean` |
| `AllowExcelExport` | `System.Boolean` |
| `AllowGrouping` | `System.Boolean` |
| `AllowNumberFormatting` | `System.Boolean` |
| `AllowPdfExport` | `System.Boolean` |
| `BeforeExport` | `System.String` |
| `BeforeServiceInvoke` | `System.String` |
| `BeginDrillThrough` | `System.String` |
| `CalculatedFieldCreate` | `System.String` |
| `CellClick` | `System.String` |
| `CellSelected` | `System.String` |
| `CellSelecting` | `System.String` |
| `CellTemplate` | `System.String` |
| `ChartSeriesCreated` | `System.String` |
| `ChartSettings` | `Syncfusion.EJ2.PivotView.PivotViewChartSettings` |
| `ChartTypes` | `System.Object` |
| `ConditionalFormatting` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DataBound` | `System.String` |
| `DataSourceSettings` | `Syncfusion.EJ2.PivotView.PivotViewDataSourceSettings` |
| `Destroyed` | `System.String` |
| `DisplayOption` | `Syncfusion.EJ2.PivotView.PivotViewDisplayOption` |
| `Drill` | `System.String` |
| `DrillThrough` | `System.String` |
| `EditCompleted` | `System.String` |
| `EditSettings` | `Syncfusion.EJ2.PivotView.PivotViewCellEditSettings` |
| `EnableFieldSearching` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePaging` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EnableValueSorting` | `System.Boolean` |
| `EnableVirtualization` | `System.Boolean` |
| `EnginePopulated` | `System.String` |
| `EnginePopulating` | `System.String` |
| `ExportAllPages` | `System.Boolean` |
| `ExportComplete` | `System.String` |
| `FetchReport` | `System.String` |
| `FieldDragStart` | `System.String` |
| `FieldDrop` | `System.String` |
| `FieldListRefreshed` | `System.String` |
| `FieldRemove` | `System.String` |
| `GridSettings` | `Syncfusion.EJ2.PivotView.PivotViewGridSettings` |
| `GroupingBarSettings` | `Syncfusion.EJ2.PivotView.PivotViewGroupingBarSettings` |
| `Height` | `System.String` |
| `Height` | `System.Double` |
| `HtmlAttributes` | `System.Object` |
| `HyperlinkCellClick` | `System.String` |
| `HyperlinkSettings` | `Syncfusion.EJ2.PivotView.PivotViewHyperlinkSettings` |
| `Load` | `System.String` |
| `LoadOnDemandInMemberEditor` | `System.Boolean` |
| `LoadReport` | `System.String` |
| `Locale` | `System.String` |
| `MaxNodeLimitInMemberEditor` | `System.Double` |
| `MaxRowsInDrillThrough` | `System.Double` |
| `MemberEditorOpen` | `System.String` |
| `MemberFiltering` | `System.String` |
| `NewReport` | `System.String` |
| `NumberFormatting` | `System.String` |
| `OnFieldDropped` | `System.String` |
| `OnHeadersSort` | `System.String` |
| `OnPdfCellRender` | `System.String` |
| `PagerSettings` | `Syncfusion.EJ2.PivotView.PivotViewPagerSettings` |
| `PageSettings` | `Syncfusion.EJ2.PivotView.PivotViewPageSettings` |
| `PivotValues` | `System.Object` |
| `RemoveReport` | `System.String` |
| `RenameReport` | `System.String` |
| `SaveReport` | `System.String` |
| `ShowFieldList` | `System.Boolean` |
| `ShowGroupingBar` | `System.Boolean` |
| `ShowToolbar` | `System.Boolean` |
| `ShowTooltip` | `System.Boolean` |
| `ShowValuesButton` | `System.Boolean` |
| `SpinnerTemplate` | `System.String` |
| `Toolbar` | `System.Object` |
| `ToolbarClick` | `System.String` |
| `ToolbarRender` | `System.String` |
| `ToolbarTemplate` | `System.String` |
| `TooltipTemplate` | `System.String` |
| `VirtualScrollSettings` | `Syncfusion.EJ2.PivotView.PivotViewVirtualScrollSettings` |
| `Width` | `System.String` |
| `Width` | `System.Double` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionBeginMethod` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionCompleteMethod` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailureMethod` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `actionObj` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `addInternalEvents` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `afterServiceInvoke` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `aggregateCellInfo` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `aggregateMenuOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `aggregateTypes` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowCalculatedField` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowConditionalFormatting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowDataCompression` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowDeferLayoutUpdate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowDrillThrough` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowEngineExport` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `allowExcelExport` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowGrouping` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowNumberFormatting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `allowPdfExport` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `appendChartElement` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `appendHtml` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `applyColumnSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `applyFormatting` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `applyRowSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `axisFieldModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `beforeColumnsRender` | event | no | candidate: typed event; payload and browser gesture proof required |
| `beforeExport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beforeServiceInvoke` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beginDrillThrough` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `bindTriggerEvents` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `calculatedFieldCreate` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `calculatedFieldModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `cellClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellDeselected` | event | no | candidate: typed event; payload and browser gesture proof required |
| `cellSelected` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellSelecting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cellTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `chart` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `chartExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `chartExportModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `chartSeriesCreated` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `chartSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `chartTypes` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `clearSelection` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `clonedDataSet` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `clonedReport` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `commonModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `conditionalFormatting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `conditionalFormattingModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `contextMenuClick` | event | no | candidate: typed event; payload and browser gesture proof required |
| `contextMenuModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `contextMenuOpen` | event | no | candidate: typed event; payload and browser gesture proof required |
| `copy` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `createCalculatedFieldDialog` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `csvExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `currencyCode` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `currentAction` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `currentView` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `dataBound` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `dataSourceSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataType` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `defaultFieldListOrder` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `destroyEngine` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `displayOption` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `drill` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `drillThrough` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `drillThroughElement` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `drillThroughModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `drillThroughValue` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `editCompleted` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `editSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableFieldSearching` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePaging` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableValueSorting` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableVirtualization` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `encodeHtml` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `engineModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `enginePopulated` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enginePopulating` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `excelExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `excelExportModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `exportAllPages` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `exportAsPivot` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `exportComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `exportSpecifiedPages` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `exportType` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `fetchReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fieldDragStart` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fieldDrop` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fieldListModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `fieldListRefreshed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fieldListSpinnerElement` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `fieldRemove` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `fillGridColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `filterTargetID` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `firstColWidth` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `getActionCompleteName` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getAfterServiceInvoke` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getAllSummaryType` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getBeforeServiceInvoke` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getCellTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getChartSettings` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getEngine` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getGridWidthAsNumber` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeaderField` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeaderSortInfo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getHeightAsNumber` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getPageSettings` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getRowText` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getStackedColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getTooltipTemplate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getValuesHeader` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getWidthAsNumber` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `globalize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `grid` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `gridSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `groupingBarModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `groupingBarSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `groupingModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `guid` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hideWaitingPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `horizontalScrollScale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `hyperlinkCellClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `hyperlinkSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `initEngine` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `isAdaptive` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isChartLoaded` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isColumnCellHyperlink` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isDragging` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isEmptyGrid` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isInitial` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isModified` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isRowCellHyperlink` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isScrolling` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isServerWaitingPopup` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isStaticFieldList` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isStaticRefresh` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isSummaryCellHyperlink` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isTabular` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isTouchMode` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isValueCellHyperlink` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isWindowResized` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `keyboardModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastAggregationInfo` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastCalcFieldInfo` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastCellClicked` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastColumn` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastFilterInfo` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastGridSettings` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `lastSortInfo` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `layoutRefresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `load` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `loadOnDemandInMemberEditor` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `loadPersistData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `loadReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `localeObj` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `maxNodeLimitInMemberEditor` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `maxRowsInDrillThrough` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `memberEditorOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `memberFiltering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `minHeight` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `minWidth` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `mouseEventArgs` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `newReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `notEmpty` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `numberFormatting` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `numberFormattingModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `olapEngineModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `onContentReady` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `onDrill` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `onFieldDropped` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `onHeadersSort` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `onPdfCellRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `onWindowResize` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pagerModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pagerSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pageSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pdfExport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `pdfExportModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotButtonModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotChartModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotColumns` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotCommon` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotDeferLayoutUpdate` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotFieldListModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `pivotValues` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `pivotView` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `posCount` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `printChart` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refresh` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `refreshData` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeInternalEvents` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `removeReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `renameReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `renderContextMenu` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `renderModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `renderPivotGrid` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizedValue` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `resizeInfo` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `rowDeselected` | event | no | candidate: typed event; payload and browser gesture proof required |
| `rowSelected` | event | no | candidate: typed event; payload and browser gesture proof required |
| `saveReport` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `scrollDirection` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `scrollerBrowserLimit` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `scrollPosObject` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `selected` | event | no | candidate: typed event; payload and browser gesture proof required |
| `setCommonColumnsWidth` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `setGridColumns` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showConditionalFormattingDialog` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showFieldList` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showGroupingBar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showNumberFormattingDialog` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showToolbar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showTooltip` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showValuesButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showWaitingPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `spinnerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `templateParser` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `toolbar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `toolbarClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `toolbarModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `toolbarRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `toolbarTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltip` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `tooltipTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `totColWidth` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `triggerColumnRenderEvent` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateDataSource` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updateGroupingReport` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `updatePageSettings` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `verticalScrollScale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `virtualDiv` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `virtualHeaderDiv` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `virtualscrollModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `virtualScrollSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
