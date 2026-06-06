# Source Discovery

Use this when the component package, d.ts path, MVC builder, or JavaScript namespace is not already known.

## Command

```bash
node .claude/skills/onboard-fusion-component/scripts/discover-syncfusion-component.mjs \
  --class ChipList
```

The output gives:

| Fact | Used For |
|---|---|
| package | confirms which Syncfusion module owns the component |
| d.ts path | public JS object contract |
| JS source path | implementation details and lifecycle quirks |
| MVC builder | builder coverage gate |
| JS global guess | raw HTML constructor |

Write the accepted facts into:

```text
tools/FusionOnboarding/wwwroot/onboarding/fusion/{componentName}/discovery/source-inventory.md
```

## Source Priority

| Priority | Source | Use |
|---:|---|---|
| 1 | Raw browser probe | proves the API works with real assets and real DOM |
| 2 | Syncfusion shipped JS source | explains lifecycle, hidden requirements, `dataBind()`, event timing |
| 3 | Syncfusion d.ts | identifies public members, event payload types, args, returns |
| 4 | Syncfusion MVC XML/builder | decides what remains builder-owned |
| 5 | Syncfusion docs/skills | accelerates setup only; never replaces proof |

`tools/SyncfusionOnboarding` is not a source of truth for the workflow. Use it
only when a current proof pass validates a specific file as vendor evidence.

## Hard Rule

Do not start C# until these facts are written down:

| Fact | Example |
|---|---|
| exact class | `ChipList` |
| exact global | `ej.buttons.ChipList` |
| exact host | `#care-tags` owns `ej2_instances[0]` |
| exact builder | `Syncfusion.EJ2.Buttons.ChipListBuilder` |
| exact d.ts and JS source | `node_modules/@syncfusion/ej2-buttons/src/chips/chip-list.d.ts/js` |
| exact event payload types | `FilteringEventArgs`, `DataStateChangeEventArgs`, etc. |

If one is unknown, stop and discover it first.
