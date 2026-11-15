import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane } from "./index";

type Block = ConstructorParameters<typeof Lane>[1];
type Step = Block[number];
type Request = Extract<Step, { kind: "request" }>;
type Source = Extract<Step, { kind: "do" }>["expr"];
type Raised = Parameters<Lane["raise"]>[0];
type Held = Raised["scope"] extends ReadonlyMap<string, infer Read> ? Read : never;

const owner: ConstructorParameters<typeof Lane>[2] = {
  runs: () => undefined, mounts: () => undefined, listens: () => undefined, holds: () => undefined, registers: () => undefined,
};
const plan: Raised["plan"] = {
  version: 7, planId: "ward",
  culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
  timeZone: { kind: "clock" }, trace: { kind: "off" }, vendors: {}, objects: {}, handlers: [],
};
const send = {
  method: "GET",
  url: { parts: [{ kind: "text", text: "/Residents" }], query: "" },
  query: { fields: [], inputs: { container: { kind: "none" }, individual: [] } },
  body: { kind: "none" }, headers: [], antiforgery: { kind: "none" }, retry: { kind: "none" }, busy: { kind: "none" }, respondedJson: false, contracts: [],
} as const;
const ref = (name: string): Source => ({ kind: "ref", name });
const lit = (value: string): Source => ({ kind: "lit", value });
const get = (name: string, ...path: string[]): Source => ({ kind: "get", target: ref(name), path, members: "optional" });
const note = (value: Source): Step => ({ kind: "set", target: ref("chart"), path: ["line"], members: "required", content: false, value });
const log = (value: string): Step => note(lit(value));
const stop = (reason: string): Step => ({ kind: "stop", reason });
const wait = (name: string): Step => ({ kind: "do", await: true, expr: ref(name) });
const request = (changes: Partial<Request> = {}): Request => ({
  kind: "request", argument: "sent", reply: "reply", send: { kind: "lit", value: send }, check: [],
  layers: [{ whileLoading: [log("defaults loading")], finally: [log("defaults finally")] }, { whileLoading: [log("loading")], finally: [log("finally")] }],
  success: { kind: "json", do: [log("success"), note(get("reply", "body", "value", "name"))] },
  statuses: [{ status: 409, kind: "json", do: [log("conflict"), note(get("reply", "status"))] }],
  otherwise: { kind: "json", do: [log("otherwise"), note(get("reply", "outcome")), note(get("reply", "body", "value"))] },
  chained: [log("chained"), note(get("reply", "body", "value", "name"))],
  ...changes,
});

class Chart {
  readonly lines: unknown[] = [];
  set line(value: unknown) { this.lines.push(value); }
}

function raised(chart: Chart, bindings: Readonly<Record<string, unknown>> = {}): Raised {
  const scope = new Map<string, Held>();
  scope.set("chart", { kind: "value", value: chart });
  for (const [name, value] of Object.entries(bindings)) scope.set(name, { kind: "value", value });
  return { plan, scope, signal: new AbortController().signal };
}

function gate<T>() {
  let release: (value: T) => void = () => undefined;
  const held = new Promise<T>(resolve => { release = resolve; });
  return { held, release };
}

function settled(): Promise<void> {
  return new Promise(resolve => { setTimeout(resolve, 0); });
}

function network(answer: Promise<Response>) {
  vi.stubGlobal("location", { href: "https://care.example/Ward" });
  const fetch = vi.fn<typeof globalThis.fetch>().mockReturnValue(answer);
  vi.stubGlobal("fetch", fetch);
  return fetch;
}

function errors() {
  return vi.spyOn(console, "error").mockImplementation(() => undefined);
}

afterEach(() => {
  vi.unstubAllGlobals();
  vi.restoreAllMocks();
});

