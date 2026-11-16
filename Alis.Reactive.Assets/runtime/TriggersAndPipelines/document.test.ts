import { describe, expect, test } from "vitest";
import { DocumentPlugin, markupDetail } from "./index";

const wardPage = new URL("https://care.example/care/residents/wards/East?ward=West&Beds=7");

function detailOf(heard: unknown[]): (...args: unknown[]) => void {
  return (...args: unknown[]) => {
    const [event] = args;
    const detail: unknown = event instanceof CustomEvent ? event.detail : "no CustomEvent";
    heard.push(detail);
  };
}

describe("when a step dispatches an app event (M1.D2)", () => {
  test("its handlers hear a CustomEvent of its name whose detail is the payload", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const heard: unknown[] = [];
    plugin.addEventListener("flagged", detailOf(heard));

    plugin.dispatch("flagged", { room: 12, reason: "fall" });

    expect(heard).toStrictEqual([{ room: 12, reason: "fall" }]);
  });

  test("a dispatch with no payload raises a detail of null", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const heard: unknown[] = [];
    plugin.addEventListener("roundStarted", detailOf(heard));

    plugin.dispatch("roundStarted", null);

    expect(heard).toStrictEqual([null]);
  });

  test("HandlersHearInTheOrderTheySubscribed: each listener runs inside the dispatch, in the order it subscribed, before the step after it", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const ran: string[] = [];
    plugin.addEventListener("callLogged", () => { ran.push("log"); });
    plugin.addEventListener("callLogged", () => { ran.push("pager"); });

    plugin.dispatch("callLogged", null);
    ran.push("desk");

    expect(ran).toStrictEqual(["log", "pager", "desk"]);
  });

  test("a dispatch of another event's name reaches no listener of this one", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const heard: unknown[] = [];
    plugin.addEventListener("slotOpened", detailOf(heard));

    plugin.dispatch("roundStarted", null);

    expect(heard).toStrictEqual([]);
  });

  test("a listener its owner removed hears no later dispatch, and the one beside it still hears", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const removed: unknown[] = [];
    const kept: unknown[] = [];
    const leaves = detailOf(removed);
    plugin.addEventListener("flagged", leaves);
    plugin.addEventListener("flagged", detailOf(kept));

    plugin.removeEventListener("flagged", leaves);
    plugin.dispatch("flagged", { room: 4 });

    expect(removed).toStrictEqual([]);
    expect(kept).toStrictEqual([{ room: 4 }]);
  });
});

describe("when the page starts, on load runs once its start subscribed every handler (M1.D1)", () => {
  test("a listener of ready runs at the start and not before, once", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const ran: string[] = [];
    plugin.addEventListener("ready", () => { ran.push("loaded"); });
    ran.push("subscribed");

    plugin.started();
    plugin.started();

    expect(ran).toStrictEqual(["subscribed", "loaded"]);
  });

  test("a listener of ready its owner removed before the start runs nothing", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);
    const ran: string[] = [];
    const leaves = () => { ran.push("removed"); };
    plugin.addEventListener("ready", leaves);
    plugin.addEventListener("ready", () => { ran.push("kept"); });

    plugin.removeEventListener("ready", leaves);
    plugin.started();

    expect(ran).toStrictEqual(["kept"]);
  });
});

describe("when markup raises an app event, its detail is read from the marked element's attributes (M1.D3)", () => {
  test("each member attribute carries its member's text, named back from the attribute as the dataset names it", () => {
    const detail = markupDetail([
      { name: "type", value: "button" },
      { name: "data-alis-raise", value: "announced" },
      { name: "data-alis-detail-party-size", value: "12" },
      { name: "data-alis-detail-capacity", value: "9" },
      { name: "data-alis-detail-type", value: "a coach party" },
      { name: "data-alis-null", value: "" },
    ]);

    expect(detail).toStrictEqual({ partySize: "12", capacity: "9", type: "a coach party" });
  });

  test("data-alis-null names each member that holds null, beside the members carried", () => {
    const detail = markupDetail([
      { name: "data-alis-raise", value: "escortRequested" },
      { name: "data-alis-detail-note", value: "Late" },
      { name: "data-alis-null", value: "guide" },
    ]);

    expect(detail).toStrictEqual({ note: "Late", guide: null });
  });

  test("a payload whose members all hold null is an object of nulls, not a detail of null", () => {
    const detail = markupDetail([
      { name: "data-alis-raise", value: "escortRequested" },
      { name: "data-alis-null", value: "guide note" },
    ]);

    expect(detail).toStrictEqual({ guide: null, note: null });
  });

  test("a marked element with neither a member attribute nor data-alis-null raises a detail of null", () => {
    const detail = markupDetail([
      { name: "class", value: "bell" },
      { name: "data-alis-raise", value: "announced" },
    ]);

    expect(detail).toBeNull();
  });

  test("a payload that carries no member and names no null is an empty object", () => {
    const detail = markupDetail([
      { name: "data-alis-raise", value: "rotaChecked" },
      { name: "data-alis-null", value: "" },
    ]);

    expect(detail).toStrictEqual({});
  });

  test("a member named with digits and underscores keeps them, and only a hyphen before a lowercase letter names a capital", () => {
    const detail = markupDetail([
      { name: "data-alis-detail-room_2", value: "East" },
      { name: "data-alis-detail-bed-number2", value: "7" },
    ]);

    expect(detail).toStrictEqual({ room_2: "East", bedNumber2: "7" });
  });
});

describe("when a step reads the page's own URL (M2.F2)", () => {
  test("query reads a key's first value, its name compared ignoring case, and null for a key the URL lacks", () => {
    const plugin = new DocumentPlugin(new EventTarget(), new URL("https://care.example/care/record/chart?TAB=medications&tab=vitals"));

    expect([plugin.query("Tab"), plugin.query("Bed")]).toStrictEqual(["medications", null]);
  });

  test("segment reads the path's segment at a placeholder's place, the path matched to the template ignoring case", () => {
    const plugin = new DocumentPlugin(new EventTarget(), wardPage);

    expect(plugin.segment(["Care", "Residents", "wards", "{Ward}"], 3)).toBe("East");
  });

  test("each read takes the address as it is when the step runs", () => {
    const address = { pathname: "/care/residents/wards/East", search: "?ward=West" };
    const plugin = new DocumentPlugin(new EventTarget(), address);
    address.pathname = "/care/residents/wards/North";
    address.search = "?ward=South";

    expect([plugin.segment(["care", "residents", "wards", "{Ward}"], 3), plugin.query("ward")]).toStrictEqual(["North", "South"]);
  });
});
