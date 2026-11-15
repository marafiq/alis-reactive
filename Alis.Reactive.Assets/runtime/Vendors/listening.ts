import { assertNever, type Listening } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { assignListener } from "./callback-properties";

export type Listener = (...args: unknown[]) => void;

export type Unsubscribe = () => void;

interface SubscriptionState {
  active: boolean;
}

export function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  const state = { active: false };
  const remove = listen(raiser, listening, event, guarding(listener, state));
  state.active = true;
  return removing(remove, state);
}

function guarding(listener: Listener, state: SubscriptionState): Listener {
  return (...args) => {
    if (state.active) listener(...args);
  };
}

function removing(remove: Unsubscribe, state: SubscriptionState): Unsubscribe {
  return () => {
    if (!state.active) return;
    state.active = false;
    remove();
  };
}

function listen(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  switch (listening.kind) {
    case "method":
      return methodPair(raiser, listening.subscribe, listening.unsubscribe, event, listener);
    case "assignment":
      return assignListener(raiser, event, listener);
    case "returned":
      return returned(methodOf(raiser, listening.subscribe)(event, listener), listening.subscribe);
    default:
      return assertNever(listening);
  }
}

function methodPair(raiser: object, subscribe: string, unsubscribe: string, event: string, listener: Listener): Unsubscribe {
  const add = methodOf(raiser, subscribe);
  const remove = methodOf(raiser, unsubscribe);
  add(event, listener);
  return () => {
    remove(event, listener);
  };
}

function returned(remove: unknown, subscribe: string): Unsubscribe {
  if (typeof remove !== "function") throw new Stop("The subscribe method '" + subscribe + "' returned no unsubscriber.");
  return () => {
    Reflect.apply(remove, undefined, []);
  };
}

function methodOf(holder: object, name: string): (...args: unknown[]) => unknown {
  const found: unknown = Reflect.get(holder, name);
  if (typeof found !== "function") throw new Stop("The method '" + name + "' is not on the object that should hold it.");
  return (...args: unknown[]): unknown => Reflect.apply(found, holder, args);
}
