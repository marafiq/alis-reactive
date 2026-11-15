import { Stop } from "../ValuesAndSources/index";
import type { Listener, Unsubscribe } from "./listening";

interface Chain {
  readonly held: unknown;
  readonly listeners: Set<Listener>;
  readonly invoke: (this: unknown, ...args: unknown[]) => unknown;
}

const callbacks = new WeakMap<object, Map<string, Chain>>();

export function assignListener(raiser: object, property: string, listener: Listener): Unsubscribe {
  if (!(property in raiser)) throw new Stop("The callback property '" + property + "' is not on the object that should hold it.");
  const held: unknown = Reflect.get(raiser, property);
  let properties = callbacks.get(raiser);
  if (properties === undefined) {
    properties = new Map();
    callbacks.set(raiser, properties);
  }
  const current = properties.get(property);
  const chain = current !== undefined && current.invoke === held ? current : install(raiser, property, held);
  properties.set(property, chain);
  chain.listeners.add(listener);
  return () => {
    chain.listeners.delete(listener);
    if (chain.listeners.size === 0 && Reflect.get(raiser, property) === chain.invoke) {
      assign(raiser, property, chain.held);
    }
  };
}

function install(raiser: object, property: string, held: unknown): Chain {
  const listeners = new Set<Listener>();
  const invoke = function (this: unknown, ...args: unknown[]): unknown {
    const hearers = [...listeners];
    try {
      return typeof held === "function" ? Reflect.apply(held, this, args) : undefined;
    } finally {
      for (const listener of hearers) listener(...args);
    }
  };
  assign(raiser, property, invoke);
  return { held, listeners, invoke };
}

function assign(raiser: object, property: string, value: unknown): void {
  const reason = "The object refused the callback property '" + property + "'.";
  try {
    if (Reflect.set(raiser, property, value) && Object.is(Reflect.get(raiser, property), value)) return;
  } catch {
    throw new Stop(reason);
  }
  throw new Stop(reason);
}
