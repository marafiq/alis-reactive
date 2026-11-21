import type { Owner } from "../OwnershipAndState/index";
import { assertNever, type Locator, type PlanDocument } from "../Schema/index";
import { hubPlugin, type ConnectionState, type ReconnectOffer } from "./connections";

export interface HubConnection {
  readonly connectionId: string | null;
  start(): Promise<void>;
  stop(): Promise<void>;
  on(name: string, handler: (...args: unknown[]) => void): void;
  invoke(method: string, ...args: unknown[]): Promise<unknown>;
  send(method: string, ...args: unknown[]): Promise<void>;
  onreconnecting(callback: () => void): void;
  onreconnected(callback: (connectionId?: string) => void): void;
  onclose(callback: () => void): void;
}

export interface HubClient {
  connection(url: string, waits: readonly number[]): HubConnection;
}

export interface HubPorts {
  readonly client: (asset: string) => Promise<HubClient>;
  readonly waits: (milliseconds: number) => Promise<void>;
  readonly logs: (error: string) => void;
}

interface HubHold {
  readonly url: string;
  readonly offer: boolean;
  readonly waits: readonly number[];
  readonly client: string;
}

type Listener = (...args: unknown[]) => void;

type Link = { readonly kind: "none" } | { readonly kind: "live"; readonly connection: HubConnection; readonly heard: Set<string> };

const lifecycle: ReadonlySet<string> = new Set(["reconnecting", "reconnected", "closed"]);

export class LiveHub {
  readonly #hold: HubHold;
  readonly #page: Hubs;
  readonly #listeners = new Map<string, Set<Listener>>();
  #link: Link = { kind: "none" };
  #state: ConnectionState = "connecting";
  #lost = false;
  #generation = 0;
  #first: Promise<void> = Promise.resolve();

  constructor(hold: HubHold, page: Hubs) {
    this.#hold = hold;
    this.#page = page;
  }

  get state(): ConnectionState {
    return this.#state;
  }

  get connectionId(): string | null {
    return this.#link.kind === "live" && this.#state === "open" ? this.#link.connection.connectionId : null;
  }

  get offers(): boolean {
    return this.#hold.offer;
  }

  get gaveUp(): boolean {
    return this.#state === "closed" && this.#lost;
  }

