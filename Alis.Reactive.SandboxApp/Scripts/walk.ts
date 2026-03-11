import { scope } from "./trace";

const log = scope("walk");

/**
 * Walks a dot-notation path on any object.
 * Shared primitive used by component reads (readExpr),
 * bind expression resolution (BindExpr), and source evaluation.
 *
 * Examples:
 *   walk(el, "checked")           -> el.checked
 *   walk(ctx, "evt.address.city") -> ctx.evt.address.city
 *   walk(root, "value")           -> root.value
 */
export function walk(root: unknown, path: string): unknown {
  const parts = path.split(".");
  let current: any = root;
  for (const part of parts) {
    if (current == null) return undefined;
    current = current[part];
  }
  log.trace("walk", { path, result: current });
  return current;
}
