import { afterEach, describe, expect, test, vi } from "vitest";
import { start } from "./index";

type Plan = Parameters<typeof start>[0];
type Handler = Plan["handlers"][number];
type Listener = (...args: unknown[]) => void;

class Ward {
  readonly endings: (() => void)[] = [];
  readonly removals: (() => void)[] = [];
  readonly drops: (() => void)[] = [];

  runs(end: () => void): void {
    this.endings.push(end);
  }

  listens(remove: () => void): void {
    this.removals.push(remove);
  }

  mounts(): void { return undefined; }
  holds(): void { return undefined; }
  registers(drop: () => void): void { this.drops.push(drop); }

  release(): void {
    for (const end of this.endings.splice(0)) end();
    for (const remove of this.removals.splice(0)) remove();
    for (const drop of this.drops.splice(0)) drop();
  }
}

class Bell {
  readonly listeners = new Map<string, Set<Listener>>();
  readonly removed: string[] = [];

  on(event: string, listener: Listener): void {
    let listeners = this.listeners.get(event);
    if (listeners === undefined) {
      listeners = new Set();
      this.listeners.set(event, listeners);
    }
    listeners.add(listener);
  }

  off(event: string, listener: Listener): void {
    this.removed.push(event);
    this.listeners.get(event)?.delete(listener);
  }

  raise(event: string, ...args: unknown[]): void {
    for (const listener of [...(this.listeners.get(event) ?? [])]) listener(...args);
  }
}

class Monitor extends Bell {
  readonly alarm = new Bell();
}

interface Host {
  current: object | null;
}

const onAndOff = { kind: "method", subscribe: "on", unsubscribe: "off" } as const;
const named = (name: string) => ({ kind: "ref", name }) as const;
const read = (name: string, path: readonly (string | number)[]) => ({ kind: "get", path, members: "required", target: named(name) }) as const;
const notes: Handler["do"] = [{ kind: "do", await: false, expr: { kind: "call", path: [1], members: "required", target: named("arguments"), args: [read("e", ["text"])] } }];

function handler(on: string, event: string, path: Handler["path"] = [], payload: Handler["payload"] = [0], body: Handler["do"] = notes): Handler {
  return { on, path, event, payload, members: "required", policy: "overlap", do: body };
}

function planOf(...handlers: Handler[]): Plan {
  return {
    version: 7, planId: "round",
    culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
    timeZone: { kind: "clock" }, trace: { kind: "off" },
    vendors: { bell: { locate: { kind: "fn", params: ["element"], body: read("element", ["current"]) }, listen: onAndOff } },
    objects: {
      bell: { kind: "element", id: "bell-element", vendor: "bell" },
      desk: { kind: "plugin", name: "ward.desk", key: "north", listen: onAndOff, data: { kind: "none" } },
    },
    handlers,
  };
}

function elementsOf(page: ReadonlyMap<string, object>) {
  return (id: string) => page.get(id) ?? null;
}

function deskOf(desk: unknown) {
  return (name: string, key: string) => name === "ward.desk" && key === "north" ? desk : undefined;
}

type TraceOf = Parameters<typeof start>[4];
type Traced = ReturnType<TraceOf>;

const quiet: Traced = {
  handler: () => quiet,
  heardOn: () => quiet,
  run: () => quiet,
  span: () => quiet,
  send: () => ({ attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }), end: () => undefined }),
  event: () => undefined,
  end: () => undefined,
};

const untraced: TraceOf = () => quiet;

class Spans {
  readonly recorded: string[] = [];

  readonly traceOf: TraceOf = () => this.#traced("plan");

  #traced(name: string): Traced {
    return {
      handler: (index, event) => {
        this.recorded.push("handler " + String(index) + " " + event);
        return quiet;
      },
      heardOn: () => quiet,
      run: () => quiet,
      span: (child, attributes) => {
        this.recorded.push(child + " starts " + JSON.stringify(attributes));
        return this.#traced(child);
      },
      send: () => quiet.send(),
      event: (event, attributes) => {
        this.recorded.push(name + " " + event + " " + JSON.stringify(attributes));
      },
      end: (outcome) => {
        this.recorded.push(name + " " + outcome);
      },
    };
  }
}

function nextTask(): Promise<void> {
  return new Promise((resolve) => { setTimeout(resolve, 0); });
}

