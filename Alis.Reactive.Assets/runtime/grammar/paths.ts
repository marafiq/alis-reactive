// Paths (grammar.md 1.3). A member path rooted at a located object is checked: every segment must
// exist, so a vendor version without a declared member fails loudly. Every other path is optional,
// as JavaScript's `?.` is. A write rooted at a located object needs its final member; any other write
// needs only the owner, and creates the final member when absent (a payload's `cancel`).
// A read's "[]" segment reads the rest of the path on each item of the list before it, in the same
// mode: an item of a located object's list is checked like the object.

import { toArray } from "./operators";
import { isMissing } from "./shape";

const EACH_ITEM = "[]";

type Owner = Record<string | number, unknown>;

export type PathMode = "checked" | "optional";

export function read(root: unknown, path: readonly string[], mode: PathMode, label: string): unknown {
  let current = root;
  for (let index = 0; index < path.length; index++) {
    const key = path[index]!;
    if (key === EACH_ITEM) return eachItem(current, path, index, mode, label);
    if (mode === "optional") {
      if (isMissing(current)) return undefined;
      current = (current as Owner)[key];
      continue;
    }
    const owner = requireOwner(current, label, path, index);
    if (!(key in owner)) throw missingMember(label, path, index);
    current = owner[key];
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
  if (path.length === 0) throw new Error(`[alis] a write needs a member: ${label}`);
  const last = path.length - 1;
  const owner = requireOwner(read(root, path.slice(0, last), mode, label), label, path, last);
  const key = path[last]!;
  if (mode === "checked" && !(key in owner)) throw missingMember(label, path, last);
  owner[key] = value;
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
  const ownerValue = read(root, path.slice(0, last), mode, label);
  if (mode === "optional" && isMissing(ownerValue)) return undefined;
  // An optional receiver may be a primitive: "text".includes(..) is a call on a string.
  const owner = mode === "optional" ? (ownerValue as Owner) : requireOwner(ownerValue, label, path, last);
  const method = owner[path[last]!];
  if (mode === "optional" && isMissing(method)) return undefined;
  if (typeof method !== "function") {
    throw new Error(`[alis] ${label}.${path.join(".")} is not a function`);
  }
  return (method as (...values: unknown[]) => unknown).apply(owner, [...args]);
}

function requireOwner(value: unknown, label: string, path: readonly string[], index: number): Owner {
  const canHaveMembers = (typeof value === "object" && value !== null) || typeof value === "function";
  if (canHaveMembers) return value as Owner;
  const at = path.slice(0, index).join(".");
  throw new Error(`[alis] ${label}${at ? "." + at : ""} is ${value === null ? "null" : typeof value}, not an object`);
}

function missingMember(label: string, path: readonly string[], index: number): Error {
  return new Error(`[alis] ${label} has no member "${path.slice(0, index + 1).join(".")}"`);
}
