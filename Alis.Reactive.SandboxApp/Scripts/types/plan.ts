import type { Vendor } from "./context";
import type { CoercionType } from "../core/coerce";
import type { Trigger } from "./triggers";
import type { Reaction } from "./reactions";

export interface Plan {
  planId: string;
  components: Record<string, ComponentEntry>;
  entries: Entry[];
  /** Set by inject.ts from container ID — used by mergePlan to deduplicate on partial reload. */
  sourceId?: string;
}

export interface ComponentEntry {
  id: string;
  vendor: Vendor;
  readExpr: string;
  componentType: string;
  /** Coercion type from C# typeof(TProp) — "string", "number", "boolean", "date", "array", "raw". */
  coerceAs: CoercionType;
}

export interface Entry {
  trigger: Trigger;
  reaction: Reaction;
}
