import { assertNever, type Listening } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";
import { assignListener } from "./callback-properties";

export type Listener = (...args: unknown[]) => void;

export type Unsubscribe = () => void;

export function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  let active = false;
  const guarded: Listener = (...args) => {
    if (active) listener(...args);
  };
  const remove = listen(raiser, listening, event, guarded);
  active = true;
  return () => {
    if (!active) return;
    active = false;
    remove();
  };
}

function listen(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  switch (listening.kind) {
    case "method": {
      const add = methodOf(raiser, listening.subscribe);
      const remove = methodOf(raiser, listening.unsubscribe);
      add(event, listener);
      return () => {
        remove(event, listener);
      };
    }
    case "assignment":
      return assignListener(raiser, event, listener);
    case "returned": {
      const remove = methodOf(raiser, listening.subscribe)(event, listener);
      if (typeof remove !== "function") throw new Stop("The subscribe method '" + listening.subscribe + "' returned no unsubscriber.");
      return () => {
        Reflect.apply(remove, undefined, []);
      };
    }
    default:
      return assertNever(listening);
  }
}

function methodOf(holder: object, name: string): (...args: unknown[]) => unknown {
  const found: unknown = Reflect.get(holder, name);
  if (typeof found !== "function") throw new Stop("The method '" + name + "' is not on the object that should hold it.");
  return (...args: unknown[]): unknown => Reflect.apply(found, holder, args);
}
