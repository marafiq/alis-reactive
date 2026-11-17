import { afterEach, describe, expect, test, vi } from "vitest";
import { Lane, listenTo } from "./index";

type Listen = Parameters<typeof listenTo>[0];
type Context = Parameters<typeof listenTo>[1];
type Statement = ConstructorParameters<typeof Lane>[1][number];
type Listener = (...args: unknown[]) => void;

const untraced: Context["trace"] = {
  handler: () => untraced,
  heardOn: () => untraced,
  run: () => untraced,
  span: () => untraced,
  send: () => ({ attempt: () => ({ headers: {}, answered: () => undefined, end: () => undefined }), end: () => undefined }),
  event: () => undefined,
  end: () => undefined,
};

class Card {
  readonly endings: (() => void)[] = [];
  readonly removals: (() => void)[] = [];

  runs(end: () => void): void {
    this.endings.push(end);
  }

  listens(remove: () => void): void {
    this.removals.push(remove);
  }

  mounts(): void { return undefined; }
  holds(): void { return undefined; }
  registers(): void { return undefined; }

  release(): void {
    for (const end of this.endings.splice(0)) end();
    for (const remove of this.removals.splice(0)) remove();
  }
}

class Feed {
  readonly listeners = new Map<string, Set<Listener>>();
  readonly removed: string[] = [];
  readonly host: { current: Feed | null };

  constructor() {
    this.host = { current: this };
  }

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

  watch(event: string, listener: Listener): () => void {
    this.on(event, listener);
    return () => {
      this.off(event, listener);
    };
  }

  raise(event: string, ...args: unknown[]): void {
    for (const listener of this.listeners.get(event) ?? []) listener(...args);
  }
}

class Chart {
  readonly lines: unknown[] = [];

  set line(value: unknown) {
    this.lines.push(value);
  }
}

const named = (name: string) => ({ kind: "ref", name }) as const;
const text = (value: string) => ({ kind: "lit", value }) as const;
const read = (name: string, path: readonly (string | number)[]) => ({ kind: "get", path, members: "required", target: named(name) }) as const;
const charted = (value: Extract<Statement, { kind: "set" }>["value"]): Statement => ({
  kind: "set", path: ["line"], members: "required", content: false, target: named("chart"), value,
});
const listened = (release: Listen["release"] = { kind: "living" }, body: Listen["do"] = [charted(read("heard", ["text"]))]): Listen => ({
  kind: "listen", object: named("feed"), event: "posted",
  listen: { kind: "method", subscribe: "on", unsubscribe: "off" }, release,
  payload: [0], members: "required", policy: "overlap", payloadName: "heard", argumentsName: "heardArguments", do: body,
});

function context(owner: Card, bindings: Readonly<Record<string, unknown>>): Context {
  return {
    owner,
    plan: {
      version: 7, planId: "card",
      culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
      timeZone: { kind: "clock" }, trace: { kind: "off" },
      vendors: {
        feed: { locate: { kind: "fn", params: ["element"], body: read("element", ["current"]) }, listen: { kind: "method", subscribe: "on", unsubscribe: "off" } },
      },
      objects: { pump: { kind: "element", id: "pump-element", vendor: "feed" } }, handlers: [],
    },
    scope: new Map(Object.entries(bindings).map(([name, value]) => [name, { kind: "value", value }])),
    signal: new AbortController().signal,
    trace: untraced,
  };
}

