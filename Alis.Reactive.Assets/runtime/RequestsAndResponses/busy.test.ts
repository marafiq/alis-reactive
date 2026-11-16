import { afterEach, describe, expect, test, vi } from "vitest";
import { attempt, captureBusy, prepare } from "./index";

const untraced: Parameters<typeof attempt>[4] = {
  attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }),
  end: () => undefined,
};
type Send = Parameters<typeof prepare>[0];
type ElementFor = NonNullable<Parameters<typeof captureBusy>[1]>;
type Target = NonNullable<ReturnType<ElementFor>>;
const culture = { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } };
const send = (busy: Send["busy"], contracts: Send["contracts"] = ["busy"]): Send => ({
  method: "GET", url: { parts: [{ kind: "text", text: "/Residents" }], query: "" },
  query: { fields: [], inputs: { container: { kind: "none" }, individual: [] } }, body: { kind: "none" }, headers: [],
  antiforgery: { kind: "none" }, retry: { kind: "none" }, busy, contracts, respondedJson: false,
});
const target = (id: string): Target & { readonly attributes: Map<string, string> } => {
  const attributes = new Map<string, string>();
  return { id, attributes, setAttribute: (name, value) => { attributes.set(name, value); }, removeAttribute: name => { attributes.delete(name); } };
};
const indicator = () => ({ show: vi.fn<(...targets: string[]) => void>(), hide: vi.fn<(...targets: string[]) => void>() });
const none = { kind: "none" } as const;
const routes = { success: { kind: "none", do: [] }, statuses: [], otherwise: { kind: "none", do: [] } } as const;
const prepareWith = (request: Send, capture: ReturnType<typeof captureBusy>, object: unknown) => prepare(request, { kind: "clock" }, culture, capture, () => object);

afterEach(() => { vi.unstubAllGlobals(); vi.restoreAllMocks(); });

describe("when Guard looks up the request's contracts, D10.21 uses the plugin boundary", () => {
  test.each([
    ["busy", "No script provides the plugin 'busy'."],
    ["notice", "No script provides the plugin 'notice'."],
  ] as const)("MissingContractStopsBeforeSend: no script provides %s", (contract, reason) => {
    const fetch = vi.fn<typeof globalThis.fetch>();
    vi.stubGlobal("fetch", fetch);
    expect(() => prepare(send(none, [contract]), { kind: "clock" }, culture)).toThrow(reason);
    expect(fetch.mock.calls).toStrictEqual([]);
  });

  test("NamedContractsAreLookedUpOnce: busy and notice are resolved without calling notice", () => {
    const busy = indicator();
    const notice = { show: vi.fn() };
    const find = vi.fn((name: string) => name === "busy" ? busy : notice);
    const request = prepare(send({ kind: "page" }, ["busy", "notice"]), { kind: "clock" }, culture, captureBusy({ kind: "page" }), find);
    expect(find.mock.calls).toStrictEqual([["busy", ""], ["notice", ""]]);
    expect(busy.show.mock.calls).toStrictEqual([[]]);
    request.releaseBusy();
    expect(busy.hide.mock.calls).toStrictEqual([[]]);
    expect(notice.show.mock.calls).toStrictEqual([]);
  });

  test("UnnamedContractsAreNotLookedUp: NoBusy holds nothing and sends normally", async () => {
    const find = vi.fn();
    const request = prepare(send(none, []), { kind: "clock" }, culture, captureBusy(none), find);
    vi.stubGlobal("fetch", vi.fn<typeof globalThis.fetch>().mockResolvedValue(new Response(null, { status: 204 })));
    const { reply } = await attempt(request, routes, new AbortController().signal, "https://care.example/", untraced);
    request.releaseBusy();
    expect([reply.status, reply.outcome]).toStrictEqual([204, "ok"]);
    expect(find.mock.calls).toStrictEqual([]);
  });

  test("AllContractsBeforeHolding: a missing notice leaves the busy indicator unheld", () => {
    const busy = indicator();
    const find = (name: string) => { if (name === "notice") throw new Error("No script provides the plugin 'notice'."); return busy; };
    expect(() => prepare(send({ kind: "page" }, ["busy", "notice"]), { kind: "clock" }, culture, captureBusy({ kind: "page" }), find)).toThrow("No script provides the plugin 'notice'.");
    expect([busy.show.mock.calls, busy.hide.mock.calls]).toStrictEqual([[], []]);
  });
});

