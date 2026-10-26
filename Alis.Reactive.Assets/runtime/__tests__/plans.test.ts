// Owners, handlers, the document's ready and raise, and slots (grammar.md 3.5, 6.1, 6.5).

import { afterEach, beforeAll, describe, expect, it } from "vitest";
import { registerPlugin, resetPluginsForTests } from "../objects/plugins";
import { Owner } from "../objects/owner";
import { startOwner } from "../plans/start";
import { documentPlugin } from "../plugins/document";
import { slotPlugin, resetSlotsForTests } from "../plugins/slot";
import { e, element, fakeWidget, on, plan, plugin, s, st } from "./support/plan";

beforeAll(() => {
  resetPluginsForTests();
  registerPlugin("alis.document", documentPlugin);
  registerPlugin("alis.slot", slotPlugin);
});

afterEach(() => {
  resetSlotsForTests();
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

describe("handlers", () => {
  it("run when the vendor raises the event, with the vendor's argument as the payload", () => {
    const grid = fakeWidget("roster");
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(
      { roster: element("roster", "fake"), status: element("status") },
      [on("roster", "rowSelected", setText("status", e.get(e.ref("e"), ["data", "name"])))],
    )]);

    grid.widget.trigger("rowSelected", { data: { name: "Margaret Hill" } });
    expect(status.textContent).toBe("Margaret Hill");
  });

  it("write a payload's cancel before the vendor reads it", () => {
    const grid = fakeWidget("roster");
    startOwner(Owner.page(), [plan(
      { roster: element("roster", "fake") },
      [on("roster", "actionBegin", st.set(e.ref("e"), ["cancel"], e.lit(true)))],
    )]);

    const args: { cancel?: boolean } = {};
    grid.widget.trigger("actionBegin", args);
    expect(args.cancel).toBe(true);
  });

  it("stop listening when their owner is disposed", () => {
    const grid = fakeWidget("roster");
    const owner = Owner.slot("panel");
    startOwner(owner, [plan({ roster: element("roster", "fake") }, [on("roster", "rowSelected")])]);
    expect(grid.widget.listenerCount("rowSelected")).toBe(1);
    owner.dispose();
    expect(grid.widget.listenerCount("rowSelected")).toBe(0);
  });

  it("isolate a failure: one failing handler stops no other", () => {
    const button = paragraph("save");
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(
      { save: element("save"), status: element("status"), missing: element("missing") },
      [
        on("save", "click", setText("missing", e.lit("never"))),
        on("save", "click", setText("status", e.lit("saved"))),
      ],
    )]);
    button.click();
    expect(status.textContent).toBe("saved");
  });
});

describe("the document", () => {
  it("runs ready handlers after every handler of the owner has subscribed", () => {
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(
      { document: plugin("alis.document"), status: element("status") },
      [
        on("document", "ready", st.do(e.call(e.obj("document"), ["raise"], [e.lit("loaded"), e.record({ who: e.lit("ready") })]))),
        on("document", "loaded", setText("status", e.get(e.ref("e"), ["who"]))),
      ],
    )]);
    expect(status.textContent).toBe("ready");
  });

  it("raises synchronously, and gives the event itself when there is no detail", () => {
    const status = paragraph("status");
    startOwner(Owner.page(), [plan(
      { document: plugin("alis.document"), status: element("status") },
      [on("document", "plain", setText("status", e.get(e.ref("e"), ["type"])))],
    )]);
    document.dispatchEvent(new CustomEvent("plain"));
    expect(status.textContent).toBe("plain");
  });

  it("reads the query string case-insensitively, first value or every value", () => {
    window.history.replaceState(null, "", "/page?Tab=medications&wing=east&wing=west");
    const status = paragraph("status");
    const all = paragraph("all");
    startOwner(Owner.page(), [plan(
      { document: plugin("alis.document"), status: element("status"), all: element("all") },
      [on("document", "ready",
        setText("status", e.as(e.get(e.get(e.obj("document"), ["query"]), ["tab"]), s.string)),
        setText("all", e.as(e.get(e.get(e.obj("document"), ["queryAll"]), ["WING"]), s.array(s.string))),
      )],
    )]);
    expect(status.textContent).toBe("medications");
    expect(all.textContent).toBe("east,west");
  });

  it("raises an event from a framework-stamped template button", () => {
    const status = paragraph("status");
    document.body.insertAdjacentHTML("beforeend", `<button id="row-edit" data-alis-raise="edit-row" data-alis-detail='{"id":4}'>Edit</button>`);
    startOwner(Owner.page(), [plan(
      { document: plugin("alis.document"), status: element("status") },
      [on("document", "edit-row", setText("status", e.get(e.ref("e"), ["id"])))],
    )]);
    document.getElementById("row-edit")!.click();
    expect(status.textContent).toBe("4");
  });
});

