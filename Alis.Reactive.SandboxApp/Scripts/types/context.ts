import type { ValidationDescriptor } from "./validation";
import type { ComponentEntry } from "./plan";

export type Vendor = "fusion" | "native";
export type EventPayload = Record<string, unknown>;

export interface ExecContext {
  readonly evt?: Record<string, unknown>;
  readonly responseBody?: unknown;
  readonly validationDesc?: ValidationDescriptor;
  readonly components?: Record<string, ComponentEntry>;
}
