import { assertNever, notBuilt, type Listening } from "../Schema/index";
import { Stop } from "../ValuesAndSources/index";

export type Listener = (...args: unknown[]) => void;

export type Unsubscribe = () => void;

export function subscribe(raiser: object, listening: Listening, event: string, listener: Listener): Unsubscribe {
  switch (listening.kind) {
    case "method": {
      const add = methodOf(raiser, listening.subscribe);
      const remove = methodOf(raiser, listening.unsubscribe);
      Reflect.apply(add, raiser, [event, listener]);
      return () => {
        Reflect.apply(remove, raiser, [event, listener]);
      };
    }
    case "assignment":
    case "returned":
      throw notBuilt("a listener by '" + listening.kind + "'");
    default:
      return assertNever(listening);
  }
}

function methodOf(holder: object, name: string): (...args: unknown[]) => unknown {
  const found: unknown = Reflect.get(holder, name);
  if (typeof found !== "function") throw new Stop("The method '" + name + "' is not on the object that should hold it.");
  return (...args: unknown[]): unknown => Reflect.apply(found, holder, args);
}
