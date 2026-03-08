# Conditions Module Implementation Plan

> **For Claude:** REQUIRED SUB-SKILL: Use superpowers:executing-plans to implement this plan task-by-task.

**Goal:** Add When/Then/Else branching with And/Or logical operators to the Alis.Reactive DSL, fully typed in C# and tested end-to-end across all 3 layers (C# snapshots, TS unit tests, Playwright browser tests).

**Architecture:** Guard algebra (ValueGuard, AllGuard, AnyGuard) evaluates BindExpr sources via the existing resolver module. A new `ConditionalReaction` branches the pipeline based on guards. C# builders provide a fluent `When().Op().Then().Else()` API with compile-time type inference for CoercionType. The runtime evaluates guards using `resolve()` + `coerce()` from `resolver.ts`.

**Tech Stack:** C# 8.0 / .NET 10, System.Text.Json polymorphic serialization, TypeScript / Vitest / jsdom, Playwright, NUnit + Verify

---

## Plan JSON Shape (Target)

```json
{
  "entries": [{
    "trigger": { "kind": "custom-event", "event": "score-check" },
    "reaction": {
      "kind": "conditional",
      "branches": [
        {
          "guard": {
            "kind": "value",
            "source": "evt.score",
            "coerceAs": "number",
            "op": "gte",
            "operand": 90
          },
          "reaction": {
            "kind": "sequential",
            "commands": [
              { "kind": "mutate-element", "target": "result", "jsEmit": "el.textContent = val", "value": "Pass" }
            ]
          }
        },
        {
          "guard": null,
          "reaction": {
            "kind": "sequential",
            "commands": [
              { "kind": "mutate-element", "target": "result", "jsEmit": "el.textContent = val", "value": "Fail" }
            ]
          }
        }
      ]
    }
  }]
}
```

## C# DSL Shape (Target)

```csharp
t.CustomEvent<ScorePayload>("score-check", (args, p) =>
    p.When(args, x => x.Score).Gte(90)
      .Then(then => then.Element("result").SetText("Pass"))
      .Else(else_ => else_.Element("result").SetText("Fail"))
)
```

## Operators (Phase 1 — 10 total)

| Category | Operator | Op String | Operand | C# Method |
|----------|----------|-----------|---------|-----------|
| Comparison | Eq | `"eq"` | single | `.Eq(value)` |
| Comparison | NotEq | `"neq"` | single | `.NotEq(value)` |
| Comparison | Gt | `"gt"` | single | `.Gt(value)` |
| Comparison | Gte | `"gte"` | single | `.Gte(value)` |
| Comparison | Lt | `"lt"` | single | `.Lt(value)` |
| Comparison | Lte | `"lte"` | single | `.Lte(value)` |
| Presence | Truthy | `"truthy"` | none | `.Truthy()` |
| Presence | Falsy | `"falsy"` | none | `.Falsy()` |
| Presence | IsNull | `"is-null"` | none | `.IsNull()` |
| Presence | NotNull | `"not-null"` | none | `.NotNull()` |

## Builder Chain

```
PipelineBuilder.When<TPayload>(args, x => x.Prop)
  → ConditionSourceBuilder<TModel>

ConditionSourceBuilder.Gte(90) / .NotNull() / .Eq("x")
  → GuardBuilder<TModel>

GuardBuilder.And(g => g.When(args, x => x.Other).Eq("y"))
  → GuardBuilder<TModel>  (wraps in AllGuard)

GuardBuilder.Or(g => g.When(args, x => x.Other).Eq("y"))
  → GuardBuilder<TModel>  (wraps in AnyGuard)

GuardBuilder.Then(then => then.Element("x").SetText("y"))
  → BranchBuilder<TModel>

BranchBuilder.ElseIf<TPayload>(args, x => x.Prop)
  → ConditionSourceBuilder<TModel>  (chains back)

BranchBuilder.Else(else_ => else_.Element("x").SetText("z"))
  → void (finalizes ConditionalReaction)
```

## File Inventory

**New C# files (6):**
- `Alis.Reactive/Descriptors/Guards/Guard.cs`
- `Alis.Reactive/Descriptors/Reactions/Branch.cs`
- `Alis.Reactive/Builders/ConditionSourceBuilder.cs`
- `Alis.Reactive/Builders/GuardBuilder.cs`
- `Alis.Reactive/Builders/BranchBuilder.cs`
- `Alis.Reactive/Builders/ConditionStart.cs`

**Modified C# files (3):**
- `Alis.Reactive/Descriptors/Reactions/Reaction.cs` — add ConditionalReaction + JsonDerivedType
- `Alis.Reactive/Builders/PipelineBuilder.cs` — add When() + BuildReaction()
- `Alis.Reactive/Builders/TriggerBuilder.cs` — use BuildReaction()
- `Alis.Reactive/ExpressionPathHelper.cs` — add GetPropertyType()

**New TS files (1):**
- `Scripts/conditions.ts`

**Modified TS files (2):**
- `Scripts/types.ts` — Guard, Branch, ConditionalReaction
- `Scripts/execute.ts` — handle ConditionalReaction

**New test files (4):**
- `Scripts/__tests__/when-evaluating-guards.test.ts`
- `Scripts/__tests__/when-branching-on-conditions.test.ts`
- `tests/Alis.Reactive.UnitTests/Conditions/WhenBranchingOnConditions.cs`
- `tests/Alis.Reactive.PlaywrightTests/Conditions/WhenConditionsEvaluateInBrowser.cs`

**Schema files (2 — source + test copy):**
- `Alis.Reactive/Schemas/reactive-plan.schema.json`
- `tests/Alis.Reactive.UnitTests/Schemas/reactive-plan.schema.json`

**Sandbox files (1):**
- `Areas/Sandbox/Views/Sandbox/Conditions.cshtml` (+ controller action)

---

## Task 1: JSON Schema — Add Guard, Branch, ConditionalReaction

**Files:**
- Modify: `Alis.Reactive/Schemas/reactive-plan.schema.json`
- Modify: `tests/Alis.Reactive.UnitTests/Schemas/reactive-plan.schema.json` (identical copy)

**Step 1: Add Guard definitions to `$defs`**

Add these definitions after `CoercionType` in the schema:

```json
"GuardOp": {
  "type": "string",
  "enum": ["eq", "neq", "gt", "gte", "lt", "lte", "truthy", "falsy", "is-null", "not-null"]
},

"Guard": {
  "oneOf": [
    { "$ref": "#/$defs/ValueGuard" },
    { "$ref": "#/$defs/AllGuard" },
    { "$ref": "#/$defs/AnyGuard" }
  ]
},
"ValueGuard": {
  "type": "object",
  "required": ["kind", "source", "coerceAs", "op"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "value" },
    "source": { "$ref": "#/$defs/BindExpr" },
    "coerceAs": { "$ref": "#/$defs/CoercionType" },
    "op": { "$ref": "#/$defs/GuardOp" },
    "operand": {}
  }
},
"AllGuard": {
  "type": "object",
  "required": ["kind", "guards"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "all" },
    "guards": { "type": "array", "items": { "$ref": "#/$defs/Guard" }, "minItems": 2 }
  }
},
"AnyGuard": {
  "type": "object",
  "required": ["kind", "guards"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "any" },
    "guards": { "type": "array", "items": { "$ref": "#/$defs/Guard" }, "minItems": 2 }
  }
},

"Branch": {
  "type": "object",
  "required": ["reaction"],
  "additionalProperties": false,
  "properties": {
    "guard": { "oneOf": [{ "$ref": "#/$defs/Guard" }, { "type": "null" }] },
    "reaction": { "$ref": "#/$defs/Reaction" }
  }
},

"ConditionalReaction": {
  "type": "object",
  "required": ["kind", "branches"],
  "additionalProperties": false,
  "properties": {
    "kind": { "const": "conditional" },
    "branches": { "type": "array", "items": { "$ref": "#/$defs/Branch" }, "minItems": 1 }
  }
}
```

