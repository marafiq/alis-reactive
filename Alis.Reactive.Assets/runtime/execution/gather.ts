// gather.ts — Gather values for HTTP requests using the SHARED value concept.
// Every field carries a ValueProducer — evaluated via evaluateValue().
// No parallel read path. Shape flows from plan → transport for wire formatting.

import type { Plan, Component, GatherInput, GatherPayloadField, HttpMethod, ObjectProducer, RequestInput, SupplementalGatherFields, ValueProducer } from "../types";
import type { ExecContext } from "../types";
import { assertNever } from "../core/assert-never";
import { evaluateValue } from "../core/evaluate";
import { RuntimePlan, type RuntimeComponent } from "../domain/runtime-plan";
import { RuntimeShape } from "../domain/runtime-shape";
import { GatherOutput, emitGatheredValue, type GatherResult, type TransportStrategy } from "./gather-transport";

export type { GatherResult } from "./gather-transport";

interface GatherRuntime {
  readonly method: HttpMethod;
  readonly plan: Plan;
  readonly runtimePlan: RuntimePlan;
  readonly ctx: ExecContext;
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
  return resolveRequestInput(input, runtime);
}

function resolveRequestInput(input: RequestInput, runtime: GatherRuntime): GatherResult {
  switch (input.kind) {
    case "none":
      return GatherOutput.empty();
    case "value":
      return resolveValueInput(input, runtime);
    case "gather":
      return resolveGatherInput(input, runtime);
    default:
      return assertNever(input, "request input");
  }
}

function resolveValueInput(input: Extract<RequestInput, { kind: "value" }>, runtime: GatherRuntime): GatherResult {
  const output = GatherOutput.for(input.transport, runtime.method);
  emitDeclaredObjectValueFields(input.value, output.transport, runtime);
  return output.toResult();
}

function resolveGatherInput(input: GatherInput, runtime: GatherRuntime): GatherResult {
  const output = GatherOutput.for(input.transport, runtime.method);

  const claims = GatherPayloadClaims.empty();
  gatherDeclaredPayloadFields(input, output.transport, runtime, claims);
  gatherBuildTimeRegisteredInputFields(input, output.transport, runtime, claims);
  emitSupplementalFields(input.supplementalFields, output.transport, runtime, claims);
  emitRuntimeRegisteredInputs(input.selection, claims, runtime, output.transport);

  return output.toResult();
}

/** Gather developer-authored request payload fields, tracking payload paths and component reads. */
function gatherDeclaredPayloadFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  for (const field of gatherInput.declaredFields) {
    emitPlanGatherPayloadField(field, transport, runtime, claims);
  }
}

/** Gather registered inputs already expanded by the C# plan domain at render time. */
function gatherBuildTimeRegisteredInputFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  for (const field of gatherInput.registeredInputFields) {
    emitPlanGatherPayloadField(field, transport, runtime, claims);
  }
}

/** Emit supplemental static/event values merged alongside plan-declared fields. */
function emitSupplementalFields(
  supplementalFields: SupplementalGatherFields,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  switch (supplementalFields.kind) {
    case "none":
      return;
    case "declared":
      for (const field of supplementalFields.fields) {
        emitPlanGatherPayloadField(field, transport, runtime, claims);
      }
      return;
    default:
      return assertNever(supplementalFields, "supplemental gather fields");
  }
}

function emitPlanGatherPayloadField(
  field: GatherPayloadField,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  claims.recordPlanField(field);

  const raw = evaluateValue(field.value, runtime.plan, runtime.ctx);
  const shape = RuntimeShape.declaredBy(field.value);
  emitGatheredValue(field.payloadPath, raw, shape, transport);
}

class GatherPayloadClaims {
  private constructor(
    private readonly payloadSlots: GatherPayloadSlots,
    private readonly componentKeys: Set<string>,
  ) {}

  static empty(): GatherPayloadClaims {
    return new GatherPayloadClaims(GatherPayloadSlots.empty(), new Set<string>());
  }

