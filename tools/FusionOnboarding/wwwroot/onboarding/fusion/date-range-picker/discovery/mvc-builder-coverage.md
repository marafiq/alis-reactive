# FusionDateRangePicker MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `DateRangePicker`
MVC builder: `DateRangePickerBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 47 |
| JS members with matching builder method | 39 |
| JS members without matching builder method | 11 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowEdit` | `System.Boolean` |
| `Blur` | `System.String` |
| `Change` | `System.String` |
| `Cleared` | `System.String` |
| `Close` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DayHeaderFormat` | `Syncfusion.EJ2.Calendars.DayHeaderFormats` |
| `Depth` | `Syncfusion.EJ2.Calendars.CalendarView` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EndDate` | `System.Object` |
| `FirstDayOfWeek` | `System.Double` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `Format` | `System.String` |
| `FullScreenMode` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `InputFormats` | `System.String[]` |
| `KeyConfigs` | `System.Object` |
| `Locale` | `System.String` |
| `Max` | `System.Object` |
| `MaxDays` | `System.Int32` |
| `Min` | `System.Object` |
| `MinDays` | `System.Int32` |
| `Navigated` | `System.String` |
| `Open` | `System.String` |
| `OpenOnFocus` | `System.Boolean` |
| `Placeholder` | `System.String` |
| `Presets` | `System.Collections.Generic.List{Syncfusion.EJ2.Calendars.DateRangePickerPreset}` |
| `RenderDayCell` | `System.String` |
| `Select` | `System.String` |
| `Separator` | `System.String` |
| `ServerTimezoneOffset` | `System.Double` |
| `ShowClearButton` | `System.Boolean` |
| `Start` | `Syncfusion.EJ2.Calendars.CalendarView` |
| `StartDate` | `System.Object` |
| `StrictMode` | `System.Boolean` |
| `Value` | `System.Object` |
| `Value` | `Syncfusion.EJ2.Calendars.DateRangePickerDateRange` |
| `WeekNumber` | `System.Boolean` |
| `WeekRule` | `Syncfusion.EJ2.Calendars.WeekRule` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `ZIndex` | `System.Int32` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `allowEdit` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `calendarMode` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cleared` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `currentView` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `depth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `endDate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `firstDayOfWeek` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `format` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fullScreenMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `getSelectedRange` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hide` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `inputFormats` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `max` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `maxDays` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `min` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minDays` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `navigated` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `navigateTo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `openOnFocus` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `presets` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `renderDayCell` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `separator` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `show` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showTodayButton` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `start` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `startDate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `strictMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `weekNumber` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
