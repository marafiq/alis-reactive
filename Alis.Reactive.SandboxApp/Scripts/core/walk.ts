import type { PathSegment } from "../types";

export function walk(root: unknown, path: string): unknown {
  if (!path) return root;
  return walkSegments(root, path.split(".").map(prop => ({ prop })));
}

export function walkSegments(root: unknown, path: PathSegment[] | undefined): unknown {
  if (!path || path.length === 0) return root;

  let current: any = root;
  for (const segment of path) {
    if (current == null) return undefined;
    current = "prop" in segment && segment.prop !== undefined
      ? current[segment.prop]
      : current[segment.index];
  }
  return current;
}

export function setSegments(root: unknown, path: PathSegment[], value: unknown): void {
  if (path.length === 0) {
    throw new Error("[alis] cannot assign to an empty member path");
  }

  const parent = walkSegments(root, path.slice(0, -1)) as any;
  if (parent == null) {
    throw new Error("[alis] cannot assign through an unresolved member path");
  }

  const last = path[path.length - 1];
  if ("prop" in last && last.prop !== undefined) {
    parent[last.prop] = value;
    return;
  }

  parent[last.index] = value;
}

export function resolveCallable(root: unknown, path: PathSegment[]): { owner: any; fn: (...args: unknown[]) => unknown } {
  if (path.length === 0) {
    throw new Error("[alis] cannot call an empty member path");
  }

  const owner = walkSegments(root, path.slice(0, -1)) as any;
  if (owner == null) {
    throw new Error("[alis] cannot resolve callable owner");
  }

  const last = path[path.length - 1];
  const fn = ("prop" in last && last.prop !== undefined)
    ? owner[last.prop]
    : owner[last.index];

  if (typeof fn !== "function") {
    throw new Error("[alis] target member is not callable");
  }

  return { owner, fn: fn as (...args: unknown[]) => unknown };
}