function noElement(): never {
  throw new Error("A living or held listener reads no element id.");
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

describe("when a pipeline listens, Ownership.tla's listening invariants hold", () => {
  test("ListenOncePerObject: a repeated step listens once, another object and another statement each listen", () => {
    const owner = new Card();
    const feed = new Feed();
    const other = new Feed();
    const chart = new Chart();
    const step = listened();
    listenTo(step, context(owner, { feed, chart }), noElement);
    listenTo(step, context(owner, { feed, chart }), noElement);
    listenTo(step, context(owner, { feed: other, chart }), noElement);
    listenTo(listened(), context(owner, { feed, chart }), noElement);

    feed.raise("posted", { text: "first" });
    other.raise("posted", { text: "second" });

    expect(chart.lines).toStrictEqual(["first", "first", "second"]);
    expect(feed.listeners.get("posted")?.size).toBe(2);
    expect(other.listeners.get("posted")?.size).toBe(1);
  });

  test.each([
    { release: { kind: "living" }, found: [] },
    { release: { kind: "held", vendor: "feed", element: ["host"] }, found: [] },
    { release: { kind: "located", id: "pump" }, found: ["pump-element", "pump-element"] },
  ] as const)("ListenLeavesWithItsOwner: $release.kind removes its own listener and keeps another owner's", ({ release, found: expected }) => {
    const card = new Card();
    const page = new Card();
    const feed = new Feed();
    const chart = new Chart();
    const step = listened(release);
    const found: string[] = [];
    const elementFor = (id: string) => {
      found.push(id);
      return feed.host;
    };
    listenTo(step, context(card, { feed, chart }), elementFor);
    listenTo(step, context(page, { feed, chart }), elementFor);

    feed.raise("posted", { text: "before" });
    card.release();
    feed.raise("posted", { text: "after" });

    expect(chart.lines).toStrictEqual(["before", "before", "after"]);
    expect(feed.removed).toStrictEqual(["posted"]);
    expect(feed.listeners.get("posted")?.size).toBe(1);
    expect(found).toStrictEqual(expected);
  });

  test.each([
    { kind: "held", vendor: "feed", element: ["host"] },
    { kind: "located", id: "pump" },
  ] as const)("NoRemoveOnDestroyedListened: $kind reads the element captured before the host clears it", (release) => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    const element = feed.host;
    listenTo(listened(release), context(owner, { feed, chart }), () => element);
    feed.raise("posted", { text: "before" });
    element.current = null;
    Object.defineProperty(feed, "host", { get: () => { throw new Error("The destroyed object was read."); } });

    owner.release();

    expect(chart.lines).toStrictEqual(["before"]);
    expect(feed.removed).toStrictEqual([]);
  });

  test.each([
    { kind: "held", vendor: "feed", element: ["host"] },
    { kind: "located", id: "pump" },
  ] as const)("NoRemoveOnDestroyedListened: $kind leaves a restarted component's new object untouched", (release) => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    const element = feed.host;
    const step = listened(release);
    listenTo(step, context(owner, { feed, chart }), () => element);
    feed.raise("posted", { text: "old" });
    const replacement = new Feed();
    element.current = replacement;
    replacement.raise("posted", { text: "before its listen" });
    listenTo(step, context(owner, { feed: replacement, chart }), () => replacement.host);
    replacement.raise("posted", { text: "new" });

    owner.release();

    expect(chart.lines).toStrictEqual(["old", "new"]);
    expect(feed.removed).toStrictEqual([]);
    expect(replacement.removed).toStrictEqual(["posted"]);
  });

  test.each([
    { kind: "held", vendor: "feed", element: ["host"] },
    { kind: "located", id: "pump" },
  ] as const)("NoRemoveOnDestroyedListened: $kind never removes from a replacement it did not listen to", (release) => {
    const owner = new Card();
    const feed = new Feed();
    const replacement = new Feed();
    const chart = new Chart();
    const element = feed.host;
    listenTo(listened(release), context(owner, { feed, chart }), () => element);
    replacement.on("posted", () => { chart.line = "the replacement's own listener"; });
    element.current = replacement;

    owner.release();
    replacement.raise("posted", { text: "after release" });

    expect(chart.lines).toStrictEqual(["the replacement's own listener"]);
    expect(feed.removed).toStrictEqual([]);
    expect(replacement.removed).toStrictEqual([]);
  });

  test.each([
    { kind: "living" },
    { kind: "held", vendor: "feed", element: ["host"] },
    { kind: "located", id: "pump" },
  ] as const)("NoRemoveOnCollected: $kind keeps neither the object, its listener nor its element", async (release) => {
    const collect = collector();
    const owner = new Card();
    const removed: string[] = [];
    const weak = subscribeCollectible(owner, removed, { kind: "method", subscribe: "on", unsubscribe: "off" }, release);

    await nextTask();
    collect();
    expect(weak.object.deref()).toBeUndefined();
    expect(weak.listener.deref()).toBeUndefined();
    expect(weak.element.deref()).toBeUndefined();
    owner.release();

    expect(removed).toStrictEqual([]);
  });

  test("a returned unsubscriber stays held until release", async () => {
    const collect = collector();
    const owner = new Card();
    const removed: string[] = [];
    const weak = subscribeCollectible(owner, removed, { kind: "returned", subscribe: "watch" });

    await nextTask();
    collect();
    expect(weak.object.deref()).toBeDefined();
    owner.release();

    expect(removed).toStrictEqual(["posted"]);
  });

  test("a listener is marked released before its element check can raise and reenter removal", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    listenTo(listened({ kind: "held", vendor: "feed", element: ["host"] }), context(owner, { feed, chart }), noElement);
    feed.raise("posted", { text: "before" });
    const [release] = owner.removals;
    if (release === undefined) throw new Error("The listener has no release.");
    let checking = false;
    Object.defineProperty(feed.host, "current", {
      get: () => {
        feed.raise("posted", { text: "during removal" });
        if (!checking) {
          checking = true;
          release();
        }
        return feed;
      },
    });

    release();
    feed.raise("posted", { text: "after" });

    expect(chart.lines).toStrictEqual(["before"]);
    expect(feed.removed).toStrictEqual(["posted"]);
  });

  test("a throwing element check leaves its listener released and calls no removal", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    listenTo(listened({ kind: "held", vendor: "feed", element: ["host"] }), context(owner, { feed, chart }), noElement);
    const [release] = owner.removals;
    if (release === undefined) throw new Error("The listener has no release.");
    Object.defineProperty(feed.host, "current", { get: () => { throw new Error("The lookup failed."); } });

    expect(release).toThrow("The lookup failed.");
    feed.raise("posted", { text: "after" });
    release();

    expect(chart.lines).toStrictEqual([]);
    expect(feed.removed).toStrictEqual([]);
  });
});

