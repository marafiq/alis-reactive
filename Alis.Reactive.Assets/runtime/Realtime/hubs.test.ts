import { describe, expect, test } from "vitest";
import { Hubs, ReconnectOffer, type HubConnection, type ReconnectContract } from "./index";

type PlanDocument = Parameters<Hubs["holds"]>[0];
type Owner = Parameters<Hubs["holds"]>[1];
type Removal = Parameters<Owner["holds"]>[0];

class Connection implements HubConnection {
  connectionId: string | null = null;
  readonly #handlers = new Map<string, ((...args: unknown[]) => void)[]>();
  readonly #starts: { resolve: () => void; reject: (error: Error) => void }[] = [];
  #reconnecting: () => void = () => undefined;
  #reconnected: (connectionId?: string) => void = () => undefined;
  #closed: () => void = () => undefined;
  #connected = false;
  stopped = false;

  start(): Promise<void> {
    return new Promise((resolve, reject) => {
      this.#starts.push({ resolve, reject });
    });
  }

  stop(): Promise<void> {
    this.stopped = true;
    return Promise.resolve();
  }

  on(name: string, handler: (...args: unknown[]) => void): void {
    this.#handlers.set(name, [...(this.#handlers.get(name) ?? []), handler]);
  }

  invoke(method: string, ...args: unknown[]): Promise<unknown> {
    if (!this.#connected) return Promise.reject(new Error("Cannot send data if the connection is not in the 'Connected' State."));
    return Promise.resolve(method + " " + JSON.stringify(args));
  }

  send(): Promise<void> {
    if (!this.#connected) return Promise.reject(new Error("Cannot send data if the connection is not in the 'Connected' State."));
    return Promise.resolve();
  }

  onreconnecting(callback: () => void): void {
    this.#reconnecting = callback;
  }

  onreconnected(callback: (connectionId?: string) => void): void {
    this.#reconnected = callback;
  }

  onclose(callback: () => void): void {
    this.#closed = callback;
  }

  get tries(): number {
    return this.#starts.length;
  }

  accepts(connectionId: string): void {
    this.connectionId = connectionId;
    this.#connected = true;
    this.#starts.at(-1)?.resolve();
  }

  refuses(): void {
    this.#starts.at(-1)?.reject(new Error("Failed to complete negotiation with the server: Error: Service Unavailable: Status code '503'"));
  }

  drops(): void {
    this.#connected = false;
    this.#reconnecting();
  }

  reconnects(connectionId: string): void {
    this.connectionId = connectionId;
    this.#connected = true;
    this.#reconnected(connectionId);
  }

  givesUp(): void {
    this.#connected = false;
    this.#closed();
  }

  pushes(name: string, ...args: unknown[]): void {
    for (const handler of this.#handlers.get(name) ?? []) handler(...args);
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
    objects: Object.fromEntries(urls.map((url) => ["$hub:" + url, {
      kind: "plugin", name: "alis.hub", key: url,
      listen: { kind: "method", subscribe: "addEventListener", unsubscribe: "removeEventListener" },
      data: { kind: "hub", offer: true, waits: [0, 2000], client: "/hub.js" },
    }] as const)),
    handlers: [],
  };
}

async function settles(): Promise<void> {
  for (let turn = 0; turn < 10; turn += 1) await Promise.resolve();
}

function page() {
  const connections: [string, Connection][] = [];
  const waited: number[] = [];
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
  const hubs = new Hubs({
    client: () => Promise.resolve({
      connection: (url) => {
        const connection = new Connection();
        connections.push([url, connection]);
        return connection;
      },
    }),
    waits: (milliseconds) => {
      waited.push(milliseconds);
      return Promise.resolve();
    },
    logs: (error) => {
      errors.push(error);
    },
  }, new ReconnectOffer(() => contract));
  const connection = (url: string) => connections.find(([at]) => at === url)?.[1] ?? new Connection();
  const heard: string[] = [];
  const hears = (url: string, ...names: string[]) => {
    for (const name of names) hubs.of(url).addEventListener(name, (...args: unknown[]) => heard.push(name + (args.length === 0 ? "" : " " + JSON.stringify(args))));
  };
  const retry = () => {
    retries.at(-1)?.();
  };
  return { hubs, connection, waited, offer, errors, heard, hears, retry };
}

describe("a SignalR hub", () => {
  test("a first start refused on every try waits the plan's waits, then runs its closed steps and shows the offer, with no reconnecting", async () => {
    const line = page();
    line.hubs.holds(planNaming("/line"), new SlotOwner());
    line.hears("/line", "reconnecting", "closed");
    for (let tried = 0; tried < 3; tried += 1) {
      await settles();
      line.connection("/line").refuses();
    }
    await settles();
    expect(line.connection("/line").tries).toBe(3);
    expect(line.waited).toEqual([0, 2000]);
    expect(line.heard).toEqual(["closed"]);
    expect(line.offer).toEqual(["show"]);
  });

  test("a retry that opens a hub that gave up runs its reconnected steps with the connection id and hides the offer", async () => {
    const line = page();
    line.hubs.holds(planNaming("/line"), new SlotOwner());
    line.hears("/line", "reconnected", "closed");
    await settles();
    line.connection("/line").accepts("first");
    await settles();
    line.connection("/line").givesUp();
    line.retry();
    await settles();
    line.connection("/line").accepts("second");
    await settles();
    expect(line.heard).toEqual(["closed", "reconnected [{\"connectionId\":\"second\"}]"]);
    expect(line.offer).toEqual(["show", "hide"]);
    expect(line.hubs.of("/line").state).toBe("open");
  });

  test("a push with one object runs its handlers with it as data, and one with two arguments logs one error and runs nothing", async () => {
    const line = page();
    line.hubs.holds(planNaming("/line"), new SlotOwner());
    line.hears("/line", "statusChanged");
    await settles();
    line.connection("/line").accepts("first");
    await settles();
    line.connection("/line").pushes("statusChanged", { resident: "Ada" }, { resident: "Ben" });
    line.connection("/line").pushes("statusChanged", { resident: "Ada" });
    expect(line.heard).toEqual(["statusChanged [{\"data\":{\"resident\":\"Ada\"}}]"]);
    expect(line.errors).toEqual(["The hub '/line' pushed 'statusChanged' with 2 arguments, where its handlers read one object."]);
  });

  test("an invoke before the first start waits for it, and one while the hub reconnects is rejected naming the hub and the method", async () => {
    const line = page();
    line.hubs.holds(planNaming("/line"), new SlotOwner());
    const early = line.hubs.of("/line").invoke("Roster");
    await settles();
    line.connection("/line").accepts("first");
    await expect(early).resolves.toBe("Roster []");
    line.connection("/line").drops();
    await expect(line.hubs.of("/line").invoke("Roster")).rejects.toThrow("The hub '/line' did not run Roster: Cannot send data if the connection is not in the 'Connected' State.");
  });

  test("unloading the only card that holds a hub that gave up stops it and hides the offer, while a hub the page also holds keeps it", async () => {
    const round = page();
    const card = new SlotOwner();
    round.hubs.holds(planNaming("/bed"), card);
    await settles();
    round.connection("/bed").accepts("first");
    await settles();
    round.connection("/bed").givesUp();
    card.release();
    expect(round.connection("/bed").stopped).toBe(true);
    expect(round.offer).toEqual(["show", "hide"]);

    const held = page();
    const heldCard = new SlotOwner();
    held.hubs.holds(planNaming("/bed"), new SlotOwner());
    held.hubs.holds(planNaming("/bed"), heldCard);
    await settles();
    held.connection("/bed").accepts("first");
    await settles();
    held.connection("/bed").givesUp();
    heldCard.release();
    expect(held.offer).toEqual(["show"]);
  });
});
