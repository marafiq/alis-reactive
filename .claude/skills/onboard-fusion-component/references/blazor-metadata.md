# Blazor Metadata

Use Syncfusion Blazor packages as a typed candidate map, not as the Alis runtime contract.

## Why It Helps

Blazor has already mapped many Syncfusion concepts into C# names:

| Blazor Signal | Use In Alis |
|---|---|
| public `SfXxx<T>` methods | C# naming and overload candidates |
| `EventArgs<T>` properties | candidate event payload vocabulary |
| XML docs | quick navigation to related concepts |
| decompiled `ClientPropertyChangeHandler` | property-change names sent to JS |
| decompiled `sfBlazor.*` calls | bridge boundary and Blazor-owned behavior |

## Acceptance Rule

Classify each Blazor candidate before exposing a Fusion API:

| Classification | Required Proof | Fusion Decision |
|---|---|
| Direct EJ2 overlap | EJ2 source/d.ts exposes the same member and raw HTML trace proves the exact value, args, return, or mutation | normal typed Fusion API |
| Bridge-computed browser behavior | Blazor JS bridge derives the value from DOM/EJ2/browser facts, and an Alis bridge can reproduce the same facts in raw HTML/Playwright | explicit typed Fusion API backed by an Alis bridge |
| Blazor-owned state behavior | Blazor C# state or lifecycle creates the value and direct browser facts are insufficient | keep out unless Alis intentionally owns the same state concept |

Do not treat `sfBlazor.*` as a rejection by itself. Treat it as a boundary that must be classified. The question is whether the behavior is direct EJ2, bridge-computed from browser facts, or Blazor-owned state.

## Kanban Example

| Candidate | Blazor Evidence | EJ2/Trace Result | Fusion Decision |
|---|---|---|---|
| `dataBinding.Result` | `DataBindingEventArgs<T>.Result` | direct EJ2 `ReturnType.result`; trace posts two typed cards | onboard typed `Result` |
| `dataBinding.Count` | `DataBindingEventArgs<T>.Count` | direct EJ2 local data bind reports `count:0` while `result.length:2` | onboard `Count`, test actual EJ2 value |
| `queryCellInfo.Data/RequestType` | `QueryCellInfoEventArgs<T>` | direct EJ2 creates `{ data, requestType }` for header/swimlane/content rows | onboard typed header args |
| `dragStart/drag/dragStop.Data` | Blazor maps card ids to typed data | direct EJ2 trace exposes `data[]` | onboard direct typed `List<TCard>` payload |
| `dragStop.DropIndex` / Blazor `DragIndex` | Blazor bridge computes DOM drop index as `index`; EJ2 computes `dropIndex` | direct EJ2 trace exposes `dropIndex` on `dragStop` | onboard direct `DropIndex`; consider `DragIndex` only as a C# alias if needed |
| `dragStart.Left/Top` | Blazor bridge reads pointer coordinates and passes them to C# | direct EJ2 trace exposes `event`, not `left/top` fields | bridge-computed; onboard only if an Alis bridge intentionally exposes pointer coordinates |
| `dragStop.IsExternal` | Blazor bridge derives external drop state from DOM/drag target | direct EJ2 public payload does not expose `isExternal` | bridge-computed; onboard only with an explicit Alis external-drop bridge |
| `dragStop.PreviousCardData` | Blazor bridge finds previous card id, then C# maps id to data | direct EJ2 public payload does not expose previous card data | bridge-computed plus Blazor state; prefer `dropIndex` unless Alis owns a bridge |
| `AddCardAsync/UpdateCardAsync/DeleteCardAsync` | Blazor mutates C# data then coordinates CRUD | direct EJ2 has `addCard/updateCard/deleteCard` methods | onboard direct EJ2 methods, not Blazor state machinery |

Raw HTML Kanban drag trace from the running sandbox confirmed direct EJ2 payload keys:

| Event | Direct EJ2 payload keys |
|---|---|
| `dragStart` | `cancel`, `data`, `element`, `event` |
| `drag` | `data`, `element`, `event` |
| `dragStop` | `cancel`, `data`, `dropIndex`, `element`, `event` |

## Commands

Install or restore the package first, then inspect XML and IL:

```bash
dotnet new classlib -o /tmp/sf-blazor-probe
dotnet add /tmp/sf-blazor-probe package Syncfusion.Blazor.Kanban --version 32.2.8
ilspycmd -t Syncfusion.Blazor.Kanban.SfKanban`1 \
  -o /tmp/sf-blazor-kanban-decompiled \
  ~/.nuget/packages/syncfusion.blazor.kanban/32.2.8/lib/netstandard2.0/Syncfusion.Blazor.Kanban.dll
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-blazor-metadata.mjs \
  --package Syncfusion.Blazor.Kanban \
  --version 32.2.8 \
  --component Kanban \
  --decompiled /tmp/sf-blazor-kanban-decompiled/Syncfusion.Blazor.Kanban.SfKanban`1.decompiled.cs
```

If the package is downloaded to a temporary location instead of the global NuGet
cache, pass its package root directly:

```bash
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-blazor-metadata.mjs \
  --package Syncfusion.Blazor.Kanban \
  --version 32.2.8 \
  --component Kanban \
  --package-root /tmp/alis-syncfusion-blazor-kanban/pkg \
  --decompiled /tmp/alis-syncfusion-blazor-kanban/decompiled/Syncfusion.Blazor.Kanban.SfKanban`1.decompiled.cs
```

## Red Flags

| Signal | Meaning |
|---|---|
| `sfBlazor.Component.*` only | likely Blazor bridge behavior, not direct EJ2 |
| method mutates Blazor C# collections before JS | use naming only; prove direct EJ2 method separately |
| payload field appears only in Blazor event args | do not expose until HTML trace proves it |
| XML type is broad or missing nested shape | use EJ2 source and trace to narrow |
