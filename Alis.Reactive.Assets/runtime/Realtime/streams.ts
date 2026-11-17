import type { Owner } from "../OwnershipAndState/index";
import { assertNever, type Locator, type PlanDocument } from "../Schema/index";
import { streamPlugin, type ConnectionState, type ReconnectOffer } from "./connections";

const connecting = 0;
const closed = 2;

type Listener = (...args: unknown[]) => void;

type Lifecycle = "reconnecting" | "reconnected" | "closed";

export interface MessageSource {
  readonly readyState: number;
  onopen: ((event: Event) => void) | null;
  onerror: ((event: Event) => void) | null;
  addEventListener(type: string, listener: (event: MessageEvent<unknown>) => void): void;
  close(): void;
}

export interface StreamPorts {
  readonly opens: (url: string) => MessageSource;
  readonly logs: (error: string) => void;
}

interface StreamHold {
  readonly url: string;
  readonly offer: boolean;
}

type Connection = { readonly kind: "none" } | { readonly kind: "live"; readonly source: MessageSource; readonly heard: Set<string> };

const lifecycle: ReadonlySet<string> = new Set<Lifecycle>(["reconnecting", "reconnected", "closed"]);

export class LiveStream {
  readonly #url: string;
  readonly #page: Streams;
  readonly #listeners = new Map<string, Set<Listener>>();
  #connection: Connection = { kind: "none" };
  #state: ConnectionState = "connecting";
  #lost = false;
  #offer = false;

  constructor(url: string, page: Streams) {
    this.#url = url;
    this.#page = page;
  }

  get state(): ConnectionState {
    return this.#state;
  }

  get offers(): boolean {
    return this.#offer;
  }

  get gaveUp(): boolean {
    return this.#state === "closed" && this.#lost;
  }

  addEventListener(name: string, listener: Listener): void {
    const listeners = this.#listeners.get(name) ?? new Set<Listener>();
    listeners.add(listener);
    this.#listeners.set(name, listeners);
    if (this.#connection.kind === "live") this.#hears(this.#connection, name);
  }

  removeEventListener(name: string, listener: Listener): void {
    this.#listeners.get(name)?.delete(listener);
  }

  close(): void {
    this.#ends();
    this.#lost = false;
    this.#state = "closed";
    this.#page.counts(this);
  }

  opens(offer: boolean): void {
    this.#offer = offer;
    this.#connects();
  }

  retries(): void {
    if (!this.gaveUp) return;
    this.#state = "reconnecting";
    this.#connects();
  }

  releases(): void {
    this.#ends();
    this.#lost = false;
    this.#state = "closed";
  }

  #connects(): void {
    const source = this.#page.ports.opens(this.#url);
    const connection = { kind: "live", source, heard: new Set<string>() } as const;
    this.#connection = connection;
    source.onopen = () => {
      if (this.#connection !== connection) return;
      this.#opened();
    };
    source.onerror = () => {
      if (this.#connection !== connection) return;
      this.#failed(source);
    };
    for (const name of this.#listeners.keys()) this.#hears(connection, name);
  }

  #hears(connection: { readonly source: MessageSource; readonly heard: Set<string> }, name: string): void {
    if (lifecycle.has(name) || connection.heard.has(name)) return;
    connection.heard.add(name);
    connection.source.addEventListener(name, (event) => {
      if (this.#connection.kind !== "live" || this.#connection.source !== connection.source) return;
      this.#received(name, event.data);
    });
  }

  #received(name: string, data: unknown): void {
    const parsed = parsedJson(data);
    if (parsed.kind === "unreadable") {
      this.#page.ports.logs("The stream '" + this.#url + "' sent the event '" + name + "', whose data is not JSON.");
      return;
    }
    this.#dispatches(name, [{ data: parsed.value }]);
  }

  #opened(): void {
    const wasLost = this.#lost || this.#state === "reconnecting";
    this.#state = "open";
    this.#lost = false;
    this.#page.counts(this);
    if (wasLost) this.#dispatches("reconnected", []);
  }

  #failed(source: MessageSource): void {
    if (source.readyState === connecting) {
      if (this.#state !== "open") return;
      this.#state = "reconnecting";
      this.#dispatches("reconnecting", []);
      return;
    }
    if (source.readyState !== closed) return;
    this.#connection = { kind: "none" };
    this.#state = "closed";
    this.#lost = true;
    this.#page.counts(this);
    this.#dispatches("closed", []);
  }

  #ends(): void {
    if (this.#connection.kind === "live") this.#connection.source.close();
    this.#connection = { kind: "none" };
  }

  #dispatches(name: string, args: readonly unknown[]): void {
    for (const listener of [...(this.#listeners.get(name) ?? [])]) listener(...args);
  }
}

type Parsed = { readonly kind: "parsed"; readonly value: unknown } | { readonly kind: "unreadable" };

function parsedJson(data: unknown): Parsed {
  if (typeof data !== "string") return { kind: "unreadable" };
  try {
    return { kind: "parsed", value: JSON.parse(data) };
  } catch {
    return { kind: "unreadable" };
  }
}

interface Held {
  readonly stream: LiveStream;
  holders: number;
}

export class Streams {
  readonly ports: StreamPorts;
  readonly #held = new Map<string, Held>();
  readonly #offer: ReconnectOffer;

  constructor(ports: StreamPorts, offer: ReconnectOffer) {
    this.ports = ports;
    this.#offer = offer;
  }

  of(url: string): LiveStream {
    const held = this.#held.get(url);
    if (held === undefined) throw new Error("The stream '" + url + "' is named by no live plan.");
    return held.stream;
  }

  holds(plan: PlanDocument, owner: Owner): void {
    for (const hold of Object.values(plan.objects).flatMap(streamsNamedAt)) this.#holds(hold, owner);
  }

  counts(stream: LiveStream): void {
    this.#offer.counts(stream, stream.offers && (stream.gaveUp || (this.#offer.holds(stream) && stream.state === "reconnecting")));
  }

  #holds(hold: StreamHold, owner: Owner): void {
    const existing = this.#held.get(hold.url);
    const held = existing ?? { stream: new LiveStream(hold.url, this), holders: 0 };
    held.holders += 1;
    this.#held.set(hold.url, held);
    owner.holds(() => {
      this.#releases(hold.url, held);
    });
    if (existing === undefined) this.#opens(held.stream, hold);
  }

  #opens(stream: LiveStream, hold: StreamHold): void {
    if (hold.offer) {
      try {
        this.#offer.contract();
      } catch (error) {
        this.ports.logs("The stream '" + hold.url + "' did not open: " + (error instanceof Error ? error.message : "its reconnect offer is missing"));
        return;
      }
    }
    stream.opens(hold.offer);
  }

  #releases(url: string, held: Held): void {
    held.holders -= 1;
    if (held.holders > 0) return;
    this.#held.delete(url);
    held.stream.releases();
    this.#offer.withdraws(held.stream);
  }
}

function streamsNamedAt(locator: Locator): StreamHold[] {
  switch (locator.kind) {
    case "plugin":
      return locator.name === streamPlugin && locator.data.kind === "stream" ? [{ url: locator.key, offer: locator.data.offer }] : [];
    case "element":
      return [];
    default:
      return assertNever(locator);
  }
}