function subscribeCollectible(owner: Card, removed: string[], listening: Listen["listen"], release: Listen["release"] = { kind: "living" }) {
  const feed = new Feed();
  Object.defineProperty(feed, "off", {
    value(event: string, listener: Listener): void {
      removed.push(event);
      feed.listeners.get(event)?.delete(listener);
    },
  });
  listenTo({ ...listened(release), listen: listening }, context(owner, { feed, chart: new Chart() }), () => feed.host);
  const listener = feed.listeners.get("posted")?.values().next().value;
  if (listener === undefined) throw new Error("The feed has no listener.");
  return { object: new WeakRef(feed), listener: new WeakRef(listener), element: new WeakRef(feed.host) };
}

describe("when a listened event runs its own pipeline", () => {
  test("the step keeps the enclosing scope and each nested payload and arguments binding", () => {
    const owner = new Card();
    const feed = new Feed();
    const inner = new Feed();
    const chart = new Chart();
    const nested = {
      ...listened({ kind: "living" }, [charted(read("outer", ["text"])), charted(read("heard", ["text"])), charted(read("last", ["text"])), charted(read("heardArguments", [1])), charted(read("lastArguments", [1]))]),
      object: named("inner"), payloadName: "last", argumentsName: "lastArguments",
    };
    const statement = listened({ kind: "living" }, [nested]);
    const lane = new Lane("overlap", [statement, charted(text("listened"))], owner);

    lane.raise(context(owner, { feed, inner, chart, outer: { text: "enclosing" } }));
    feed.raise("posted", { text: "first" }, "first argument");
    inner.raise("posted", { text: "second" }, "second argument");
    owner.release();
    feed.raise("posted", { text: "released outer" });
    inner.raise("posted", { text: "released inner" });

    expect(chart.lines).toStrictEqual(["listened", "enclosing", "first", "second", "first argument", "second argument"]);
  });

  test("its own payload can be written in the listened object's turn", () => {
    const owner = new Card();
    const feed = new Feed();
    const payload = { text: "before" };
    const statement = listened({ kind: "living" }, [{ kind: "set", path: ["text"], members: "required", content: false, target: named("heard"), value: text("cancelled") }]);
    listenTo(statement, context(owner, { feed }), noElement);

    feed.raise("posted", payload);

    expect(payload).toStrictEqual({ text: "cancelled" });
  });

  test("an empty payload path runs on a raise with no arguments", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    listenTo({ ...listened({ kind: "living" }, [charted(text("quiet"))]), payload: [] }, context(owner, { feed, chart }), noElement);

    feed.raise("posted");

    expect(chart.lines).toStrictEqual(["quiet"]);
  });

  test.each(["overlap", "skip", "replace"] as const)("the listened event uses its own %s lane", async (policy) => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    let open: () => void = () => undefined;
    const settled = new Promise<void>((resolve) => { open = resolve; });
    const statement = { ...listened({ kind: "living" }, [charted(read("heard", ["text"])), { kind: "do", await: true, expr: named("settled") }, charted(read("heard", ["end"]))]), policy };
    listenTo(statement, context(owner, { feed, chart, settled }), noElement);

    feed.raise("posted", { text: "first", end: "first end" });
    feed.raise("posted", { text: "second", end: "second end" });
    open();
    await nextTask();

    const expected = {
      overlap: ["first", "second", "first end", "second end"],
      skip: ["first", "first end"],
      replace: ["first", "second", "second end"],
    };
    expect(chart.lines).toStrictEqual(expected[policy]);
  });

  test("owner release ends a listened run that is waiting", async () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    let open: () => void = () => undefined;
    const settled = new Promise<void>((resolve) => { open = resolve; });
    listenTo(listened({ kind: "living" }, [charted(text("before")), { kind: "do", await: true, expr: named("settled") }, charted(text("after"))]), context(owner, { feed, chart, settled }), noElement);
    feed.raise("posted", {});

    owner.release();
    open();
    await nextTask();

    expect(chart.lines).toStrictEqual(["before"]);
  });

  test("a missing unsubscribe stops the step's pipeline before its next statement", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    Object.defineProperty(feed, "off", { value: undefined });
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);

    new Lane("overlap", [listened(), charted(text("after"))], owner).raise(context(owner, { feed, chart }));
    feed.raise("posted", { text: "heard" });

    expect(logged.mock.calls).toStrictEqual([["The method 'off' is not on the object that should hold it."]]);
    expect(chart.lines).toStrictEqual([]);
    expect(feed.listeners.size).toBe(0);
  });
});

