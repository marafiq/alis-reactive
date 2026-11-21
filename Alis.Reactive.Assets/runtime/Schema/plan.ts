export type JsonValue = string | number | boolean | null | readonly JsonValue[] | { readonly [key: string]: JsonValue };

export interface PlanDocument {
  readonly version: 7;
  readonly planId: string;
  readonly culture: Culture;
  readonly timeZone: TimeZone;
  readonly trace: Trace;
  readonly vendors: Readonly<Record<string, Vendor>>;
  readonly objects: Readonly<Record<string, Locator>>;
  readonly handlers: readonly Handler[];
}

export interface Culture {
  readonly name: string;
  readonly collation: string;
  readonly decimal: string;
  readonly group: string;
  readonly integer: NumberText;
  readonly fraction: NumberText;
}

export interface NumberText {
  readonly group: boolean;
  readonly exponent: boolean;
}

export type TimeZone = ClockTimeZone | ZoneTimeZone;

export interface ClockTimeZone {
  readonly kind: "clock";
}

export interface ZoneTimeZone {
  readonly kind: "zone";
  readonly id: string;
}

export type Trace = OffTrace | OnTrace;

export interface OffTrace {
  readonly kind: "off";
}

export interface OnTrace {
  readonly kind: "on";
  readonly traceparent: string;
  readonly tracestate: string;
  readonly console: boolean;
  readonly sources: readonly Source[];
}

export interface Source {
  readonly view: string;
  readonly line: number;
}

export interface Vendor {
  readonly locate: Expression;
  readonly listen: Listening;
}

export type Expression = LitExpression | ObjExpression | RefExpression | GetExpression | DataExpression | AsExpression | CallExpression | FnExpression | AnswerExpression | RecordExpression | ListExpression | OpExpression | SignalExpression;

export interface LitExpression {
  readonly kind: "lit";
  readonly value: JsonValue;
}

export interface ObjExpression {
  readonly kind: "obj";
  readonly id: string;
}

export interface RefExpression {
  readonly kind: "ref";
  readonly name: string;
}

export interface GetExpression {
  readonly kind: "get";
  readonly path: readonly PathStep[];
  readonly members: PathMembers;
  readonly target: Expression;
}

export type PathStep = string | number;

export type PathMembers = "required" | "optional" | "requiredIfHeld" | "optionalMember";

export interface DataExpression {
  readonly kind: "data";
  readonly path: readonly DataStep[];
  readonly target: Expression;
}

export interface DataStep {
  readonly name: string;
  readonly requires: readonly string[];
  readonly absent: Absence;
}

export type Absence = StartAbsence | NothingAbsence | UnboundAbsence;

export interface StartAbsence {
  readonly kind: "start";
  readonly value: JsonValue;
}

export interface NothingAbsence {
  readonly kind: "nothing";
}

export interface UnboundAbsence {
  readonly kind: "unbound";
}

export interface AsExpression {
  readonly kind: "as";
  readonly expr: Expression;
  readonly shape: Shape;
}

export type Shape = StringShape | NumberShape | BooleanShape | DateShape | InstantShape | FileShape | AnyShape | ArrayShape | NullableShape | ObjectShape | EnumShape;

export interface StringShape {
  readonly kind: "string";
  readonly date: DateMeaning;
  readonly negativeZero: string;
}

export type DateMeaning = "none" | "date" | "instant";

export interface NumberShape {
  readonly kind: "number";
  readonly of: NumberOf;
  readonly text: NumberCulture;
  readonly overflow: NumberOverflow;
}

export type NumberOf = "integer" | "fraction";

export type NumberCulture = "view" | "invariant";

export type NumberOverflow = "nothing" | "infinity";

export interface BooleanShape {
  readonly kind: "boolean";
}

export interface DateShape {
  readonly kind: "date";
}

export interface InstantShape {
  readonly kind: "instant";
  readonly from: DateMeaning;
}

export interface FileShape {
  readonly kind: "file";
}

export interface AnyShape {
  readonly kind: "any";
}

export interface ArrayShape {
  readonly kind: "array";
  readonly item: Shape;
}

export interface NullableShape {
  readonly kind: "nullable";
  readonly inner: Shape;
}

export interface ObjectShape {
  readonly kind: "object";
  readonly fields: Readonly<Record<string, FieldConversion>>;
}

export interface FieldConversion {
  readonly shape: Shape;
  readonly nothing: FieldNothing;
}

export type FieldNothing = "omit" | "null";

export interface EnumShape {
  readonly kind: "enum";
  readonly flags: boolean;
  readonly values: readonly LiveEnumValue[];
}

