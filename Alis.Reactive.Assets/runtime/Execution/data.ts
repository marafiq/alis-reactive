import { assertNever, type Absence, type DataStep } from "../Schema/index";
import { nothing, Stop, value, type Read } from "../ValuesAndSources/index";
import { isAnObject } from "./context";

export function read(target: Read, path: readonly DataStep[]): Read {
  const [step, ...past] = path;
  if (step === undefined) return target;
  if (target.kind === "nothing" || target.value === null) return nothing;
  const holder = target.value;
  if (!isAnObject(holder)) throw new Stop("The data that should hold '" + step.name + "' is not an object.");
  const lacking = step.requires.find((name) => !holds(holder, name));
  if (lacking !== undefined) throw new Stop("The data that holds '" + step.name + "' lacks '" + lacking + "', which its type requires.");
  if (!holds(holder, step.name)) return absent(step.absent, step.name);
  const held: unknown = Reflect.get(holder, step.name);
  if (held instanceof Promise) throw new Stop("The data's member '" + step.name + "' holds a promise where its type reads a value.");
  return read(value(held), past);
}

function holds(holder: object, name: string): boolean {
  const inheritedByEveryObject = name in Object.prototype && !Object.hasOwn(holder, name);
  return !inheritedByEveryObject && Reflect.get(holder, name) !== undefined;
}

function absent(absence: Absence, name: string): Read {
  switch (absence.kind) {
    case "start":
      return value(absence.value);
    case "nothing":
      return nothing;
    case "unbound":
      throw new Stop("The data lacks '" + name + "', and its binding cannot make the holder its start is read from.");
    default:
      return assertNever(absence);
  }
}
