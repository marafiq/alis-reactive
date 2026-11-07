export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };

export type Absence = StartAbsence | NothingAbsence | UnboundAbsence;

export interface AnswerExpression {
  readonly kind: "answer";
  readonly arguments: string;
  readonly body: Expression;
}

export interface ArrayShape {
  readonly kind: "array";
  readonly item: Shape;
}

export interface AsExpression {
  readonly kind: "as";
  readonly expr: Expression;
  readonly shape: Shape;
}

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

export interface DataExpression {
  readonly kind: "data";
  readonly target: Expression;
  readonly path: readonly DataStep[];
}

export interface DataStep {
  readonly name: string;
  readonly requires: readonly string[];
  readonly absent: Absence;
}

export interface DateShape {
  readonly kind: "date";
}

export interface DoStatement {
  readonly kind: "do";
  readonly expr: Expression;
  readonly await: boolean;
}

export interface ElementLocator {
  readonly kind: "element";
  readonly id: string;
  readonly vendor: string;
}

export type Expression = LiteralExpression | ObjectExpression | RefExpression | GetExpression | DataExpression | CallExpression | FnExpression | AnswerExpression | OperatorExpression | AsExpression;

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
  readonly members: ReadPresence;
  readonly policy: Policy;
  readonly do: readonly Statement[];
}

export interface IfStatement {
  readonly kind: "if";
  readonly when: Expression;
  readonly then: readonly Statement[];
  readonly else: readonly Statement[];
}

export interface InstantShape {
  readonly kind: "instant";
}

export type LetBinding = "line" | "read";

export interface LetStatement {
  readonly kind: "let";
  readonly name: string;
  readonly expr: Expression;
  readonly await: boolean;
  readonly binds: LetBinding;
}

export type Listening = MethodListening | AssignmentListening | ReturnedListening;

export interface LiteralExpression {
  readonly kind: "lit";
  readonly value: JsonValue;
}

export type Locator = ElementLocator | PluginLocator;

export interface MethodListening {
  readonly kind: "method";
  readonly subscribe: string;
  readonly unsubscribe: string;
}

export interface NothingAbsence {
  readonly kind: "nothing";
}

export interface NullableShape {
  readonly kind: "nullable";
  readonly inner: Shape;
}

export interface ObjectExpression {
  readonly kind: "obj";
  readonly id: string;
}

export interface ObjectShape {
  readonly kind: "object";
  readonly fields: { readonly [key: string]: Shape };
}

export type Operator = "eq" | "neq" | "lt" | "lte" | "gt" | "gte" | "and" | "or" | "not" | "trim" | "trimStart" | "trimEnd" | "upperInvariant" | "lowerInvariant" | "upper" | "lower" | "upperTurkic" | "lowerTurkic" | "contains" | "startsWith" | "endsWith" | "nullOrEmpty" | "includes" | "sumInt" | "sumDouble" | "sumDecimal" | "compareNumbers" | "compareOrdinal" | "compareBooleans" | "compareCulture" | "coalesce" | "cond" | "nullableValue" | "matches";

export interface OperatorExpression {
  readonly kind: "op";
  readonly op: Operator;
  readonly args: readonly Expression[];
}

export type PathStep = string | number;

export interface PlanCulture {
  readonly collation: string;
}

export interface PlanDocument {
  readonly version: number;
  readonly planId: string;
  readonly culture: PlanCulture;
  readonly vendors: { readonly [key: string]: Vendor };
  readonly objects: { readonly [key: string]: Locator };
  readonly handlers: readonly Handler[];
}

export interface PluginLocator {
  readonly kind: "plugin";
  readonly name: string;
  readonly listen: Listening;
}

export type Policy = "overlap" | "replace" | "skip";

export type Presence = "required" | "optional";

export type ReadPresence = "required" | "optional" | "requiredIfHeld" | "optionalMember";

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

export type Shape = DateShape | InstantShape | NullableShape | ArrayShape | ObjectShape;

export interface StartAbsence {
  readonly kind: "start";
  readonly value: JsonValue;
}

export type Statement = SetStatement | DoStatement | LetStatement | IfStatement;

export interface UnboundAbsence {
  readonly kind: "unbound";
}

export interface Vendor {
  readonly locate: Expression;
  readonly listen: Listening;
}
