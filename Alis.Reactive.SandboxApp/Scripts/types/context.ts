import type { ValidationDescriptor } from "./validation";
import type { ComponentEntry } from "./plan";

export type Vendor = "fusion" | "native";
export type CoercionType = "string" | "number" | "boolean" | "date" | "raw" | "array";
export type EventPayload = Record<string, unknown>;

export interface ExecContext {
  evt?: Record<string, unknown>;
  responseBody?: unknown;
  validationDesc?: ValidationDescriptor;
  components?: Record<string, ComponentEntry>;
}