describe("when the listening API differs, the step follows the declaration", () => {
  test("callback listening releases the assigned function and restores the vendor's value", () => {
    const owner = new Card();
    const chart = new Chart();
    const feed: { posted: Listener | null } = { posted: null };
    listenTo({ ...listened(), listen: { kind: "assignment" } }, context(owner, { feed, chart }), noElement);

    feed.posted?.({ text: "before" });
    owner.release();
    feed.posted?.({ text: "after" });

    expect(chart.lines).toStrictEqual(["before"]);
    expect(feed.posted).toBeNull();
  });

  test("a missing subscribe stops the enclosing pipeline", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    Object.defineProperty(feed, "on", { value: undefined });
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);

    new Lane("overlap", [listened(), charted(text("after"))], owner).raise(context(owner, { feed, chart }));

    expect(logged.mock.calls).toStrictEqual([["The method 'on' is not on the object that should hold it."]]);
    expect(chart.lines).toStrictEqual([]);
  });

  test("a returned subscribe without a remover stops the pipeline and its added listener runs nothing", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    Object.defineProperty(feed, "watch", { value: (event: string, listener: Listener) => { feed.on(event, listener); } });
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);

    new Lane("overlap", [{ ...listened(), listen: { kind: "returned", subscribe: "watch" } }, charted(text("after"))], owner).raise(context(owner, { feed, chart }));
    feed.raise("posted", { text: "later" });

    expect(logged.mock.calls).toStrictEqual([["The subscribe method 'watch' returned no unsubscriber."]]);
    expect(chart.lines).toStrictEqual([]);
  });

  test("an absent callback property stops the enclosing pipeline", () => {
    const owner = new Card();
    const chart = new Chart();
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);

    new Lane("overlap", [{ ...listened(), listen: { kind: "assignment" } }, charted(text("after"))], owner).raise(context(owner, { feed: {}, chart }));

    expect(logged.mock.calls).toStrictEqual([["The callback property 'posted' is not on the object that should hold it."]]);
    expect(chart.lines).toStrictEqual([]);
  });

  test("a required payload missing from a raise stops that listener's run", () => {
    const owner = new Card();
    const feed = new Feed();
    const chart = new Chart();
    const logged = vi.spyOn(console, "error").mockImplementation(() => undefined);
    listenTo(listened(), context(owner, { feed, chart }), noElement);

    feed.raise("posted");
    feed.raise("posted", { text: "received" });

    expect(logged.mock.calls).toStrictEqual([["The member '0' is not on the object that should hold it."]]);
    expect(chart.lines).toStrictEqual(["received"]);
  });
});

