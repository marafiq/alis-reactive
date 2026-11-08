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

export type BodyKind = "html" | "json";

export interface BooleanForm {
  readonly kind: "boolean";
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

export interface DateForm {
  readonly kind: "date";
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

export type Expression = LiteralExpression | ObjectExpression | RefExpression | GetExpression | DataExpression | CallExpression | FnExpression | AnswerExpression | OperatorExpression | AsExpression | RecordExpression | ListExpression;

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

export interface HeldRelease {
  readonly kind: "held";
  readonly vendor: string;
  readonly element: readonly PathStep[];
}

export type HttpMethod = "GET";

export type HttpOutcome = "success" | "status" | "network" | "redirect" | "kind";

export interface HttpResponse {
  readonly ok: boolean;
  readonly status: number;
  readonly outcome: HttpOutcome;
  readonly body: JsonValue;
}

export interface HttpSend {
  readonly method: HttpMethod;
  readonly path: readonly PathPart[];
  readonly search: string;
  readonly query: readonly QueryField[];
  readonly success: BodyKind;
}

export interface IfStatement {
  readonly kind: "if";
  readonly when: Expression;
  readonly then: readonly Statement[];
  readonly else: readonly Statement[];
}

export interface InstantForm {
  readonly kind: "instant";
}

export interface InstantShape {
  readonly kind: "instant";
}

export type ItemWhenNothing = "empty" | "stop";

export type LetBinding = "line" | "read";

export interface LetStatement {
  readonly kind: "let";
  readonly name: string;
  readonly expr: Expression;
  readonly await: boolean;
  readonly binds: LetBinding;
}

export interface ListExpression {
  readonly kind: "list";
  readonly items: readonly Expression[];
}

export interface ListForm {
  readonly kind: "list";
  readonly item: ScalarKind;
  readonly itemWhenNothing: ItemWhenNothing;
}

export interface ListenStatement {
  readonly kind: "listen";
  readonly object: Expression;
  readonly event: string;
  readonly listen: Listening;
  readonly release: Release;
  readonly payload: readonly PathStep[];
  readonly members: ReadPresence;
  readonly policy: Policy;
  readonly payloadName: string;
  readonly argumentsName: string;
  readonly do: readonly Statement[];
}

export type Listening = MethodListening | AssignmentListening | ReturnedListening;

export interface LiteralExpression {
  readonly kind: "lit";
  readonly value: JsonValue;
}

export interface LivingRelease {
  readonly kind: "living";
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

export interface NumberForm {
  readonly kind: "number";
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

export type PathPart = TextPart | ValuePart;

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
  readonly key: string;
  readonly listen: Listening;
}

export type Policy = "overlap" | "replace" | "skip";

export type Presence = "required" | "optional";

export interface QueryField {
  readonly name: string;
  readonly value: JsonValue;
  readonly form: WireForm;
  readonly whenNothing: WhenNothing;
}

export type ReadPresence = "required" | "optional" | "requiredIfHeld" | "optionalMember";

export interface RecordExpression {
  readonly kind: "record";
  readonly fields: readonly RecordField[];
}

export interface RecordField {
  readonly name: string;
  readonly value: Expression;
}

export interface RefExpression {
  readonly kind: "ref";
  readonly name: string;
}

export type Release = LivingRelease | HeldRelease;

export interface ReturnedListening {
  readonly kind: "returned";
  readonly subscribe: string;
}

export type ScalarKind = "text" | "number" | "boolean" | "date" | "instant" | "written";

export interface SetStatement {
  readonly kind: "set";
  readonly target: Expression;
  readonly path: readonly PathStep[];
  readonly members: Presence;
  readonly value: Expression;
  readonly content: boolean;
}

export type Shape = DateShape | InstantShape | NullableShape | ArrayShape | ObjectShape;

export interface StartAbsence {
  readonly kind: "start";
  readonly value: JsonValue;
}

export type Statement = SetStatement | DoStatement | LetStatement | IfStatement | ListenStatement;

export interface TextForm {
  readonly kind: "text";
}

export interface TextPart {
  readonly kind: "text";
  readonly text: string;
}

export interface UnboundAbsence {
  readonly kind: "unbound";
}

export interface ValuePart {
  readonly kind: "value";
  readonly name: string;
  readonly value: JsonValue;
  readonly form: WireForm;
}

export interface Vendor {
  readonly locate: Expression;
  readonly listen: Listening;
}

export type WhenNothing = "leave" | "empty";

export type WireForm = TextForm | NumberForm | BooleanForm | DateForm | InstantForm | WrittenForm | ListForm;

export interface WrittenForm {
  readonly kind: "written";
}