export interface LiveEnumValue {
  readonly held: JsonValue;
  readonly live: JsonValue;
  readonly number: number;
}

export interface CallExpression {
  readonly kind: "call";
  readonly path: readonly PathStep[];
  readonly members: PathMembers;
  readonly target: Expression;
  readonly args: readonly Expression[];
}

export interface FnExpression {
  readonly kind: "fn";
  readonly params: readonly string[];
  readonly body: Expression;
}

export interface AnswerExpression {
  readonly kind: "answer";
  readonly arguments: string;
  readonly body: Expression;
}

export interface RecordExpression {
  readonly kind: "record";
  readonly fields: readonly RecordField[];
}

export interface RecordField {
  readonly name: string;
  readonly value: Expression;
}

export interface ListExpression {
  readonly kind: "list";
  readonly items: readonly Expression[];
}

export interface OpExpression {
  readonly kind: "op";
  readonly op: Operator;
  readonly args: readonly Expression[];
}

export type Operator = "eq" | "neq" | "lt" | "lte" | "gt" | "gte" | "and" | "or" | "not" | "add" | "sub" | "mul" | "mod" | "neg" | "coalesce" | "cond" | "nullableValue" | "nonNull" | "empty" | "matches" | "toArray" | "finite" | "compareNumbers" | "compareOrdinal" | "compareBooleans" | "compareCulture" | "merge" | "trim" | "trimStart" | "trimEnd" | "upperInvariant" | "lowerInvariant" | "upper" | "lower" | "upperTurkic" | "lowerTurkic" | "contains" | "startsWith" | "endsWith" | "nullOrEmpty" | "includes" | "sumInt" | "sumDouble" | "sumDecimal" | "eqDecimal" | "dateKey" | "wallKey" | "instantKey" | "dayKey" | "timeKey" | "spanKey" | "year" | "month" | "day" | "hour" | "minute" | "second" | "dayOfWeek" | "emailAddress" | "creditCard" | "url" | "precisionScale" | "inFlags";

export interface SignalExpression {
  readonly kind: "signal";
}

export type Listening = MethodListening | AssignmentListening | ReturnedListening;

export interface MethodListening {
  readonly kind: "method";
  readonly subscribe: string;
  readonly unsubscribe: string;
}

export interface AssignmentListening {
  readonly kind: "assignment";
}

export interface ReturnedListening {
  readonly kind: "returned";
  readonly subscribe: string;
}

export type Locator = ElementLocator | PluginLocator;

export interface ElementLocator {
  readonly kind: "element";
  readonly id: string;
  readonly vendor: string;
}

export interface PluginLocator {
  readonly kind: "plugin";
  readonly name: string;
  readonly key: string;
  readonly listen: Listening;
  readonly data: PluginData;
}

export type PluginData = NonePluginData | HubPluginData | StreamPluginData | ModelPluginData | FormPluginData;

export interface NonePluginData {
  readonly kind: "none";
}

export interface HubPluginData {
  readonly kind: "hub";
  readonly offer: boolean;
  readonly waits: readonly number[];
  readonly client: string;
}

export interface StreamPluginData {
  readonly kind: "stream";
  readonly offer: boolean;
}

export interface ModelPluginData {
  readonly kind: "model";
  readonly inputs: Readonly<Record<string, InputBinding>>;
  readonly containers: Readonly<Record<string, Container>>;
}

export interface InputBinding {
  readonly name: string;
  readonly path: readonly PathStep[];
  readonly value: Expression;
  readonly committed: string;
  readonly invalidClass: string;
  readonly message: string;
}

export interface Container {
  readonly model: string;
  readonly kind: ContainerKind;
  readonly summary: string;
  readonly live: readonly Live[];
}

export type ContainerKind = "form" | "element";

export interface Live {
  readonly validator: string;
  readonly ruleSet: string;
  readonly body: Expression;
}

export interface FormPluginData {
  readonly kind: "form";
  readonly registration: Registration;
}

export interface Registration {
  readonly validator: string;
  readonly ruleSet: string;
  readonly start: JsonValue;
  readonly scalars: readonly BoundScalar[];
  readonly objects: readonly BoundObject[];
  readonly lists: readonly BoundList[];
  readonly rules: RuleGroup;
}

export interface BoundScalar {
  readonly path: readonly RegistrationStep[];
  readonly shape: Shape;
  readonly before: string;
  readonly after: string;
}

export type RegistrationStep = PathStep | EachRegistrationStep;

export interface EachRegistrationStep {
  readonly kind: "each";
}

export interface BoundObject {
  readonly path: readonly RegistrationStep[];
  readonly start: JsonValue;
  readonly json: BoundObjectJson;
}