function collector(): () => void {
  const host = Reflect.get(globalThis, "process") as {
    getBuiltinModule(name: "v8"): { setFlagsFromString(flags: string): void };
    getBuiltinModule(name: "vm"): { runInNewContext(source: string): unknown };
  };
  host.getBuiltinModule("v8").setFlagsFromString("--expose-gc");
  const collect = host.getBuiltinModule("vm").runInNewContext("gc");
  if (typeof collect !== "function") throw new Error("V8 did not expose collection.");
  return () => { Reflect.apply(collect, undefined, []); };
}

afterEach(() => {
  vi.restoreAllMocks();
});

describe("when a plan starts, a handler subscribes to the object its row names", () => {
  test("the object that raises is the located object read down the handler's path", () => {
    const ward = new Ward();
    const monitor = new Monitor();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "sounded", ["alarm"])), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined), untraced);

    monitor.raise("sounded", { text: "the monitor" }, note);
    monitor.alarm.raise("sounded", { text: "Ada" }, note);

    expect(lines).toStrictEqual(["Ada"]);
    expect(monitor.listeners.size).toBe(0);
  });

  test("a handler on a path is removed from the object below while its element holds the component", () => {
    const ward = new Ward();
    const monitor = new Monitor();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "sounded", ["alarm"])), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined), untraced);

    ward.release();
    monitor.alarm.raise("sounded", { text: "after" }, note);

    expect(monitor.alarm.removed).toStrictEqual(["sounded"]);
    expect(lines).toStrictEqual([]);
  });

  test("a member its path names that the object lacks starts no handler, the console naming its element and vendor, and the plan's other handlers subscribe", () => {
    const ward = new Ward();
    const monitor = new Monitor();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(planOf(handler("bell", "sounded", ["siren"]), handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined), untraced);

    monitor.raise("rang", { text: "Ada" }, note);

    expect(logged.mock.calls).toStrictEqual([["The handler of 'sounded' on the element 'bell-element' of the vendor 'bell' did not start: The member 'siren' is not on the object that should hold it."]]);
    expect(lines).toStrictEqual(["Ada"]);
  });

  test.each([
    { page: new Map<string, Host>(), reason: "No element has the id 'bell-element'." },
    { page: new Map<string, Host>([["bell-element", { current: null }]]), reason: "The element 'bell-element' holds no object of the vendor 'bell'." },
  ])("a handler whose object the page lacks never starts, the console naming its element and vendor, and the plan's other handlers subscribe: $reason", ({ page, reason }) => {
    const ward = new Ward();
    const bell = new Bell();
    const desk = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(planOf(handler("bell", "rang"), handler("desk", "called")), ward, elementsOf(page), deskOf(desk), untraced);

    page.set("bell-element", { current: bell });
    bell.raise("rang", { text: "the bell" }, note);
    desk.raise("called", { text: "the desk" }, note);

    expect(logged.mock.calls).toStrictEqual([["The handler of 'rang' on the element 'bell-element' of the vendor 'bell' did not start: " + reason]]);
    expect(lines).toStrictEqual(["the desk"]);
    expect(bell.listeners.size).toBe(0);
  });

  test("a plugin's handler subscribes to the object the plugin hands, and is removed while it lives", () => {
    const ward = new Ward();
    const desk = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("desk", "called")), ward, elementsOf(new Map()), deskOf(desk), untraced);

    desk.raise("called", { text: "before" }, note);
    ward.release();
    desk.raise("called", { text: "after" }, note);

    expect(lines).toStrictEqual(["before"]);
    expect(desk.removed).toStrictEqual(["called"]);
  });

  test("a plugin that hands no object starts no handler, the console naming the plugin, and the plan's other handlers subscribe", () => {
    const ward = new Ward();
    const bell = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(planOf(handler("desk", "called"), handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined), untraced);

    bell.raise("rang", { text: "Ada" }, note);

    expect(logged.mock.calls).toStrictEqual([["The handler of 'called' on the plugin 'ward.desk' did not start: The object that raises 'called' is not on the page."]]);
    expect(lines).toStrictEqual(["Ada"]);
  });
});