  addEventListener(name: string, listener: Listener): void {
    const listeners = this.#listeners.get(name) ?? new Set<Listener>();
    listeners.add(listener);
    this.#listeners.set(name, listeners);
    if (this.#link.kind === "live") this.#hears(this.#link, name);
  }

  removeEventListener(name: string, listener: Listener): void {
    this.#listeners.get(name)?.delete(listener);
  }

  invoke(method: string, ...args: unknown[]): Promise<unknown> {
    return this.#calls(method, (connection) => connection.invoke(method, ...args));
  }

  send(method: string, ...args: unknown[]): Promise<void> {
    return this.#calls(method, (connection) => connection.send(method, ...args));
  }

  opens(): void {
    this.#first = this.#starts(this.#generation);
  }

  retries(): void {
    if (!this.gaveUp) return;
    this.#state = "reconnecting";
    this.#page.counts(this);
    void this.#retried(this.#generation);
  }

  releases(): void {
    this.#generation += 1;
    if (this.#link.kind === "live") this.#stops(this.#link.connection);
    this.#link = { kind: "none" };
    this.#lost = false;
    this.#state = "closed";
  }

  async #calls<T>(method: string, call: (connection: HubConnection) => Promise<T>): Promise<T> {
    await this.#first;
    if (this.#link.kind !== "live") throw new Error(this.#didNotRun(method, "it never connected"));
    try {
      return await call(this.#link.connection);
    } catch (error) {
      throw new Error(this.#didNotRun(method, messageOf(error)), { cause: error });
    }
  }

  #didNotRun(method: string, why: string): string {
    return "The hub '" + this.#hold.url + "' did not run " + method + ": " + why;
  }

  async #starts(generation: number): Promise<void> {
    for (const wait of [...this.#hold.waits, undefined]) {
      if (await this.#tries(generation)) return;
      if (generation !== this.#generation) return;
      if (wait === undefined) {
        this.#givesUp();
        return;
      }
      await this.#page.ports.waits(wait);
      if (generation !== this.#generation) return;
    }
  }

  async #retried(generation: number): Promise<void> {
    if (await this.#tries(generation)) return;
    if (generation !== this.#generation) return;
    this.#givesUp();
  }

  async #tries(generation: number): Promise<boolean> {
    const link = await this.#linked(generation);
    if (generation !== this.#generation) return true;
    if (link.kind === "none") return false;
    try {
      await link.connection.start();
    } catch {
      return false;
    }
    if (generation !== this.#generation) return true;
    this.#opened(link.connection);
    return true;
  }

  async #linked(generation: number): Promise<Link> {
    if (this.#link.kind === "live") return this.#link;
    try {
      const client = await this.#page.ports.client(this.#hold.client);
      if (generation !== this.#generation) return { kind: "none" };
      const link = { kind: "live", connection: client.connection(this.#hold.url, this.#hold.waits), heard: new Set<string>() } as const;
      this.#link = link;
      this.#follows(link.connection);
      for (const name of this.#listeners.keys()) this.#hears(link, name);
      return link;
    } catch (error) {
      this.#page.ports.logs("The hub '" + this.#hold.url + "' did not start: " + messageOf(error));
      return { kind: "none" };
    }
  }

  #follows(connection: HubConnection): void {
    const generation = this.#generation;
    connection.onreconnecting(() => {
      if (generation !== this.#generation) return;
      this.#state = "reconnecting";
      this.#page.counts(this);
      this.#dispatches("reconnecting", []);
    });
    connection.onreconnected((connectionId) => {
      if (generation !== this.#generation) return;
      this.#state = "open";
      this.#lost = false;
      this.#page.counts(this);
      this.#dispatches("reconnected", [{ connectionId: connectionId ?? null }]);
    });
    connection.onclose(() => {
      if (generation !== this.#generation) return;
      this.#givesUp();
    });
  }

  #hears(link: { readonly connection: HubConnection; readonly heard: Set<string> }, name: string): void {
    if (lifecycle.has(name) || link.heard.has(name)) return;
    link.heard.add(name);
    link.connection.on(name, (...args: unknown[]) => {
      if (this.#link !== link) return;
      this.#received(name, args);
    });
  }

  #received(name: string, args: readonly unknown[]): void {
    const [message] = args;
    if (args.length !== 1 || !isPlainObject(message)) {
      this.#page.ports.logs("The hub '" + this.#hold.url + "' pushed '" + name + "' with " + shapeOf(args) + ", where its handlers read one object.");
      return;
    }
    this.#dispatches(name, [{ data: message }]);
  }

  #opened(connection: HubConnection): void {
    const wasLost = this.#lost || this.#state === "reconnecting";
    this.#state = "open";
    this.#lost = false;
    this.#page.counts(this);
    if (wasLost) this.#dispatches("reconnected", [{ connectionId: connection.connectionId }]);
  }

  #givesUp(): void {
    this.#state = "closed";
    this.#lost = true;
    this.#page.counts(this);
    this.#dispatches("closed", []);
  }

  #stops(connection: HubConnection): void {
    connection.stop().catch((error: unknown) => {
      this.#page.ports.logs("The hub '" + this.#hold.url + "' did not stop: " + messageOf(error));
    });
  }

  #dispatches(name: string, args: readonly unknown[]): void {
    for (const listener of [...(this.#listeners.get(name) ?? [])]) listener(...args);
  }
}

function shapeOf(args: readonly unknown[]): string {
  if (args.length === 1) return "one argument that is no object";
  return String(args.length) + " arguments";
}

function isPlainObject(value: unknown): value is object {
  return typeof value === "object" && value !== null && !Array.isArray(value);
}

function messageOf(error: unknown): string {
  return error instanceof Error ? error.message : String(error);
}

interface Held {
  readonly hub: LiveHub;
  holders: number;
}

export class Hubs {
  readonly ports: HubPorts;
  readonly #held = new Map<string, Held>();
  readonly #offer: ReconnectOffer;

  constructor(ports: HubPorts, offer: ReconnectOffer) {
    this.ports = ports;
    this.#offer = offer;
  }

  of(url: string): LiveHub {
    const held = this.#held.get(url);
    if (held === undefined) throw new Error("The hub '" + url + "' is named by no live plan.");
    return held.hub;
  }

  holds(plan: PlanDocument, owner: Owner): void {
    for (const hold of Object.values(plan.objects).flatMap(hubsNamedAt)) this.#holds(hold, owner);
  }

  counts(hub: LiveHub): void {
    this.#offer.counts(hub, hub.offers && (hub.gaveUp || (this.#offer.holds(hub) && hub.state === "reconnecting")));
  }

  #holds(hold: HubHold, owner: Owner): void {
    const existing = this.#held.get(hold.url);
    const held = existing ?? { hub: new LiveHub(hold, this), holders: 0 };
    held.holders += 1;
    this.#held.set(hold.url, held);
    owner.holds(() => {
      this.#releases(hold.url, held);
    });
    if (existing === undefined) this.#opens(held.hub, hold);
  }

  #opens(hub: LiveHub, hold: HubHold): void {
    if (hold.offer) {
      try {
        this.#offer.contract();
      } catch (error) {
        this.ports.logs("The hub '" + hold.url + "' did not start: " + messageOf(error));
        return;
      }
    }
    hub.opens();
  }

  #releases(url: string, held: Held): void {
    held.holders -= 1;
    if (held.holders > 0) return;
    this.#held.delete(url);
    held.hub.releases();
    this.#offer.withdraws(held.hub);
  }
}

function hubsNamedAt(locator: Locator): HubHold[] {
  switch (locator.kind) {
    case "plugin":
      return locator.name === hubPlugin && locator.data.kind === "hub" ? [{ url: locator.key, offer: locator.data.offer, waits: locator.data.waits, client: locator.data.client }] : [];
    case "element":
      return [];
    default:
      return assertNever(locator);
  }
}

export function hubClient(found: unknown): HubClient {
  if (typeof found !== "object" || found === null) throw new Error("The plugin 'alis.signalr' is no object.");
  const connection: unknown = Reflect.get(found, "connection");
  if (typeof connection !== "function") throw new Error("The plugin 'alis.signalr' has no connection(url, waits).");
  return {
    connection: (url, waits) => {
      const made: unknown = Reflect.apply(connection, found, [url, waits]);
      return made as HubConnection;
    },
  };
}
