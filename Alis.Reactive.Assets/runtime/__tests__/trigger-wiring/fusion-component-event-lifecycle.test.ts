import { afterEach, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../../lifecycle/boot";
import type { Component, JsType, Plan, Reaction, Shape, ValueProducer } from "../../types";

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
      partialPlan(planId, "server-fusion-plan", {
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

function rootPlan(planId: string, components: Record<string, Component>): Plan {
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
  partId: string,
  entries: Partial<Pick<Plan, "components" | "behaviors" | "types">>,
): Plan {
  return {
    version: 3,
    planId,
    scope: { kind: "partial", partId },
    types: entries.types ?? {},
    components: entries.components ?? {},
    behaviors: entries.behaviors ?? [],
  };
}

function nativeTextType(): JsType {
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

function fusionEventType(): JsType {
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

function displayComponent(id: string): Component {
  return {
    id,
    vendor: "native",
    type: "native.input",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function fusionComponent(id: string): Component {
  return {
    id,
    vendor: "fusion",
    type: "fusion.fake",
    contribution: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function setText(component: string, value: string): Reaction {
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