describe("when a handler's object raises, each raise reads its own payload", () => {
  test("the payload is read at its position in the listener's arguments and bound as e beside arguments", () => {
    const ward = new Ward();
    const bell = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "rang", [], [2, "detail"])), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined), untraced);

    bell.raise("rang", { detail: { text: "the first argument" } }, note, { detail: { text: "Ada" } });

    expect(lines).toStrictEqual(["Ada"]);
  });

  test("a payload read that stops stops that raise's run alone, its reason in the console, and the next raise runs", () => {
    const ward = new Ward();
    const bell = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(planOf(handler("bell", "rang", [], [0, "detail"])), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined), untraced);

    bell.raise("rang", {}, note);
    bell.raise("rang", { detail: { text: "Ben" } }, note);

    expect(logged.mock.calls).toStrictEqual([["The member 'detail' is not on the object that should hold it."]]);
    expect(lines).toStrictEqual(["Ben"]);
  });
});

describe("when a handler's owner releases it, Ownership.tla's release holds", () => {
  test("a handler of a component its element still holds is removed", () => {
    const ward = new Ward();
    const bell = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined), untraced);

    ward.release();
    bell.raise("rang", { text: "after" }, note);

    expect(bell.removed).toStrictEqual(["rang"]);
    expect(lines).toStrictEqual([]);
  });

  test("NoUnsubscribeOnDestroyed: a component its host destroyed is called nothing", () => {
    const ward = new Ward();
    const bell = new Bell();
    const element: Host = { current: bell };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
    element.current = null;
    Object.defineProperty(bell, "off", { value: () => { throw new Error("The destroyed bell was called."); } });

    ward.release();

    expect(bell.removed).toStrictEqual([]);
  });

  test("NoUnsubscribeOnDestroyed: a component its host took off its element, the member its vendor locates it at gone, is called nothing and its release throws nothing (D12.5, X12.7)", () => {
    const ward = new Ward();
    const bell = new Bell();
    const element: Host = { current: bell };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
    Reflect.deleteProperty(element, "current");
    Object.defineProperty(bell, "off", { value: () => { throw new Error("The destroyed bell was called."); } });

    ward.release();

    expect(bell.removed).toStrictEqual([]);
  });

  test("HostDestroys: after a restart, neither the old object nor the new is called", () => {
    const ward = new Ward();
    const bell = new Bell();
    const restarted = new Bell();
    const element: Host = { current: bell };
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
    element.current = restarted;
    restarted.on("rang", () => { lines.push("the restarted bell's own listener"); });

    restarted.raise("rang", { text: "the restarted bell" }, note);
    ward.release();
    restarted.raise("rang", { text: "after release" }, note);

    expect(lines).toStrictEqual(["the restarted bell's own listener", "the restarted bell's own listener"]);
    expect(bell.removed).toStrictEqual([]);
    expect(restarted.removed).toStrictEqual([]);
  });

  test("the listener is marked released before its element check runs", () => {
    const ward = new Ward();
    const bell = new Bell();
    const element: Host = { current: bell };
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
    const [release] = ward.removals;
    if (release === undefined) throw new Error("The handler has no release.");
    Object.defineProperty(element, "current", {
      get: () => {
        bell.raise("rang", { text: "during the check" }, note);
        return bell;
      },
    });

    release();

    expect(lines).toStrictEqual([]);
    expect(bell.removed).toStrictEqual(["rang"]);
  });

  test("ThrownListenerRunsNothing: a check that throws is that listener's error, its removal not called and its raises running nothing", () => {
    const ward = new Ward();
    const bell = new Bell();
    const element: Host = { current: bell };
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
    const [release] = ward.removals;
    if (release === undefined) throw new Error("The handler has no release.");
    Object.defineProperty(element, "current", { get: () => { throw new Error("The lookup failed."); } });

    expect(release).toThrow("The lookup failed.");
    bell.raise("rang", { text: "after" }, note);

    expect(lines).toStrictEqual([]);
    expect(bell.removed).toStrictEqual([]);
  });

  test("the handler holds its element weakly, and a release whose element was collected removes nothing", async () => {
    const collect = collector();
    const ward = new Ward();
    const bell = new Bell();
    const element = startedOn(ward, bell);

    await nextTask();
    collect();
    expect(element.deref()).toBeUndefined();
    ward.release();

    expect(bell.removed).toStrictEqual([]);
  });
});