**Step 2: Update the `Reaction` oneOf to include ConditionalReaction**

Change:
```json
"Reaction": {
  "oneOf": [
    { "$ref": "#/$defs/SequentialReaction" }
  ]
}
```
To:
```json
"Reaction": {
  "oneOf": [
    { "$ref": "#/$defs/SequentialReaction" },
    { "$ref": "#/$defs/ConditionalReaction" }
  ]
}
```

**Step 3: Copy the updated schema to test directory**

```bash
cp Alis.Reactive/Schemas/reactive-plan.schema.json tests/Alis.Reactive.UnitTests/Schemas/reactive-plan.schema.json
```

**Step 4: Verify existing tests still pass**

```bash
dotnet test tests/Alis.Reactive.UnitTests -v q
```
Expected: All existing tests PASS (schema is additive, nothing removed).

**Step 5: Commit**

```bash
git add Alis.Reactive/Schemas/reactive-plan.schema.json tests/Alis.Reactive.UnitTests/Schemas/reactive-plan.schema.json
git commit -m "schema: add Guard, Branch, ConditionalReaction to reactive-plan schema"
```

---

## Task 2: TS Types — Guard, Branch, ConditionalReaction Interfaces

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Scripts/types.ts`

**Step 1: Add Guard types after the ExecContext interface**

```typescript
// -- Guards ---------------------------------------------------

export type Guard = ValueGuard | AllGuard | AnyGuard;

export type GuardOp =
  | "eq" | "neq" | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy" | "is-null" | "not-null";

export interface ValueGuard {
  kind: "value";
  source: string;     // BindExpr
  coerceAs: "string" | "number" | "boolean" | "raw";
  op: GuardOp;
  operand?: unknown;
}

export interface AllGuard {
  kind: "all";
  guards: Guard[];
}

export interface AnyGuard {
  kind: "any";
  guards: Guard[];
}
```

**Step 2: Add Branch interface**

```typescript
// -- Branches -------------------------------------------------

export interface Branch {
  guard: Guard | null;
  reaction: Reaction;
}
```

**Step 3: Add ConditionalReaction and update Reaction union**

Change:
```typescript
export type Reaction = SequentialReaction;
```
To:
```typescript
export type Reaction = SequentialReaction | ConditionalReaction;

export interface ConditionalReaction {
  kind: "conditional";
  branches: Branch[];
}
```

**Step 4: Verify typecheck**

```bash
npm run typecheck
```
Expected: 0 errors (types are additive, no consumers yet).

**Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/types.ts
git commit -m "types: add Guard, Branch, ConditionalReaction TS interfaces"
```

---

## Task 3: Write Failing TS Direct Tests + Implement conditions.ts

**Files:**
- Create: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts`
- Create: `Alis.Reactive.SandboxApp/Scripts/conditions.ts`

**Step 1: Write the test file**

```typescript
import { describe, it, expect } from "vitest";
import { evaluateGuard } from "../conditions";
import type { Guard, ExecContext } from "../types";

function ctx(evt: Record<string, unknown>): ExecContext {
  return { evt };
}

describe("when evaluating guards", () => {

  // -- Comparison operators ---
  describe("comparison operators", () => {
    it("eq — matches when value equals operand", () => {
      const guard: Guard = { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" };
      expect(evaluateGuard(guard, ctx({ status: "active" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ status: "inactive" }))).toBe(false);
    });

    it("neq — matches when value does not equal operand", () => {
      const guard: Guard = { kind: "value", source: "evt.status", coerceAs: "string", op: "neq", operand: "deleted" };
      expect(evaluateGuard(guard, ctx({ status: "active" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ status: "deleted" }))).toBe(false);
    });

    it("gt — matches when value is greater than operand", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gt", operand: 50 };
      expect(evaluateGuard(guard, ctx({ score: 75 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 50 }))).toBe(false);
      expect(evaluateGuard(guard, ctx({ score: 25 }))).toBe(false);
    });

    it("gte — matches when value is greater than or equal to operand", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      expect(evaluateGuard(guard, ctx({ score: 90 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 95 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 89 }))).toBe(false);
    });

    it("lt — matches when value is less than operand", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "lt", operand: 10 };
      expect(evaluateGuard(guard, ctx({ score: 5 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 10 }))).toBe(false);
    });

    it("lte — matches when value is less than or equal to operand", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "lte", operand: 100 };
      expect(evaluateGuard(guard, ctx({ score: 100 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 99 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 101 }))).toBe(false);
    });
  });

  // -- Presence operators ---
  describe("presence operators", () => {
    it("truthy — matches when value is truthy", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "string", op: "truthy" };
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(false);
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(false);
    });

    it("falsy — matches when value is falsy", () => {
      const guard: Guard = { kind: "value", source: "evt.name", coerceAs: "string", op: "falsy" };
      expect(evaluateGuard(guard, ctx({ name: "" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ name: null }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ name: "Alice" }))).toBe(false);
    });

    it("is-null — matches when value is null or undefined", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(true);
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(false);
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(false);
    });

    it("not-null — matches when value is not null/undefined", () => {
      const guard: Guard = { kind: "value", source: "evt.value", coerceAs: "raw", op: "not-null" };
      expect(evaluateGuard(guard, ctx({ value: 0 }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ value: "" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ value: null }))).toBe(false);
      expect(evaluateGuard(guard, ctx({}))).toBe(false);
    });
  });

  // -- Logical composition ---
  describe("logical composition", () => {
    it("all — matches when every inner guard passes", () => {
      const guard: Guard = {
        kind: "all",
        guards: [
          { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
          { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ score: 95, status: "active" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: 95, status: "inactive" }))).toBe(false);
      expect(evaluateGuard(guard, ctx({ score: 50, status: "active" }))).toBe(false);
    });

    it("any — matches when at least one inner guard passes", () => {
      const guard: Guard = {
        kind: "any",
        guards: [
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
          { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
        ],
      };
      expect(evaluateGuard(guard, ctx({ role: "admin" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ role: "superuser" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ role: "viewer" }))).toBe(false);
    });

    it("nested all inside any — complex composition", () => {
      // (score >= 90 AND active) OR vip
      const guard: Guard = {
        kind: "any",
        guards: [
          {
            kind: "all",
            guards: [
              { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
              { kind: "value", source: "evt.active", coerceAs: "boolean", op: "truthy" },
            ],
          },
          { kind: "value", source: "evt.vip", coerceAs: "boolean", op: "truthy" },
        ],
      };
      // High score + active → true
      expect(evaluateGuard(guard, ctx({ score: 95, active: true, vip: false }))).toBe(true);
      // VIP regardless → true
      expect(evaluateGuard(guard, ctx({ score: 10, active: false, vip: true }))).toBe(true);
      // Neither → false
      expect(evaluateGuard(guard, ctx({ score: 50, active: false, vip: false }))).toBe(false);
      // High score but not active, not VIP → false
      expect(evaluateGuard(guard, ctx({ score: 95, active: false, vip: false }))).toBe(false);
    });
  });

  // -- Coercion integration ---
  describe("coercion integration", () => {
    it("coerces string score to number before comparison", () => {
      const guard: Guard = { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 };
      // score comes as string "95" from DOM, coerced to number 95
      expect(evaluateGuard(guard, ctx({ score: "95" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ score: "50" }))).toBe(false);
    });

    it("coerces string boolean for truthy check", () => {
      const guard: Guard = { kind: "value", source: "evt.enabled", coerceAs: "boolean", op: "truthy" };
      expect(evaluateGuard(guard, ctx({ enabled: "true" }))).toBe(true);
      expect(evaluateGuard(guard, ctx({ enabled: "false" }))).toBe(false);
      expect(evaluateGuard(guard, ctx({ enabled: "" }))).toBe(false);
    });
  });

  // -- Edge cases ---
  describe("edge cases", () => {
    it("returns false when source path does not exist", () => {
      const guard: Guard = { kind: "value", source: "evt.missing.deep", coerceAs: "string", op: "eq", operand: "x" };
      expect(evaluateGuard(guard, ctx({ name: "test" }))).toBe(false);
    });

    it("is-null returns true when source path does not exist", () => {
      const guard: Guard = { kind: "value", source: "evt.missing", coerceAs: "raw", op: "is-null" };
      expect(evaluateGuard(guard, ctx({}))).toBe(true);
    });
  });
});
```

**Step 2: Run test to verify it fails**

```bash
npm test -- --run Scripts/__tests__/when-evaluating-guards.test.ts
```
Expected: FAIL — `Cannot find module '../conditions'`

**Step 3: Implement conditions.ts**

Create `Alis.Reactive.SandboxApp/Scripts/conditions.ts`:

```typescript
import type { Guard, ValueGuard, ExecContext } from "./types";
import { resolveAs } from "./resolver";
import { scope } from "./trace";

