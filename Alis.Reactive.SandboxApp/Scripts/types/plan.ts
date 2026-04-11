// ────────────────────────────────────────────────────────────────
// V3 Reactive Plan types — generated from reactive-plan.schema.json
// Every type matches a $defs entry exactly.
// ────────────────────────────────────────────────────────────────

// ── Plan (top-level) ──────────────────────────────────────────

export interface Plan {
  version: 3;
  planId: string;
  partId?: string;
  types: Record<string, JsType>;
  components: Record<string, Component>;
  behaviors: Behavior[];
}

// ── JsType ────────────────────────────────────────────────────

export interface JsType {
  properties?: Record<string, Property>;
  methods?: Record<string, Method>;
  events?: Record<string, Event>;
}

export interface Property {
  path: Path;
  shape: Shape;
  access: "read" | "write" | "readwrite";
}

export interface Method {
  path: Path;
  args?: Shape[];
  returns?: Shape;
}

export interface Event {
  channel: string;
  payloadType?: string;
}


// ── Component ─────────────────────────────────────────────────

export type Vendor = "native" | "fusion";

export interface Component {
  id: string;
  vendor: Vendor;
  type: string;
  bindingPath?: string;
  valueMember?: string;
  container?: ContainerScope;
}

export interface ContainerScope {
  components: string[];
  validationRules?: ComponentValidation[];
}

export interface ComponentValidation {
  component: string;
  value: ValueProducer;
  serverFieldName?: string;
  rules: ValidationRule[];
}

export type ValidationRuleName =
  | "required" | "empty"
  | "minLength" | "maxLength"
  | "email" | "regex" | "url" | "creditCard"
  | "range" | "exclusiveRange"
  | "min" | "max" | "gt" | "lt"
  | "equalTo" | "notEqual" | "notEqualTo"
  | "atLeastOne";

export interface ValidationRule {
  name: ValidationRuleName;
  message: string;
  constraint?: ValueProducer;
  otherValue?: ValueProducer;
  when?: Condition;
  shape?: Shape;
}

// ── Source ─────────────────────────────────────────────────────

export type Source = ComponentSource | PayloadSource;

export interface ComponentSource {
  kind: "component";
  component: string;
}

export type PayloadScope = "event" | "success" | "error" | "request" | "dispatch" | "local";

export interface PayloadSource {
  kind: "payload";
  scope: PayloadScope;
  type?: string;
}

// ── Behavior ──────────────────────────────────────────────────

export interface Behavior {
  startsWhen: StartsWhen;
  reaction: Reaction;
}

// ── StartsWhen (triggers) ─────────────────────────────────────

export type StartsWhen =
  | PageReadyTrigger
  | DocumentEventTrigger
  | ComponentEventTrigger
  | ServerPushTrigger
  | SignalRTrigger;

export interface PageReadyTrigger {
  kind: "page-ready";
}

export interface DocumentEventTrigger {
  kind: "document-event";
  event: string;
  payloadType?: string;
}

export interface ComponentEventTrigger {
  kind: "component-event";
  component: string;
  event: string;
}

export interface ServerPushTrigger {
  kind: "server-push";
  url: string;
  event?: string;
  payloadType?: string;
}

export interface SignalRTrigger {
  kind: "signalr";
  hubUrl: string;
  method: string;
  payloadType?: string;
}

// ── Reaction (discriminated union — 9 kinds) ──────────────────

export type Reaction =
  | SequenceReaction
  | ParallelReaction
  | BranchReaction
  | SetReaction
  | CallReaction
  | RequestReaction
  | DispatchReaction
  | InjectReaction
  | ShowValidationErrorsReaction;

export interface SequenceReaction {
  kind: "sequence";
  steps: Reaction[];
}

export interface ParallelReaction {
  kind: "parallel";
  steps: Reaction[];
  onSettled?: Reaction;
}

export interface BranchReaction {
  kind: "branch";
  cases: BranchCase[];
}

export interface BranchCase {
  when?: Condition;
  reaction: Reaction;
}

export interface SetReaction {
  kind: "set";
  on: Source;
  property: string;
  value: ValueProducer;
}

export interface CallReaction {
  kind: "call";
  on: Source;
  method: string;
  args?: ValueProducer[];
}

