export interface Plan {
  version: 2;
  planId: string;
  sourceId?: string;
  contracts: Record<string, CapabilityContract>;
  objects: Record<string, RuntimeObject>;
  bindings: Record<string, FieldBinding>;
  workflows: Workflow[];
}

export type ContractKind = "component" | "element" | "event-object" | "service";
export type ContractResolver = "native-element" | "fusion-instance" | "event-object" | "context-object";

export interface CapabilityContract {
  kind: ContractKind;
  resolver: ContractResolver;
  members: Record<string, ContractMember>;
  events?: Record<string, EventContract>;
}

export type ContractMember = PropertyMember | MethodMember;

export interface PropertyMember {
  kind: "property";
  path: PathSegment[];
  shape: ValueShape;
  access: "read" | "write" | "readwrite";
}

export interface MethodMember {
  kind: "method";
  path: PathSegment[];
  args?: ValueShape[];
  returns?: ValueShape | "void";
}

export interface EventContract {
  channel: string;
  eventObject?: EventObjectReference;
  data?: Record<string, ValueExpr>;
}

export interface EventObjectReference {
  contract: string;
}

export interface RuntimeObject {
  contract: string;
  elementId?: string;
}

export interface FieldBinding {
  object: string;
  valueMember: string;
  shape: ValueShape;
}

export type ValueShape =
  | ScalarValueShape
  | ArrayValueShape
  | ObjectValueShape
  | AnyValueShape;

export interface ScalarValueShape {
  kind: "scalar";
  type: "string" | "number" | "boolean" | "date" | "raw";
}

export interface ArrayValueShape {
  kind: "array";
  item: ValueShape;
}

export interface ObjectValueShape {
  kind: "object";
  fields?: Record<string, ValueShape>;
  additional?: boolean;
}

export interface AnyValueShape {
  kind: "any";
}

export type ValueExpr =
  | LiteralValueExpr
  | BindingValueExpr
  | MemberValueExpr
  | ContextValueExpr
  | AccessValueExpr
  | ObjectValueExpr
  | ArrayValueExpr
  | ConvertValueExpr;

export interface LiteralValueExpr {
  kind: "literal";
  value?: unknown;
}

export interface BindingValueExpr {
  kind: "binding";
  binding: string;
}

export interface MemberValueExpr {
  kind: "member";
  object: string;
  member: string;
}

export interface ContextValueExpr {
  kind: "context";
  scope: "event" | "response" | "request" | "local";
  path?: PathSegment[];
}

export interface AccessValueExpr {
  kind: "access";
  value: ValueExpr;
  path: PathSegment[];
}

export interface ObjectValueExpr {
  kind: "object";
  fields: Record<string, ValueExpr>;
}

export interface ArrayValueExpr {
  kind: "array";
  items: ValueExpr[];
}

export interface ConvertValueExpr {
  kind: "convert";
  value: ValueExpr;
  to: ValueShape;
}

export interface BindingMapValueExpr {
  kind: "binding-map";
  include: "all" | string[];
}

export type RequestInputValue = ValueExpr | BindingMapValueExpr;

export type PlanPredicate =
  | ComparePredicate
  | AllPredicate
  | AnyPredicate
  | NotPredicate
  | ConfirmPredicate;

export interface ComparePredicate {
  kind: "compare";
  left: ValueExpr;
  op: CompareOp;
  right?: ValueExpr;
  as?: ValueShape;
  itemAs?: ValueShape;
}

export interface AllPredicate {
  kind: "all";
  terms: PlanPredicate[];
}

export interface AnyPredicate {
  kind: "any";
  terms: PlanPredicate[];
}

export interface NotPredicate {
  kind: "not";
  term: PlanPredicate;
}

export interface ConfirmPredicate {
  kind: "confirm";
  message: string;
}

export type CompareOp =
  | "eq" | "neq" | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy" | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export interface Workflow {
  when: PlanSubscription;
  run: PlanAction;
}

export type PlanSubscription =
  | DomReadySubscription
  | DocumentEventSubscription
  | ObjectEventSubscription
  | ServerPushSubscription
  | SignalRSubscription;

export interface DomReadySubscription {
  kind: "dom-ready";
}

export interface DocumentEventSubscription {
  kind: "document-event";
  name: string;
}

export interface ObjectEventSubscription {
  kind: "object-event";
  object: string;
  event: string;
}

export interface ServerPushSubscription {
  kind: "server-push";
  url: string;
  eventType?: string;
}

export interface SignalRSubscription {
  kind: "signalr";
  hubUrl: string;
  method: string;
}

export type PlanAction =
  | SequenceAction
  | BranchAction
  | ParallelAction
  | SetAction
  | CallAction
  | DispatchAction
  | RequestAction
  | InjectAction
  | ShowValidationErrorsAction;

export interface SequenceAction {
  kind: "sequence";
  steps: PlanAction[];
}

export interface BranchAction {
  kind: "branch";
  cases: BranchCase[];
}

export interface BranchCase {
  when?: PlanPredicate;
  run: PlanAction;
}

export interface ParallelAction {
  kind: "parallel";
  steps: PlanAction[];
  onSettled?: PlanAction;
}

export interface SetAction {
  kind: "set";
  target: ActionTarget;
  value: ValueExpr;
}

export interface CallAction {
  kind: "call";
  target: ActionTarget;
  args?: ValueExpr[];
}

export interface DispatchAction {
  kind: "dispatch";
  name: string;
  detail?: ValueExpr;
}

export interface RequestAction {
  kind: "request";
  request: RequestPlan;
}

export interface InjectAction {
  kind: "inject";
  object: string;
  value?: ValueExpr;
}

export interface ShowValidationErrorsAction {
  kind: "show-validation-errors";
  formId: string;
}

export interface ActionTarget {
  object: string;
  member: string;
}

export interface RequestPlan {
  method: "GET" | "POST" | "PUT" | "DELETE";
  url: string;
  input?: RequestInput;
  validation?: RequestValidation;
  before?: PlanAction[];
  onSuccess?: ResponseHandlerPlan[];
  onError?: ResponseHandlerPlan[];
  onSettled?: PlanAction[];
  next?: RequestPlan;
}

export interface RequestInput {
  transport: "query" | "json" | "form-data";
  value: RequestInputValue;
}

export interface RequestValidation {
  formId: string;
  fields: RequestValidationField[];
}

export interface RequestValidationField {
  binding: string;
  rules: RequestValidationRule[];
}

export type ValidationRuleName =
  | "required" | "empty"
  | "minLength" | "maxLength"
  | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange"
  | "min" | "max" | "gt" | "lt"
  | "equalTo" | "notEqual" | "notEqualTo"
  | "atLeastOne";

export interface RequestValidationRule {
  rule: ValidationRuleName;
  message: string;
  constraint?: unknown;
  otherBinding?: string;
  as?: ValueShape;
  when?: PlanPredicate;
}

export interface ResponseHandlerPlan {
  statusCode?: number;
  run: PlanAction;
}

export type PathSegment =
  | { prop: string; index?: never }
  | { index: number; prop?: never };
