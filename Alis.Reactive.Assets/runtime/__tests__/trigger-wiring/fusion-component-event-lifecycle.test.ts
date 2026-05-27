import { afterEach, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../../lifecycle/boot";
import type { ComponentObject, BrowserObjectContract, PlanDocument, ReactionGraph, Shape, ValueProducer } from "../../types";

const stringShape: Shape = { kind: "string" };

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("fusion component event lifecycle", () => {
  it("unwires component events when a partial slot unloads", () => {
    document.body.innerHTML = `
      <span id="status"></span>
      <div id="fusion-source"></div>
    `;
    const fusionRoot = new FakeFusionRoot();
    const fusionElement = elementById("fusion-source") as FusionHostElement;
    fusionElement.ej2_instances = [fusionRoot];

    const planId = "Resident.PartialFusionEvent";
    const resident = rootPlan(planId, {
      status: displayComponent("status"),
    });
    boot(resident);

    loadPartialSlot("fusion-slot", [
      partialPlan(planId, {
        types: { "fusion.fake": fusionEventType() },
        components: {
          source: fusionComponent("fusion-source"),
        },
        behaviors: [
          {
            startsWhen: {
              kind: "component-event",
              component: "source",
              event: "changed",
            },
            reaction: setText("status", "changed"),
          },
        ],
      }),
    ]);

    fusionRoot.emit("change");
    expect(textFor("status")).toBe("changed");

    elementById("status").textContent = "";
    unloadPartialSlot("fusion-slot");
    fusionRoot.emit("change");

    expect(textFor("status")).toBe("");
  });
});

interface FusionHostElement extends HTMLElement {
  ej2_instances?: unknown[];
}

class FakeFusionRoot {
  private readonly handlers = new Map<string, Set<(args: unknown) => void>>();

  addEventListener(channel: string, handler: (args: unknown) => void): void {
    this.handlersFor(channel).add(handler);
  }

  removeEventListener(channel: string, handler: (args: unknown) => void): void {
    this.handlers.get(channel)?.delete(handler);
  }

  emit(channel: string): void {
    for (const handler of this.handlersFor(channel)) handler({});
  }

  private handlersFor(channel: string): Set<(args: unknown) => void> {
    let handlers = this.handlers.get(channel);
    if (handlers === undefined) {
      handlers = new Set<(args: unknown) => void>();
      this.handlers.set(channel, handlers);
    }

    return handlers;
  }
}

function rootPlan(planId: string, components: Record<string, ComponentObject>): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "root" },
    types: { "native.input": nativeTextType() },
    components,
    behaviors: [],
  };
}

function partialPlan(
  planId: string,
  entries: Partial<Pick<PlanDocument, "components" | "behaviors" | "types">>,
): PlanDocument {
  return {
    version: 3,
    planId,
    scope: { kind: "partial" },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: entries.behaviors ?? [],
  };
}

function nativeTextType(): BrowserObjectContract {
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

function fusionEventType(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {
      changed: {
        channel: "change",
        payloadType: { kind: "untyped" },
      },
    },
  };
}

function displayComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.input",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function fusionComponent(id: string): ComponentObject {
  return {
    id,
    vendor: "fusion",
    type: "fusion.fake",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function setText(component: string, value: string): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component },
    property: "textContent",
    value: literal(value),
  };
}

function literal(value: string): ValueProducer {
  return { kind: "literal", value, shape: stringShape };
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
