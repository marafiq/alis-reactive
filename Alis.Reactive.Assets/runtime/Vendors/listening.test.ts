import { describe, expect, test } from "vitest";
import { subscribe, type Listener } from "./index";

class WardFeed {
  readonly listeners = new Map<string, Set<Listener>>();

  readonly removals: string[] = [];

  on(event: string, listener: Listener): void {
    let listeners = this.listeners.get(event);
    if (listeners === undefined) {
      listeners = new Set();
      this.listeners.set(event, listeners);
    }
    listeners.add(listener);
  }

  off(event: string, listener: Listener): void {
    this.removals.push(event);
    this.post(event, "during removal");
    this.listeners.get(event)?.delete(listener);
  }

  subscribe(event: string, listener: Listener): () => void {
    this.on(event, listener);
    return () => {
      this.off(event, listener);
    };
  }

  post(event: string, ...args: unknown[]): void {
    for (const listener of this.listeners.get(event) ?? []) listener(...args);
  }
}

describe("when a vendor listens by a method pair", () => {
  test("ListenLeavesWithItsOwner: the named pair hears the named event and removes only its own listener", () => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    const release = subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", (...args) => {
      heard.push(["card", ...args]);
    });
    subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", (...args) => {
      heard.push(["page", ...args]);
    });

    feed.post("another", "unheard");
    feed.post("posted", "water", 4);
    release();
    feed.post("posted", "tea", 7);
    release();

    expect(heard).toStrictEqual([["card", "water", 4], ["page", "water", 4], ["page", "during removal"], ["page", "tea", 7]]);
    expect(feed.removals).toStrictEqual(["posted"]);
    expect(feed.listeners.get("posted")?.size).toBe(1);
  });

  test.each([undefined, "not a function"])("D12.8: a subscribe member held as %s is refused", (on) => {
    const feed = { on, off: () => undefined };

    expect(() => subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", () => undefined))
      .toThrow(expect.objectContaining({ name: "Stop", message: "The method 'on' is not on the object that should hold it." }));
  });

  test.each([undefined, "not a function"])("D12.8: an unsubscribe member held as %s is refused before subscribing", (off) => {
    const added: string[] = [];
    const feed = {
      on(event: string): void {
        added.push(event);
      },
      off,
    };

    expect(() => subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", () => undefined))
      .toThrow(expect.objectContaining({ name: "Stop", message: "The method 'off' is not on the object that should hold it." }));
    expect(added).toStrictEqual([]);
  });

  test("ThrownListenerRunsNothing: a method release silences its listener before a removal that raises and throws", () => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    Object.defineProperty(feed, "off", {
      value(event: string): never {
        feed.post(event, "during failed removal");
        throw new Error("The feed refused removal.");
      },
    });
    const release = subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", (...args) => {
      heard.push(args);
    });
    feed.post("posted", "before");

    expect(release).toThrow(new Error("The feed refused removal."));
    feed.post("posted", "after");
    release();

    expect(heard).toStrictEqual([["before"]]);
  });

  test("ListenLeavesWithItsOwner: release keeps the unsubscribe found with the subscribe", () => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    const release = subscribe(feed, { kind: "method", subscribe: "on", unsubscribe: "off" }, "posted", (...args) => {
      heard.push(args);
    });
    Object.defineProperty(feed, "off", { value: undefined });
    feed.post("posted", "before");

    release();
    feed.post("posted", "after");

    expect(heard).toStrictEqual([["before"]]);
    expect(feed.removals).toStrictEqual(["posted"]);
  });
});

describe("when a vendor returns its unsubscriber", () => {
  test("ListenLeavesWithItsOwner: release calls the returned function for its own listener", () => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    const release = subscribe(feed, { kind: "returned", subscribe: "subscribe" }, "posted", (...args) => {
      heard.push(["card", ...args]);
    });
    subscribe(feed, { kind: "returned", subscribe: "subscribe" }, "posted", (...args) => {
      heard.push(["page", ...args]);
    });

    feed.post("another", "unheard");
    feed.post("posted", "water", 4);
    release();
    feed.post("posted", "tea", 7);
    release();

    expect(heard).toStrictEqual([["card", "water", 4], ["page", "water", 4], ["page", "during removal"], ["page", "tea", 7]]);
    expect(feed.removals).toStrictEqual(["posted"]);
    expect(feed.listeners.get("posted")?.size).toBe(1);
  });

  test.each([undefined, "not a function"])("D12.8: a returned subscribe held as %s is refused", (start) => {
    expect(() => subscribe({ start }, { kind: "returned", subscribe: "start" }, "posted", () => undefined))
      .toThrow(expect.objectContaining({ name: "Stop", message: "The method 'start' is not on the object that should hold it." }));
  });

  test.each([undefined, null, "not a function", {}])("D12.8: a subscribe returning %s starts no listener", (result) => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    const vendor = {
      start(event: string, listener: Listener): unknown {
        feed.on(event, listener);
        feed.post(event, "inside subscribe");
        return result;
      },
    };

    expect(() => subscribe(vendor, { kind: "returned", subscribe: "start" }, "posted", (...args) => {
      heard.push(args);
    })).toThrow(expect.objectContaining({ name: "Stop", message: "The subscribe method 'start' returned no unsubscriber." }));
    feed.post("posted", "after refusal");

    expect(heard).toStrictEqual([]);
  });

  test("ListenLeavesWithItsOwner: the returned function is called without a receiver or arguments", () => {
    const removed: unknown[][] = [];
    const vendor = {
      start(): (this: unknown, ...args: unknown[]) => void {
        return function (this: unknown, ...args: unknown[]): void {
          removed.push([this === undefined, args]);
        };
      },
    };
    const release = subscribe(vendor, { kind: "returned", subscribe: "start" }, "posted", () => undefined);

    release();

    expect(removed).toStrictEqual([[true, []]]);
  });

  test("ThrownListenerRunsNothing: a returned release silences its listener before a remover that raises and throws", () => {
    const feed = new WardFeed();
    const heard: unknown[][] = [];
    const vendor = {
      start(event: string, listener: Listener): () => void {
        feed.on(event, listener);
        return () => {
          feed.post(event, "during failed removal");
          throw new Error("The returned remover failed.");
        };
      },
    };
    const release = subscribe(vendor, { kind: "returned", subscribe: "start" }, "posted", (...args) => {
      heard.push(args);
    });
    feed.post("posted", "before");

    expect(release).toThrow(new Error("The returned remover failed."));
    feed.post("posted", "after");
    release();

    expect(heard).toStrictEqual([["before"]]);
  });
});