export type BoundObjectJson = "fill" | "replace";

export interface BoundList {
  readonly path: readonly RegistrationStep[];
  readonly item: JsonValue;
  readonly json: BoundListJson;
}

export type BoundListJson = "append" | "replace";

export interface RuleGroup {
  readonly path: readonly RegistrationStep[];
  readonly when: Expression;
  readonly fields: readonly RuleField[];
  readonly groups: readonly RuleGroup[];
}

export interface RuleField {
  readonly path: readonly RegistrationStep[];
  readonly tests: readonly RuleTest[];
}

export interface RuleTest {
  readonly check: Expression;
  readonly when: Expression;
  readonly message: string;
}

export interface Handler {
  readonly on: string;
  readonly path: readonly PathStep[];
  readonly event: string;
  readonly payload: readonly PathStep[];
  readonly members: PathMembers;
  readonly policy: Policy;
  readonly do: readonly Statement[];
}

export type Policy = "overlap" | "replace" | "skip";

export type Statement = SetStatement | DoStatement | LetStatement | IfStatement | AllStatement | RequestStatement | ListenStatement | StopStatement;

export interface SetStatement {
  readonly kind: "set";
  readonly path: readonly PathStep[];
  readonly members: SetMembers;
  readonly content: boolean;
  readonly target: Expression;
  readonly value: Expression;
}

export type SetMembers = "required" | "optional";

export interface DoStatement {
  readonly kind: "do";
  readonly await: boolean;
  readonly expr: Expression;
}

export interface LetStatement {
  readonly kind: "let";
  readonly name: string;
  readonly await: boolean;
  readonly binds: LetBinds;
  readonly expr: Expression;
}

export type LetBinds = "line" | "read";

export interface IfStatement {
  readonly kind: "if";
  readonly when: Expression;
  readonly then: readonly Statement[];
  readonly else: readonly Statement[];
}

export interface AllStatement {
  readonly kind: "all";
  readonly blocks: readonly (readonly Statement[])[];
}

export interface RequestStatement {
  readonly kind: "request";
  readonly argument: string;
  readonly reply: string;
  readonly send: Expression;
  readonly check: readonly Statement[];
  readonly layers: readonly Layer[];
  readonly success: Route;
  readonly statuses: readonly StatusRoute[];
  readonly otherwise: OtherwiseRoute;
  readonly chained: readonly Statement[];
}

export interface Layer {
  readonly whileLoading: readonly Statement[];
  readonly finally: readonly Statement[];
}

export interface Route {
  readonly kind: SuccessBody;
  readonly do: readonly Statement[];
}

export type SuccessBody = "none" | "json" | "text" | "html" | "file";

export interface StatusRoute {
  readonly status: number;
  readonly kind: ErrorBody;
  readonly do: readonly Statement[];
}

export type ErrorBody = "none" | "json" | "text" | "html";

export interface OtherwiseRoute {
  readonly kind: OtherwiseBody;
  readonly do: readonly Statement[];
}

export type OtherwiseBody = "none" | "json";

export interface ListenStatement {
  readonly kind: "listen";
  readonly event: string;
  readonly listen: Listening;
  readonly release: Release;
  readonly payload: readonly PathStep[];
  readonly members: PathMembers;
  readonly policy: Policy;
  readonly payloadName: string;
  readonly argumentsName: string;
  readonly object: Expression;
  readonly do: readonly Statement[];
}

export type Release = LivingRelease | HeldRelease | LocatedRelease;

export interface LivingRelease {
  readonly kind: "living";
}

export interface HeldRelease {
  readonly kind: "held";
  readonly vendor: string;
  readonly element: readonly PathStep[];
}

export interface LocatedRelease {
  readonly kind: "located";
  readonly id: string;
}

export interface StopStatement {
  readonly kind: "stop";
  readonly reason: string;
}

export interface FormCheck {
  readonly registration: string;
  readonly body: CheckedBody;
  readonly inputs: readonly SentInput[];
  readonly place: FormPlace;
}

export interface CheckedBody {
  readonly format: BodyFormat;
  readonly value: EvaluatedValue;
}

export type BodyFormat = "none" | "json" | "urlEncoded" | "multipart" | "text" | "file";

export type EvaluatedValue = unknown;

export interface SentInput {
  readonly name: string;
  readonly entry: InputEntry;
}

export interface InputEntry {
  readonly id: string;
  readonly binding: InputBinding;
}

export interface FormPlace {
  readonly destination: FormDestination;
}

export type FormDestination = SummaryFormDestination | NoticeFormDestination;

export interface SummaryFormDestination {
  readonly kind: "summary";
  readonly containerId: string;
}

