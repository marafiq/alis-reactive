// gather.ts — Gather values for HTTP requests using the SHARED value concept.
// Every field carries a ValueProducer — evaluated via evaluateValue().
// No parallel read path. Shape flows from plan → transport for wire formatting.

import type { Plan, Component, GatherInput, GatherPayloadField, HttpMethod, ObjectProducer, RequestInput, SupplementalGatherFields, Transport, ValueProducer } from "../types";
import type { ExecContext } from "../types";
import { assertNever } from "../core/assert-never";
import { toString } from "../core/shape-convert";
import { scope } from "../core/trace";
import { evaluateValue } from "../core/evaluate";
import { RuntimePlan, type RuntimeComponent } from "../domain/runtime-plan";
import { PlainObjectRecord } from "../domain/object-record";
import { RuntimeShape } from "../domain/runtime-shape";
import { HttpRequestMethod } from "../domain/http-request-method";

const log = scope("gather");

export interface GatherResult {
  urlParams: string[];
  body: Record<string, unknown> | FormData;
}

/** Transport strategies for emitting name/value pairs into GET, FormData, or JSON. */
interface TransportStrategy {
  emitScalar(name: string, value: unknown, shape: RuntimeShape): void;
  emitArray(name: string, items: unknown[], itemShape: RuntimeShape): void;
}

interface GatherRuntime {
  readonly method: HttpMethod;
  readonly plan: Plan;
  readonly runtimePlan: RuntimePlan;
  readonly ctx: ExecContext;
}

class GatherOutput {
  private constructor(
    private readonly urlParams: string[],
    private readonly body: GatherRequestBody,
    readonly transport: TransportStrategy,
  ) {}

  static empty(): GatherResult {
    return { urlParams: [], body: {} };
  }

  static for(requestTransport: Transport, method: HttpMethod): GatherOutput {
    const urlParams: string[] = [];
    const requestMethod = HttpRequestMethod.from(method);
    if (requestMethod.sendsInputInQueryString()) {
      return new GatherOutput(urlParams, GatherRequestBody.empty(), createGetTransport(urlParams));
    }

    if (requestTransport === "form-data") {
      const formData = new FormData();
      return new GatherOutput(urlParams, new MultipartGatherBody(formData), createFormDataTransport(formData));
    }

    const body = new JsonGatherBody({});
    return new GatherOutput(urlParams, body, createJsonTransport(body.record));
  }

  toResult(): GatherResult {
    return { urlParams: this.urlParams, body: this.body.value() };
  }
}

abstract class GatherRequestBody {
  static empty(): GatherRequestBody {
    return new JsonGatherBody({});
  }

  abstract value(): Record<string, unknown> | FormData;
}

class JsonGatherBody extends GatherRequestBody {
  constructor(readonly record: Record<string, unknown>) {
    super();
  }

  value(): Record<string, unknown> {
    return this.record;
  }
}

class MultipartGatherBody extends GatherRequestBody {
  constructor(private readonly formData: FormData) {
    super();
  }

  value(): FormData {
    return this.formData;
  }
}

