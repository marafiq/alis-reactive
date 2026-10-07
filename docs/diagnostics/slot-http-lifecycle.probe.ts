// Opt-in characterization of an unresolved defect, excluded from the regression gate.
// These assertions describe the observed current behavior, not the desired cancellation contract.
import { afterEach, expect, it, vi } from "vitest";
import { boot, loadPartialSlot, resetBootStateForTests, unloadPartialSlot } from "../../Alis.Reactive.Assets/runtime/lifecycle/boot";
import type { PlanDocument, ReactionGraph, RequestPlan } from "../../Alis.Reactive.Assets/runtime/types/index";

afterEach(() => {
  resetBootStateForTests();
  vi.unstubAllGlobals();
  document.body.innerHTML = "";
});

function deferred<T>() {
  let resolve!: (value: T) => void;
  let reject!: (error: Error) => void;
  const promise = new Promise<T>((yes, no) => { resolve = yes; reject = no; });
  return { promise, resolve, reject };
}

function setText(component: string, value: string): ReactionGraph {
  return {
    kind: "set", on: { kind: "component", component }, property: "textContent",
    value: { kind: "literal", value, shape: { kind: "string" } },
  };
}

function request(url: string, routeTarget: string, completionTarget: string): RequestPlan {
  return {
    method: "GET", url, input: { kind: "none" }, validation: { kind: "none" },
    whileLoading: [],
    success: [{ match: { kind: "any" }, reaction: setText(routeTarget, "late success") }],
    error: [{ match: { kind: "any" }, reaction: setText(routeTarget, "late error") }],
    finally: [setText(completionTarget, "Finally ran")],
    chain: { kind: "terminal" },
  };
}

function plan(scope: "root" | "partial"): PlanDocument {
  return {
    version: 3, planId: "SlotHttpProbe", scope: { kind: scope },
    types: {}, components: {}, behaviors: [],
  };
}

function partial(event: string, reaction: ReactionGraph): PlanDocument {
  const slotPlan = plan("partial");
  slotPlan.behaviors.push({ startsWhen: { kind: "document-event", event }, reaction });
  return slotPlan;
}

function dispatch(event: string): void {
  document.dispatchEvent(new CustomEvent(event));
}

function text(id: string): string { return document.getElementById(id)!.textContent!; }

const scenarios = [
  { outcome: "success", status: 200 },
  { outcome: "error", status: 500 },
  { outcome: "network failure", status: 0 },
  { outcome: "deferred body", status: 200 },
] as const;

for (const retire of ["unload", "replace"] as const) {
  for (const scenario of scenarios) {
    it(`records effects of ${scenario.outcome} after slot ${retire}`, async () => {
      const ids = ["retired-route", "retired-finally", "chain-route", "chain-finally", "sibling-route", "sibling-finally"];
      document.body.innerHTML = ids.map(id => `<p id="${id}"></p>`).join("");
      const root = plan("root");
      root.types["native.text"] = {
        properties: { textContent: { path: [{ kind: "property", name: "textContent" }], shape: { kind: "string" }, access: "readwrite" } },
        methods: {}, events: {},
      };
      for (const id of ids) {
        root.components[id] = {
          id, type: "native.text", vendor: "native", role: { kind: "object-target" },
          binding: { kind: "none" }, container: { kind: "none" },
        };
      }
      boot(root);

      const retiredResponse = deferred<Response>();
      const retiredBody = deferred<string>();
      const siblingResponse = deferred<Response>();
      const readBody = vi.fn(() => retiredBody.promise);
      const fetchMock = vi.fn((url: string, _init: RequestInit) => {
        if (url === "/retired") return retiredResponse.promise;
        if (url === "/sibling") return siblingResponse.promise;
        return Promise.resolve(new Response("{}", { headers: { "Content-Type": "application/json" } }));
      });
      vi.stubGlobal("fetch", fetchMock);

      const retiredRequest = request("/retired", "retired-route", "retired-finally");
      retiredRequest.chain = { kind: "follow-up", next: request("/chain", "chain-route", "chain-finally") };
      loadPartialSlot("retired-slot", [partial("start-retired", { kind: "request", request: retiredRequest })]);
      loadPartialSlot("sibling-slot", [partial("start-sibling", { kind: "request", request: request("/sibling", "sibling-route", "sibling-finally") })]);
      dispatch("start-retired");
      dispatch("start-sibling");
      await vi.waitFor(() => expect(fetchMock).toHaveBeenCalledTimes(2));

      if (scenario.outcome === "deferred body") {
        retiredResponse.resolve({
          ok: true, status: 200, headers: new Headers({ "Content-Type": "application/json" }), text: readBody,
        } as unknown as Response);
        await vi.waitFor(() => expect(readBody).toHaveBeenCalledOnce());
      }

      if (retire === "unload") unloadPartialSlot("retired-slot");
      else {
        loadPartialSlot("retired-slot", [partial("replacement-ready", setText("retired-route", "replacement is current"))]);
        dispatch("replacement-ready");
        expect(text("retired-route")).toBe("replacement is current");
      }

      // The old listener is removed. It is the request already in flight that survives.
      dispatch("start-retired");
      expect(fetchMock).toHaveBeenCalledTimes(2);
      expect(text("retired-finally")).toBe("");

      siblingResponse.resolve(new Response("{}", { headers: { "Content-Type": "application/json" } }));
      await vi.waitFor(() => expect(text("sibling-finally")).toBe("Finally ran"));

      if (scenario.outcome === "network failure") retiredResponse.reject(new TypeError("probe network failure"));
      else if (scenario.outcome === "deferred body") retiredBody.resolve("{}");
      else retiredResponse.resolve(new Response("{}", { status: scenario.status, headers: { "Content-Type": "application/json" } }));
      await vi.waitFor(() => expect(text("retired-finally")).toBe("Finally ran"));
      const successful = scenario.status === 200;
      if (successful) await vi.waitFor(() => expect(text("chain-finally")).toBe("Finally ran"));

      const observation = {
        retire, outcome: scenario.outcome,
        requestCarriesAbortSignal: fetchMock.mock.calls[0]![1].signal !== undefined,
        retiredRequests: fetchMock.mock.calls.filter(([url]) => url === "/retired").length,
        retiredRoute: text("retired-route"), retiredFinally: text("retired-finally"),
        chainedRequests: fetchMock.mock.calls.filter(([url]) => url === "/chain").length,
        siblingRoute: text("sibling-route"), siblingFinally: text("sibling-finally"),
      };
      console.info("[slot-http-probe] " + JSON.stringify(observation));
      expect(observation).toEqual({
        retire, outcome: scenario.outcome, requestCarriesAbortSignal: false,
        retiredRequests: 1,
        retiredRoute: successful ? "late success" : "late error", retiredFinally: "Finally ran",
        chainedRequests: successful ? 1 : 0, siblingRoute: "late success", siblingFinally: "Finally ran",
      });
    });
  }
}