const log = scope("conditions");

/**
 * Evaluates a guard against the execution context.
 * Returns true if the guard passes, false otherwise.
 */
export function evaluateGuard(guard: Guard, ctx?: ExecContext): boolean {
  switch (guard.kind) {
    case "value":
      return evaluateValueGuard(guard, ctx);
    case "all":
      return guard.guards.every(g => evaluateGuard(g, ctx));
    case "any":
      return guard.guards.some(g => evaluateGuard(g, ctx));
  }
}

function evaluateValueGuard(guard: ValueGuard, ctx?: ExecContext): boolean {
  const resolved = resolveAs(guard.source, guard.coerceAs, ctx);
  const op = guard.op;
  const operand = guard.operand;

  log.trace("eval", { source: guard.source, op, resolved, operand });

  switch (op) {
    // -- Comparison (require operand) --
    case "eq":     return resolved === operand;
    case "neq":    return resolved !== operand;
    case "gt":     return (resolved as number) > (operand as number);
    case "gte":    return (resolved as number) >= (operand as number);
    case "lt":     return (resolved as number) < (operand as number);
    case "lte":    return (resolved as number) <= (operand as number);

    // -- Presence (no operand) --
    case "truthy":   return !!resolved;
    case "falsy":    return !resolved;
    case "is-null":  return resolved == null;
    case "not-null": return resolved != null;
  }
}
```

**Step 4: Run test to verify it passes**

```bash
npm test -- --run Scripts/__tests__/when-evaluating-guards.test.ts
```
Expected: ALL PASS

**Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/conditions.ts Alis.Reactive.SandboxApp/Scripts/__tests__/when-evaluating-guards.test.ts
git commit -m "feat: conditions.ts guard evaluator with 16 direct unit tests"
```

---

## Task 4: Write Failing TS Integration Tests + Update execute.ts

**Files:**
- Create: `Alis.Reactive.SandboxApp/Scripts/__tests__/when-branching-on-conditions.test.ts`
- Modify: `Alis.Reactive.SandboxApp/Scripts/execute.ts`

**Step 1: Write integration tests (test through boot())**

```typescript
import { describe, it, expect } from "vitest";
import { boot } from "../boot";

describe("when branching on conditions", () => {

  it("takes then-branch when guard passes", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-1", payload: { score: 95 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-1" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Pass" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Fail" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Pass");
  });

  it("takes else-branch when guard fails", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-2", payload: { score: 40 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-2" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Pass" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Fail" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Fail");
  });

  it("takes then-branch without else when guard passes", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-3", payload: { active: true } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-3" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.active", coerceAs: "boolean", op: "truthy" },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Active");
  });

  it("skips all branches when no guard matches and no else", () => {
    document.body.innerHTML = '<span id="result">original</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-4", payload: { active: false } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-4" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.active", coerceAs: "boolean", op: "truthy" },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("original");
  });

  it("AND composition — both guards must pass", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-5", payload: { score: 95, status: "active" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-5" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                    { kind: "value", source: "evt.status", coerceAs: "string", op: "eq", operand: "active" },
                  ],
                },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Active High Scorer" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Other" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Active High Scorer");
  });

  it("OR composition — first matching guard wins", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-6", payload: { role: "superuser" } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-6" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "any",
                  guards: [
                    { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "admin" },
                    { kind: "value", source: "evt.role", coerceAs: "string", op: "eq", operand: "superuser" },
                  ],
                },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Authorized" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Denied" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Authorized");
  });

  it("multi-branch — first matching branch wins (ElseIf behavior)", () => {
    document.body.innerHTML = '<span id="grade">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{ kind: "dispatch", event: "cond-test-7", payload: { score: 85 } }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-7" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 90 },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "grade", jsEmit: "el.textContent = val", value: "A" },
                ]},
              },
              {
                guard: { kind: "value", source: "evt.score", coerceAs: "number", op: "gte", operand: 80 },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "grade", jsEmit: "el.textContent = val", value: "B" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "grade", jsEmit: "el.textContent = val", value: "C" },
                ]},
              },
            ],
          },
        },
      ],
    });

    // score=85 fails >=90, passes >=80 → "B"
    expect(document.getElementById("grade")!.textContent).toBe("B");
  });

  it("condition uses source from nested payload property", () => {
    document.body.innerHTML = '<span id="result">—</span>';

    boot({
      entries: [
        {
          trigger: { kind: "dom-ready" },
          reaction: {
            kind: "sequential",
            commands: [{
              kind: "dispatch",
              event: "cond-test-8",
              payload: { user: { role: "admin", level: 3 } },
            }],
          },
        },
        {
          trigger: { kind: "custom-event", event: "cond-test-8" },
          reaction: {
            kind: "conditional",
            branches: [
              {
                guard: {
                  kind: "all",
                  guards: [
                    { kind: "value", source: "evt.user.role", coerceAs: "string", op: "eq", operand: "admin" },
                    { kind: "value", source: "evt.user.level", coerceAs: "number", op: "gte", operand: 3 },
                  ],
                },
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Super Admin" },
                ]},
              },
              {
                guard: null,
                reaction: { kind: "sequential", commands: [
                  { kind: "mutate-element", target: "result", jsEmit: "el.textContent = val", value: "Regular" },
                ]},
              },
            ],
          },
        },
      ],
    });

    expect(document.getElementById("result")!.textContent).toBe("Super Admin");
  });
});
```

**Step 2: Run test to verify it fails**

```bash
npm test -- --run Scripts/__tests__/when-branching-on-conditions.test.ts
```
Expected: FAIL — `executeReaction` doesn't handle `"conditional"` kind.

**Step 3: Update execute.ts to handle ConditionalReaction**