describe("when a request frame runs, it keeps Request.tla's order and its own reply scope", () => {
  test("OneRouteRuns, FinallyRunsWhenLoadingRan, EachLayerPairs, ChainedOnlyAfterSuccess: success completes its whole route then both layers then chained then after", async () => {
    const answer = gate<Response>();
    const routeTail = gate<unknown>();
    const fetch = network(answer.held);
    const chart = new Chart();
    const lane = new Lane("overlap", [
      request({ success: { kind: "json", do: [log("success"), note(get("reply", "body", "value", "name")), wait("tail"), log("route end")] } }),
      log("after"),
    ], owner);
    lane.raise(raised(chart, { tail: routeTail.held }));
    expect(chart.lines).toStrictEqual(["defaults loading", "loading"]);
    expect(fetch.mock.calls.length).toBe(1);
    answer.release(new Response('{"name":"Ada"}', { headers: { "Content-Type": "application/json" } }));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "success", "Ada"]);
    routeTail.release(null);
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "success", "Ada", "route end", "finally", "defaults finally", "chained", "Ada", "after"]);
  });

  test("ReadOnceBeforeCheck: binds the one send before check and loading", async () => {
    network(Promise.resolve(new Response(null, { status: 204 })));
    const chart = new Chart();
    const source = { send: vi.fn(() => { chart.line = "read"; return send; }) };
    const lane = new Lane("overlap", [request({
      send: { kind: "call", path: ["send"], members: "required", target: ref("source"), args: [] },
      check: [log("check"), note(get("sent", "method"))],
      success: { kind: "none", do: [log("success")] }, chained: [],
    }), log("after")], owner);
    lane.raise(raised(chart, { source }));
    await settled();
    expect(chart.lines).toStrictEqual(["read", "check", "GET", "defaults loading", "loading", "success", "finally", "defaults finally", "after"]);
    expect(source.send.mock.calls.length).toBe(1);
  });

  test("FailedGuardHoldsNothing, StepsAfterRunUnlessStopped: loading completes before a missing path stops and both finallies run without sending", () => {
    const fetch = network(gate<Response>().held);
    const logged = errors();
    const chart = new Chart();
    const noPath = { ...send, url: { parts: [{ kind: "text", text: "/Rooms/" }, { kind: "member", name: "Code", value: { kind: "nothing" }, form: { kind: "text" } }], query: "" } } as const;
    new Lane("overlap", [request({ send: { kind: "lit", value: noPath } }), log("after")], owner).raise(raised(chart));
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "finally", "defaults finally"]);
    expect(fetch.mock.calls.length).toBe(0);
    expect(logged.mock.calls).toStrictEqual([["The request's path member 'Code' must hold nonempty text with no '/'."]]);
  });

  test("FailedCheckSendsNothing: a stopped check runs no loading or finally and no later step", () => {
    const fetch = network(gate<Response>().held);
    const logged = errors();
    const chart = new Chart();
    new Lane("overlap", [request({ check: [log("checking"), stop("Name required")] }), log("after")], owner).raise(raised(chart));
    expect(chart.lines).toStrictEqual(["checking"]);
    expect(fetch.mock.calls.length).toBe(0);
    expect(logged.mock.calls).toStrictEqual([["Name required"]]);
  });

  test("EachLayerPairs: stopping the request's loading runs only the completed defaults' finally", () => {
    const fetch = network(gate<Response>().held);
    errors();
    const chart = new Chart();
    new Lane("overlap", [request({
      layers: [{ whileLoading: [log("defaults loading")], finally: [log("defaults finally")] }, { whileLoading: [log("loading"), stop("No room")], finally: [log("finally")] }],
    }), log("after")], owner).raise(raised(chart));
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "defaults finally"]);
    expect(fetch.mock.calls.length).toBe(0);
  });

  test("SuccessWithNoRouteRunsNone: a successful none route bypasses otherwise and still reaches chained", async () => {
    network(Promise.resolve(new Response(null, { status: 204 })));
    const chart = new Chart();
    new Lane("overlap", [request({ success: { kind: "none", do: [] }, chained: [log("chained"), note(get("reply", "status"))] }), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "finally", "defaults finally", "chained", 204, "after"]);
  });

  test("OneRouteRuns, ChainedOnlyAfterSuccess: status takes its own route and finally then after without chained", async () => {
    network(Promise.resolve(new Response('{"message":"Taken"}', { status: 409, headers: { "Content-Type": "application/json" } })));
    const chart = new Chart();
    new Lane("overlap", [request(), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "conflict", 409, "finally", "defaults finally", "after"]);
  });

  test("FailureRunsTheCatchAll: network failure logs once, runs otherwise and both layers, then after", async () => {
    const logged = errors();
    network(gate<Response>().held);
    vi.stubGlobal("fetch", vi.fn<typeof globalThis.fetch>().mockRejectedValue(new TypeError("offline")));
    const chart = new Chart();
    new Lane("overlap", [request(), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "otherwise", "network", null, "finally", "defaults finally", "after"]);
    expect(logged.mock.calls).toStrictEqual([["Request 'GET /Residents' ended with network."]]);
  });

  test("FailureRunsTheCatchAll: content failure runs no success or status route and logs once", async () => {
    const logged = errors();
    network(Promise.resolve(new Response("<p>Login</p>", { headers: { "Content-Type": "text/html" } })));
    const chart = new Chart();
    new Lane("overlap", [request(), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "otherwise", "content", null, "finally", "defaults finally", "after"]);
    expect(logged.mock.calls).toStrictEqual([["Request 'GET /Residents' ended with content."]]);
  });

  test("StepsAfterRunUnlessStopped: a stop in the selected route unwinds both layers and runs neither chained nor after", async () => {
    errors();
    network(Promise.resolve(new Response("{}", { headers: { "Content-Type": "application/json" } })));
    const chart = new Chart();
    new Lane("overlap", [request({ success: { kind: "json", do: [log("route"), stop("No bed"), log("tail")] } }), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "route", "finally", "defaults finally"]);
  });

  test("FinallyRunsOnce: a stop in finally runs the other layer once and skips chained and after", async () => {
    errors();
    network(Promise.resolve(new Response(null, { status: 204 })));
    const chart = new Chart();
    new Lane("overlap", [request({
      layers: [{ whileLoading: [], finally: [log("defaults finally")] }, { whileLoading: [], finally: [log("finally"), stop("Locked"), log("tail")] }],
      success: { kind: "none", do: [] },
    }), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["finally", "defaults finally"]);
  });

  test("InnerFinallyFirst: a stopped inner request's route unwinds inner layers before outer layers", async () => {
    errors();
    network(Promise.resolve(new Response(null, { status: 204 })));
    const chart = new Chart();
    const inner = request({
      layers: [{ whileLoading: [], finally: [log("inner defaults")] }, { whileLoading: [], finally: [log("inner finally")] }],
      success: { kind: "none", do: [log("inner route"), stop("Taken")] },
    });
    new Lane("overlap", [request({ success: { kind: "none", do: [log("outer route"), inner, log("outer tail")] } }), log("after")], owner).raise(raised(chart));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "outer route", "inner route", "inner finally", "inner defaults", "finally", "defaults finally"]);
  });

  test("ReplyIsLexical: route and chained read their reply while after reads only its enclosing binding", async () => {
    network(Promise.resolve(new Response('{"name":"Ada"}', { headers: { "Content-Type": "application/json" } })));
    const chart = new Chart();
    new Lane("overlap", [request(), note(ref("reply"))], owner).raise(raised(chart, { reply: "outside" }));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "success", "Ada", "finally", "defaults finally", "chained", "Ada", "outside"]);
  });

  test("NothingRunsAfterAnAbort: dispose aborts fetch, runs owed layers and ignores the late answer without an error", async () => {
    const answer = gate<Response>();
    const fetch = network(answer.held);
    const logged = errors();
    const chart = new Chart();
    const lane = new Lane("overlap", [request(), log("after")], owner);
    lane.raise(raised(chart));
    lane.dispose();
    expect(fetch.mock.calls[0]?.[1]?.signal?.aborted).toBe(true);
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "finally", "defaults finally"]);
    answer.release(new Response("<p>late</p>", { headers: { "Content-Type": "text/html" } }));
    await settled();
    expect(chart.lines).toStrictEqual(["defaults loading", "loading", "finally", "defaults finally"]);
    expect(logged.mock.calls).toStrictEqual([]);
  });
});