function startedOn(ward: Ward, bell: Bell): WeakRef<Host> {
  const element: Host = { current: bell };
  start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined), untraced);
  return new WeakRef(element);
}

describe("when a plan binds an input", () => {
  test("the input's commit is heard through its own object until its plan's owner releases it", () => {
    const ward = new Ward();
    const picker = new Bell();
    start(withInput("first", "first"), ward, elementsOf(aPicker(picker)), deskOf(new Bell()), untraced);
    expect([...(picker.listeners.get("change") ?? [])]).toHaveLength(1);
    ward.release();
    expect([picker.removed, [...(picker.listeners.get("change") ?? [])]]).toStrictEqual([["change"], []]);
  });

  test("an input whose element the page lacks is not heard, and the console says why", () => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const ward = new Ward();
    start(withInput("first", "first"), ward, elementsOf(new Map()), deskOf(new Bell()), untraced);
    expect(logged.mock.calls).toStrictEqual([["The commit of input 'repeated' is not heard: No element has the id 'repeated'."]]);
    ward.release();
  });
});

describe("when a plan contributes an input already held on the page", () => {
  test("R14.6 starts none of the conflicting plan's handlers and logs one complete error", () => {
    const first = new Ward();
    const second = new Ward();
    const desk = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(withInput("first", "first"), first, elementsOf(aPicker()), deskOf(desk), untraced);
    start(withInput("second", "second"), second, elementsOf(aPicker()), deskOf(desk), untraced);
    desk.raise("first", { text: "first heard" }, note);
    desk.raise("second", { text: "second heard" }, note);
    second.release();

    expect(lines).toStrictEqual(["first heard"]);
    expect(logged.mock.calls).toStrictEqual([["Input 'repeated' is already held by plan 'first' on the page; plan 'second' on the page did not start."]]);
    first.release();
    start(withInput("third", "third"), second, elementsOf(aPicker()), deskOf(desk), untraced);
    desk.raise("third", { text: "after release" }, note);
    expect(lines).toStrictEqual(["first heard", "after release"]);
    second.release();
  });
});

function aPicker(picker = new Bell()): ReadonlyMap<string, object> {
  return new Map([["repeated", { current: picker }]]);
}

function withInput(planId: string, event: string): Plan {
  const plan = planOf(handler("desk", event));
  return { ...plan, planId, objects: { ...plan.objects, repeated: { kind: "element", id: "repeated", vendor: "bell" }, $model: {
    kind: "plugin", name: "alis.model", key: "", listen: onAndOff, data: {
      kind: "model", containers: {}, inputs: {
        repeated: { name: "Nights", path: ["nights"], converted: { kind: "as", expr: { kind: "lit", value: 5 }, shape: { kind: "nullable", inner: { kind: "any" } } }, blank: { kind: "lit", value: null }, sent: { kind: "lit", value: { kind: "field", form: { kind: "text" }, whenNothing: "empty", whenBlank: "send" } }, committed: "change", invalidClass: "invalid", message: "" },
      },
    },
  } } };
}

