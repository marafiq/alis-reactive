export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };

export interface AssignmentListening {
  readonly kind: "assignment";
}

export interface CallExpression {
  readonly kind: "call";
  readonly target: Expression;
  readonly path: readonly PathStep[];
  readonly members: ReadPresence;
  readonly args: readonly Expression[];
}

export interface ElementLocator {
  readonly kind: "element";
  readonly id: string;
  readonly vendor: string;
}

export type Expression = LiteralExpression | ObjectExpression | RefExpression | GetExpression | CallExpression | FnExpression | OperatorExpression;

export interface FnExpression {
  readonly kind: "fn";
  readonly params: readonly string[];
  readonly body: Expression;
}

export interface GetExpression {
  readonly kind: "get";
  readonly target: Expression;
  readonly path: readonly PathStep[];
  readonly members: ReadPresence;
}

export interface Handler {
  readonly on: string;
  readonly path: readonly PathStep[];
  readonly event: string;
  readonly payload: readonly PathStep[];
  readonly policy: Policy;
  readonly do: readonly Statement[];
}

export type Listening = MethodListening | AssignmentListening | ReturnedListening;

export interface LiteralExpression {
  readonly kind: "lit";
  readonly value: JsonValue;
}

export type Locator = ElementLocator;

export interface MethodListening {
  readonly kind: "method";
  readonly subscribe: string;
  readonly unsubscribe: string;
}

export interface ObjectExpression {
  readonly kind: "obj";
  readonly id: string;
}

export type Operator = "eq" | "neq" | "lt" | "lte" | "gt" | "gte" | "and" | "or" | "not" | "trim" | "trimStart" | "trimEnd" | "upperInvariant" | "lowerInvariant" | "upper" | "lower" | "upperTurkic" | "lowerTurkic" | "contains" | "startsWith" | "endsWith" | "nullOrEmpty" | "includes" | "sumInt" | "sumDouble" | "sumDecimal";

export interface OperatorExpression {
  readonly kind: "op";
  readonly op: Operator;
  readonly args: readonly Expression[];
}

export type PathStep = string | number;

export interface PlanDocument {
  readonly version: number;
  readonly planId: string;
  readonly vendors: { readonly [key: string]: Vendor };
  readonly objects: { readonly [key: string]: Locator };
  readonly handlers: readonly Handler[];
}

export type Policy = "overlap" | "replace" | "skip";

export type Presence = "required" | "optional";

export type ReadPresence = "required" | "optional" | "requiredIfHeld";

export interface RefExpression {
  readonly kind: "ref";
  readonly name: string;
}

export interface ReturnedListening {
  readonly kind: "returned";
  readonly subscribe: string;
}

export interface SetStatement {
  readonly kind: "set";
  readonly target: Expression;
  readonly path: readonly PathStep[];
  readonly members: Presence;
  readonly value: Expression;
}

export type Statement = SetStatement;

export interface Vendor {
  readonly locate: Expression;
  readonly listen: Listening;
}
