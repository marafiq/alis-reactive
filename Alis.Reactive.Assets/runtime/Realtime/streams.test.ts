import { describe, expect, test } from "vitest";
import { Streams, type MessageSource, type ReconnectContract } from "./index";

type PlanDocument = Parameters<Streams["holds"]>[0];
type Owner = Parameters<Streams["holds"]>[1];
type Removal = Parameters<Owner["holds"]>[0];

const connecting = 0;
const open = 1;
const closed = 2;

class Source implements MessageSource {
  readyState = connecting;
  onopen: ((event: Event) => void) | null = null;
  onerror: ((event: Event) => void) | null = null;
  ended = false;
  readonly #heard = new Map<string, ((event: MessageEvent<unknown>) => void)[]>();

  addEventListener(type: string, listener: (event: MessageEvent<unknown>) => void): void {
    this.#heard.set(type, [...(this.#heard.get(type) ?? []), listener]);
  }

  close(): void {
    this.ended = true;
    this.readyState = closed;
  }

  opens(): void {
    this.readyState = open;
    this.onopen?.(new Event("open"));
  }

  drops(): void {
    this.readyState = connecting;
    this.onerror?.(new Event("error"));
  }

  givesUp(): void {
    this.readyState = closed;
    this.onerror?.(new Event("error"));
  }

  sends(type: string, data: string): void {
    for (const listener of this.#heard.get(type) ?? []) listener(new MessageEvent(type, { data }));
  }
}

class SlotOwner implements Owner {
  readonly #held: Removal[] = [];
  readonly #other: Removal[] = [];

  runs(end: Removal): void {
    this.#other.push(end);
  }

  mounts(unmount: Removal): void {
    this.#other.push(unmount);
  }

  listens(remove: Removal): void {
    this.#other.push(remove);
  }

  holds(release: Removal): void {
    this.#held.push(release);
  }

  registers(drop: Removal): void {
    this.#other.push(drop);
  }