```typescript
import type { Reaction, Command, ExecContext } from "./types";
import { scope } from "./trace";
import { mutateElement } from "./element";
import { evaluateGuard } from "./conditions";

const log = scope("command");

export function executeReaction(reaction: Reaction, ctx?: ExecContext): void {
  switch (reaction.kind) {
    case "sequential":
      log.debug("sequential", { commands: reaction.commands.length });
      for (const cmd of reaction.commands) {
        executeCommand(cmd, ctx);
      }
      break;

    case "conditional":
      log.debug("conditional", { branches: reaction.branches.length });
      for (const branch of reaction.branches) {
        if (branch.guard === null || evaluateGuard(branch.guard, ctx)) {
          log.trace("branch-taken", { guard: branch.guard?.kind ?? "else" });
          executeReaction(branch.reaction, ctx);
          return; // first match wins
        }
      }
      log.trace("no-branch-taken");
      break;
  }
}

function executeCommand(cmd: Command, ctx?: ExecContext): void {
  switch (cmd.kind) {
    case "dispatch":
      log.trace("dispatch", { event: cmd.event, payload: cmd.payload });
      document.dispatchEvent(
        new CustomEvent(cmd.event, { detail: cmd.payload ?? {} })
      );
      break;

    case "mutate-element":
      log.trace("mutate-element", { target: cmd.target, jsEmit: cmd.jsEmit });
      mutateElement(cmd, ctx);
      break;
  }
}
```

**Step 4: Run integration tests to verify they pass**

```bash
npm test -- --run Scripts/__tests__/when-branching-on-conditions.test.ts
```
Expected: ALL PASS

**Step 5: Run ALL TS tests**

```bash
npm test
```
Expected: ALL PASS (existing tests unchanged, new tests added).

**Step 6: Commit**

```bash
git add Alis.Reactive.SandboxApp/Scripts/__tests__/when-branching-on-conditions.test.ts Alis.Reactive.SandboxApp/Scripts/execute.ts
git commit -m "feat: ConditionalReaction handler in execute.ts, 8 integration tests"
```

---

## Task 5: C# Guard Descriptors + Branch + ConditionalReaction

**Files:**
- Create: `Alis.Reactive/Descriptors/Guards/Guard.cs`
- Create: `Alis.Reactive/Descriptors/Reactions/Branch.cs`
- Modify: `Alis.Reactive/Descriptors/Reactions/Reaction.cs`
- Modify: `Alis.Reactive/ExpressionPathHelper.cs`

**Step 1: Create Guard.cs with all guard types + constants**

Create `Alis.Reactive/Descriptors/Guards/Guard.cs`:

```csharp
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Alis.Reactive.Descriptors.Guards
{
    /// <summary>
    /// Operator string constants for ValueGuard.
    /// </summary>
    public static class GuardOp
    {
        public const string Eq = "eq";
        public const string Neq = "neq";
        public const string Gt = "gt";
        public const string Gte = "gte";
        public const string Lt = "lt";
        public const string Lte = "lte";
        public const string Truthy = "truthy";
        public const string Falsy = "falsy";
        public const string IsNull = "is-null";
        public const string NotNull = "not-null";
    }

    /// <summary>
    /// Coercion type string constants. Tells the runtime how to coerce
    /// the resolved BindExpr value before guard evaluation.
    /// </summary>
    public static class CoercionTypes
    {
        public const string String = "string";
        public const string Number = "number";
        public const string Boolean = "boolean";
        public const string Raw = "raw";

        /// <summary>
        /// Infers the coercion type from a C# property type.
        /// </summary>
        public static string InferFromType(System.Type type)
        {
            var underlying = System.Nullable.GetUnderlyingType(type) ?? type;

            if (underlying == typeof(string)) return String;
            if (underlying == typeof(bool)) return Boolean;
            if (underlying == typeof(int) || underlying == typeof(long) ||
                underlying == typeof(double) || underlying == typeof(float) ||
                underlying == typeof(decimal) || underlying == typeof(short) ||
                underlying == typeof(byte)) return Number;
            return Raw;
        }
    }

    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(ValueGuard), "value")]
    [JsonDerivedType(typeof(AllGuard), "all")]
    [JsonDerivedType(typeof(AnyGuard), "any")]
    public abstract class Guard
    {
    }

    public sealed class ValueGuard : Guard
    {
        public string Source { get; }
        public string CoerceAs { get; }
        public string Op { get; }

        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public object? Operand { get; }

        public ValueGuard(string source, string coerceAs, string op, object? operand = null)
        {
            Source = source;
            CoerceAs = coerceAs;
            Op = op;
            Operand = operand;
        }
    }

    public sealed class AllGuard : Guard
    {
        public IReadOnlyList<Guard> Guards { get; }

        public AllGuard(IReadOnlyList<Guard> guards)
        {
            Guards = guards;
        }
    }

    public sealed class AnyGuard : Guard
    {
        public IReadOnlyList<Guard> Guards { get; }

        public AnyGuard(IReadOnlyList<Guard> guards)
        {
            Guards = guards;
        }
    }
}
```

**Step 2: Create Branch.cs**

Create `Alis.Reactive/Descriptors/Reactions/Branch.cs`:

```csharp
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Descriptors.Reactions
{
    public sealed class Branch
    {
        [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
        public Guard? Guard { get; }

        public Reaction Reaction { get; }

        public Branch(Guard? guard, Reaction reaction)
        {
            Guard = guard;
            Reaction = reaction;
        }
    }
}
```

**Step 3: Add ConditionalReaction to Reaction.cs**

Add `[JsonDerivedType(typeof(ConditionalReaction), "conditional")]` to the Reaction base class and add the ConditionalReaction class:

```csharp
using System.Collections.Generic;
using System.Text.Json.Serialization;
using Alis.Reactive.Descriptors.Commands;

namespace Alis.Reactive.Descriptors.Reactions
{
    [JsonPolymorphic(TypeDiscriminatorPropertyName = "kind")]
    [JsonDerivedType(typeof(SequentialReaction), "sequential")]
    [JsonDerivedType(typeof(ConditionalReaction), "conditional")]
    public abstract class Reaction
    {
    }

    public sealed class SequentialReaction : Reaction
    {
        public List<Command> Commands { get; }

        public SequentialReaction(List<Command> commands)
        {
            Commands = commands;
        }
    }

    public sealed class ConditionalReaction : Reaction
    {
        public IReadOnlyList<Branch> Branches { get; }

        public ConditionalReaction(IReadOnlyList<Branch> branches)
        {
            Branches = branches;
        }
    }
}
```

**Step 4: Add GetPropertyType to ExpressionPathHelper.cs**

Add this method to the existing `ExpressionPathHelper` class:

```csharp
/// <summary>
/// Extracts the actual property type from an expression, unwrapping
/// any Convert node (boxing of value types to object?).
/// </summary>
public static System.Type GetPropertyType<TSource>(Expression<Func<TSource, object?>> expression)
{
    var body = expression.Body;

    // Unwrap Convert (boxing of value types like int → object)
    if (body is UnaryExpression unary && unary.NodeType == ExpressionType.Convert)
        body = unary.Operand;

    if (body is MemberExpression member)
    {
        if (member.Member is System.Reflection.PropertyInfo prop) return prop.PropertyType;
        if (member.Member is System.Reflection.FieldInfo field) return field.FieldType;
    }

    return typeof(object);
}
```

**Step 5: Verify it compiles**

```bash
dotnet build Alis.Reactive/Alis.Reactive.csproj
```
Expected: 0 errors

**Step 6: Commit**

