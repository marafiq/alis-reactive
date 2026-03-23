import type { BindSource } from "./sources";
import type { CoercionType } from "../core/coerce";

export type GuardOp =
  | "eq" | "neq"
  | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy"
  | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export type Guard = ValueGuard | AllGuard | AnyGuard | InvertGuard | ConfirmGuard;

export interface ValueGuard {
  kind: "value";
  source: BindSource;
  coerceAs: CoercionType;
  op: GuardOp;
  operand?: unknown;
  rightSource?: BindSource;
  elementCoerceAs?: CoercionType;
}

export interface AllGuard {
  kind: "all";
  guards: Guard[];
}

export interface AnyGuard {
  kind: "any";
  guards: Guard[];
}

export interface InvertGuard {
  kind: "not";
  inner: Guard;
}

export interface ConfirmGuard {
  kind: "confirm";
  message: string;
}
