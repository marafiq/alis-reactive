// Paths (grammar.md 1.3). A member path rooted at a located object is checked: every segment must
// exist, so a vendor version without a declared member fails loudly. Every other path is optional,
// as JavaScript's `?.` is. A write rooted at a located object needs its final member; any other write
// needs only the owner, and creates the final member when absent (a payload's `cancel`).

import { isMissing } from "./shape";

type Owner = Record<string | number, unknown>;

export type PathMode = "checked" | "optional";

export function read(root: unknown, path: readonly string[], mode: PathMode, label: string): unknown {
  let current = root;
  for (let index = 0; index < path.length; index++) {
    const key = path[index]!;
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