```bash
git add Alis.Reactive/Descriptors/Guards/Guard.cs Alis.Reactive/Descriptors/Reactions/Branch.cs Alis.Reactive/Descriptors/Reactions/Reaction.cs Alis.Reactive/ExpressionPathHelper.cs
git commit -m "feat: Guard descriptors (Value/All/Any), Branch, ConditionalReaction, CoercionType inference"
```

---

## Task 6: C# Builders — ConditionSourceBuilder, GuardBuilder, BranchBuilder, ConditionStart

**Files:**
- Create: `Alis.Reactive/Builders/ConditionSourceBuilder.cs`
- Create: `Alis.Reactive/Builders/GuardBuilder.cs`
- Create: `Alis.Reactive/Builders/BranchBuilder.cs`
- Create: `Alis.Reactive/Builders/ConditionStart.cs`

**Step 1: Create ConditionSourceBuilder.cs**

```csharp
using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Provides operator methods after a When() source is specified.
    /// Returns GuardBuilder on each operator call.
    /// </summary>
    public class ConditionSourceBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly string _source;
        private readonly string _coercion;
        private readonly BranchBuilder<TModel>? _branchBuilder;

        /// <summary>From PipelineBuilder.When (top-level condition).</summary>
        internal ConditionSourceBuilder(PipelineBuilder<TModel> pipeline, string source, string coercion)
        {
            _pipeline = pipeline;
            _source = source;
            _coercion = coercion;
        }

        /// <summary>From ConditionStart.When (inside And/Or lambda).</summary>
        internal ConditionSourceBuilder(string source, string coercion)
        {
            _source = source;
            _coercion = coercion;
        }

        /// <summary>From BranchBuilder.ElseIf (chained branch).</summary>
        internal ConditionSourceBuilder(PipelineBuilder<TModel> pipeline, string source, string coercion, BranchBuilder<TModel> branchBuilder)
        {
            _pipeline = pipeline;
            _source = source;
            _coercion = coercion;
            _branchBuilder = branchBuilder;
        }

        // -- Comparison operators (require operand) --

        public GuardBuilder<TModel> Eq(object operand) => MakeGuard(GuardOp.Eq, operand);
        public GuardBuilder<TModel> NotEq(object operand) => MakeGuard(GuardOp.Neq, operand);
        public GuardBuilder<TModel> Gt(object operand) => MakeGuard(GuardOp.Gt, operand);
        public GuardBuilder<TModel> Gte(object operand) => MakeGuard(GuardOp.Gte, operand);
        public GuardBuilder<TModel> Lt(object operand) => MakeGuard(GuardOp.Lt, operand);
        public GuardBuilder<TModel> Lte(object operand) => MakeGuard(GuardOp.Lte, operand);

        // -- Presence operators (no operand) --

        public GuardBuilder<TModel> Truthy() => MakeGuard(GuardOp.Truthy);
        public GuardBuilder<TModel> Falsy() => MakeGuard(GuardOp.Falsy);
        public GuardBuilder<TModel> IsNull() => MakeGuard(GuardOp.IsNull);
        public GuardBuilder<TModel> NotNull() => MakeGuard(GuardOp.NotNull);

        private GuardBuilder<TModel> MakeGuard(string op, object? operand = null)
        {
            var guard = new ValueGuard(_source, _coercion, op, operand);

            if (_pipeline != null && _branchBuilder != null)
                return new GuardBuilder<TModel>(_pipeline, guard, _branchBuilder);

            if (_pipeline != null)
                return new GuardBuilder<TModel>(_pipeline, guard);

            // Inner guard (from ConditionStart, inside And/Or lambda)
            return new GuardBuilder<TModel>(guard);
        }
    }
}
```

**Step 2: Create GuardBuilder.cs**

```csharp
using System;
using System.Collections.Generic;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Provides And/Or composition and Then to create a branch.
    /// </summary>
    public class GuardBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel>? _pipeline;
        private readonly BranchBuilder<TModel>? _branchBuilder;
        internal Guard Guard { get; }

        /// <summary>Top-level guard (from PipelineBuilder.When).</summary>
        internal GuardBuilder(PipelineBuilder<TModel> pipeline, Guard guard)
        {
            _pipeline = pipeline;
            Guard = guard;
        }

        /// <summary>ElseIf guard (from BranchBuilder.ElseIf).</summary>
        internal GuardBuilder(PipelineBuilder<TModel> pipeline, Guard guard, BranchBuilder<TModel> branchBuilder)
        {
            _pipeline = pipeline;
            Guard = guard;
            _branchBuilder = branchBuilder;
        }

        /// <summary>Inner guard (from ConditionStart.When, inside And/Or lambda).</summary>
        internal GuardBuilder(Guard guard)
        {
            Guard = guard;
        }

        /// <summary>
        /// Combines this guard with another via AND (both must pass).
        /// </summary>
        public GuardBuilder<TModel> And(Func<ConditionStart<TModel>, GuardBuilder<TModel>> configure)
        {
            var innerGuard = configure(new ConditionStart<TModel>()).Guard;
            var allGuard = new AllGuard(new List<Guard> { Guard, innerGuard });

            if (_pipeline != null && _branchBuilder != null)
                return new GuardBuilder<TModel>(_pipeline, allGuard, _branchBuilder);
            if (_pipeline != null)
                return new GuardBuilder<TModel>(_pipeline, allGuard);
            return new GuardBuilder<TModel>(allGuard);
        }

        /// <summary>
        /// Combines this guard with another via OR (either must pass).
        /// </summary>
        public GuardBuilder<TModel> Or(Func<ConditionStart<TModel>, GuardBuilder<TModel>> configure)
        {
            var innerGuard = configure(new ConditionStart<TModel>()).Guard;
            var anyGuard = new AnyGuard(new List<Guard> { Guard, innerGuard });

            if (_pipeline != null && _branchBuilder != null)
                return new GuardBuilder<TModel>(_pipeline, anyGuard, _branchBuilder);
            if (_pipeline != null)
                return new GuardBuilder<TModel>(_pipeline, anyGuard);
            return new GuardBuilder<TModel>(anyGuard);
        }

        /// <summary>
        /// Creates the then-branch for this guard.
        /// </summary>
        public BranchBuilder<TModel> Then(Action<PipelineBuilder<TModel>> configure)
        {
            var thenPipeline = new PipelineBuilder<TModel>();
            configure(thenPipeline);
            var branch = new Branch(Guard, thenPipeline.BuildReaction());

            if (_branchBuilder != null)
            {
                _branchBuilder.AddBranch(branch);
                return _branchBuilder;
            }

            return new BranchBuilder<TModel>(_pipeline!, new List<Branch> { branch });
        }
    }
}
```

**Step 3: Create BranchBuilder.cs**

```csharp
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Provides ElseIf and Else methods after a Then branch is created.
    /// </summary>
    public class BranchBuilder<TModel> where TModel : class
    {
        private readonly PipelineBuilder<TModel> _pipeline;
        private readonly List<Branch> _branches;

        internal BranchBuilder(PipelineBuilder<TModel> pipeline, List<Branch> branches)
        {
            _pipeline = pipeline;
            _branches = branches;
            _pipeline.SetConditional(new ConditionalReaction(_branches));
        }

        internal void AddBranch(Branch branch)
        {
            _branches.Add(branch);
            _pipeline.SetConditional(new ConditionalReaction(_branches));
        }

        /// <summary>
        /// Adds an ElseIf branch with a new condition.
        /// </summary>
        public ConditionSourceBuilder<TModel> ElseIf<TPayload>(
            TPayload payload, Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propType = ExpressionPathHelper.GetPropertyType(path);
            var coercion = CoercionTypes.InferFromType(propType);
            return new ConditionSourceBuilder<TModel>(_pipeline, source, coercion, this);
        }

        /// <summary>
        /// Adds the default (else) branch — executed when no guard matches.
        /// </summary>
        public void Else(Action<PipelineBuilder<TModel>> configure)
        {
            var elsePipeline = new PipelineBuilder<TModel>();
            configure(elsePipeline);
            AddBranch(new Branch(null, elsePipeline.BuildReaction()));
        }
    }
}
```