describe("when a listen reaches its object by the element's id, its stop names that element", () => {
  const located = { ...listened({ kind: "located", id: "pump" }), listen: { kind: "assignment" } } as const;

  test("a refused listen names the element, its vendor and the event, and adds no release", () => {
    const owner = new Card();
    const feed = Object.freeze({ posted: null });
    const element = { current: feed };

    expect(() => { listenTo(located, context(owner, { feed, chart: new Chart() }), () => element); })
      .toThrow(expect.objectContaining({ name: "Stop", message: "The listen to 'posted' on the element 'pump-element' of the vendor 'feed' did not start: The object refused the callback property 'posted'." }));
    expect(owner.removals).toStrictEqual([]);
  });

  test("a refused removal names the element, its vendor and the event", () => {
    const owner = new Card();
    const feed: { posted: Listener | null } = { posted: null };
    const element = { current: feed };
    listenTo(located, context(owner, { feed, chart: new Chart() }), () => element);
    Object.freeze(feed);

    expect(() => { owner.release(); })
      .toThrow(expect.objectContaining({ name: "Stop", message: "The listen to 'posted' on the element 'pump-element' of the vendor 'feed' was not removed: The object refused the callback property 'posted'." }));
  });

  test("a listen on an object a payload holds keeps the vendor's sentence", () => {
    const owner = new Card();
    const element: { current: object | null } = { current: null };
    const feed = Object.freeze({ posted: null, host: element });
    element.current = feed;

    expect(() => { listenTo({ ...located, release: { kind: "held", vendor: "feed", element: ["host"] } }, context(owner, { feed, chart: new Chart() }), noElement); })
      .toThrow(expect.objectContaining({ name: "Stop", message: "The object refused the callback property 'posted'." }));
  });
});

function vendorRemover(removed: string[]): () => void {
  return () => { removed.push("the vendor's remover"); };
}

function aReturnThatKeepsNoObject(owner: Card, removed: string[]) {
  const feed = new Feed();
  const remove = vendorRemover(removed);
  Object.defineProperty(feed, "watch", {
    value(event: string, listener: Listener): () => void {
      feed.on(event, listener);
      return remove;
    },
  });
  listenTo({ ...listened(), listen: { kind: "returned", subscribe: "watch" } }, context(owner, { feed, chart: new Chart() }), noElement);
  const listener = feed.listeners.get("posted")?.values().next().value;
  if (listener === undefined) throw new Error("The feed has no listener.");
  return { object: new WeakRef(feed), listener: new WeakRef(listener) };
}

test("NoRemoveOnCollected: a returned remover retains only what the vendor's function holds", async () => {
  const collect = collector();
  const owner = new Card();
  const removed: string[] = [];
  const weak = aReturnThatKeepsNoObject(owner, removed);

  await nextTask();
  collect();
  expect(weak.object.deref()).toBeUndefined();
  expect(weak.listener.deref()).toBeUndefined();
  owner.release();

  expect(removed).toStrictEqual([]);
});