  release(): void {
    for (const release of this.#held.splice(0)) release();
  }
}

function planNaming(...urls: string[]): PlanDocument {
  return {
    version: 7, planId: "round",
    culture: { name: "en-US", collation: "en-US", decimal: ".", group: ",", integer: { group: false, exponent: false }, fraction: { group: true, exponent: true } },
    timeZone: { kind: "clock" }, trace: { kind: "off" }, vendors: {},
    objects: Object.fromEntries(urls.map((url) => ["$stream:" + url, {
      kind: "plugin", name: "alis.stream", key: url,
      listen: { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" },
      data: { kind: "stream", offer: true },
    }] as const)),
    handlers: [],
  };
}

function page() {
  const sources: [string, Source][] = [];
  const offer: string[] = [];
  const errors: string[] = [];
  const retries: (() => void)[] = [];
  const contract: ReconnectContract = {
    show: (retry) => {
      offer.push("show");
      retries.push(retry);
    },
    hide: () => {
      offer.push("hide");
    },
  };
  const streams = new Streams({
    opens: (url) => {
      const source = new Source();
      sources.push([url, source]);
      return source;
    },
    reconnect: () => contract,
    logs: (error) => {
      errors.push(error);
    },
  });
  const latest = (url: string) => sources.filter(([at]) => at === url).map(([, source]) => source).at(-1) ?? new Source();
  const heard: string[] = [];
  const hears = (url: string, ...names: string[]) => {
    for (const name of names) streams.of(url).addEventListener(name, (...args: unknown[]) => heard.push(name + (args.length === 0 ? "" : " " + JSON.stringify(args))));
  };
  const retry = () => {
    retries.at(-1)?.();
  };
  return { streams, latest, offer, errors, heard, hears, retry, opened: () => sources.length };
}

describe("a server-sent event stream", () => {
  test("a drop runs its reconnecting steps, and the reopened connection its reconnected steps, with no offer", () => {
    const board = page();
    board.streams.holds(planNaming("/board"), new SlotOwner());
    board.hears("/board", "reconnecting", "reconnected", "closed");
    board.latest("/board").opens();
    board.latest("/board").drops();
    expect(board.streams.of("/board").state).toBe("reconnecting");
    board.latest("/board").opens();
    expect(board.heard).toEqual(["reconnecting", "reconnected"]);
    expect(board.streams.of("/board").state).toBe("open");
    expect(board.offer).toEqual([]);
  });

  test("a stream refused at its first start runs its closed steps and shows the offer", () => {
    const board = page();
    board.streams.holds(planNaming("/board"), new SlotOwner());
    board.hears("/board", "reconnecting", "closed");
    board.latest("/board").givesUp();
    expect(board.heard).toEqual(["closed"]);
    expect(board.offer).toEqual(["show"]);
  });

  test("two given-up streams show one offer, unloading one keeps it, and the other opening on a retry hides it", () => {
    const round = page();
    const card = new SlotOwner();
    round.streams.holds(planNaming("/board"), new SlotOwner());
    round.streams.holds(planNaming("/beds"), card);
    round.latest("/board").opens();
    round.latest("/beds").opens();
    round.latest("/board").givesUp();
    round.latest("/beds").givesUp();
    expect(round.offer).toEqual(["show"]);
    card.release();
    expect(round.offer).toEqual(["show"]);
    round.retry();
    round.latest("/board").opens();
    expect(round.offer).toEqual(["show", "hide"]);
  });

  test("unloading the only given-up stream hides the offer", () => {
    const round = page();
    const card = new SlotOwner();
    round.streams.holds(planNaming("/beds"), card);
    round.latest("/beds").givesUp();
    card.release();
    expect(round.offer).toEqual(["show", "hide"]);
  });

  test("a retry keeps the offer shown while its connection is refused, and the next retry that opens hides it", () => {
    const board = page();
    board.streams.holds(planNaming("/board"), new SlotOwner());
    board.latest("/board").givesUp();
    board.retry();
    expect(board.streams.of("/board").state).toBe("reconnecting");
    board.latest("/board").givesUp();
    expect(board.offer).toEqual(["show"]);
    board.retry();
    board.latest("/board").opens();
    expect(board.offer).toEqual(["show", "hide"]);
    expect(board.opened()).toBe(3);
  });

  test("a step's close ends the connection, runs no closed steps, shows no offer and hears no later message", () => {
    const alerts = page();
    alerts.streams.holds(planNaming("/alerts"), new SlotOwner());
    alerts.hears("/alerts", "closed", "alert");
    alerts.latest("/alerts").opens();
    alerts.streams.of("/alerts").close();
    alerts.latest("/alerts").sends("alert", "{}");
    expect(alerts.latest("/alerts").ended).toBe(true);
    expect(alerts.streams.of("/alerts").state).toBe("closed");
    expect(alerts.heard).toEqual([]);
    expect(alerts.offer).toEqual([]);
  });

  test("data that is not JSON runs no handler and logs one error, and the next message runs", () => {
    const board = page();
    board.streams.holds(planNaming("/board"), new SlotOwner());
    board.hears("/board", "bulletin", "allClear");
    board.latest("/board").opens();
    board.latest("/board").sends("bulletin", "a bulletin");
    board.latest("/board").sends("allClear", "{}");
    expect(board.heard).toEqual(['allClear [{"data":{}}]']);
    expect(board.errors).toEqual(["The stream '/board' sent the event 'bulletin', whose data is not JSON."]);
  });

  test("two owners naming one stream share one connection, held until both release it", () => {
    const round = page();
    const first = new SlotOwner();
    const second = new SlotOwner();
    round.streams.holds(planNaming("/board"), first);
    round.streams.holds(planNaming("/board"), second);
    expect(round.opened()).toBe(1);
    first.release();
    expect(round.latest("/board").ended).toBe(false);
    second.release();
    expect(round.latest("/board").ended).toBe(true);
    expect(() => round.streams.of("/board")).toThrow("The stream '/board' is named by no live plan.");
  });
});
