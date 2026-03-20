import type { CoercionType } from "../core/coerce";
import type { Vendor } from "./context";

export interface ValidationDescriptor {
  formId: string;
  planId?: string;
  fields: ValidationField[];
}

export interface ValidationField {
  fieldName: string;
  rules: ValidationRule[];
  // Enriched at boot from plan.components:
  fieldId?: string;
  vendor?: Vendor;
  readExpr?: string;
}

export type ValidationRuleType =
  | "required" | "empty"
  | "minLength" | "maxLength"
  | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange"
  | "min" | "max" | "gt" | "lt"
  | "equalTo" | "notEqual" | "notEqualTo"
  | "atLeastOne";

export interface ValidationRule {
  rule: ValidationRuleType;
  message: string;
  constraint?: unknown;
  field?: string;
  coerceAs?: CoercionType;
  when?: ValidationCondition;
}

export interface ValidationCondition {
  field: string;
  op: "truthy" | "falsy" | "eq" | "neq";
  value?: unknown;
}
