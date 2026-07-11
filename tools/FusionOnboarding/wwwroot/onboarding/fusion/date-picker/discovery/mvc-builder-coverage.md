# FusionDatePicker MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `DatePicker`
MVC builder: `DatePickerBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 43 |
| JS members with matching builder method | 25 |
| JS members without matching builder method | 12 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `AllowEdit` | `System.Boolean` |
| `Blur` | `System.String` |
| `CalendarMode` | `Syncfusion.EJ2.Calendars.CalendarType` |
| `Change` | `System.String` |
| `Cleared` | `System.String` |
| `Close` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DayHeaderFormat` | `Syncfusion.EJ2.Calendars.DayHeaderFormats` |
| `Depth` | `Syncfusion.EJ2.Calendars.CalendarView` |
| `Destroyed` | `System.String` |
| `Enabled` | `System.Boolean` |
| `EnableMask` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `FirstDayOfWeek` | `System.Int32` |
| `FloatLabelType` | `Syncfusion.EJ2.Inputs.FloatLabelType` |
| `Focus` | `System.String` |
| `Format` | `System.String` |
| `FullScreenMode` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `InputFormats` | `System.String[]` |
| `KeyConfigs` | `System.Object` |
| `Locale` | `System.String` |
| `MaskPlaceholder` | `Syncfusion.EJ2.Calendars.DatePickerMaskPlaceholder` |
| `Max` | `System.Object` |
| `Min` | `System.Object` |
| `Navigated` | `System.String` |
| `Open` | `System.String` |
| `OpenOnFocus` | `System.Boolean` |
| `Placeholder` | `System.String` |
| `RenderDayCell` | `System.String` |
| `ServerTimezoneOffset` | `System.Double` |
| `ShowClearButton` | `System.Boolean` |
| `ShowTodayButton` | `System.Boolean` |
| `Start` | `Syncfusion.EJ2.Calendars.CalendarView` |
| `StrictMode` | `System.Boolean` |
| `Value` | `System.Object` |
| `WeekNumber` | `System.Boolean` |
| `WeekRule` | `Syncfusion.EJ2.Calendars.WeekRule` |
| `Width` | `System.String` |
| `Width` | `System.Double` |
| `ZIndex` | `System.Int32` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `addDate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `allowEdit` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `blur` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cleared` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `close` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `currentView` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `enabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableMask` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `floatLabelType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `focus` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `focusIn` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `focusOut` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `format` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fullScreenMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hide` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `inputFormats` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `isMultiSelection` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `maskPlaceholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `navigateTo` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `open` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `openOnFocus` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `placeholder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `readonly` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `serverTimezoneOffset` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `show` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showClearButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `strictMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `values` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `width` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `zIndex` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