export interface NoticeFormDestination {
  readonly kind: "notice";
  readonly separator: string;
}

export interface HttpSend {
  readonly method: RequestMethod;
  readonly url: RequestUrl;
  readonly query: SentQuery;
  readonly body: SentBody;
  readonly headers: readonly SentHeader[];
  readonly antiforgery: Antiforgery;
  readonly retry: RetryPolicy;
  readonly busy: Busy;
  readonly respondedJson: boolean;
  readonly contracts: readonly RequestContract[];
}

export type RequestMethod = "GET" | "HEAD" | "OPTIONS" | "POST" | "PUT" | "DELETE";

export interface RequestUrl {
  readonly parts: readonly PathPart[];
  readonly query: string;
}

export type PathPart = TextPathPart | MemberPathPart;

export interface TextPathPart {
  readonly kind: "text";
  readonly text: string;
}

export interface MemberPathPart {
  readonly kind: "member";
  readonly name: string;
  readonly value: ValueRead;
  readonly form: ScalarWireForm;
}

export type ValueRead = NothingValueRead | HeldValueRead;

export interface NothingValueRead {
  readonly kind: "nothing";
}

export interface HeldValueRead {
  readonly kind: "held";
  readonly value: EvaluatedValue;
}

export type ScalarWireForm = TextScalarWireForm | NumberScalarWireForm | BooleanScalarWireForm | DateScalarWireForm | InstantScalarWireForm | WrittenScalarWireForm | EnumScalarWireForm | FlagsScalarWireForm;

export interface TextScalarWireForm {
  readonly kind: "text";
}

export interface NumberScalarWireForm {
  readonly kind: "number";
}

export interface BooleanScalarWireForm {
  readonly kind: "boolean";
}

export interface DateScalarWireForm {
  readonly kind: "date";
}

export interface InstantScalarWireForm {
  readonly kind: "instant";
}

export interface WrittenScalarWireForm {
  readonly kind: "written";
}

export interface EnumScalarWireForm {
  readonly kind: "enum";
  readonly values: readonly EnumValue[];
}

export interface EnumValue {
  readonly held: JsonValue;
  readonly name: string;
}

export interface FlagsScalarWireForm {
  readonly kind: "flags";
  readonly values: readonly FlagValue[];
}

export interface FlagValue {
  readonly held: JsonValue;
  readonly name: string;
  readonly number: number;
}

export interface SentQuery {
  readonly fields: readonly SentField[];
  readonly inputs: SentInputs;
}

export interface SentField {
  readonly name: string;
  readonly path: readonly PathStep[];
  readonly value: ValueRead;
  readonly form: WireForm;
  readonly whenNothing: WhenNothing;
  readonly when: boolean;
}

export type WireForm = TextScalarWireForm | NumberScalarWireForm | BooleanScalarWireForm | DateScalarWireForm | InstantScalarWireForm | WrittenScalarWireForm | EnumScalarWireForm | FlagsScalarWireForm | ListWireForm;

export interface ListWireForm {
  readonly kind: "list";
  readonly item: ScalarWireForm;
  readonly itemWhenNothing: ItemWhenNothing;
}

export type ItemWhenNothing = "empty" | "stop";

export type WhenNothing = "leave" | "empty";

export interface SentInputs {
  readonly container: ContainerCapture;
  readonly individual: readonly InputCapture[];
}

export type ContainerCapture = NoneContainerCapture | MissingContainerCapture | CapturedContainerCapture;

export interface NoneContainerCapture {
  readonly kind: "none";
}

export interface MissingContainerCapture {
  readonly kind: "missing";
  readonly id: string;
}

export interface CapturedContainerCapture {
  readonly kind: "captured";
  readonly id: string;
  readonly body: EvaluatedValue;
  readonly inputs: readonly SentInput[];
}

export type InputCapture = MissingInputCapture | CapturedInputCapture | LeftOutInputCapture;

export interface MissingInputCapture {
  readonly kind: "missing";
  readonly id: string;
  readonly name: string;
}

export interface CapturedInputCapture {
  readonly kind: "captured";
  readonly input: SentInput;
}

export interface LeftOutInputCapture {
  readonly kind: "leftOut";
}

export type SentBody = NoneSentBody | JsonSentBody | UrlEncodedSentBody | MultipartSentBody | TextSentBody | FileSentBody;

export interface NoneSentBody {
  readonly kind: "none";
}

export interface JsonSentBody {
  readonly kind: "json";
  readonly value: EvaluatedValue;
  readonly guarded: readonly GuardedMember[];
  readonly enums: readonly SentEnum[];
  readonly inputs: SentInputs;
}

