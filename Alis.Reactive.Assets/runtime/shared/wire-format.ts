import type { Shape } from "../types/index";
import { RuntimeShape } from "../browser-objects/runtime-shape";

export function formatForWire(value: unknown, shape: Shape): unknown {
  return RuntimeShape.from(shape).formatForWire(value);
}
