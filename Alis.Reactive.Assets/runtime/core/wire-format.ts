// wire-format.ts — Shape-aware wire formatting for HTTP transport.
// Date timestamps → ISO strings. Shared by gather, headers, route params.

import type { Shape } from "../types";
import { RuntimeShape } from "../domain/runtime-shape";

/** Shape-aware wire formatting. Date timestamps -> ISO strings for HTTP transport. */
export function formatForWire(value: unknown, shape: Shape): unknown {
  return RuntimeShape.from(shape).formatForWire(value);
}
