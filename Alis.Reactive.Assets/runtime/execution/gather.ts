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
    gatherDeclaredPayloadFields(this.input, output.transport, runtime, claims);
    gatherBuildTimeRegisteredInputFields(this.input, output.transport, runtime, claims);
    emitSupplementalFields(this.input, output.transport, runtime, claims);

    RuntimeRegisteredInputSelection
      .from(this.input.selection, claims)
      .emitInto(runtime, output.transport);

    return output.toResult();
  }
}

/** Gather developer-authored request payload fields, tracking payload paths and component reads. */
function gatherDeclaredPayloadFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  for (const field of gatherInput.declaredFields) {
    PlanGatherPayloadField.from(field).emitInto(transport, runtime, claims);
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
    PlanGatherPayloadField.from(field).emitInto(transport, runtime, claims);
  }
}

/** Emit supplemental static/event values merged alongside plan-declared fields. */
function emitSupplementalFields(
  gatherInput: GatherInput,
  transport: TransportStrategy,
  runtime: GatherRuntime,
  claims: GatherPayloadClaims,
): void {
  SupplementalGatherValues.from(gatherInput.supplementalFields).emitInto(transport, runtime, claims);
}

class PlanGatherPayloadField {
  private constructor(private readonly field: GatherPayloadField) {}

  static from(field: GatherPayloadField): PlanGatherPayloadField {
    return new PlanGatherPayloadField(field);
  }

  emitInto(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void {
    claims.recordPlanField(this.field);

    const raw = evaluateValue(this.field.value, runtime.plan, runtime.ctx);
    const shape = RuntimeShape.declaredBy(this.field.value);
    emitGatheredValue(this.field.payloadPath, raw, shape, transport);
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

  recordPlanField(field: GatherPayloadField): void {
    this.claimPayloadPath(field.payloadPath);
    PlannedGatherComponentRead.from(field.value).recordIn(this.componentKeys);
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

abstract class PlannedGatherComponentRead {
  static from(producer: ValueProducer): PlannedGatherComponentRead {
    const producerReadsRuntimeValue = producer.kind === "read";
    if (!producerReadsRuntimeValue) return NoPlannedGatherComponentRead.instance;

    const source = producer.from;
    const producerReadsComponent = source.kind === "component";
    if (!producerReadsComponent) return NoPlannedGatherComponentRead.instance;

    return new PlannedComponentGatherRead(source.component);
  }

  abstract recordIn(gatheredComponents: Set<string>): void;
}

class NoPlannedGatherComponentRead extends PlannedGatherComponentRead {
  static readonly instance = new NoPlannedGatherComponentRead();

  recordIn(): void {
    return;
  }
}

class PlannedComponentGatherRead extends PlannedGatherComponentRead {
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
        return new DeclaredSupplementalGatherValues(supplementalFields.fields);
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
  constructor(private readonly fields: GatherPayloadField[]) {
    super();
  }

  emitInto(
    transport: TransportStrategy,
    runtime: GatherRuntime,
    claims: GatherPayloadClaims,
  ): void {
    for (const field of this.fields) {
      PlanGatherPayloadField.from(field).emitInto(transport, runtime, claims);
    }
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
      emitGatheredValue(key, value, RuntimeShape.declaredBy(producer), transport);
    });
  }

  private emitEach(emit: (key: string, producer: ValueProducer) => void): void {
    for (const [key, producer] of Object.entries(this.fields)) {
      emit(key, producer);
    }
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

    emitGatheredValue(
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
