// wire-format.ts — Shape-aware formatting at the HTTP transport boundary.
//
// Runtime convention: shape-applied date values are Date objects. For HTTP
// headers, route params, and query strings (which must be strings), a Date is
// formatted to ISO. JSON body serialization handles Date.toJSON automatically,
// so formatForWire only matters where the destination is string-shaped.

import type { Shape } from "../types";

/** Shape-aware wire formatting. Date objects -> ISO strings for HTTP transport. */
export function formatForWire(value: unknown, shape: Shape): unknown {
  if (shape.kind === "none") return value;
  if (value instanceof Date) return value.toISOString();
  if (shape.kind === "nullable" && shape.inner.kind === "date" && value == null) return null;
  return value;
}
