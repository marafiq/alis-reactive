import { assertNever, type PathMembers, type PathStep } from "../Schema/index";
import { nothing, Stop, value, type Read } from "./read";

export function got(target: Read, path: readonly PathStep[], members: PathMembers): Read {
  return path.reduce((holder, step) => member(holder, step, members), target);
}

export type Returns = "value" | "awaited";

export function called(target: Read, path: readonly PathStep[], members: PathMembers, args: () => readonly unknown[], returns: Returns): Read {
  const method = path.at(-1);
  if (method === undefined) return calledItself(target, args, returns);
  const holder = got(target, path.slice(0, -1), members);
  if (holder.kind === "nothing" || holder.value === null) return ofNothing(method, members);
  const box = boxed(holder.value);
  const held: unknown = Reflect.get(box, method);
  if (held === undefined) return absentMethod(method, members, method in box);
  if (typeof held !== "function") throw new Stop("The member '" + String(method) + "' is no method.");
  return returned(returns, "The method '" + String(method) + "'", Reflect.apply(held, holder.value, args()));
}

function calledItself(target: Read, args: () => readonly unknown[], returns: Returns): Read {
  if (target.kind === "nothing" || typeof target.value !== "function") throw new Stop("The object called as a function is no function.");
  return returned(returns, "The function", Reflect.apply(target.value, undefined, args()));
}

function returned(returns: Returns, callee: string, result: unknown): Read {
  switch (returns) {
    case "awaited":
      return value(result);
    case "value":
      return readAsAValue(callee, result);
    default:
      return assertNever(returns);
  }
}

function readAsAValue(callee: string, result: unknown): Read {
  if (!(result instanceof Promise)) return value(result);
  void result.catch(() => undefined);
  throw new Stop(callee + " returned a promise where its declaration reads a value; a method that returns one is declared with MethodAsync.");
}

function absentMethod(method: PathStep, members: PathMembers, held: boolean): Read {
  switch (members) {
    case "required":
    case "requiredIfHeld":
      throw new Stop(held ? "The member '" + String(method) + "' is no method." : "The member '" + String(method) + "' is not on the object that should hold it.");
    case "optional":
    case "optionalMember":
      return nothing;
    default:
      return assertNever(members);
  }
}

function member(holder: Read, step: PathStep, members: PathMembers): Read {
  if (holder.kind === "nothing" || holder.value === null) return ofNothing(step, members);
  const box = boxed(holder.value);
  if (!(step in box)) return lacked(step, members);
  const held: unknown = Reflect.get(box, step);
  if (held instanceof Promise) throw new Stop("The member '" + String(step) + "' holds a promise where its declaration reads a value.");
  return value(held);
}

function boxed(held: unknown): object {
  if ((typeof held === "object" && held !== null) || typeof held === "function") return held;
  return Object(held) as object;
}

function ofNothing(step: PathStep, members: PathMembers): Read {
  switch (members) {
    case "required":
    case "optionalMember":
      throw new Stop("Nothing holds the member '" + String(step) + "'.");
    case "optional":
    case "requiredIfHeld":
      return nothing;
    default:
      return assertNever(members);
  }
}

function lacked(step: PathStep, members: PathMembers): Read {
  switch (members) {
    case "required":
    case "requiredIfHeld":
      throw new Stop("The member '" + String(step) + "' is not on the object that should hold it.");
    case "optional":
    case "optionalMember":
      return nothing;
    default:
      return assertNever(members);
  }
}
