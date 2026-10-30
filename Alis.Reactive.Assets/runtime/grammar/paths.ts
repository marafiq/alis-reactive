// Paths (grammar.md 1.3). A member path rooted at a located object is checked: every segment must
// exist, so a vendor version without a declared member fails loudly. Every other path is optional,
// as JavaScript's `?.` is. A write rooted at a located object needs its final member; any other write
// needs only the holder of its final member, and creates that member when absent (a payload's `cancel`).
// A read's "[]" segment reads the rest of the path on each item of the list before it, in the same
// mode: an item of a located object's list is checked like the object.

import { toArray } from "./operators";
import { isMissing } from "./shape";

const EACH_ITEM = "[]";

/** A value that can hold members: an object or a function. */
type Holder = Record<string | number, unknown>;

export function canHaveMembers(value: unknown): value is Holder {
  return (typeof value === "object" && value !== null) || typeof value === "function";
}

export type PathMode = "checked" | "optional";

export function read(root: unknown, path: readonly string[], mode: PathMode, label: string): unknown {
  let current = root;
  for (let index = 0; index < path.length; index++) {
    const key = path[index]!;
    if (key === EACH_ITEM) return eachItem(current, path, index, mode, label);
    if (mode === "optional") {
      if (isMissing(current)) return undefined;
      current = (current as Holder)[key];
      continue;
    }
    const holder = requireHolder(current, label, path, index);
    if (!(key in holder)) throw missingMember(label, path, index);
    current = holder[key];
  }
  return current;
}

function eachItem(list: unknown, path: readonly string[], index: number, mode: PathMode, label: string): unknown[] {
  const listPath = path.slice(0, index).join(".");
  const itemLabel = listPath === "" ? `${label}[]` : `${label}.${listPath}[]`;
  const rest = path.slice(index + 1);
  return toArray(list).map(item => read(item, rest, mode, itemLabel));
}

export function write(root: unknown, path: readonly string[], value: unknown, mode: PathMode, label: string): void {
  const last = path.length - 1;
  const holder = requireHolder(read(root, path.slice(0, last), mode, label), label, path, last);
  const key = path[last]!;
  if (mode === "checked" && !(key in holder)) throw missingMember(label, path, last);
  holder[key] = value;
}

export function call(
  root: unknown,
  path: readonly string[],
  args: readonly unknown[],
  mode: PathMode,
  label: string,
): unknown {
  if (path.length === 0) {
    if (mode === "optional" && isMissing(root)) return undefined;
    if (typeof root !== "function") throw new Error(`[alis] ${label} is not a function`);
    return (root as (...values: unknown[]) => unknown)(...args);
  }

  const last = path.length - 1;
  const holderValue = read(root, path.slice(0, last), mode, label);
  if (mode === "optional" && isMissing(holderValue)) return undefined;
  // An optional target may be a primitive: "text".includes(..) is a call on a string.
  const holder = mode === "optional" ? (holderValue as Holder) : requireHolder(holderValue, label, path, last);
  const method = holder[path[last]!];
  if (mode === "optional" && isMissing(method)) return undefined;
  if (typeof method !== "function") {
    throw new Error(`[alis] ${label}.${path.join(".")} is not a function`);
  }
  return (method as (...values: unknown[]) => unknown).apply(holder, [...args]);
}

function requireHolder(value: unknown, label: string, path: readonly string[], index: number): Holder {
  if (canHaveMembers(value)) return value;
  const at = path.slice(0, index).join(".");
  throw new Error(`[alis] ${label}${at ? "." + at : ""} is ${value === null ? "null" : typeof value}, not an object`);
}

function missingMember(label: string, path: readonly string[], index: number): Error {
  return new Error(`[alis] ${label} has no member "${path.slice(0, index + 1).join(".")}"`);
}