describe("slots", () => {
  function slotPlanHtml(statusText: string): string {
    const slotPlan = plan(
      { document: plugin("alis.document"), status: element("status") },
      [on("document", "ready", setText("status", e.lit(statusText)))],
    );
    return `<p id="inside">loaded</p><script type="application/json" data-reactive-plan>${JSON.stringify(slotPlan)}</script>`;
  }

  it("load the HTML, start only the slot's handlers, and keep the plan script out of the markup", () => {
    const panel = document.createElement("div");
    panel.id = "panel";
    document.body.appendChild(panel);
    const status = paragraph("status");
    const pageRuns = paragraph("page-runs");
    startOwner(Owner.page(), [plan(
      { document: plugin("alis.document"), pageRuns: element("page-runs") },
      [on("document", "ready", st.set(e.obj("pageRuns"), ["textContent"],
        e.as(e.op("add", e.as(e.get(e.obj("pageRuns"), ["textContent"]), s.number), e.lit(1)), s.string)))],
    )]);
    expect(pageRuns.textContent).toBe("1");

    const slot = slotPlugin.object("panel") as { load(html: string): void };
    slot.load(slotPlanHtml("from the slot"));
    expect(document.getElementById("inside")?.textContent).toBe("loaded");
    expect(panel.querySelector("[data-reactive-plan]")).toBeNull();
    expect(status.textContent).toBe("from the slot");
    expect(pageRuns.textContent).toBe("1");
  });

  it("announce unmount on the old content and mount on the new, in that order", () => {
    const panel = document.createElement("div");
    panel.id = "panel";
    panel.innerHTML = "<p>old</p>";
    document.body.appendChild(panel);
    const seen: string[] = [];
    const record = (name: string) => (event: Event) =>
      seen.push(`${name}:${((event as CustomEvent).detail.root as HTMLElement).innerHTML}`);
    const onUnmount = record("unmount");
    const onMount = record("mount");
    document.addEventListener("alis:unmount", onUnmount);
    document.addEventListener("alis:mount", onMount);

    (slotPlugin.object("panel") as { load(html: string): void }).load("<p>new</p>");
    document.removeEventListener("alis:unmount", onUnmount);
    document.removeEventListener("alis:mount", onMount);
    expect(seen).toEqual(["unmount:<p>old</p>", "mount:<p>new</p>"]);
  });

  it("release the owners of the slots nested inside content that is replaced or unloaded", () => {
    const outer = document.createElement("div");
    outer.id = "outer";
    document.body.appendChild(outer);
    const status = paragraph("status");
    const slots = (id: string) => slotPlugin.object(id) as { load(html: string): void; unload(): void };
    const innerPlan = plan(
      { document: plugin("alis.document"), status: element("status") },
      [on("document", "shiftChanged", setText("status", e.get(e.ref("e"), ["shift"])))],
    );
    const innerHtml = `<p>card</p><script type="application/json" data-reactive-plan>${JSON.stringify(innerPlan)}</script>`;
    const shiftChanged = (shift: string) => document.dispatchEvent(new CustomEvent("shiftChanged", { detail: { shift } }));

    slots("outer").load(`<div id="inner"></div>`);
    slots("inner").load(innerHtml);
    shiftChanged("night");
    expect(status.textContent).toBe("night");

    slots("outer").load("<p>replaced</p>");
    shiftChanged("day");
    expect(status.textContent).toBe("night");

    slots("outer").load(`<div id="inner"></div>`);
    slots("inner").load(innerHtml);
    shiftChanged("evening");
    expect(status.textContent).toBe("evening");

    slots("outer").unload();
    shiftChanged("late");
    expect(status.textContent).toBe("evening");
  });

  it("unload the previous load's handlers when the slot loads again", () => {
    const panel = document.createElement("div");
    panel.id = "panel";
    document.body.appendChild(panel);
    const grid = fakeWidget("roster");
    const slotPlan = plan({ roster: element("roster", "fake") }, [on("roster", "rowSelected")]);
    const html = `<script type="application/json" data-reactive-plan>${JSON.stringify(slotPlan)}</script>`;
    const slot = slotPlugin.object("panel") as { load(html: string): void; unload(): void };

    slot.load(html);
    slot.load(html);
    expect(grid.widget.listenerCount("rowSelected")).toBe(1);
    slot.unload();
    expect(grid.widget.listenerCount("rowSelected")).toBe(0);
  });
});
