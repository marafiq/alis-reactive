import type { DataStep } from "./plan";
import { assertNever, nothing, Stop, value, type Read } from "./read";

export function readData(start: Read, path: readonly DataStep[]): Read {
  let read = start;
  for (const step of path) {
    if (read.kind === "nothing" || read.value === null) return nothing;
    const holder = objectHolding(read.value, step);
    const missing = step.requires.find((name) => !holds(holder, name));
    if (missing !== undefined) throw new Stop("The data lacks the member '" + missing + "', which its type requires.");
    if (!holds(holder, step.name)) return absent(step);
    const held: unknown = Reflect.get(holder, step.name);
    if (held instanceof Promise) throw new Stop("The member '" + step.name + "' holds a promise where a value was declared.");
    read = value(held);
  }
  return read;
}

function objectHolding(held: unknown, step: DataStep): object {
  const isObject = typeof held === "object" && held !== null && !Array.isArray(held);
  if (!isObject) throw new Stop("The member '" + step.name + "' is read on a value that is not an object.");
  return held;
}

function holds(holder: object, name: string): boolean {
  const own = Object.hasOwn(holder, name);
  const fromItsClass = !own && name in holder && !(name in Object.prototype);
  return (own || fromItsClass) && Reflect.get(holder, name) !== undefined;
}

function absent(step: DataStep): Read {
  const absence = step.absent;
  switch (absence.kind) {
    case "start":
      return value(absence.value);
    case "nothing":
      return nothing;
    case "unbound":
      throw new Stop("The data lacks the member '" + step.name + "', and the endpoint binds no one value for it.");
    default:
      return assertNever(absence);
  }
}
