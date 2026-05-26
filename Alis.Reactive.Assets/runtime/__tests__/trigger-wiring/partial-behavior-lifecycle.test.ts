import { afterEach, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../../lifecycle/boot";
import type { BranchCase, ComponentObject, Condition, BrowserObjectContract, Plan, Reaction, Shape, ValueProducer } from "../../types";

const stringShape: Shape = { kind: "string" };
const booleanShape: Shape = { kind: "boolean" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("partial behavior lifecycle", () => {
  it("unwires nested branch and else behavior when a partial slot unloads", () => {
    document.body.innerHTML = `<span id="status"></span>`;
    const planId = "Resident.PartialBranch";
    const resident = rootPlan(planId, {
      status: displayComponent("status"),
    });
    boot(resident);

    loadPartialSlot("drawer-slot", [
      partialPlan(planId, {
        behaviors: [
          {
            startsWhen: {
              kind: "document-event",
              event: "drawer:tick",
              payloadType: { kind: "untyped" },
            },
            reaction: branch([
              { guard: { kind: "when", condition: falseCondition() }, reaction: setText("status", "outer guarded") },
              {
                guard: { kind: "default" },
                reaction: branch([
                  { guard: { kind: "when", condition: falseCondition() }, reaction: setText("status", "inner guarded") },
                  { guard: { kind: "default" }, reaction: setText("status", "inner else") },
                ]),
              },
            ]),
          },
        ],
      }),
    ]);

    document.dispatchEvent(new CustomEvent("drawer:tick"));
    expect(textFor("status")).toBe("inner else");

    elementById("status").textContent = "";
    unloadPartialSlot("drawer-slot");
    document.dispatchEvent(new CustomEvent("drawer:tick"));

    expect(textFor("status")).toBe("");
  });
});

function rootPlan(planId: string, components: Record<string, ComponentObject>): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: { "native.input": nativeInputType() },
    components,
    behaviors: [],
  };
}

function partialPlan(
  planId: string,
  entries: Partial<Pick<Plan, "components" | "behaviors" | "types">>,
): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: entries.behaviors ?? [],
  };
}

function nativeInputType(): BrowserObjectContract {
  return {
    properties: {
      textContent: {
        path: [{ kind: "property", name: "textContent" }],
        shape: stringShape,
        access: "readwrite",
      },
    },
    methods: {},
    events: {},
  };
}

function displayComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function falseCondition(): Condition {
  return {
    kind: "compare",
    left: literal(false, booleanShape),
    op: "truthy",
    right: { kind: "none" },
    shape: booleanShape,
    itemShape: noneShape,
  };
}

function setText(component: string, value: string): Reaction {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value: literal(value, stringShape),
  };
}

function branch(cases: BranchCase[]): Reaction {
  return { kind: "branch", cases };
}

function literal(value: string | boolean, shape: Shape): ValueProducer {
  return { kind: "literal", value, shape };
}

function textFor(id: string): string {
  return elementById(id).textContent ?? "";
}

function elementById(id: string): HTMLElement {
  const element = document.getElementById(id);
  expect(element).not.toBeNull();
  if (element === null) {
    throw new Error(`Expected DOM fixture "${id}" to exist`);
  }

  return element;
}
