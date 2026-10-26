// The vendor driver's second way to listen (grammar.md 6.9 E4): a listener assigned to the property the
// event names. The method style is covered by the handler tests in plans.test.ts.

import { afterEach, beforeAll, describe, expect, it } from "vitest";
import { registerModule, resetModulesForTests } from "../objects/modules";
import { Owner } from "../objects/owner";
import { startOwner } from "../plans/start";
import { documentModule } from "../modules/document";
import { subscribeVendor } from "../objects/vendor-driver";
import { e, element, fakeWidget, on, plan, s, st, vendors } from "./support/plan";

beforeAll(() => {
  resetModulesForTests();
  registerModule("alis.document", documentModule);
});

afterEach(() => {
  document.body.innerHTML = "";
});

function paragraph(id: string): HTMLElement {
  const p = document.createElement("p");
  p.id = id;
  document.body.appendChild(p);
  return p;
}

const setText = (id: string, value: Parameters<typeof st.set>[2]) =>
  st.set(e.obj(id), ["textContent"], e.as(value, s.string));

const bellObjects = () => ({ bell: element("bell", "assigned"), status: element("status"), desk: element("desk") });

describe("a listener assigned to a callback property", () => {
  it("runs the handler when the widget calls the function it finds in the property", () => {
    const bell = fakeWidget("bell", { onPressed: null });
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(bellObjects(), [
      on("bell", "onPressed", setText("status", e.get(e.ref("e"), ["room"]))),
    ])]);

    bell.widget.invoke("onPressed", { room: "12" });
    expect(status.textContent).toBe("12");
  });

  it("keeps calling the function the property already held, before the handler", () => {
    const order: string[] = [];
    const own = function (this: unknown, call: { room: string }): void {
      order.push(`own:${call.room}:${this === bell.widget}`);
    };
    const bell = fakeWidget("bell", { onPressed: own });
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(bellObjects(), [
      on("bell", "onPressed", setText("status", e.get(e.ref("e"), ["room"]))),
    ])]);

    bell.widget.invoke("onPressed", { room: "12" });
    expect(order).toEqual(["own:12:true"]);
    expect(status.textContent).toBe("12");
  });

  it("runs two handlers of one callback in plan order", () => {
    const bell = fakeWidget("bell", { onPressed: null });
    const status = paragraph("status");
    const desk = paragraph("desk");
    startOwner(Owner.page(), [plan(bellObjects(), [
      on("bell", "onPressed", setText("status", e.get(e.ref("e"), ["room"]))),
      on("bell", "onPressed", setText("desk", e.as(e.get(e.obj("status"), ["textContent"]), s.string))),
    ])]);

    bell.widget.invoke("onPressed", { room: "12" });
    expect(status.textContent).toBe("12");
    expect(desk.textContent).toBe("12");
  });

  it("puts back what the property held when the owner is disposed", () => {
    const own = (): void => {};
    const bell = fakeWidget("bell", { onPressed: own });
    const status = paragraph("status");
    const owner = Owner.slot("desk");
    startOwner(owner, [plan(bellObjects(), [
      on("bell", "onPressed", setText("status", e.get(e.ref("e"), ["room"]))),
    ])]);
    expect(bell.widget.onPressed).not.toBe(own);

    owner.dispose();

    expect(bell.widget.onPressed).toBe(own);
    bell.widget.invoke("onPressed", { room: "12" });
    expect(status.textContent).toBe("");
  });

  it("leaves no disposed handler behind whichever owner goes first", () => {
    for (const firstToGo of ["page", "slot"]) {
      const bell = fakeWidget("bell", { onPressed: null });
      const status = paragraph("status");
      const desk = paragraph("desk");
      const page = Owner.page();
      const slot = Owner.slot("desk");
      startOwner(page, [plan(bellObjects(), [on("bell", "onPressed", setText("status", e.lit("page")))])]);
      startOwner(slot, [plan(bellObjects(), [on("bell", "onPressed", setText("desk", e.lit("slot")))])]);

      (firstToGo === "page" ? page : slot).dispose();
      bell.widget.invoke("onPressed", { room: "12" });
      expect([status.textContent, desk.textContent]).toEqual(firstToGo === "page" ? ["", "slot"] : ["page", ""]);

      (firstToGo === "page" ? slot : page).dispose();
      expect(bell.widget.onPressed).toBeNull();
      document.body.innerHTML = "";
    }
  });

  it("is a boundary error when the object has no such property", () => {
    const bell = fakeWidget("bell", { onPressed: null });

    expect(() => subscribeVendor(bell.widget, "fake", vendors.assigned!, "onRung", () => {}))
      .toThrow('[alis] fake onRung has no member "onRung"');
    expect(bell.widget.onPressed).toBeNull();
  });
});
