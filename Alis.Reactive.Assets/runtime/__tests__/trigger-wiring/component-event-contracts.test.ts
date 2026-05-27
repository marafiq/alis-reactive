import { afterEach, describe, expect, it } from "vitest";
import { boot, resetBootStateForTests } from "../../lifecycle/boot";
import type { ComponentObject, BrowserObjectContract, PlanDocument, ReactionGraph, Shape, ValueExpression } from "../../types";

const stringShape: Shape = { kind: "string" };

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("component event trigger contracts", () => {
  it("subscribes to the declared browser channel for the component event", () => {
    document.body.innerHTML = `
      <button id="event-source"></button>
      <span id="status"></span>
    `;

    boot(planWithComponentEvent({
      changed: {
        channel: "click",
        payloadType: { kind: "untyped" },
      },
    }));

    elementById("event-source").dispatchEvent(new Event("changed"));
    expect(document.getElementById("status")?.textContent).toBe("");

    elementById("event-source").dispatchEvent(new Event("click"));
    expect(document.getElementById("status")?.textContent).toBe("changed");
  });

  it("rejects component event triggers that are missing from the object contract", () => {
    expect(() => boot(planWithComponentEvent({})))
      .toThrow('[alis] event "changed" is not declared on component "source" (type: native.event-source; declared events: none)');
  });
});

function planWithComponentEvent(events: BrowserObjectContract["events"]): PlanDocument {
  return {
    version: 3,
    planId: "Resident.EventContracts",
    scope: { kind: "root" },
    types: {
      "native.event-source": eventSourceType(events),
      "native.display": displayType(),
    },
    components: {
      source: component("event-source", "native.event-source"),
      status: component("status", "native.display"),
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
  };
}

function eventSourceType(events: BrowserObjectContract["events"]): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events,
  };
}

function displayType(): BrowserObjectContract {
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

function component(id: string, type: string): ComponentObject {
  return {
    id,
    vendor: "native",
    type,
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function setText(componentKey: string, value: string): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component: componentKey },
    property: "textContent",
    value: literal(value),
  };
}

function literal(value: string): ValueExpression {
  return { kind: "literal", value, shape: stringShape };
}

function elementById(id: string): HTMLElement {
  const element = document.getElementById(id);
  expect(element).not.toBeNull();
  if (element === null) {
    throw new Error(`Expected DOM fixture "${id}" to exist`);
  }

  return element;
}