const anEventTarget = { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" } as const;
const lineOf = (name: string) => ({ kind: "set", path: ["line"], members: "required", content: false, target: read("e", ["chart"]), value: read("e", [name]) }) as const;
const aSave: Handler["do"] = [lineOf("starts"), { kind: "do", await: true, expr: { kind: "call", path: ["settle"], members: "required", target: named("e"), args: [] } }, lineOf("ends")];

class Chart {
  readonly lines: unknown[] = [];

  set line(written: unknown) {
    this.lines.push(written);
  }
}

function settled(): { readonly held: Promise<unknown>; readonly opens: () => void } {
  let opens = (): void => undefined;
  const held = new Promise<unknown>((resolve) => { opens = () => { resolve(null); }; });
  return { held, opens: () => { opens(); } };
}

function anAppEventsPlan(policy: Handler["policy"]): Plan {
  const plan = planOf({ on: "$document", path: [], event: "chartSaveRequested", payload: [0, "detail"], members: "required", policy, do: aSave });
  return { ...plan, objects: { $document: { kind: "plugin", name: "alis.document", key: "", listen: anEventTarget, data: { kind: "none" } } } };
}

function theDocument(events: EventTarget) {
  return (name: string) => name === "alis.document" ? events : undefined;
}

describe("when an app event's handler hears the event on the document, its policy keeps Lanes.tla's invariants", () => {
  test("SkippedTriggerStartsNothing: a raise of the event while the handler's run waits starts nothing, and one after it ended starts a run", async () => {
    const events = new EventTarget();
    const chart = new Chart();
    const first = settled();
    start(anAppEventsPlan("skip"), new Ward(), elementsOf(new Map()), theDocument(events), untraced);

    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => first.held, starts: "saving 1", ends: "saved 1" } }));
    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => settled().held, starts: "saving 2", ends: "saved 2" } }));
    first.opens();
    await nextTask();
    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => settled().held, starts: "saving 3", ends: "saved 3" } }));

    expect(chart.lines).toStrictEqual(["saving 1", "saved 1", "saving 3"]);
  });

  test("ReplacedRunRunsNothingAfter: a raise of the event while the handler's run waits ends that run, whose later step never runs", async () => {
    const events = new EventTarget();
    const chart = new Chart();
    const first = settled();
    const second = settled();
    start(anAppEventsPlan("replace"), new Ward(), elementsOf(new Map()), theDocument(events), untraced);

    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => first.held, starts: "saving 1", ends: "saved 1" } }));
    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => second.held, starts: "saving 2", ends: "saved 2" } }));
    second.opens();
    await nextTask();
    first.opens();
    await nextTask();

    expect(chart.lines).toStrictEqual(["saving 1", "saving 2", "saved 2"]);
  });

  test("under overlap a raise while the handler's run waits starts a second run beside it, each going on as its own promise resolves", async () => {
    const events = new EventTarget();
    const chart = new Chart();
    const first = settled();
    const second = settled();
    start(anAppEventsPlan("overlap"), new Ward(), elementsOf(new Map()), theDocument(events), untraced);

    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => first.held, starts: "saving 1", ends: "saved 1" } }));
    events.dispatchEvent(new CustomEvent("chartSaveRequested", { detail: { chart, settle: () => second.held, starts: "saving 2", ends: "saved 2" } }));
    second.opens();
    await nextTask();
    first.opens();
    await nextTask();

    expect(chart.lines).toStrictEqual(["saving 1", "saving 2", "saved 2", "saved 1"]);
  });
});

describe("when a traced plan boots, its boot is a span that records no value (D20.4, D20.5)", () => {
  test("a boot that starts every handler ends completed, each handler traced by its position and its event", () => {
    const spans = new Spans();
    const bell = new Bell();

    start(planOf(handler("bell", "rang"), handler("bell", "sounded")), new Ward(), elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined), spans.traceOf);

    expect(spans.recorded).toStrictEqual(["alis.boot starts {\"alis.plan_id\":\"round\"}", "handler 0 rang", "handler 1 sounded", "alis.boot completed"]);
  });

  test("a handler that does not start records an error with no reason on the boot, which goes on, and the console keeps the reason", () => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const spans = new Spans();

    start(planOf(handler("desk", "called"), handler("bell", "rang")), new Ward(), elementsOf(new Map([["bell-element", { current: new Bell() }]])), deskOf(undefined), spans.traceOf);

    expect([spans.recorded, logged.mock.calls.length]).toStrictEqual([
      ["alis.boot starts {\"alis.plan_id\":\"round\"}", "handler 0 called", "alis.boot alis.error {}", "handler 1 rang", "alis.boot completed"],
      1,
    ]);
  });

  test("a plan whose inputs cannot join the page ends its boot stopped with no reason, and the console keeps the reason", () => {
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    const spans = new Spans();
    const first = new Ward();
    start(withInput("first", "first"), first, elementsOf(aPicker()), deskOf(new Bell()), untraced);

    start(withInput("second", "second"), new Ward(), elementsOf(aPicker()), deskOf(new Bell()), spans.traceOf);

    expect([spans.recorded, logged.mock.calls]).toStrictEqual([
      ["alis.boot starts {\"alis.plan_id\":\"second\"}", "alis.boot alis.stop {}", "alis.boot stopped"],
      [["Input 'repeated' is already held by plan 'first' on the page; plan 'second' on the page did not start."]],
    ]);
    first.release();
  });
});
