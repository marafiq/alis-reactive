import { afterEach, describe, expect, it, vi } from "vitest";
import { resetLiveClearForTests, wireLiveValidation } from "../validation/live-clear";
import type {
  ComponentObject,
  ComponentValidation,
  BrowserObjectContract,
  PlanDocument,
  Shape,
  ValueExpression,
} from "../types/index";

const stringShape: Shape = { kind: "string" };
const noneShape: Shape = { kind: "none" };

afterEach(() => {
  resetLiveClearForTests();
  document.body.innerHTML = "";
});

describe("live validation wiring", () => {
  it("retries component change wiring when a fusion root becomes ready after DOM wiring", () => {
    document.body.innerHTML = `
      <form id="resident-form">
        <div id="care-date"></div>
      </form>
      <div id="Runtime_LiveValidation_validation_summary" hidden></div>
    `;

    const reactivePlan = planWithValidationField(fusionField("care-date"));

    wireLiveValidation(reactivePlan, "resident-form");

    const fusionRoot = new FakeFusionRoot();
    fusionHost("care-date").ej2_instances = [fusionRoot];
    wireLiveValidation(reactivePlan, "resident-form");

    fusionRoot.emit("change");

    expect(document.getElementById("Runtime_LiveValidation_validation_summary")?.textContent)
      .toContain("Care date is required");
  });

  it("does not hide a malformed fusion event source as lifecycle readiness", () => {
    document.body.innerHTML = `
      <form id="resident-form">
        <div id="care-date"></div>
      </form>
    `;

    fusionHost("care-date").ej2_instances = [{}];

    expect(() => wireLiveValidation(planWithValidationField(fusionField("care-date")), "resident-form"))
      .toThrow(TypeError);
  });

  it("partial abort forgets only the live validation wiring owned by that signal", () => {
    document.body.innerHTML = `
      <form id="resident-form">
        <div id="care-date"></div>
      </form>
    `;

    const reactivePlan = planWithValidationField(fusionField("care-date"));
    const fieldElement = fusionHost("care-date");
    const addEventListener = vi.spyOn(fieldElement, "addEventListener");
    const bootLifetime = new AbortController();

    wireLiveValidation(reactivePlan, "resident-form", bootLifetime.signal);

    expect(eventListenerCount(addEventListener, "input")).toBe(1);
    expect(eventListenerCount(addEventListener, "blur")).toBe(1);

    const fusionRoot = new FakeFusionRoot();
    fieldElement.ej2_instances = [fusionRoot];
    const partialLifetime = new AbortController();
    wireLiveValidation(reactivePlan, "resident-form", partialLifetime.signal);

    expect(fusionRoot.listenerCount("change")).toBe(1);

    partialLifetime.abort();
    expect(fusionRoot.listenerCount("change")).toBe(0);

    wireLiveValidation(reactivePlan, "resident-form", new AbortController().signal);

    expect(fusionRoot.listenerCount("change")).toBe(1);
    expect(eventListenerCount(addEventListener, "input")).toBe(1);
    expect(eventListenerCount(addEventListener, "blur")).toBe(1);
  });
});

function eventListenerCount(
  addEventListener: ReturnType<typeof vi.spyOn>,
  eventName: string,
): number {
  return addEventListener.mock.calls
    .filter(call => call[0] === eventName)
    .length;
}

function planWithValidationField(component: ComponentObject): PlanDocument {
  return {
    version: 3,
    planId: "Runtime.LiveValidation",
    scope: { kind: "root" },
    types: { "fusion.fake": fusionType(), "native.form": nativeType() },
    components: {
      "resident-form": validationContainer("resident-form", [
        validationRule("care-date"),
      ]),
      "care-date": component,
    },
    behaviors: [],
  };
}

function validationContainer(id: string, validationRules: ComponentValidation[]): ComponentObject {
  return {
    id,
    vendor: "native",
    type: "native.form",
    role: { kind: "validation-container" },
    binding: { kind: "none" },
    container: {
      kind: "validation-container",
      validationRules,
    },
  };
}

function validationRule(componentKey: string): ComponentValidation {
  return {
    component: componentKey,
    value: literal("", stringShape),
    serverFieldName: "CareDate",
    rules: [
      {
        name: "required",
        message: "Care date is required",
        execution: {
          kind: "none",
          activation: { kind: "always" },
          comparisonShape: noneShape,
        },
      },
    ],
  };
}

function fusionField(id: string): ComponentObject {
  return {
    id,
    vendor: "fusion",
    type: "fusion.fake",
    role: { kind: "object-target" },
    binding: { kind: "none" },
    container: { kind: "none" },
  };
}

function literal(value: string, shape: Shape): ValueExpression {
  return { kind: "literal", value, shape };
}

function fusionType(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function nativeType(): BrowserObjectContract {
  return {
    properties: {},
    methods: {},
    events: {},
  };
}

function fusionHost(id: string): FusionHostElement {
  return document.getElementById(id) as FusionHostElement;
}

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

  listenerCount(channel: string): number {
    return this.handlers.get(channel)?.size ?? 0;
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