describe("when a request holds busy targets, Composition.tla counts every pipeline of the page", () => {
  test.each([false, true])("IndicatorNeverHidesEarly: two requests on one target, reverse completion %s", reverse => {
    const panel = target("shared-panel");
    const busy = indicator();
    const request = send({ kind: "targets", values: [{ kind: "element", id: "shared-panel" }] });
    const first = prepareWith(request, captureBusy(request.busy, () => panel), busy);
    const second = prepareWith(request, captureBusy(request.busy, () => panel), busy);
    expect([busy.show.mock.calls, [...panel.attributes]]).toStrictEqual([[["shared-panel"]], [["aria-busy", "true"]]]);
    const sooner = reverse ? second : first;
    const later = reverse ? first : second;
    sooner.releaseBusy();
    expect([busy.hide.mock.calls, [...panel.attributes]]).toStrictEqual([[], [["aria-busy", "true"]]]);
    later.releaseBusy();
    expect([busy.hide.mock.calls, [...panel.attributes]]).toStrictEqual([[["shared-panel"]], []]);
  });

  test("EachTargetHasItsOwnCount: two requests on two targets release independently", () => {
    const one = target("one-panel");
    const two = target("two-panel");
    const busy = indicator();
    const firstSend = send({ kind: "targets", values: [{ kind: "element", id: "one-panel" }] });
    const secondSend = send({ kind: "targets", values: [{ kind: "element", id: "two-panel" }] });
    const first = prepareWith(firstSend, captureBusy(firstSend.busy, () => one), busy);
    const second = prepareWith(secondSend, captureBusy(secondSend.busy, () => two), busy);
    first.releaseBusy();
    expect([busy.hide.mock.calls, [...one.attributes], [...two.attributes]]).toStrictEqual([[["one-panel"]], [], [["aria-busy", "true"]]]);
    second.releaseBusy();
    expect([busy.hide.mock.calls, [...two.attributes]]).toStrictEqual([[["one-panel"], ["two-panel"]], []]);
  });

  test("HoldOncePerRequest: a request holds each distinct target once", () => {
    const one = target("many-one");
    const two = target("many-two");
    const busy = indicator();
    const request = send({ kind: "targets", values: [{ kind: "element", id: "many-one" }, { kind: "element", id: "many-two" }, { kind: "element", id: "many-one" }] });
    const prepared = prepareWith(request, captureBusy(request.busy, id => id === "many-one" ? one : two), busy);
    expect(busy.show.mock.calls).toStrictEqual([["many-one"], ["many-two"]]);
    prepared.releaseBusy();
    expect([busy.hide.mock.calls, [...one.attributes], [...two.attributes]]).toStrictEqual([[["many-one"], ["many-two"]], [], []]);
  });

  test("PageHasNoTargetArgument: the page count is separate from an element's count", () => {
    const panel = target("page");
    const busy = indicator();
    const pageSend = send({ kind: "page" });
    const panelSend = send({ kind: "targets", values: [{ kind: "element", id: "page" }] });
    const first = prepareWith(pageSend, captureBusy(pageSend.busy), busy);
    const second = prepareWith(pageSend, captureBusy(pageSend.busy), busy);
    const third = prepareWith(panelSend, captureBusy(panelSend.busy, () => panel), busy);
    first.releaseBusy();
    expect([busy.show.mock.calls, busy.hide.mock.calls]).toStrictEqual([[[], ["page"]], []]);
    second.releaseBusy();
    expect([busy.hide.mock.calls, [...panel.attributes]]).toStrictEqual([[[]], [["aria-busy", "true"]]]);
    third.releaseBusy();
    expect(busy.hide.mock.calls).toStrictEqual([[], ["page"]]);
  });

  test("FailedGuardHoldsNothing: one missing target prevents every hold", () => {
    const panel = target("present-panel");
    const busy = indicator();
    const request = send({ kind: "targets", values: [{ kind: "element", id: "present-panel" }, { kind: "missing", id: "lost-panel" }] });
    expect(() => prepareWith(request, captureBusy(request.busy, () => panel), busy)).toThrow("The request's busy target 'lost-panel' is missing.");
    expect([busy.show.mock.calls, [...panel.attributes]]).toStrictEqual([[], []]);
  });

  test("MissingAtCheckStaysMissing: a target added during loading does not repair the captured absence", () => {
    const busy = indicator();
    const request = send({ kind: "targets", values: [{ kind: "element", id: "late-panel" }] });
    const find = vi.fn<ElementFor>().mockReturnValueOnce(null).mockReturnValue(target("late-panel"));
    const captured = captureBusy(request.busy, find);
    expect(() => prepareWith(request, captured, busy)).toThrow("The request's busy target 'late-panel' is missing.");
    expect([find.mock.calls, busy.show.mock.calls]).toStrictEqual([[["late-panel"]], []]);
  });

  test("HeldElementLeavesWithItsSlot: release uses the captured element and still calls hide", () => {
    const panel = target("removed-panel");
    const busy = indicator();
    const request = send({ kind: "targets", values: [{ kind: "element", id: "removed-panel" }] });
    const find = vi.fn<ElementFor>().mockReturnValueOnce(panel).mockReturnValue(null);
    const captured = captureBusy(request.busy, find);
    const prepared = prepareWith(request, captured, busy);
    prepared.releaseBusy();
    expect([find.mock.calls, busy.hide.mock.calls, [...panel.attributes]]).toStrictEqual([[["removed-panel"]], [["removed-panel"]], []]);
  });

  test("ComponentReadOnce: the activated row's element is captured before loading", () => {
    const row = target("residents-0-0");
    const busy = indicator();
    const component = { control: row };
    const request = send({ kind: "targets", values: [{ kind: "component", value: { kind: "held", value: component } }] });
    const fromComponent = vi.fn((value: unknown) => value === component ? component.control : null);
    const captured = captureBusy(request.busy, () => null, fromComponent);
    component.control = target("residents-1-0");
    const prepared = prepareWith(request, captured, busy);
    expect([fromComponent.mock.calls.length, busy.show.mock.calls, [...row.attributes], [...component.control.attributes]]).toStrictEqual([1, [["residents-0-0"]], [["aria-busy", "true"]], []]);
    prepared.releaseBusy();
    expect(busy.hide.mock.calls).toStrictEqual([["residents-0-0"]]);
  });

  test.each<Extract<Send["busy"], { kind: "targets" }>["values"][number]>([
    { kind: "component", value: { kind: "nothing" } }, { kind: "component", value: { kind: "held", value: null } },
  ])("MissingComponentStops: a missing component element is refused at Guard", value => {
    const busy = indicator();
    const request = send({ kind: "targets", values: [value] });
    const captured = captureBusy(request.busy, () => null, () => null);
    expect(() => prepareWith(request, captured, busy)).toThrow("The request's busy component holds no element.");
    expect(busy.show.mock.calls).toStrictEqual([]);
  });

  test("WireGuardBeforeHold: an invalid path never shows busy", () => {
    const busy = indicator();
    const request: Send = { ...send({ kind: "page" }), url: { parts: [{ kind: "member", name: "Id", value: { kind: "nothing" }, form: { kind: "text" } }], query: "" } };
    expect(() => prepareWith(request, captureBusy(request.busy), busy)).toThrow("The request's path member 'Id' must hold nonempty text with no '/'.");
    expect(busy.show.mock.calls).toStrictEqual([]);
  });

  test("FailedShowRollsBackEveryHold: a plugin failure cannot strand a target's count", () => {
    const panel = target("failed-show");
    const busy = indicator();
    busy.show.mockImplementationOnce(() => { throw new Error("show failed"); });
    const request = send({ kind: "targets", values: [{ kind: "element", id: "failed-show" }] });
    expect(() => prepareWith(request, captureBusy(request.busy, () => panel), busy)).toThrow("show failed");
    expect([busy.hide.mock.calls, [...panel.attributes]]).toStrictEqual([[["failed-show"]], []]);
    const next = prepareWith(request, captureBusy(request.busy, () => panel), busy);
    next.releaseBusy();
    expect(busy.show.mock.calls).toStrictEqual([["failed-show"], ["failed-show"]]);
  });

  test("BusyContractKeepsItsReceiver: show and hide run on the provided object", () => {
    const busy = {
      calls: [] as string[],
      show() { this.calls.push("show"); },
      hide() { this.calls.push("hide"); },
    };
    const request = prepareWith(send({ kind: "page" }), captureBusy({ kind: "page" }), busy);
    request.releaseBusy();
    expect(busy.calls).toStrictEqual(["show", "hide"]);
  });

  test("ReleaseSurvivesPluginFailure: every target loses its count even if hide throws", () => {
    const one = target("throw-one");
    const two = target("throw-two");
    const busy = indicator();
    busy.hide.mockImplementationOnce(() => { throw new Error("hide failed"); });
    const request = send({ kind: "targets", values: [{ kind: "element", id: "throw-one" }, { kind: "element", id: "throw-two" }] });
    const prepared = prepareWith(request, captureBusy(request.busy, id => id === "throw-one" ? one : two), busy);
    expect(() => { prepared.releaseBusy(); }).toThrow("hide failed");
    expect([busy.hide.mock.calls, [...one.attributes], [...two.attributes]]).toStrictEqual([[["throw-one"], ["throw-two"]], [], []]);
    const next = prepareWith(request, captureBusy(request.busy, id => id === "throw-one" ? one : two), busy);
    next.releaseBusy();
    expect(busy.show.mock.calls).toStrictEqual([["throw-one"], ["throw-two"], ["throw-one"], ["throw-two"]]);
  });
});
