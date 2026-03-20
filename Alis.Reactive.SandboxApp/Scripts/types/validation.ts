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
  | "required" | "minLength" | "maxLength" | "email" | "regex" | "url"
  | "range" | "min" | "max" | "gt" | "lt" | "equalTo" | "atLeastOne";

export interface ValidationRule {
  rule: ValidationRuleType;
  message: string;
  constraint?: boolean | number | string | [number, number];
  when?: ValidationCondition;
}

export interface ValidationCondition {
  field: string;
  op: "truthy" | "falsy" | "eq" | "neq";
  value?: unknown;
}
