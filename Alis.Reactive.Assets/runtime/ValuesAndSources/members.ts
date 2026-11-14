import { assertNever, type PathMembers, type PathStep } from "../Schema/index";
import { nothing, Stop, value, type Read } from "./read";

export function got(target: Read, path: readonly PathStep[], members: PathMembers): Read {
  return path.reduce((holder, step) => member(holder, step, members), target);
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
