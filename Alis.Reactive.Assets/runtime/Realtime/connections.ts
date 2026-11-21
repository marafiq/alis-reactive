import { assertNever, type Locator } from "../Schema/index";
import type { Attributes } from "../Tracing/index";

export const streamPlugin = "alis.stream";
export const hubPlugin = "alis.hub";

export type ConnectionState = "connecting" | "open" | "reconnecting" | "closed";

export interface ReconnectContract {
  show(retry: () => void): void;
  hide(): void;
}

export interface Offered {
  retries(): void;
}

export class ReconnectOffer {
  readonly #reconnect: () => ReconnectContract;
  readonly #offered = new Set<Offered>();

  constructor(reconnect: () => ReconnectContract) {
    this.#reconnect = reconnect;
  }

  contract(): ReconnectContract {
    return this.#reconnect();
  }

  holds(connection: Offered): boolean {
    return this.#offered.has(connection);
  }

  counts(connection: Offered, unanswered: boolean): void {
    const before = this.#offered.size;
    if (unanswered) this.#offered.add(connection);
    else this.#offered.delete(connection);
    this.#tells(before);
  }

  withdraws(connection: Offered): void {
    this.counts(connection, false);
  }

  #tells(before: number): void {
    const after = this.#offered.size;
    if (before === 0 && after > 0) this.#reconnect().show(() => {
      this.#retries();
    });
    if (before > 0 && after === 0) this.#reconnect().hide();
  }

  #retries(): void {
    for (const connection of [...this.#offered]) connection.retries();
  }
}

export function transportOf(locator: Locator): Attributes {
  switch (locator.kind) {
    case "plugin":
      if (locator.name === streamPlugin) return { "alis.transport": "server-sent events" };
      return locator.name === hubPlugin ? { "alis.transport": "SignalR" } : {};
    case "element":
      return {};
    default:
      return assertNever(locator);
  }
}

export function callsAHub(locator: Locator): boolean {
  switch (locator.kind) {
    case "plugin":
      return locator.name === hubPlugin;
    case "element":
      return false;
    default:
      return assertNever(locator);
  }
}

export function reconnectContract(found: unknown): ReconnectContract {
  if (typeof found !== "object" || found === null) throw new Error("The plugin 'reconnect' is no object.");
  const show: unknown = Reflect.get(found, "show");
  const hide: unknown = Reflect.get(found, "hide");
  if (typeof show !== "function" || typeof hide !== "function") throw new Error("The plugin 'reconnect' has no show(retry) and hide().");
  return {
    show: (retry) => {
      Reflect.apply(show, found, [retry]);
    },
    hide: () => {
      Reflect.apply(hide, found, []);
    },
  };
}
