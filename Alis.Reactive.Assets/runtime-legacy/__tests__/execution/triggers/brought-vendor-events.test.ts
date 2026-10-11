import { afterEach, describe, expect, it } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../../../lifecycle/boot";
import { injectPartial } from "../../../execution/partials/inject";
import type {
  BrowserObjectContract, ComponentObject, PlanDocument, ReactionGraph, Shape, VendorContract,
} from "../../../types/index";
import { testVendors } from "../../support/vendors";

// A vendor the runtime has no code for: the plan says its object lives on the element as
// `picker`, listeners go through on/off, and a listener receives the event's argument.
const pickerVendor: VendorContract = {
  objectPath: [{ kind: "property", name: "picker" }],
  subscribe: "on",
  unsubscribe: "off",
  payload: "argument",
};

const vendors: Record<string, VendorContract> = { ...testVendors, picker: pickerVendor };
const stringShape: Shape = { kind: "string" };

afterEach(() => {
  resetBootStateForTests();
  document.body.innerHTML = "";
});

describe("a vendor declared as plan data", () => {
  it("delivers the listener's argument as the payload, through the vendor's own subscribe method", () => {
    document.body.innerHTML = `<span id="status"></span><select id="room"></select>`;
    const picker = attachPicker("room");

    boot(plan("Resident.BroughtVendor", "root", [onChangeShowValue()]));
    picker.emit("change", { value: "Garden Suite" });

    expect(text("status")).toBe("Garden Suite");
  });

  it("removes the listener through the vendor's unsubscribe method when its slot unloads", () => {
    document.body.innerHTML = `<span id="status"></span><select id="room"></select>`;
    const picker = attachPicker("room");
    boot(plan("Resident.BroughtVendorSlot", "root", []));

    loadPartialSlot("room-slot", [plan("Resident.BroughtVendorSlot", "partial", [onChangeShowValue()])]);
    expect(picker.listenerCount("change")).toBe(1);

    unloadPartialSlot("room-slot");
    expect(picker.listenerCount("change")).toBe(0);
  });

  it("hands an empty object to the reaction when the vendor passes no argument", () => {
    document.body.innerHTML = `<span id="status">before</span><select id="room"></select>`;
    const picker = attachPicker("room");

    boot(plan("Resident.BroughtVendorEmpty", "root", [onChangeShowValue()]));
    picker.emit("change", undefined);

    expect(text("status")).toBe("");
  });
});

describe("alis:mount and alis:unmount", () => {
  it("lets the app construct a widget in injected HTML before the slot's plan wires it", () => {
    document.body.innerHTML = `<span id="status"></span><div id="slot"></div>`;
    boot(plan("Resident.MountedWidget", "root", []));
    const constructed: FakePicker[] = [];
    const construct = (event: Event) => {
      const root = (event as CustomEvent<{ root: HTMLElement }>).detail.root;
      const select = root.querySelector("select");
      if (select !== null) constructed.push(attachPicker(select.id));
    };
    document.addEventListener("alis:mount", construct);

    const partial = plan("Resident.MountedWidget", "partial", [onChangeShowValue()]);
    injectPartial(
      document.getElementById("slot")!,
      `<select id="room"></select><script type="application/json" data-reactive-plan>${JSON.stringify(partial)}</script>`,
      "room-slot",
    );
    constructed[0]!.emit("change", { value: "Courtyard" });
    document.removeEventListener("alis:mount", construct);

    expect(text("status")).toBe("Courtyard");
  });

  it("announces unmount while the old content is still in the container", () => {
    document.body.innerHTML = `<div id="slot"><select id="room"></select></div>`;
    const seen: string[] = [];
    const record = (event: Event) => {
      seen.push((event as CustomEvent<{ root: HTMLElement }>).detail.root.innerHTML);
    };
    document.addEventListener("alis:unmount", record);

    injectPartial(document.getElementById("slot")!, `<p>next</p>`, "room-slot");
    document.removeEventListener("alis:unmount", record);

    expect(seen).toEqual([`<select id="room"></select>`]);
  });
});

type PickerListener = (value: unknown) => void;

class FakePicker {
  private readonly listeners = new Map<string, Set<PickerListener>>();

  on(eventName: string, listener: PickerListener): void {
    this.listenersFor(eventName).add(listener);
  }

  off(eventName: string, listener: PickerListener): void {
    this.listenersFor(eventName).delete(listener);
  }

  emit(eventName: string, value: unknown): void {
    for (const listener of this.listenersFor(eventName)) listener(value);
  }

  listenerCount(eventName: string): number {
    return this.listenersFor(eventName).size;
  }

  private listenersFor(eventName: string): Set<PickerListener> {
    let listeners = this.listeners.get(eventName);
    if (listeners === undefined) {
      listeners = new Set<PickerListener>();
      this.listeners.set(eventName, listeners);
    }

    return listeners;
  }
}

function attachPicker(elementId: string): FakePicker {
  const picker = new FakePicker();
  (document.getElementById(elementId) as HTMLElement & { picker?: FakePicker }).picker = picker;
  return picker;
}

function plan(planId: string, scope: "root" | "partial", behaviors: PlanDocument["behaviors"]): PlanDocument {
  const components: Record<string, ComponentObject> = {
    status: {
      id: "status",
      vendor: "native",
      type: "native.element.status",
      role: { kind: "object-target" },
      binding: { kind: "none" },
      container: { kind: "none" },
    },
  };
  if (behaviors.length > 0) {
    components.room = {
      id: "room",
      vendor: "picker",
      type: "picker.component.room",
      role: { kind: "object-target" },
      binding: { kind: "none" },
      container: { kind: "none" },
    };
  }

  return {
    version: 3,
    vendors,
    planId,
    scope: { kind: scope },
    types: {
      "native.element.status": statusType(),
      "picker.component.room": { properties: {}, methods: {}, events: { change: { channel: "change" } } },
    },
    components,
    behaviors,
  };
}

function statusType(): BrowserObjectContract {
  return {
    properties: {
      text: { path: [{ kind: "property", name: "textContent" }], shape: stringShape, access: "write" },
    },
    methods: {},
    events: {},
  };
}

function onChangeShowValue(): PlanDocument["behaviors"][number] {
  return {
    startsWhen: { kind: "component-event", component: "room", event: "change" },
    reaction: showPayloadValue(),
  };
}

function showPayloadValue(): ReactionGraph {
  return {
    kind: "set",
    on: { kind: "component", component: "status" },
    property: "text",
    value: {
      kind: "read",
      from: { kind: "payload", scope: "event" },
      member: "value",
      path: [{ kind: "property", name: "value" }],
      shape: stringShape,
      access: { kind: "property" },
    },
  };
}

function text(id: string): string {
  return document.getElementById(id)?.textContent ?? "";
}