export interface GuardedMember {
  readonly path: readonly string[];
  readonly when: boolean;
}

export interface SentEnum {
  readonly name: string;
  readonly path: readonly string[];
  readonly form: JsonForm;
}

export type JsonForm = EnumJsonForm | FlagsJsonForm | ListJsonForm;

export interface EnumJsonForm {
  readonly kind: "enum";
  readonly values: readonly JsonEnumValue[];
}

export interface JsonEnumValue {
  readonly held: JsonValue;
  readonly sent: JsonValue;
}

export interface FlagsJsonForm {
  readonly kind: "flags";
  readonly values: readonly JsonFlagValue[];
}

export interface JsonFlagValue {
  readonly held: JsonValue;
  readonly number: number;
  readonly sent: JsonValue;
}

export interface ListJsonForm {
  readonly kind: "list";
  readonly item: JsonForm;
  readonly itemWhenNothing: ItemWhenNothing;
}

export interface UrlEncodedSentBody {
  readonly kind: "urlEncoded";
  readonly fields: readonly SentField[];
  readonly inputs: SentInputs;
}

export interface MultipartSentBody {
  readonly kind: "multipart";
  readonly parts: readonly MultipartPart[];
  readonly inputs: SentInputs;
}

export type MultipartPart = FieldMultipartPart | FileMultipartPart;

export interface FieldMultipartPart {
  readonly kind: "field";
  readonly field: SentField;
}

export interface FileMultipartPart {
  readonly kind: "file";
  readonly name: string;
  readonly value: ValueRead;
  readonly when: boolean;
}

export interface TextSentBody {
  readonly kind: "text";
  readonly value: ValueRead;
  readonly inputs: SentInputs;
}

export interface FileSentBody {
  readonly kind: "file";
  readonly value: ValueRead;
  readonly inputs: SentInputs;
}

export interface SentHeader {
  readonly name: string;
  readonly value: ValueRead;
  readonly form: ScalarWireForm;
  readonly when: boolean;
}

export type Antiforgery = NoneAntiforgery | TokenAntiforgery;

export interface NoneAntiforgery {
  readonly kind: "none";
}

export interface TokenAntiforgery {
  readonly kind: "token";
  readonly value: string;
  readonly place: TokenPlace;
}

export type TokenPlace = HeaderTokenPlace | FieldTokenPlace;

export interface HeaderTokenPlace {
  readonly kind: "header";
  readonly name: string;
}

export interface FieldTokenPlace {
  readonly kind: "field";
  readonly name: string;
}

export type RetryPolicy = NoneRetryPolicy | StandardRetryPolicy;

export interface NoneRetryPolicy {
  readonly kind: "none";
}

export interface StandardRetryPolicy {
  readonly kind: "standard";
  readonly attempts: number;
  readonly firstWaitMilliseconds: number;
}

export type Busy = NoneBusy | PageBusy | TargetsBusy;

export interface NoneBusy {
  readonly kind: "none";
}

export interface PageBusy {
  readonly kind: "page";
}

export interface TargetsBusy {
  readonly kind: "targets";
  readonly values: readonly BusyTarget[];
}

export type BusyTarget = MissingBusyTarget | ElementBusyTarget | ComponentBusyTarget;

export interface MissingBusyTarget {
  readonly kind: "missing";
  readonly id: string;
}

export interface ElementBusyTarget {
  readonly kind: "element";
  readonly id: string;
}

export interface ComponentBusyTarget {
  readonly kind: "component";
  readonly id: ValueRead;
}

export type RequestContract = "busy" | "notice";

export interface HttpResponse {
  readonly status: number;
  readonly outcome: HttpOutcome;
  readonly headers: BrowserHeaders;
  readonly request: EvaluatedValue;
  readonly inputs: readonly SentInput[];
  readonly body: ResponseBody;
}

export type HttpOutcome = "ok" | "status" | "network" | "redirect" | "content";

export type BrowserHeaders = Headers;

export type ResponseBody = NoneResponseBody | JsonResponseBody | TextResponseBody | HtmlResponseBody | FileResponseBody;

export interface NoneResponseBody {
  readonly kind: "none";
}

export interface JsonResponseBody {
  readonly kind: "json";
  readonly value: EvaluatedValue;
}

export interface TextResponseBody {
  readonly kind: "text";
  readonly value: string;
}

export interface HtmlResponseBody {
  readonly kind: "html";
  readonly value: string;
}

export interface FileResponseBody {
  readonly kind: "file";
  readonly value: ResponseFile;
}

export interface ResponseFile {
  readonly name: string;
  readonly size: number;
  readonly type: string;
}