export interface RequestReaction {
  kind: "request";
  request: Request;
}

export interface DispatchReaction {
  kind: "dispatch";
  event: string;
  data?: ValueProducer;
  payloadType?: string;
}

export interface InjectReaction {
  kind: "inject";
  component: string;
  value: ValueProducer;
}

export interface ShowValidationErrorsReaction {
  kind: "show-validation-errors";
  container: string;
}

// ── Request ───────────────────────────────────────────────────

export type HttpMethod = "GET" | "POST" | "PUT" | "DELETE" | "PATCH";

export interface Request {
  method: HttpMethod;
  url: string;
  headers?: Record<string, ValueProducer>;
  container?: string;
  input?: RequestInput;
  before?: Reaction[];
  success?: ResponseHandler[];
  error?: ResponseHandler[];
  complete?: Reaction[];
  next?: Request;
}

export type RequestInput = GatherInput | ValueInput;

export type Transport = "query" | "json" | "form-data";

export interface GatherInput {
  kind: "gather";
  components: GatherField[];
  transport: Transport;
  statics?: ValueProducer;
  includeAll?: boolean;
}

export interface ValueInput {
  kind: "value";
  value: ValueProducer;
  transport: Transport;
}

export interface GatherField {
  key: string;
  value: ValueProducer;
}

export interface ResponseHandler {
  status?: number;
  reaction: Reaction;
}

// ── ValueProducer (discriminated union — 4 kinds) ─────────────

export type ValueProducer =
  | LiteralProducer
  | ReadProducer
  | ObjectProducer
  | ArrayProducer;

export interface LiteralProducer {
  kind: "literal";
  value: string | number | boolean | null;
  shape?: Shape;
}

export interface ReadProducer {
  kind: "read";
  from: Source;
  member: string;
  path?: Path;
  shape?: Shape;
}

export interface ObjectProducer {
  kind: "object";
  fields: Record<string, ValueProducer>;
  shape?: Shape;
}

export interface ArrayProducer {
  kind: "array";
  items: ValueProducer[];
  shape?: Shape;
}

// ── Condition (discriminated union — 5 kinds) ─────────────────

export type Condition =
  | CompareCondition
  | AllCondition
  | AnyCondition
  | NotCondition
  | ConfirmCondition;

export type CompareOp =
  | "eq" | "neq" | "gt" | "gte" | "lt" | "lte"
  | "truthy" | "falsy" | "is-null" | "not-null"
  | "is-empty" | "not-empty"
  | "in" | "not-in" | "between"
  | "array-contains"
  | "contains" | "starts-with" | "ends-with" | "matches" | "min-length";

export interface CompareCondition {
  kind: "compare";
  left: ValueProducer;
  op: CompareOp;
  right?: ValueProducer;
  shape?: Shape;
  itemShape?: Shape;
}

export interface AllCondition {
  kind: "all";
  terms: Condition[];
}

export interface AnyCondition {
  kind: "any";
  terms: Condition[];
}

export interface NotCondition {
  kind: "not";
  term: Condition;
}

export interface ConfirmCondition {
  kind: "confirm";
  message: string;
}

// ── Shape (discriminated union — 9 kinds) ─────────────────────

export type Shape =
  | StringShape
  | NumberShape
  | BooleanShape
  | DateShape
  | RawShape
  | ArrayShape
  | ObjectShape
  | NullableShape
  | AnyShape
  | NoneShape;

export interface StringShape {
  kind: "string";
}

export interface NumberShape {
  kind: "number";
}

export interface BooleanShape {
  kind: "boolean";
}

export interface DateShape {
  kind: "date";
}

export interface RawShape {
  kind: "raw";
}

export interface ArrayShape {
  kind: "array";
  item: Shape;
}

export interface ObjectShape {
  kind: "object";
  fields?: Record<string, Shape>;
  additional?: boolean;
}

export interface NullableShape {
  kind: "nullable";
  inner: Shape;
}

export interface AnyShape {
  kind: "any";
}

export interface NoneShape {
  kind: "none";
}

// ── Path ──────────────────────────────────────────────────────

export type Path = PathSegment[];

export type PathSegment = PropertySegment | IndexSegment;

export interface PropertySegment {
  kind: "property";
  name: string;
}

export interface IndexSegment {
  kind: "index";
  index: number;
}