**Step 4: Create ConditionStart.cs**

```csharp
using System;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Guards;

namespace Alis.Reactive.Builders
{
    /// <summary>
    /// Entry point for building inner guards inside And/Or lambdas.
    /// </summary>
    public class ConditionStart<TModel> where TModel : class
    {
        public ConditionSourceBuilder<TModel> When<TPayload>(
            TPayload payload, Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propType = ExpressionPathHelper.GetPropertyType(path);
            var coercion = CoercionTypes.InferFromType(propType);
            return new ConditionSourceBuilder<TModel>(source, coercion);
        }
    }
}
```

**Step 5: Verify it compiles (may fail — PipelineBuilder not yet updated)**

```bash
dotnet build Alis.Reactive/Alis.Reactive.csproj
```
Expected: Compilation errors — `PipelineBuilder` doesn't have `When()`, `BuildReaction()`, or `SetConditional()` yet. That's Task 7.

**Step 6: Commit the new files (even with build errors — they'll be resolved in Task 7)**

Do NOT commit yet. Continue to Task 7.

---

## Task 7: Wire PipelineBuilder.When() + BuildReaction() + Update TriggerBuilder

**Files:**
- Modify: `Alis.Reactive/Builders/PipelineBuilder.cs`
- Modify: `Alis.Reactive/Builders/TriggerBuilder.cs`

**Step 1: Update PipelineBuilder.cs**

Replace entire file:

```csharp
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Alis.Reactive.Descriptors.Commands;
using Alis.Reactive.Descriptors.Guards;
using Alis.Reactive.Descriptors.Reactions;

namespace Alis.Reactive.Builders
{
    public class PipelineBuilder<TModel> where TModel : class
    {
        internal List<Command> Commands { get; } = new List<Command>();
        internal ConditionalReaction? Conditional { get; private set; }

        public PipelineBuilder<TModel> Dispatch(string eventName)
        {
            Commands.Add(new DispatchCommand(eventName));
            return this;
        }

        public PipelineBuilder<TModel> Dispatch<TPayload>(string eventName, TPayload payload)
        {
            Commands.Add(new DispatchCommand(eventName, payload));
            return this;
        }

        public ElementBuilder<TModel> Element(string elementId)
        {
            return new ElementBuilder<TModel>(this, elementId);
        }

        /// <summary>
        /// Starts a conditional branch. The source expression determines
        /// the BindExpr path and infers CoercionType from the property type.
        /// </summary>
        public ConditionSourceBuilder<TModel> When<TPayload>(
            TPayload payload, Expression<Func<TPayload, object?>> path)
        {
            var source = ExpressionPathHelper.ToEventPath(path);
            var propType = ExpressionPathHelper.GetPropertyType(path);
            var coercion = CoercionTypes.InferFromType(propType);
            return new ConditionSourceBuilder<TModel>(this, source, coercion);
        }

        /// <summary>
        /// Sets the conditional reaction (called by BranchBuilder).
        /// </summary>
        internal void SetConditional(ConditionalReaction conditional)
        {
            Conditional = conditional;
        }

        /// <summary>
        /// Builds the appropriate Reaction based on what was configured.
        /// If When/Then/Else was used → ConditionalReaction.
        /// If only commands → SequentialReaction.
        /// </summary>
        internal Reaction BuildReaction()
        {
            if (Conditional != null)
            {
                if (Commands.Count > 0)
                    throw new InvalidOperationException(
                        "Cannot mix commands and When/Then/Else at the same pipeline level. " +
                        "Move commands inside Then/Else branches.");
                return Conditional;
            }
            return new SequentialReaction(Commands);
        }
    }
}
```

**Step 2: Update TriggerBuilder.cs to use BuildReaction()**

Replace `new SequentialReaction(pb.Commands)` with `pb.BuildReaction()` in all 3 methods:

```csharp
using System;
using Alis.Reactive.Descriptors;
using Alis.Reactive.Descriptors.Triggers;

namespace Alis.Reactive.Builders
{
    public sealed class TriggerBuilder<TModel> where TModel : class
    {
        private readonly IReactivePlan<TModel> _plan;

        public TriggerBuilder(IReactivePlan<TModel> plan)
        {
            _plan = plan;
        }

        public TriggerBuilder<TModel> DomReady(Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            _plan.AddEntry(new Entry(new DomReadyTrigger(), pb.BuildReaction()));
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent(string eventName, Action<PipelineBuilder<TModel>> configure)
        {
            var pb = new PipelineBuilder<TModel>();
            configure(pb);
            _plan.AddEntry(new Entry(new CustomEventTrigger(eventName), pb.BuildReaction()));
            return this;
        }

        public TriggerBuilder<TModel> CustomEvent<TPayload>(string eventName,
            Action<TPayload, PipelineBuilder<TModel>> configure)
            where TPayload : new()
        {
            var pb = new PipelineBuilder<TModel>();
            configure(new TPayload(), pb);
            _plan.AddEntry(new Entry(new CustomEventTrigger(eventName), pb.BuildReaction()));
            return this;
        }
    }
}
```

**Step 3: Verify full solution compiles**

```bash
dotnet build
```
Expected: 0 errors

**Step 4: Run existing tests to verify no regressions**

```bash
dotnet test tests/Alis.Reactive.UnitTests -v q
```
Expected: All existing tests PASS (BuildReaction() returns SequentialReaction when no conditions used — same behavior).

**Step 5: Commit all C# builder work**

```bash
git add Alis.Reactive/Builders/ Alis.Reactive/Descriptors/Guards/ Alis.Reactive/Descriptors/Reactions/ Alis.Reactive/ExpressionPathHelper.cs
git commit -m "feat: C# condition builders — When/Then/Else/ElseIf/And/Or with typed CoercionType inference"
```

---

## Task 8: C# Snapshot Tests

**Files:**
- Create: `tests/Alis.Reactive.UnitTests/Conditions/WhenBranchingOnConditions.cs`
- 6 `.verified.txt` files will be generated by Verify

**Step 1: Create test payloads and test class**

Create `tests/Alis.Reactive.UnitTests/Conditions/WhenBranchingOnConditions.cs`:

```csharp
using Alis.Reactive.Builders;

namespace Alis.Reactive.UnitTests.Conditions;

public class ScorePayload
{
    public int Score { get; set; }
    public string Status { get; set; } = "";
}

public class UserPayload
{
    public string? Name { get; set; }
    public string Role { get; set; } = "";
    public bool IsActive { get; set; }
}

[TestFixture]
public class WhenBranchingOnConditions : PlanTestBase
{
    [Test]
    public Task Simple_when_then_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("score-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
              .Then(then => then.Element("result").SetText("Pass"))
              .Else(else_ => else_.Element("result").SetText("Fail"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task When_then_without_else()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("user-check", (args, p) =>
            p.When(args, x => x.Name).NotNull()
              .Then(then => then.Element("greeting").SetText("Welcome"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task And_composition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("score-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
              .And(g => g.When(args, x => x.Status).Eq("active"))
              .Then(then => then.Element("result").SetText("Active High Scorer"))
              .Else(else_ => else_.Element("result").SetText("Other"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Or_composition()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("auth-check", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
              .Or(g => g.When(args, x => x.Role).Eq("superuser"))
              .Then(then => then.Element("panel").Show())
              .Else(else_ => else_.Element("panel").Hide())
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task ElseIf_multi_branch()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<ScorePayload>("grade-check", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
              .Then(then => then.Element("grade").SetText("A"))
              .ElseIf(args, x => x.Score).Gte(80)
              .Then(then => then.Element("grade").SetText("B"))
              .ElseIf(args, x => x.Score).Gte(70)
              .Then(then => then.Element("grade").SetText("C"))
              .Else(else_ => else_.Element("grade").SetText("F"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }

    [Test]
    public Task Presence_operators()
    {
        var plan = CreatePlan();
        Trigger(plan).CustomEvent<UserPayload>("status-check", (args, p) =>
            p.When(args, x => x.IsActive).Truthy()
              .Then(then => then
                  .Element("badge").AddClass("active")
                  .Element("status").SetText("Online"))
              .Else(else_ => else_
                  .Element("badge").AddClass("inactive")
                  .Element("status").SetText("Offline"))
        );
        var json = plan.Render();
        AssertSchemaValid(json);
        return VerifyJson(json);
    }
}
```

**Step 2: Run tests to generate snapshots**

```bash
dotnet test tests/Alis.Reactive.UnitTests --filter "FullyQualifiedName~WhenBranchingOnConditions" -v q
```
Expected: FAIL — `.verified.txt` files don't exist yet. Verify generates `.received.txt` files.

**Step 3: Accept snapshots**

For each test, copy `.received.txt` → `.verified.txt`:

```bash
cd tests/Alis.Reactive.UnitTests/Conditions
for f in *.received.txt; do cp "$f" "${f/received/verified}"; done
```

**Step 4: Run tests again to verify they pass**

```bash
dotnet test tests/Alis.Reactive.UnitTests --filter "FullyQualifiedName~WhenBranchingOnConditions" -v q
```
Expected: ALL PASS (6 tests)

**Step 5: Run ALL C# unit tests**

```bash
dotnet test tests/Alis.Reactive.UnitTests -v q
```
Expected: ALL PASS (existing + 6 new)

**Step 6: Commit**

```bash
git add tests/Alis.Reactive.UnitTests/Conditions/
git commit -m "test: 6 C# snapshot tests for conditions — When/Then/Else/ElseIf/And/Or"
```

---

## Task 9: Sandbox Conditions Page

**Files:**
- Modify: `Alis.Reactive.SandboxApp/Controllers/SandboxController.cs` (add action)
- Create: `Alis.Reactive.SandboxApp/Areas/Sandbox/Views/Sandbox/Conditions.cshtml`
- Modify: navigation layout (add link)

**Step 1: Add controller action**

Add to `SandboxController.cs` (or wherever sandbox actions live):

```csharp
public IActionResult Conditions() => View();
```

**Step 2: Create Conditions.cshtml**

This view tests conditions end-to-end: dispatches events with payloads, conditional reactions mutate DOM.

```cshtml
@using Alis.Reactive
@using Alis.Reactive.Builders
@{
    ViewData["Title"] = "Conditions";
    var plan = new ReactivePlan<TestModels.ScoreModel>();
}

<h2>Conditions Showcase</h2>

<div>
    <h3>Score Grade (When/Then/ElseIf/Else)</h3>
    <p>Score: <span id="score-display">—</span></p>
    <p>Grade: <span id="grade">—</span></p>
    <button id="btn-high" onclick="document.dispatchEvent(new CustomEvent('set-score', {detail:{score:95}}))">Score 95</button>
    <button id="btn-mid" onclick="document.dispatchEvent(new CustomEvent('set-score', {detail:{score:85}}))">Score 85</button>
    <button id="btn-low" onclick="document.dispatchEvent(new CustomEvent('set-score', {detail:{score:40}}))">Score 40</button>
</div>

<div>
    <h3>AND Condition</h3>
    <p>Result: <span id="and-result">—</span></p>
    <button id="btn-and-pass" onclick="document.dispatchEvent(new CustomEvent('and-test', {detail:{score:95,status:'active'}}))">Score 95 + Active</button>
    <button id="btn-and-fail" onclick="document.dispatchEvent(new CustomEvent('and-test', {detail:{score:95,status:'inactive'}}))">Score 95 + Inactive</button>
</div>

<div>
    <h3>OR Condition</h3>
    <p>Result: <span id="or-result">—</span></p>
    <button id="btn-or-admin" onclick="document.dispatchEvent(new CustomEvent('or-test', {detail:{role:'admin'}}))">Admin</button>
    <button id="btn-or-super" onclick="document.dispatchEvent(new CustomEvent('or-test', {detail:{role:'superuser'}}))">Superuser</button>
    <button id="btn-or-viewer" onclick="document.dispatchEvent(new CustomEvent('or-test', {detail:{role:'viewer'}}))">Viewer</button>
</div>

@{
    // When/Then/ElseIf/Else: grade assignment
    Html.On(plan, t => t
        .CustomEvent<TestModels.ScorePayload>("set-score", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
              .Then(then => then
                  .Element("grade").SetText("A")
                  .Element("score-display").SetText(args, x => x.Score))
              .ElseIf(args, x => x.Score).Gte(80)
              .Then(then => then
                  .Element("grade").SetText("B")
                  .Element("score-display").SetText(args, x => x.Score))
              .Else(else_ => else_
                  .Element("grade").SetText("F")
                  .Element("score-display").SetText(args, x => x.Score))
        )
    );

    // AND condition
    Html.On(plan, t => t
        .CustomEvent<TestModels.ScorePayload>("and-test", (args, p) =>
            p.When(args, x => x.Score).Gte(90)
              .And(g => g.When(args, x => x.Status).Eq("active"))
              .Then(then => then.Element("and-result").SetText("Active High Scorer"))
              .Else(else_ => else_.Element("and-result").SetText("Nope"))
        )
    );

    // OR condition
    Html.On(plan, t => t
        .CustomEvent<TestModels.UserPayload>("or-test", (args, p) =>
            p.When(args, x => x.Role).Eq("admin")
              .Or(g => g.When(args, x => x.Role).Eq("superuser"))
              .Then(then => then.Element("or-result").SetText("Authorized"))
              .Else(else_ => else_.Element("or-result").SetText("Denied"))
        )
    );
}

<script type="application/json" id="alis-plan" data-trace="debug">@Html.Raw(plan.Render())</script>
<script type="module" src="~/js/alis-reactive.js" asp-append-version="true"></script>
```

Note: `TestModels` namespace should contain simple payload classes. If the sandbox already has a models file, add them there. Otherwise create a `Models/TestModels.cs`:

```csharp
namespace TestModels
{
    public class ScoreModel
    {
        public string? Id { get; set; }
    }

    public class ScorePayload
    {
        public int Score { get; set; }
        public string Status { get; set; } = "";
    }

    public class UserPayload
    {
        public string? Name { get; set; }
        public string Role { get; set; } = "";
        public bool IsActive { get; set; }
    }
}
```

**Step 3: Build and verify**

```bash
npm run build && dotnet build
```
Expected: 0 errors

**Step 4: Run the app and verify in browser**

```bash
dotnet run --project Alis.Reactive.SandboxApp
```

Navigate to `/Sandbox/Conditions`. Click buttons and verify:
- "Score 95" → Grade shows "A", score shows "95"
- "Score 85" → Grade shows "B", score shows "85"
- "Score 40" → Grade shows "F", score shows "40"
- "Score 95 + Active" → AND result shows "Active High Scorer"
- "Score 95 + Inactive" → AND result shows "Nope"
- "Admin" → OR result shows "Authorized"
- "Superuser" → OR result shows "Authorized"
- "Viewer" → OR result shows "Denied"

**Step 5: Commit**

```bash
git add Alis.Reactive.SandboxApp/
git commit -m "feat: Sandbox Conditions page — When/Then/ElseIf/Else + And/Or interactive demo"
```

---

## Task 10: Playwright Browser Tests

**Files:**
- Create: `tests/Alis.Reactive.PlaywrightTests/Conditions/WhenConditionsEvaluateInBrowser.cs`

**Step 1: Write Playwright test**

```csharp
using Microsoft.Playwright;

namespace Alis.Reactive.PlaywrightTests.Conditions;

[TestFixture]
public class WhenConditionsEvaluateInBrowser : PlaywrightTestBase
{
    [Test]
    public async Task When_then_else_takes_correct_branch()
    {
        var page = await GetPage();
        await page.GotoAsync(BaseUrl + "/Sandbox/Conditions");
        await WaitForTraceMessage(page, "booted");

        // Click Score 95 → should get grade A
        await page.ClickAsync("#btn-high");
        await Expect(page.Locator("#grade")).ToHaveTextAsync("A");
        await Expect(page.Locator("#score-display")).ToHaveTextAsync("95");

        // Click Score 85 → should get grade B
        await page.ClickAsync("#btn-mid");
        await Expect(page.Locator("#grade")).ToHaveTextAsync("B");

        // Click Score 40 → should get grade F
        await page.ClickAsync("#btn-low");
        await Expect(page.Locator("#grade")).ToHaveTextAsync("F");
    }

    [Test]
    public async Task And_condition_requires_both_guards()
    {
        var page = await GetPage();
        await page.GotoAsync(BaseUrl + "/Sandbox/Conditions");
        await WaitForTraceMessage(page, "booted");

        // Both pass → success
        await page.ClickAsync("#btn-and-pass");
        await Expect(page.Locator("#and-result")).ToHaveTextAsync("Active High Scorer");

        // One fails → else
        await page.ClickAsync("#btn-and-fail");
        await Expect(page.Locator("#and-result")).ToHaveTextAsync("Nope");
    }

    [Test]
    public async Task Or_condition_requires_any_guard()
    {
        var page = await GetPage();
        await page.GotoAsync(BaseUrl + "/Sandbox/Conditions");
        await WaitForTraceMessage(page, "booted");

        await page.ClickAsync("#btn-or-admin");
        await Expect(page.Locator("#or-result")).ToHaveTextAsync("Authorized");

        await page.ClickAsync("#btn-or-super");
        await Expect(page.Locator("#or-result")).ToHaveTextAsync("Authorized");

        await page.ClickAsync("#btn-or-viewer");
        await Expect(page.Locator("#or-result")).ToHaveTextAsync("Denied");
    }
}
```

Note: Adapt the test base class methods (`GetPage`, `WaitForTraceMessage`, `BaseUrl`, `Expect`) to match the existing `PlaywrightTestBase` pattern. Read the actual base class before writing final code.

**Step 2: Run Playwright tests**

```bash
dotnet test tests/Alis.Reactive.PlaywrightTests --filter "FullyQualifiedName~WhenConditionsEvaluateInBrowser" -v n
```
Expected: ALL PASS (3 tests). If the app needs to be running, start it in background first.

**Step 3: Commit**

```bash
git add tests/Alis.Reactive.PlaywrightTests/Conditions/
git commit -m "test: 3 Playwright browser tests for conditions — When/Then/Else, And, Or"
```

---

## Task 11: Full Verification + Final Commit

**Step 1: Run ALL tests**

```bash
# TypeScript unit tests
npm test

# C# unit + schema tests
dotnet test tests/Alis.Reactive.UnitTests -v q

# Playwright browser tests (requires app running)
dotnet test tests/Alis.Reactive.PlaywrightTests -v q
```

Expected: ALL PASS across all 3 layers.

**Step 2: Verify test counts**

```
TS unit tests:     ~130 existing + 16 guard + 8 integration = ~154
C# unit tests:     ~35 existing + 6 condition snapshots = ~41
Playwright tests:  ~10 existing + 3 condition tests = ~13
Total:             ~208 tests
```

**Step 3: Run typecheck**

```bash
npm run typecheck
```
Expected: 0 errors.

**Step 4: Final commit (if any unstaged changes)**

```bash
git status
# If clean, done. If anything unstaged:
git add -A
git commit -m "chore: conditions module complete — all tests pass across 3 layers"
```

---

## Summary of Commits

| # | Message | Tests Added |
|---|---------|-------------|
| 1 | schema: add Guard, Branch, ConditionalReaction to reactive-plan schema | 0 (existing pass) |
| 2 | types: add Guard, Branch, ConditionalReaction TS interfaces | 0 |
| 3 | feat: conditions.ts guard evaluator with 16 direct unit tests | 16 |
| 4 | feat: ConditionalReaction handler in execute.ts, 8 integration tests | 8 |
| 5 | feat: Guard descriptors (Value/All/Any), Branch, ConditionalReaction, CoercionType | 0 |
| 6 | feat: C# condition builders — When/Then/Else/ElseIf/And/Or | 0 |
| 7 | test: 6 C# snapshot tests for conditions | 6 |
| 8 | feat: Sandbox Conditions page | 0 |
| 9 | test: 3 Playwright browser tests for conditions | 3 |

**Total new tests: ~33** (16 direct guard + 8 integration + 6 snapshots + 3 Playwright)

---

## Architecture Notes for Implementation

### CoercionType Inference Table

| C# Type | CoercionType | Example |
|---------|-------------|---------|
| `string` | `"string"` | `x => x.Name` |
| `int`, `int?`, `long`, `double`, `float`, `decimal`, `short`, `byte` | `"number"` | `x => x.Score` |
| `bool`, `bool?` | `"boolean"` | `x => x.IsActive` |
| Everything else | `"raw"` | `x => x.Data` |

### Guard Evaluation Contract

- **ValueGuard**: resolve source → coerce → compare with operand
- **AllGuard**: `guards.every(g => evaluate(g))` — short-circuits on first false
- **AnyGuard**: `guards.some(g => evaluate(g))` — short-circuits on first true
- **null guard (else branch)**: always passes — must be last in branches array
- **First match wins**: iterate branches top-down, execute first matching branch's reaction

### Presence operators and coercion

For presence operators (truthy, falsy, is-null, not-null), coercion is applied BEFORE the check:
- `is-null` / `not-null`: check against the **coerced** value (coerceAs "raw" recommended)
- `truthy` / `falsy`: check truthiness of the **coerced** value (coerceAs "boolean" for string→bool)

### The And/Or lambda pattern

```csharp
// This works because the lambda captures 'args' from outer scope
p.When(args, x => x.Score).Gte(90)
  .And(g => g.When(args, x => x.Status).Eq("active"))
  .Then(...)
```

The `g` parameter is a `ConditionStart<TModel>` which only exposes `When<TPayload>()`. The lambda must return a `GuardBuilder<TModel>` (from calling an operator method), ensuring you can't accidentally call `.Then()` inside the lambda.
