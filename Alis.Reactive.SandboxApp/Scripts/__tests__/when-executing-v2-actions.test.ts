import { beforeEach, describe, expect, it, vi } from "vitest";
import { executeAction } from "../execution/execute";
import type { PlanAction } from "../types";
import {
  contextObjectContract,
  createPlan,
  flushAsync,
  htmlBlockContract,
  nativeTextContract,
  method,
  scalar,
} from "./support/v2-fixtures";

describe("when executing v2 actions", () => {
  beforeEach(() => {
    document.body.innerHTML = `
      <input id="resident-name" value="Ava" />
      <div id="status"></div>
      <div id="secondary"></div>
      <div id="settled"></div>
    `;

    (globalThis as { alis?: unknown }).alis = {
      objects: {
        audit: {
          remember: vi.fn(),
        },
      },
    };
  });

  it("branches on binding values and can call context-object methods", async () => {
    const plan = createPlan({
      contracts: {
        "native.text": nativeTextContract,
        "html.block": htmlBlockContract,
        "app.audit": contextObjectContract({
          remember: method("remember", [scalar("string")]),
        }),
      },
      objects: {
        residentName: { contract: "native.text", elementId: "resident-name" },
        status: { contract: "html.block", elementId: "status" },
        audit: { contract: "app.audit" },
      },
      bindings: {
        "Resident.Name": {
          object: "residentName",
          valueMember: "value",
          shape: scalar("string"),
        },
      },
    });

    const action: PlanAction = {
      kind: "branch",
      cases: [
        {
          when: {
            kind: "compare",
            left: { kind: "binding", binding: "Resident.Name" },
            op: "eq",
            right: { kind: "literal", value: "Ava" },
            as: scalar("string"),
          },
          run: {
            kind: "sequence",
            steps: [
              {
                kind: "set",
                target: { object: "status", member: "text" },
                value: { kind: "literal", value: "match" },
              },
              {
                kind: "call",
                target: { object: "audit", member: "remember" },
                args: [{ kind: "binding", binding: "Resident.Name" }],
              },
            ],
          },
        },
        {
          run: {
            kind: "set",
            target: { object: "status", member: "text" },
            value: { kind: "literal", value: "miss" },
          },
        },
      ],
    };

    await executeAction(action, { plan });

    expect(document.getElementById("status")?.textContent).toBe("match");
    expect((globalThis as any).alis.objects.audit.remember).toHaveBeenCalledWith("Ava");
  });

  it("runs parallel steps and then executes onSettled", async () => {
    const plan = createPlan({
      contracts: {
        "html.block": htmlBlockContract,
      },
      objects: {
        status: { contract: "html.block", elementId: "status" },
        secondary: { contract: "html.block", elementId: "secondary" },
        settled: { contract: "html.block", elementId: "settled" },
      },
    });

    const action: PlanAction = {
      kind: "parallel",
      steps: [
        {
          kind: "set",
          target: { object: "status", member: "text" },
          value: { kind: "literal", value: "first" },
        },
        {
          kind: "set",
          target: { object: "secondary", member: "text" },
          value: { kind: "literal", value: "second" },
        },
      ],
      onSettled: {
        kind: "set",
        target: { object: "settled", member: "text" },
        value: { kind: "literal", value: "done" },
      },
    };

    await executeAction(action, { plan });
    await flushAsync();

    expect(document.getElementById("status")?.textContent).toBe("first");
    expect(document.getElementById("secondary")?.textContent).toBe("second");
    expect(document.getElementById("settled")?.textContent).toBe("done");
  });
});
