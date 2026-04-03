---
title: Writing Tests
description: How to write V2-focused tests that prove behavior without coupling to deleted internals.
sidebar:
  order: 2
---

Tests should describe behavior using the active V2 model.

## C# tests

C# tests should render the plan and verify:

- contracts
- objects
- bindings
- workflows
- schema validity

Prefer scenario names that describe user-visible behavior, not deleted internal nouns.

## TypeScript runtime tests

Construct a V2 plan object and execute the real runtime path.

```typescript
import { describe, expect, it } from "vitest";
import { boot } from "../lifecycle/boot";

describe("when a dom-ready workflow dispatches an event", () => {
  it("fires the event immediately", () => {
    let fired = false;
    document.addEventListener("ready-evt", () => { fired = true; });

    boot({
      version: 2,
      planId: "Test.Model",
      contracts: {},
      objects: {},
      bindings: {},
      workflows: [
        {
          when: { kind: "dom-ready" },
          run: { kind: "dispatch", name: "ready-evt" },
        },
      ],
    });

    expect(fired).toBe(true);
  });
});
```

## Browser tests

Playwright tests should prove the complete path:

- Razor authoring
- rendered V2 JSON
- runtime boot
- browser behavior

## Test naming rules

- Prefer `workflow`, `binding`, `contract`, `action`, `request`, and `subscription`.
- Avoid deleted pre-V2 vocabulary unless the test is explicitly enforcing its removal.
- Prefer scenario language over implementation language.

## Helper rules

- Keep helpers small and composable.
- Do not hide broad workflows behind one helper.
- If a helper forces the reader to understand too many concepts, split it.
