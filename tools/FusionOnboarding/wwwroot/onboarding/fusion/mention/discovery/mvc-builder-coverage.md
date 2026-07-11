# FusionMention MVC Builder Coverage

Status: static-discovery.

Syncfusion class: `Mention`
MVC builder: `MentionBuilder`
XML source: `/Users/muhammadadnanrafiq/.nuget/packages/syncfusion.ej2.aspnet.core/32.2.8/lib/net10.0/Syncfusion.EJ2.xml`

Builder coverage is initial-render evidence only. A builder-owned member is not
accepted as a Fusion runtime API unless raw EJ2 trace proves post-render
read/write behavior is needed.

## Counts

| Item | Count |
|---|---:|
| MVC builder overloads | 38 |
| JS members with matching builder method | 34 |
| JS members without matching builder method | 6 |

## Builder Methods

| Builder Method | Parameters |
|---|---|
| `ActionBegin` | `System.String` |
| `ActionComplete` | `System.String` |
| `ActionFailure` | `System.String` |
| `AllowSpaces` | `System.Boolean` |
| `BeforeOpen` | `System.String` |
| `Change` | `System.String` |
| `Closed` | `System.String` |
| `Created` | `System.String` |
| `CssClass` | `System.String` |
| `DataSource` | `System.Object` |
| `DataSource` | `System.String[]` |
| `DataSource` | `System.Double[]` |
| `DebounceDelay` | `System.Double` |
| `Destroyed` | `System.String` |
| `DisplayTemplate` | `System.String` |
| `Fields` | `Syncfusion.EJ2.DropDowns.MentionFieldSettings` |
| `Filtering` | `System.String` |
| `FilterType` | `Syncfusion.EJ2.DropDowns.FilterType` |
| `Highlight` | `System.Boolean` |
| `HtmlAttributes` | `System.Object` |
| `IgnoreCase` | `System.Boolean` |
| `ItemTemplate` | `System.String` |
| `Locale` | `System.String` |
| `MentionChar` | `System.Char` |
| `MinLength` | `System.Int32` |
| `NoRecordsTemplate` | `System.String` |
| `Opened` | `System.String` |
| `PopupHeight` | `System.String` |
| `PopupWidth` | `System.String` |
| `Query` | `System.String` |
| `RequireLeadingSpace` | `System.Boolean` |
| `Select` | `System.String` |
| `ShowMentionChar` | `System.Boolean` |
| `SortOrder` | `Syncfusion.EJ2.DropDowns.SortOrder` |
| `SpinnerTemplate` | `System.String` |
| `SuffixText` | `System.String` |
| `SuggestionCount` | `System.Int32` |
| `Target` | `System.String` |

## JS Member Coverage

| JS Member | Kind | Builder-Owned Candidate | Decision |
|---|---|---:|---|
| `actionBegin` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionComplete` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `actionFailure` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `allowSpaces` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `beforeOpen` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `change` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `closed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `created` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `cssClass` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `debounceDelay` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `destroy` | method | no | skip: lifecycle cleanup, not Fusion plan behavior |
| `destroyed` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `disableItem` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `displayTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `fields` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `filtering` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `filterType` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `hidePopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `hideSpinner` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `highlight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `ignoreCase` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `itemTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `locale` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `mentionChar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `minLength` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `noRecordsTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `opened` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `popupHeight` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `popupWidth` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `query` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `requireLeadingSpace` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `search` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `select` | event | yes | candidate: typed event; payload and browser gesture proof required |
| `showMentionChar` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `showPopup` | method | no | candidate: runtime method or method return source; raw EJ2 visible effect proof required |
| `sortOrder` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `spinnerTemplate` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `suffixText` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `suggestionCount` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
| `target` | property | yes | builder-owned unless post-render read/write behavior is proven necessary |