  recordPlanField(field: GatherPayloadField): void {
    this.claimPayloadPath(field.payloadPath);
    recordPlannedGatherComponentRead(field.value, this.componentKeys);
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
  private constructor(private readonly claimedPaths: GatherPayloadPath[]) {}

  static empty(): GatherPayloadSlots {
    return new GatherPayloadSlots([]);
  }

  claimDeclared(payloadPath: string): void {
    this.claimedPaths.push(GatherPayloadPath.from(payloadPath));
  }

  tryClaim(payloadPath: string): boolean {
    const incoming = GatherPayloadPath.from(payloadPath);
    const payloadPathAlreadyClaimed = this.claimedPaths.some(path => path.overlaps(incoming));
    if (payloadPathAlreadyClaimed) return false;

    this.claimedPaths.push(incoming);
    return true;
  }
}

class GatherPayloadPath {
  private constructor(private readonly parts: [string, ...string[]]) {}

  static from(key: string): GatherPayloadPath {
    const parts = key.split(".");
    const keyContainsEmptySegment = parts.some(part => part.length === 0);
    if (keyContainsEmptySegment) {
      throw new Error(`[alis] gather key "${key}" contains an empty path segment`);
    }

    return new GatherPayloadPath(parts as [string, ...string[]]);
  }

  overlaps(other: GatherPayloadPath): boolean {
    return this.isPrefixOf(other) || other.isPrefixOf(this);
  }

  private isPrefixOf(other: GatherPayloadPath): boolean {
    if (this.parts.length > other.parts.length) return false;

    return this.parts.every((part, index) => part === other.parts[index]);
  }
}

function recordPlannedGatherComponentRead(producer: ValueProducer, gatheredComponents: Set<string>): void {
  const producerReadsRuntimeValue = producer.kind === "read";
  if (!producerReadsRuntimeValue) return;

  const source = producer.from;
  const producerReadsComponent = source.kind === "component";
  if (!producerReadsComponent) return;

  gatheredComponents.add(source.component);
}

function emitRuntimeRegisteredInputs(
  selection: GatherInput["selection"],
  claims: GatherPayloadClaims,
  runtime: GatherRuntime,
  transport: TransportStrategy,
): void {
  switch (selection.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of runtime.runtimePlan.components.entries()) {
        emitRuntimeRegisteredInput(component, claims, transport);
      }
      return;
    default:
      return assertNever(selection, "gather selection");
  }
}

function emitDeclaredObjectValueFields(
  producer: ObjectProducer,
  transport: TransportStrategy,
  runtime: GatherRuntime,
): void {
  for (const [key, fieldProducer] of Object.entries(producer.fields)) {
    const value = evaluateValue(fieldProducer, runtime.plan, runtime.ctx);
    emitGatheredValue(key, value, RuntimeShape.declaredBy(fieldProducer), transport);
  }
}

interface RegisteredInputGatherContract {
  readonly valueMember: string;
  readonly bindingPath: string;
}

function emitRuntimeRegisteredInput(
  component: RuntimeComponent,
  claims: GatherPayloadClaims,
  transport: TransportStrategy,
): void {
  if (claims.hasComponent(component.key)) return;

  const contract = registeredInputGatherContract(component.definition);
  if (contract === undefined) return;

  const registeredInputIsMounted = component.tryElement() !== undefined;
  if (!registeredInputIsMounted) return;

  const payloadSlotWasReserved = claims.tryClaimRuntimePayloadPath(contract.bindingPath);
  if (!payloadSlotWasReserved) return;

  const object = component.object();
  const runtimeValue = object.read(contract.valueMember);

  emitGatheredValue(
    contract.bindingPath,
    runtimeValue.usingDeclaredShape(),
    RuntimeShape.from(runtimeValue.shape),
    transport,
  );
}

function registeredInputGatherContract(component: Component): RegisteredInputGatherContract | undefined {
  const binding = component.binding;
  if (binding.kind === "none") return undefined;

  return {
    valueMember: binding.valueMember,
    bindingPath: binding.bindingPath,
  }
}
