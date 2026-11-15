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
    start(planOf(handler("bell", "sounded", ["alarm"])), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined));

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
    start(planOf(handler("bell", "sounded", ["alarm"])), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined));

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
    start(planOf(handler("bell", "sounded", ["siren"]), handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: monitor }]])), deskOf(undefined));

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
    start(planOf(handler("bell", "rang"), handler("desk", "called")), ward, elementsOf(page), deskOf(desk));

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
    start(planOf(handler("desk", "called")), ward, elementsOf(new Map()), deskOf(desk));

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
    start(planOf(handler("desk", "called"), handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined));

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
    start(planOf(handler("bell", "rang", [], [2, "detail"])), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined));

    bell.raise("rang", { detail: { text: "the first argument" } }, note, { detail: { text: "Ada" } });

    expect(lines).toStrictEqual(["Ada"]);
  });

  test("a payload read that stops stops that raise's run alone, its reason in the console, and the next raise runs", () => {
    const ward = new Ward();
    const bell = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(planOf(handler("bell", "rang", [], [0, "detail"])), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined));

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
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", { current: bell }]])), deskOf(undefined));

    ward.release();
    bell.raise("rang", { text: "after" }, note);

    expect(bell.removed).toStrictEqual(["rang"]);
    expect(lines).toStrictEqual([]);
  });

  test("NoUnsubscribeOnDestroyed: a component its host destroyed is called nothing", () => {
    const ward = new Ward();
    const bell = new Bell();
    const element: Host = { current: bell };
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined));
    element.current = null;
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
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined));
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
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined));
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
    start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined));
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
  start(planOf(handler("bell", "rang")), ward, elementsOf(new Map([["bell-element", element]])), deskOf(undefined));
  return new WeakRef(element);
}

describe("when a plan contributes an input already held on the page", () => {
  test("R14.6 starts none of the conflicting plan's handlers and logs one complete error", () => {
    const first = new Ward();
    const second = new Ward();
    const desk = new Bell();
    const lines: unknown[] = [];
    const note = (line: unknown) => { lines.push(line); };
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    start(withInput("first", "first"), first, elementsOf(new Map()), deskOf(desk));
    start(withInput("second", "second"), second, elementsOf(new Map()), deskOf(desk));
    desk.raise("first", { text: "first heard" }, note);
    desk.raise("second", { text: "second heard" }, note);
    second.release();

    expect(lines).toStrictEqual(["first heard"]);
    expect(logged.mock.calls).toStrictEqual([["Input 'repeated' is already held by plan 'first' on the page; plan 'second' on the page did not start."]]);
    first.release();
    start(withInput("third", "third"), second, elementsOf(new Map()), deskOf(desk));
    desk.raise("third", { text: "after release" }, note);
    expect(lines).toStrictEqual(["first heard", "after release"]);
    second.release();
  });
});

function withInput(planId: string, event: string): Plan {
  const plan = planOf(handler("desk", event));
  return { ...plan, planId, objects: { ...plan.objects, $model: {
    kind: "plugin", name: "alis.model", key: "", listen: onAndOff, data: {
      kind: "model", containers: {}, inputs: {
        repeated: { name: "Nights", path: ["nights"], value: { kind: "lit", value: 5 }, committed: "change", invalidClass: "invalid", message: "" },
      },
    },
  } } };
}
