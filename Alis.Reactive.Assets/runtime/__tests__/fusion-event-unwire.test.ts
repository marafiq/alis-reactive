import { afterEach, beforeEach, describe, expect, it } from "vitest";
import { Button } from "@syncfusion/ej2-buttons";
import { wire } from "../events/event-fusion";

// A real Syncfusion component, wired the way a partial slot wires it: the slot's abort signal unwires it.
// The other side, a component its host destroyed before the slot unloads, is proven in the browser:
// Node's AbortSignal swallows an exception thrown by an abort listener, so jsdom cannot show it.
let slot: AbortController;
let acknowledge: Button;

beforeEach(() => {
  document.body.innerHTML = `<button id="acknowledge-handover"></button>`;
  acknowledge = new Button({ content: "Acknowledge handover" }, "#acknowledge-handover");
  slot = new AbortController();
});

afterEach(() => {
  document.body.innerHTML = "";
});

describe("a fusion event wired for a partial slot", () => {
  it("no longer reaches its handler once the slot unloads", () => {
    const heard: unknown[] = [];
    wire(acknowledge, "created", data => heard.push(data), { signal: slot.signal });

    slot.abort();
    acknowledge.trigger("created", { name: "created" });

    expect(heard).toEqual([]);
  });

  it("reaches its handler while the slot is loaded", () => {
    const heard: unknown[] = [];
    wire(acknowledge, "created", data => heard.push(data), { signal: slot.signal });

    acknowledge.trigger("created", { name: "created" });

    expect(heard).toEqual([{ name: "created" }]);
  });
});
