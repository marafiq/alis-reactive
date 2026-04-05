// core/walk.ts — Path walking primitives.
// Two flavors:
//   walkPath(root, Path)  — structured path segments (V3 plan-driven)
//   walk(root, string)    — dot-notation string (legacy, used in tests/validation)

import type { Path, PathSegment } from "../types";

/**
 * Walk a structured Path (array of PathSegment) on any object.
 * Used by the shared resolver for property reads, writes, and method calls.
 */
export function walkPath(root: unknown, path: Path): unknown {
  let current: any = root;
  for (const seg of path) {
    if (current == null) return undefined;
    switch (seg.kind) {
      case "property":
        current = current[seg.name];
        break;
      case "index":
        current = current[seg.index];
        break;
    }
  }
  return current;
}

/**
 * Walk a structured Path but stop one segment short.
 * Returns { owner, key } so the caller can assign or call on the final segment.
 */
export function walkPathParent(root: unknown, path: Path): { owner: any; key: string | number } {
  if (path.length === 0) throw new Error("[alis] walkPathParent: empty path");
  let current: any = root;
  for (let i = 0; i < path.length - 1; i++) {
    const seg = path[i];
    if (current == null) throw new Error(`[alis] walkPathParent: null at segment ${i}`);
    switch (seg.kind) {
      case "property":
        current = current[seg.name];
        break;
      case "index":
        current = current[seg.index];
        break;
    }
  }
  const last = path[path.length - 1];
  const key = last.kind === "property" ? last.name : last.index;
  return { owner: current, key };
}

/**
 * Walks a dot-notation path on any object.
 * Kept for backward compatibility with validation condition readers
 * and tests that use string paths.
 */
export function walk(root: unknown, path: string): unknown {
  const parts = path.split(".");
  let current: any = root;
  for (const part of parts) {
    if (current == null) return undefined;
    current = current[part];
  }
  return current;
}
