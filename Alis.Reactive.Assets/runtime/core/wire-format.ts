// wire-format.ts — Shape-aware wire formatting for HTTP transport.
// Date timestamps → ISO strings. Shared by gather, headers, route params.

import type { Shape } from "../types";

/** Shape-aware wire formatting. Date timestamps -> ISO strings for HTTP transport. */
export function formatForWire(value: unknown, shape: Shape): unknown {
  if (shape.kind === "none") return value;
  if (shape.kind === "date" && typeof value === "number" && !isNaN(value))
    return new Date(value).toISOString();
  if (shape.kind === "nullable" && shape.inner.kind === "date" && typeof value === "number" && !isNaN(value))
    return new Date(value).toISOString();
  return value;
}
