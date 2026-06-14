# FusionBulletChart MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `BulletChart`
MVC builder: `BulletChartBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 52 |
| JS members with matching builder method | 49 |
| JS members without matching builder method | 44 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `Animation` | `Syncfusion.EJ2.Charts.BulletChartAnimation` |
| `BeforePrint` | `System.String` |
| `Border` | `Syncfusion.EJ2.Charts.BulletChartContainerBorder` |
| `BulletChartMouseClick` | `System.String` |
| `CategoryField` | `System.String` |
| `CategoryLabelStyle` | `Syncfusion.EJ2.Charts.BulletChartBulletLabelStyle` |
| `DataLabel` | `Syncfusion.EJ2.Charts.BulletChartBulletDataLabel` |
| `DataSource` | `System.Object` |
| `EnableGroupSeparator` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `Height` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Interval` | `System.Double` |
| `LabelFormat` | `System.String` |
| `LabelPosition` | `Syncfusion.EJ2.Charts.LabelsPlacement` |
| `LabelStyle` | `Syncfusion.EJ2.Charts.BulletChartBulletLabelStyle` |
| `LegendRender` | `System.String` |
| `LegendSettings` | `Syncfusion.EJ2.Charts.BulletChartBulletChartLegendSettings` |
| `Load` | `System.String` |
| `Loaded` | `System.String` |
| `Locale` | `System.String` |
| `MajorTickLines` | `Syncfusion.EJ2.Charts.BulletChartMajorTickLines` |
| `Margin` | `Syncfusion.EJ2.Charts.BulletChartMargin` |
| `Maximum` | `System.Double` |
| `Minimum` | `System.Double` |
| `MinorTickLines` | `Syncfusion.EJ2.Charts.BulletChartMinorTickLines` |
| `MinorTicksPerInterval` | `System.Double` |
| `OpposedPosition` | `System.Boolean` |
| `Orientation` | `Syncfusion.EJ2.Charts.OrientationType` |
| `Query` | `System.String` |
| `Ranges` | `System.Collections.Generic.List{Syncfusion.EJ2.Charts.Range}` |
| `Subtitle` | `System.String` |
| `SubtitleStyle` | `Syncfusion.EJ2.Charts.BulletChartBulletLabelStyle` |
| `TabIndex` | `System.Double` |
| `TargetColor` | `System.String` |
| `TargetField` | `System.String` |
| `TargetTypes` | `System.Object` |
| `TargetWidth` | `System.Double` |
| `Theme` | `Syncfusion.EJ2.Charts.ChartTheme` |
| `TickPosition` | `Syncfusion.EJ2.Charts.TickPosition` |
| `Title` | `System.String` |
| `TitlePosition` | `Syncfusion.EJ2.Charts.TextPosition` |
| `TitleStyle` | `Syncfusion.EJ2.Charts.BulletChartBulletLabelStyle` |
| `Tooltip` | `Syncfusion.EJ2.Charts.BulletChartBulletTooltipSettings` |
| `TooltipRender` | `System.String` |
| `Type` | `Syncfusion.EJ2.Charts.FeatureType` |
| `ValueBorder` | `Syncfusion.EJ2.Charts.BulletChartValueBorder` |
| `ValueField` | `System.String` |
| `ValueFill` | `System.String` |
| `ValueHeight` | `System.Double` |
| `Width` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `animateSeries` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `animation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `availableSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `beforePrint` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `border` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `bottomSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `bulletAxis` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `bulletChartLegendModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `bulletChartMouseClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `bulletChartOnMouseClick` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `bulletChartRect` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `bulletMouseLeave` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `bulletTooltipModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `categoryField` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `categoryLabelStyle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `chartKeyDown` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `chartKeyUp` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `containerHeight` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `containerWidth` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `createSvg` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `dataLabel` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dataModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `dataSource` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `delayRedraw` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `enableGroupSeparator` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `format` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `getActualIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getBulletBounds` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `getMaxLabelWidth` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `height` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `initialClipRect` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `interval` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `intervalDivs` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `intl` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `isTouch` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `labelFormat` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `labelPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `labelStyle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `leftSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `legendRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `legendSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `load` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `loaded` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `locale` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `majorTickLines` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `margin` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `maximum` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `maxLabelSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `maxTitleSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `minimum` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minorTickLines` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minorTicksPerInterval` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `mouseX` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `mouseY` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `opposedPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `orientation` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `print` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `rangeCollection` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `ranges` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `redraw` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `removeSvg` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `renderer` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `resizeBound` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `rightSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `scale` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `setTabIndex` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `subtitle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `subtitleStyle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `svgObject` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `tabIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `targetColor` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `targetField` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `targetTypes` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `targetWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `theme` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `themeStyle` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `tickPosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `title` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `titlePosition` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `titleStyle` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltip` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `tooltipRender` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `topSize` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueBorder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueField` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueFill` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `valueHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `visibleRanges` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
