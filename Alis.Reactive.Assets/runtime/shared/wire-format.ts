import type { Shape } from "../types/index";
import { RuntimeShape } from "../browser-objects/runtime-shape";

/** Formats values for HTTP transport according to their declared shape. */
export function formatForWire(value: unknown, shape: Shape): unknown {
  return RuntimeShape.from(shape).formatForWire(value);
}
