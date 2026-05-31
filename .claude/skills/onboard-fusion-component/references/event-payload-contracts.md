# Event Payload Contracts

Use this before adding any `FusionXxxEvents` or `Events/FusionXxxOn*.cs` type.

Event payloads are separate JS objects. They can have readable properties, writable properties, and callable methods. Treat them like component objects, but with `PayloadSource.Event()` as the runtime root.

## Required Matrix

| Event | Trigger Gesture | Payload Type From d.ts | Raw Keys | Methods | Mutations/Calls Proved | Typed C# Shape |
|---|---|---|---|---|---|---|
| `filtering` | type in popup input | `FilteringEventArgs` | `text`, `preventDefaultAction`, `updateData` | `updateData` | set `preventDefaultAction`; call `updateData(data)` | args props + extensions |
| `popupOpen` | click schedule cell/event | `PopupOpenEventArgs` | `type`, `data`, `cancel` | none | set `cancel` prevents popup | args props + `PreventDefault` |
| `dataStateChange` | grid sort/page/filter | `DataStateChangeEventArgs` | `skip`, `take`, `sorted`, `action` | maybe none | read full state and post to server | nested state types |

Do not use this table as source truth. Fill it from the component being onboarded.

## Inspect d.ts Payload Type

Start with the event row from the component surface, for example:

```ts
filtering: EmitType<FilteringEventArgs>;
```

Then inspect the payload type:

```bash
node .claude/skills/onboard-fusion-component/scripts/inspect-syncfusion-event-payload.mjs \
  --type FilteringEventArgs
```

If the type is imported from another Syncfusion package, run without `--dts` so the script searches all Syncfusion d.ts files.

## Raw Browser Proof

Wire the event in the raw probe constructor, not after render, unless Syncfusion source proves late assignment works.

```javascript
const ej2 = new ej.dropdowns.AutoComplete({
  filtering: args => {
    window.__lastArgs = args;
    probe.event("filtering payload", args);
  }
});
```

For payload methods, call the method inside the real event lifecycle and verify the visible effect:

```javascript
filtering: args => {
  args.preventDefaultAction = true;
  probe.eventCall("filtering.updateData", args, "updateData", () =>
    args.updateData([{ text: "Aspirin", value: "rx-aspirin" }])
  );
}
```

For writable properties, mutate the payload inside the event and verify the outcome:

```javascript
popupOpen: args => {
  args.cancel = true;
  probe.event("popupOpen cancelled", args);
}
```

## C# Mapping

| Proven JS Payload Behavior | C# Pattern |
|---|---|
| `args.text` read | property on event args type |
| `args.action.requestType` read | nested event args type |
| `args.cancel = true` | extension method emitting `ReactionGraph.Set(PayloadSource.Event(), "cancel", ...)` |
| `args.preventDefaultAction = true` | extension method emitting `ReactionGraph.Set(PayloadSource.Event(), "preventDefaultAction", ...)` |
| `args.updateData(data)` | extension method emitting `ReactionGraph.Call(PayloadSource.Event(), "updateData", ...)` |

Use event-arg extension methods for payload mutations/calls. Do not put arbitrary public strings in the DSL.

## Proof Gates

Each event payload member needs a consumer proof:

| Member Kind | Minimum Proof |
|---|---|
| readable scalar | displayed in sandbox or sent via gather |
| readable object | nested property displayed or posted to server |
| readable typed array | typed indexed member displayed, or whole typed array posted to server |
| writable property | visible behavior changes because of the mutation |
| callable method | visible behavior changes because the method ran |

Array payloads are not a reason to expose loose public APIs. Model the array as
`List<T>` in the event args and prove the actual member path (`Data[0].Summary`,
`Result[0].Name`, etc.) or prove the whole array as a gather source. Dynamic
array transforms belong in plugin code because plugin is the intentional escape
hatch for behavior that is difficult to express as deterministic typed member
paths.

Examples:

| Component | Event | Payload Behavior | Proof |
|---|---|---|---|
| AutoComplete/MultiSelect | `filtering` | `preventDefaultAction`, `updateData(data)` | server-filtered popup shows returned rows |
| Schedule | `popupOpen` | `cancel` | Syncfusion editor/quick-info does not open; custom drawer/dialog opens |
| Grid | `dataStateChange` | `skip`, `take`, `sorted`, `action.*` | server receives full state and grid refreshes |

If a payload method exists in d.ts but cannot be made to produce a visible/runtime effect in raw HTML, do not onboard it.

## Concrete Proof Recipes

### Dropdown Filtering Payload With `updateData`

This proves a payload method, not a component method.

```javascript
const rows = [
  { text: "Aspirin", value: "rx-aspirin" },
  { text: "Metformin", value: "rx-metformin" }
];

const ej2 = new ej.dropdowns.AutoComplete({
  fields: { text: "text", value: "value" },
  filtering: args => {
    probe.event("filtering payload", args);
    args.preventDefaultAction = true;
    probe.eventCall("filtering.updateData", args, "updateData", () =>
      args.updateData(rows)
    );
  }
});
```

Required visible proof: typing opens the popup and shows `Aspirin`/`Metformin`.

### Schedule Popup Payload With `cancel`

This proves a writable payload property.

```javascript
const ej2 = new ej.schedule.Schedule({
  selectedDate: new Date(2026, 4, 29),
  eventSettings: { dataSource: [{ Id: 1, Subject: "Shift", StartTime: new Date(2026, 4, 29, 9), EndTime: new Date(2026, 4, 29, 10) }] },
  popupOpen: args => {
    probe.event("popupOpen payload", args);
    if (args.type === "Editor") {
      args.cancel = true;
      probe.event("popupOpen cancelled", args);
    }
  }
});
```

Required visible proof: the built-in editor does not open when the edit gesture is performed. If the Fusion slice opens a custom drawer/dialog, Playwright must assert that replacement UI.

### Grid Data State Payload

This proves nested event payload shape under real gestures.

```javascript
const ej2 = new ej.grids.Grid({
  allowSorting: true,
  allowPaging: true,
  pageSettings: { pageSize: 10 },
  dataSource: { result: rows, count: rows.length },
  dataStateChange: args => {
    probe.event("dataStateChange payload", args);
  },
  columns: [
    { field: "name", headerText: "Name" },
    { field: "age", headerText: "Age" }
  ]
});
```

Required gestures and proof:

| Gesture | Expected Payload |
|---|---|
| click sortable header | `action.requestType`, `action.columnName`, `action.direction`, `sorted[]` |
| click next page | `skip`, `take`, `action.currentPage`, `action.requestType` |
| apply filter/search when enabled | `where[]` or `search[]` plus full state |

Do not model only the first payload sample. Capture each gesture that the typed API claims to support because Syncfusion payload shape changes by action.
