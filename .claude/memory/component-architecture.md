# Component Architecture — Foundational Design Notes

## User's Mental Model (Critical — never violate)

The framework will onboard **100+ component vertical slices**. Each component defines:
1. **Properties (read)** — readExpr: property path from vendor root (e.g., "checked", "value")
2. **Properties (write)** — jsEmit: JS expression that writes value (e.g., "el.checked=val")
3. **Methods** — jsEmit: JS expression that calls methods (e.g., "el.focus()")
4. **Methods with params** — jsEmit includes parameters
5. **Events** — JS event name to wire + payload handling

**Vendor determines root:**
- `native` → `el` (DOM element)
- `fusion` → `el.ej2_instances[0]` (Syncfusion component instance)

## Architecture Flow
```
C# DSL (expresses dev intent)
  → Descriptors (carry state of what dev wants)
    → Plan JSON (serialized contract)
      → Runtime (dumb executor of plan primitives)
```

## Foundational Problem (repeated many times)
Plan JSON and runtime keep diverging from architecture with hacks:
- evalRead: runtime invents behavior (checkbox detection, comp. prefix, fallbacks)
- trigger.ts: inline ej2_instances[0] access
- These hacks corrupt the architecture for 100+ components at scale

## Component Interaction Types (from vertical slice analysis)

Each component vertical slice defines these via C# extension methods:

### 1. Property Write (jsEmit pattern — CORRECT)
- C#: `p.Component<FusionNumericTextBox>(m => m.Amount).SetValue(42)`
- Plan: `{ jsEmit: "var c=el.ej2_instances[0]; c.value=Number(val); c.dataBind()", value: "42" }`
- Runtime: `new Function("el", "val", jsEmit).call(null, el, val)` — dumb executor

### 2. Method Call (jsEmit pattern — CORRECT)
- C#: `p.Component<FusionNumericTextBox>(m => m.Amount).FocusIn()`
- Plan: `{ jsEmit: "el.ej2_instances[0].focusIn()" }`
- Runtime: `new Function("el", "val", jsEmit).call(null, el, undefined)` — dumb executor

### 3. Property Read (evalRead — BROKEN)
- C#: `gather.Include<FusionNumericTextBox>(m => m.Amount)` → readExpr: "comp.value"
- C#: `gather.Include<NativeDropDown>(m => m.Status)` → readExpr: null (omitted!)
- Runtime: evalRead has heuristics — checks el.type, comp. prefix, fallbacks

### 4. Event Wiring (trigger.ts — BROKEN for root resolution)
- C#: `.Reactive(plan, evt => evt.Changed, (args, p) => ...)`
- Plan: `{ kind: "component-event", vendor: "fusion", jsEvent: "change" }`
- Runtime: inline `el.ej2_instances?.[0]` access in trigger.ts

### 5. Value/BindExpr Reading (ref: pattern — FUTURE)
- C#: `p.Component<FusionNumericTextBox>(m => m.Amount).Value()` → `"ref:Amount.value"`
- types.ts: `BindSource = EventSource` (ComponentSource is "Future")
- Not yet used in runtime — marked for future extension

## Required Modules (user's explicit requirements)

### 1. Dot-path walking module (shared primitive)
JSON dot walking / bind expression resolution should be its OWN module because:
- Used by BindExpr resolution (evt.address.city) in resolver.ts
- Used by readExpr resolution (value, checked) in component.ts
- Used by gather setNested (Address.Street) in gather.ts
- Will be used in more places as framework grows
- Pure utility — zero side effects, zero DOM, zero vendor knowledge
- Currently DUPLICATED: resolveEventPath() in resolver.ts has same for-loop as proposed walkPath()

### 2. Component module (vendor abstraction)
- `resolveRoot(el, vendor)` — single source of truth for vendor → root
- `evalRead(id, vendor, readExpr)` — uses resolveRoot + dot-path walking
- ALL modules import from here — no inline ej2_instances access anywhere
- trigger.ts uses resolveRoot for event wiring

### 3. Resolver module (bind expression resolution — already exists)
- `resolve(expr, ctx)` — walks BindExpr against ExecContext
- Should use dot-path walking module instead of inline for-loop
- Separate from component reading (different root, different context)

## CLAUDE.md Rules (must document)
- `component.ts` is the ONLY module that resolves vendor → root
- Dot-path walking is a shared primitive — import from its module
- Plan carries ALL behavior info — runtime never invents
- New component = new vertical slice. Zero runtime changes needed.
- No module ever writes `ej2_instances` inline — that knowledge lives in component.ts

