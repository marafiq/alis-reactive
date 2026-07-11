# FusionInPlaceEditor MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `InPlaceEditor`
MVC builder: `InPlaceEditorBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 44 |
| JS members with matching builder method | 35 |
| JS members without matching builder method | 18 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionFailure` | `System.String` |
| `ActionOnBlur` | `Syncfusion.EJ2.InPlaceEditor.ActionBlur` |
| `ActionSuccess` | `System.String` |
| `Adaptor` | `Syncfusion.EJ2.InPlaceEditor.AdaptorType` |
| `BeforeSanitizeHtml` | `System.String` |
| `BeginEdit` | `System.String` |
| `CancelButton` | `System.Object` |
| `CancelClick` | `System.String` |
| `Change` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `Destroyed` | `System.String` |
| `Disabled` | `System.Boolean` |
| `EditableOn` | `Syncfusion.EJ2.InPlaceEditor.EditableType` |
| `EmptyText` | `System.String` |
| `EnableEditMode` | `System.Boolean` |
| `EnableHtmlParse` | `System.Boolean` |
| `EnableHtmlSanitizer` | `System.Boolean` |
| `EnablePersistence` | `System.Boolean` |
| `EnableRtl` | `System.Boolean` |
| `EndEdit` | `System.String` |
| `HtmlAttributes` | `System.Object` |
| `Locale` | `System.String` |
| `Mode` | `Syncfusion.EJ2.InPlaceEditor.RenderMode` |
| `Name` | `System.String` |
| `PopupSettings` | `Syncfusion.EJ2.InPlaceEditor.InPlaceEditorPopupSettings` |
| `PrimaryKey` | `System.String` |
| `PrimaryKey` | `System.Double` |
| `SaveButton` | `System.Object` |
| `ShowButtons` | `System.Boolean` |
| `SubmitClick` | `System.String` |
| `SubmitOnEnter` | `System.Boolean` |
| `Template` | `System.String` |
| `TextOption` | `Syncfusion.EJ2.InPlaceEditor.TextOptionType` |
| `Type` | `Syncfusion.EJ2.InPlaceEditor.InputType` |
| `Url` | `System.String` |
| `Validating` | `System.String` |
| `ValidationRules` | `System.Object` |
| `Value` | `System.Object` |
| `Value` | `System.String` |
| `Value` | `System.Double` |
| `Value` | `System.String[]` |
| `Value` | `System.Double[]` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionOnBlur` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `actionSuccess` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `adaptor` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `atcModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `beforeSanitizeHtml` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `beginEdit` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cancelButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `cancelClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `colorModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `comboBoxModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `dateRangeModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `disabled` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `editableOn` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `emptyText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableEditMode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlParse` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enableHtmlSanitizer` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `enablePersistence` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `endEdit` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `extendModelValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `mode` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `model` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `multiSelectModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `name` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `needsID` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `popupSettings` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `primaryKey` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `printValue` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `requiredModules` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `rteModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `sanitizeHelper` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `save` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `saveButton` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `setValue` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `showButtons` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `sliderModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `submitClick` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `submitOnEnter` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `template` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `textOption` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `timeModule` | property | no | candidate: runtime property source/write only if raw EJ2 proof shows useful behavior |
| `type` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `url` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `validate` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `validating` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `value` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