function createGetTransport(urlParams: string[]): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(GatherScalarWireValue.from(wire, name))}`);
    },
    emitArray: (name, items, itemShape) => {
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects cannot be sent via GET");
      gatheredItems.emitToQueryString(name, itemShape, urlParams);
    },
  };
}

function createFormDataTransport(formData: FormData): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      formData.append(name, GatherScalarWireValue.from(wire, name));
    },
    emitArray: (name, items, itemShape) => {
      GatheredArrayItems.from(items).appendToFormData(name, itemShape, formData);
    },
  };
}

function createJsonTransport(body: Record<string, unknown>): TransportStrategy {
  return {
    emitScalar: (name, value, shape) => {
      const wire = shape.formatForWire(value);
      JsonBodyPath.from(name).assign(body, JsonBodyValue.fromWire(wire));
    },
    emitArray: (name, items, itemShape) => {
      const gatheredItems = GatheredArrayItems.from(items);
      if (gatheredItems.containsFile) throw new Error("[alis] File objects require transport: form-data");
      const wireItems = gatheredItems.toJsonValue(itemShape);
      JsonBodyPath.from(name).assign(body, wireItems);
    },
  };
}

function emitValue(name: string, raw: unknown, shape: RuntimeShape, transport: TransportStrategy): void {
  GatheredValue.from(name, raw, shape).emitInto(transport);
}

/**
 * Resolve gather input into GatherResult (urlParams + body/FormData).
 */
export function resolveGather(
  input: RequestInput,
  method: HttpMethod,
  plan: Plan,
  ctx: ExecContext,
): GatherResult {
  const runtime = { method, plan, runtimePlan: RuntimePlan.from(plan), ctx };
  return RequestInputResolver.from(input).resolve(runtime);
}

abstract class RequestInputResolver {
  static from(input: RequestInput): RequestInputResolver {
    switch (input.kind) {
      case "none":
        return EmptyRequestInputResolver.instance;
      case "value":
        return new ValueRequestInputResolver(input);
      case "gather":
        return new GatherRequestInputResolver(input);
      default:
        return assertNever(input, "request input");
    }
  }

  abstract resolve(runtime: GatherRuntime): GatherResult;
}

class EmptyRequestInputResolver extends RequestInputResolver {
  static readonly instance = new EmptyRequestInputResolver();

  resolve(): GatherResult {
    return GatherOutput.empty();
  }
}

class ValueRequestInputResolver extends RequestInputResolver {
  constructor(private readonly input: Extract<RequestInput, { kind: "value" }>) {
    super();
  }

  resolve(runtime: GatherRuntime): GatherResult {
    const output = GatherOutput.for(this.input.transport, runtime.method);
    DeclaredObjectValueFields.from(this.input.value).emitInto(output.transport, runtime);
    return output.toResult();
  }
}

class GatherRequestInputResolver extends RequestInputResolver {
  constructor(private readonly input: GatherInput) {
    super();
  }

  resolve(runtime: GatherRuntime): GatherResult {
    const output = GatherOutput.for(this.input.transport, runtime.method);

    const claims = GatherPayloadClaims.empty();
    gatherExplicitPayloadFields(this.input, output.transport, runtime, claims);
    emitSupplementalFields(this.input, output.transport, runtime, claims);

    RuntimeRegisteredInputSelection
      .from(this.input.selection, claims)
      .emitInto(runtime, output.transport);

    return output.toResult();
  }
}

/** Gather explicit request payload fields, tracking both their payload paths and any component reads. */
function gatherExplicitPayloadFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  for (const field of gatherInput.payloadFields) {
    ExplicitGatherPayloadField.from(field).emitInto(transport, runtime, claims);
  }
}

/** Emit supplemental static/event values merged alongside explicit payload fields. */
function emitSupplementalFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  SupplementalGatherValues.from(gatherInput.supplementalFields).emitInto(transport, runtime, claims);
}

class GatheredValue {
  private constructor(
    private readonly name: string,
    private readonly raw: unknown,
    private readonly shape: RuntimeShape,
  ) {}

  static from(name: string, raw: unknown, shape: RuntimeShape): GatheredValue {
    return new GatheredValue(name, raw, shape);
  }

  emitInto(transport: TransportStrategy): void {
    const fileList = BrowserFileList.tryFrom(this.raw);
    const rawValueIsBrowserFileList = fileList !== undefined;
    if (rawValueIsBrowserFileList) {
      transport.emitArray(this.name, fileList.files(), this.shape);
      log.trace("file.emitted", { name: this.name, count: fileList.count });
      return;
    }

    const arrayValue = GatheredArrayValue.tryFrom(this.raw, this.shape);
    const rawValueIsArray = arrayValue !== undefined;
    if (rawValueIsArray) {
      transport.emitArray(this.name, arrayValue.items, arrayValue.itemShape);
      return;
    }

    transport.emitScalar(this.name, this.raw, this.shape);
  }
}

class BrowserFileList {
  private constructor(
    private readonly value: FileList,
    readonly count: number,
  ) {}

  static tryFrom(raw: unknown): BrowserFileList | undefined {
    const browserExposesFileList = typeof FileList !== "undefined";
    if (!browserExposesFileList) return undefined;

    const rawIsFileList = raw instanceof FileList;
    if (!rawIsFileList) return undefined;

    return new BrowserFileList(raw, raw.length);
  }

  files(): File[] {
    return Array.from(this.value);
  }
}

class GatheredArrayValue {
  private constructor(
    readonly items: unknown[],
    readonly itemShape: RuntimeShape,
  ) {}

  static tryFrom(raw: unknown, shape: RuntimeShape): GatheredArrayValue | undefined {
    const rawIsArray = Array.isArray(raw);
    if (!rawIsArray) return undefined;

    return new GatheredArrayValue(raw, shape.item());
  }
}

class GatheredArrayItems {
  private constructor(private readonly items: GatheredArrayItem[]) {}

  static from(items: unknown[]): GatheredArrayItems {
    return new GatheredArrayItems(items.map(item => GatheredArrayItem.from(item)));
  }

  get containsFile(): boolean {
    return this.items.some(item => item.containsFile);
  }

  emitToQueryString(name: string, itemShape: RuntimeShape, urlParams: string[]): void {
    for (const item of this.items) {
      item.emitToQueryString(name, itemShape, urlParams);
    }
  }

  appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void {
    for (const item of this.items) {
      item.appendToFormData(name, itemShape, formData);
    }
  }

  toJsonValue(itemShape: RuntimeShape): unknown[] {
    return JsonArrayBodyValue.fromItems(
      this.items.map(item => item.rawValue),
      itemShape
    );
  }
}

abstract class GatheredArrayItem {
  static from(item: unknown): GatheredArrayItem {
    const itemIsBrowserFile = item instanceof File;
    if (itemIsBrowserFile) return new UploadedFileArrayItem(item);

    const wrapper = PlainObjectRecord.tryFrom(item);
    const itemCanWrapBrowserFile = wrapper !== undefined;
    if (itemCanWrapBrowserFile) {
      const rawFile = wrapper.get("rawFile");
      const itemWrapsBrowserFile = rawFile instanceof File;
      if (itemWrapsBrowserFile) return new UploadedFileArrayItem(rawFile);
    }

    return new SerializableArrayItem(item);
  }

  abstract get containsFile(): boolean;
  abstract get rawValue(): unknown;
  abstract appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void;

  emitToQueryString(name: string, itemShape: RuntimeShape, urlParams: string[]): void {
    const wire = itemShape.formatForWire(this.rawValue);
    urlParams.push(`${encodeURIComponent(name)}=${encodeURIComponent(GatherScalarWireValue.from(wire, name))}`);
  }
}

class UploadedFileArrayItem extends GatheredArrayItem {
  constructor(private readonly file: File) {
    super();
  }

  get containsFile(): boolean {
    return true;
  }

  get rawValue(): unknown {
    return this.file;
  }

  appendToFormData(name: string, _itemShape: RuntimeShape, formData: FormData): void {
    formData.append(name, this.file, this.file.name);
  }
}

class SerializableArrayItem extends GatheredArrayItem {
  constructor(private readonly value: unknown) {
    super();
  }

  get containsFile(): boolean {
    return false;
  }

  get rawValue(): unknown {
    return this.value;
  }

  appendToFormData(name: string, itemShape: RuntimeShape, formData: FormData): void {
    const wire = itemShape.formatForWire(this.value);
    formData.append(name, GatherScalarWireValue.from(wire, name));
  }
}

class GatherScalarWireValue {
  static from(value: unknown, name: string): string {
    const result = toString(value);
    if (result.ok) return result.value;

    throw new Error(`[alis] gather value "${name}" cannot be serialized as a scalar: ${result.error}`);
  }
}

class ExplicitGatherPayloadField {
  private constructor(private readonly field: GatherPayloadField) {}

  static from(field: GatherPayloadField): ExplicitGatherPayloadField {
    return new ExplicitGatherPayloadField(field);
  }

  emitInto(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void {
    claims.recordDeclaredField(this.field);

    const raw = evaluateValue(this.field.value, runtime.plan, runtime.ctx);
    const shape = RuntimeShape.declaredBy(this.field.value);
    emitValue(this.field.payloadPath, raw, shape, transport);
  }
}

class GatherPayloadClaims {
  private constructor(
    private readonly payloadSlots: GatherPayloadSlots,
    private readonly componentKeys: Set<string>,
  ) {}

  static empty(): GatherPayloadClaims {
    return new GatherPayloadClaims(GatherPayloadSlots.empty(), new Set<string>());
  }

  recordDeclaredField(field: GatherPayloadField): void {
    this.claimPayloadPath(field.payloadPath);
    ExplicitGatherComponentRead.from(field.value).recordIn(this.componentKeys);
  }

  claimPayloadPath(payloadPath: string): void {
    this.payloadSlots.claimDeclared(payloadPath);
  }

  tryClaimRuntimePayloadPath(payloadPath: string): boolean {
    return this.payloadSlots.tryClaim(payloadPath);
  }

  hasComponent(componentKey: string): boolean {
    return this.componentKeys.has(componentKey);
  }
}

class GatherPayloadSlots {
  private constructor(private readonly claimedPaths: JsonBodyPath[]) {}

  static empty(): GatherPayloadSlots {
    return new GatherPayloadSlots([]);
  }

  claimDeclared(payloadPath: string): void {
    this.claimedPaths.push(JsonBodyPath.from(payloadPath));
  }

  tryClaim(payloadPath: string): boolean {
    const incoming = JsonBodyPath.from(payloadPath);
    const payloadPathAlreadyClaimed = this.claimedPaths.some(path => path.overlaps(incoming));
    if (payloadPathAlreadyClaimed) return false;

    this.claimedPaths.push(incoming);
    return true;
  }
}

abstract class ExplicitGatherComponentRead {
  static from(producer: ValueProducer): ExplicitGatherComponentRead {
    const producerReadsRuntimeValue = producer.kind === "read";
    if (!producerReadsRuntimeValue) return NoExplicitGatherComponentRead.instance;

    const source = producer.from;
    const producerReadsComponent = source.kind === "component";
    if (!producerReadsComponent) return NoExplicitGatherComponentRead.instance;

    return new ComponentReadGatherField(source.component);
  }

  abstract recordIn(gatheredComponents: Set<string>): void;
}

class NoExplicitGatherComponentRead extends ExplicitGatherComponentRead {
  static readonly instance = new NoExplicitGatherComponentRead();

  recordIn(): void {
    return;
  }
}

class ComponentReadGatherField extends ExplicitGatherComponentRead {
  constructor(private readonly componentKey: string) {
    super();
  }

  recordIn(gatheredComponents: Set<string>): void {
    gatheredComponents.add(this.componentKey);
  }
}

abstract class RuntimeRegisteredInputSelection {
  static from(
    selection: GatherInput["selection"],
    claims: GatherPayloadClaims,
  ): RuntimeRegisteredInputSelection {
    switch (selection.kind) {
      case "explicit":
        return PlanDeclaredGatherFieldsOnly.instance;
      case "all-registered-inputs":
        return new AllRegisteredInputsRuntimeSelection(claims);
      default:
        return assertNever(selection, "gather selection");
    }
  }

  abstract emitInto(runtime: GatherRuntime, transport: TransportStrategy): void;
}

class PlanDeclaredGatherFieldsOnly extends RuntimeRegisteredInputSelection {
  static readonly instance = new PlanDeclaredGatherFieldsOnly();

  emitInto(): void {
    return;
  }
}

class AllRegisteredInputsRuntimeSelection extends RuntimeRegisteredInputSelection {
  constructor(private readonly claims: GatherPayloadClaims) {
    super();
  }

  emitInto(runtime: GatherRuntime, transport: TransportStrategy): void {
    for (const component of runtime.runtimePlan.components.entries()) {
      const runtimeField = RuntimeRegisteredInputGatherField.tryFrom(component, this.claims);
      if (runtimeField === undefined) continue;

      runtimeField.emitInto(transport);
    }
  }
}

abstract class SupplementalGatherValues {
  static from(supplementalFields: SupplementalGatherFields): SupplementalGatherValues {
    switch (supplementalFields.kind) {
      case "none":
        return NoSupplementalGatherValues.instance;
      case "declared":
        return new DeclaredSupplementalGatherValues(supplementalFields.value);
      default:
        return assertNever(supplementalFields, "supplemental gather fields");
    }
  }

  abstract emitInto(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void;
}

class NoSupplementalGatherValues extends SupplementalGatherValues {
  static readonly instance = new NoSupplementalGatherValues();

  emitInto(): void {
    return;
  }
}

class DeclaredSupplementalGatherValues extends SupplementalGatherValues {
  constructor(private readonly producer: ObjectProducer) {
    super();
  }

  emitInto(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void {
    DeclaredObjectValueFields.from(this.producer).emitIntoGather(transport, runtime, claims);
  }
}

class DeclaredObjectValueFields {
  private constructor(private readonly fields: Record<string, ValueProducer>) {
  }

  static from(producer: ObjectProducer): DeclaredObjectValueFields {
    return new DeclaredObjectValueFields(producer.fields);
  }

  emitInto(transport: TransportStrategy, runtime: GatherRuntime): void {
    this.emitEach((key, producer) => {
      const value = evaluateValue(producer, runtime.plan, runtime.ctx);
      emitValue(key, value, RuntimeShape.declaredBy(producer), transport);
    });
  }

  emitIntoGather(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void {
    this.emitEach((key, producer) => {
      claims.claimPayloadPath(key);
      const value = evaluateValue(producer, runtime.plan, runtime.ctx);
      emitValue(key, value, RuntimeShape.declaredBy(producer), transport);
    });
  }

  private emitEach(emit: (key: string, producer: ValueProducer) => void): void {
    for (const [key, producer] of Object.entries(this.fields)) {
      emit(key, producer);
    }
  }
}

class JsonBodyValue {
  static fromWire(wireValue: unknown): unknown {
    const emptyTextRepresentsClearedField = wireValue === "";
    if (emptyTextRepresentsClearedField) return null;

    return wireValue;
  }
}

class JsonArrayBodyValue {
  static fromItems(items: unknown[], itemShape: RuntimeShape): unknown[] {
    if (!itemShape.isDeclared) return items;

    return items.map(item => itemShape.formatForWire(item));
  }
}

class RuntimeRegisteredInputGatherField {
  private constructor(
    private readonly component: RuntimeComponent,
    private readonly contract: RegisteredInputGatherContract,
  ) {}

  static tryFrom(
    component: RuntimeComponent,
    claims: GatherPayloadClaims,
  ): RuntimeRegisteredInputGatherField | undefined {
    const componentAlreadyGatheredAtBuildTime = claims.hasComponent(component.key);
    if (componentAlreadyGatheredAtBuildTime) return undefined;

    const contract = RegisteredInputGatherContract.tryFrom(component.definition);
    if (contract === undefined) return undefined;

    const registeredInputIsMounted = component.tryElement() !== undefined;
    if (!registeredInputIsMounted) return undefined;

    const payloadSlotWasReserved = claims.tryClaimRuntimePayloadPath(contract.bindingPath);
    if (!payloadSlotWasReserved) return undefined;

    return new RuntimeRegisteredInputGatherField(component, contract);
  }

  emitInto(transport: TransportStrategy): void {
    const object = this.component.object();
    const runtimeValue = object.read(this.contract.valueMember);

    emitValue(
      this.contract.bindingPath,
      runtimeValue.usingDeclaredShape(),
      RuntimeShape.from(runtimeValue.shape),
      transport,
    );
  }
}

class RegisteredInputGatherContract {
  private constructor(
    readonly valueMember: string,
    readonly bindingPath: string,
  ) {}

  static tryFrom(component: Component): RegisteredInputGatherContract | undefined {
    const binding = component.binding;
    if (binding.kind === "none") return undefined;

    return new RegisteredInputGatherContract(binding.valueMember, binding.bindingPath);
  }
}

class JsonBodyPath {
  private constructor(
    private readonly key: string,
    private readonly parts: [string, ...string[]],
  ) {}

  static from(key: string): JsonBodyPath {
    const parts = key.split(".");
    const keyContainsEmptySegment = parts.some(part => part.length === 0);
    if (keyContainsEmptySegment) {
      throw new Error(`[alis] gather key "${key}" contains an empty path segment`);
    }

    return new JsonBodyPath(key, parts as [string, ...string[]]);
  }

  assign(body: Record<string, unknown>, value: unknown): void {
    const pathTargetsRootField = this.parts.length === 1;
    if (pathTargetsRootField) {
      JsonBodyLeaf.from(body, this.rootPart(), this.key).assign(value);
      return;
    }

    const parent = this.parentObject(body);
    JsonBodyLeaf.from(parent, this.lastPart(), this.key).assign(value);
  }

  overlaps(other: JsonBodyPath): boolean {
    return this.isPrefixOf(other) || other.isPrefixOf(this);
  }

  private isPrefixOf(other: JsonBodyPath): boolean {
    if (this.parts.length > other.parts.length) return false;

    return this.parts.every((part, index) => part === other.parts[index]);
  }

  private parentObject(body: Record<string, unknown>): Record<string, unknown> {
    let current = body;
    const parentPath = this.parts.slice(0, -1);
    const walkedPath: string[] = [];

    for (const segment of parentPath) {
      walkedPath.push(segment);
      current = JsonBodySlot.from(current, segment, this.key, walkedPath.join(".")).ensureObject();
    }

    return current;
  }

  private rootPart(): string {
    return this.parts[0];
  }

  private lastPart(): string {
    const part = this.parts[this.parts.length - 1];
    if (part === undefined) {
      throw new Error("[alis] gather path is empty");
    }

    return part;
  }
}

class JsonBodySlot {
  private constructor(
    private readonly parent: Record<string, unknown>,
    private readonly segment: string,
    private readonly ownerKey: string,
    private readonly segmentPath: string,
  ) {}

  static from(
    parent: Record<string, unknown>,
    segment: string,
    ownerKey: string,
    segmentPath: string,
  ): JsonBodySlot {
    return new JsonBodySlot(parent, segment, ownerKey, segmentPath);
  }

  ensureObject(): Record<string, unknown> {
    const value = this.parent[this.segment];
    const segmentHasNoValue = !(this.segment in this.parent);
    if (segmentHasNoValue) {
      this.parent[this.segment] = {};
      return this.parent[this.segment] as Record<string, unknown>;
    }

    const nestedObject = PlainObjectRecord.tryFrom(value);
    if (nestedObject !== undefined) return nestedObject.raw;

    throw new Error(
      `[alis] gather key "${this.ownerKey}" conflicts at "${this.segmentPath}": ` +
      "an existing scalar value cannot hold nested fields. " +
      `Use either "${this.segmentPath}" or "${this.segmentPath}.*" fields, not both.`
    );
  }
}

class JsonBodyLeaf {
  private constructor(
    private readonly parent: Record<string, unknown>,
    private readonly segment: string,
    private readonly ownerKey: string,
  ) {}

  static from(parent: Record<string, unknown>, segment: string, ownerKey: string): JsonBodyLeaf {
    return new JsonBodyLeaf(parent, segment, ownerKey);
  }

  assign(value: unknown): void {
    const existingValue = this.parent[this.segment];
    const segmentHasValue = this.segment in this.parent;
    const existingValueIsNestedObject = PlainObjectRecord.tryFrom(existingValue) !== undefined;
    const incomingValueIsNestedObject = PlainObjectRecord.tryFrom(value) !== undefined;
    const assignmentWouldReplaceNestedFields =
      segmentHasValue
      && existingValueIsNestedObject
      && !incomingValueIsNestedObject;
    if (assignmentWouldReplaceNestedFields) {
      throw new Error(
        `[alis] gather key "${this.ownerKey}" conflicts at "${this.segment}": ` +
        "nested fields were already assigned under this key. " +
        `Use either "${this.segment}" or "${this.segment}.*" fields, not both.`
      );
    }

    this.parent[this.segment] = value;
  }
}