## User Constraints (explicit)
- Do NOT change .Reactive() or event listener extensions C# DSL — well designed, keep as-is
- Schema should document vendor→root→readExpr architecture clearly
- "Value" is a singular concept for input components — read and write are two sides of same coin
- readExpr required on plan JSON enforces vertical slices declare it
- Get EVIDENCE from actual plan JSON (snapshots, sandbox pages) — do not design in vacuum
- Broader schema review needed for consistency with vendor-agnostic architecture

## Evidence from Actual Plan JSON (verified snapshots)

### EventSource path format — ALWAYS "evt." prefixed
- ExpressionPathHelper.ToEventPath() returns "evt." + camelCase members
- Verified: `"path": "evt.score"`, `"path": "evt.address.city"`
- BindExpr (MutateElementCommand.source) uses same format: "evt.address.city"
- walk(ctx, "evt.value") → ctx.evt.value ✓ (ctx = { evt: { value: 42 } })

### ComponentGather — readExpr OPTIONAL in current schema
- Schema: required = ["kind", "componentId", "vendor", "name"] — NO readExpr
- readExpr description: "Prefixes: el. (DOM), comp. (vendor component). E.g. comp.value, el.checked"
- Native: readExpr omitted (null) → runtime falls back to el.value/el.checked (HEURISTIC)
- Fusion: readExpr = "comp.value" → runtime uses comp prefix convention (HEURISTIC)

### ValidationField — readExpr OPTIONAL in current schema
- Schema: required = ["fieldId", "fieldName", "vendor", "rules"] — NO readExpr
- Verified snapshot shows native field WITHOUT readExpr
- Schema says: "Null for native inputs (uses .value/.checked)"

### GatherItem.cs — readExpr nullable with JsonIgnore
- `[JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]`
- `public string? ReadExpr { get; }` — nullable, default null
- Constructor: `string? readExpr = null`

### ElementId vs FieldName vs EventPath
- m => m.Address.City → ElementId: "Address_City" (underscores)
- m => m.Address.City → FieldName: "Address.City" (dots for model binding)
- x => x.Address.City → EventPath: "evt.address.city" (camelCase, evt. prefix)

## Dot-Path Walking is FRAMEWORK-WIDE (user's critical insight)

The dot-path / JSON walking concept is the fundamental primitive for the ENTIRE framework,
not just component reads. It bridges C# expressions and runtime execution.

### C# Expression → Dot-Path String → Runtime Walk

```
C# Expression                    →  ExpressionPathHelper  →  Dot-path string  →  Runtime walk()
x => x.Address.Street            →  ToEventPath()         →  "evt.address.street"  →  walk(ctx, "evt.address.street")
x => x.IntValue                  →  ToEventPath()         →  "evt.intValue"        →  walk(ctx, "evt.intValue")
m => m.Address.City              →  ToPropertyName()      →  "Address.City"         →  used in gather body nesting
m => m.Amount                    →  ToElementId()         →  "Amount"               →  document.getElementById("Amount")
```

### Current uses of dot-path walking:
1. **Event payload** → `walk(ctx, "evt.address.city")` — BindExpr resolution
2. **Component reads** → `walk(root, "value")` — readExpr from vendor root
3. **Gather body nesting** → `setNested(body, "Address.City", val)` — nested JSON construction

### Future uses (user described):
4. **Response body traversal**: `success.AsJson<T>().data.Items` →
   Plan carries dot-path, runtime walks response body: `walk(responseBody, "data.items")`
5. **Component data source assignment**:
   `p.Component<DropDown>().DataSource = success.AsJson<T>().data.Items`
   Left side: vendor-aware (resolveRoot + property path)
   Right side: vendor-agnostic value walking (walk response body)

### The pattern:
- **Right side (source)**: dot-path walking on ANY object (event, response, etc.) — vendor-agnostic
- **Left side (target)**: component property — vendor-AWARE (resolveRoot + property path)
- **walk.ts** is the shared primitive for BOTH sides

### User quote:
"this concept is fundamental for whole framework to hold end to end"
"only difference here is that right side does not need to know about vendor or component
is value walking but left hand side same"

## Behavior Tests Must Reflect Architecture
User explicitly said: "Behavior tests must be refactored to reflect this reality. Basically end to end."
Tests need readExpr in test data — no more relying on runtime heuristics.

## Current File Analysis
- `resolver.ts` has both dot-path walking AND evalRead (mixed concerns)
- `trigger.ts` has inline ej2_instances access (lines 34-44)
- `gather.ts` calls evalRead with vendor, has setNested with dot-path walking
- `validation.ts` calls evalRead with vendor (lines 37, 140, 188)
- `element.ts` uses jsEmit (correct pattern — dumb executor)
- `conditions.ts` uses resolveSource from resolver.ts (correct — no vendor logic)
