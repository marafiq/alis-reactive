// gather.ts — Gather values for HTTP requests using the SHARED value concept.
// Every field carries a ValueProducer — evaluated via evaluateValue().
// No parallel read path. Shape flows from plan → transport for wire formatting.

import type { Plan, GatherInput, RequestPayloadAssignment, HttpMethod, RequestInput } from "../types";
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
    case "gather":
      return resolveGatherInput(input, runtime);
    default:
      return assertNever(input, "request input");
  }
}

function resolveGatherInput(input: GatherInput, runtime: GatherRuntime): GatherResult {
  const output = GatherOutput.for(input.transport, runtime.method);

  for (const field of input.fields) {
    emitPlanRequestPayloadAssignment(field, output.transport, runtime);
  }

  emitRuntimeRegisteredInputs(input.selection, runtime, output.transport);

  return output.toResult();
}

function emitPlanRequestPayloadAssignment(
  field: RequestPayloadAssignment,
  transport: TransportStrategy,
  runtime: GatherRuntime,
): void {
  const raw = evaluateValue(field.source, runtime.plan, runtime.ctx);
  const shape = RuntimeShape.declaredBy(field.source);
  emitGatheredValue(field.target, raw, shape, transport);
}

function emitRuntimeRegisteredInputs(
  selection: GatherInput["selection"],
  runtime: GatherRuntime,
  transport: TransportStrategy,
): void {
  switch (selection.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of runtime.runtimePlan.components.entries()) {
        emitRuntimeRegisteredInput(component, transport);
      }
      return;
    default:
      return assertNever(selection, "gather selection");
  }
}

function emitRuntimeRegisteredInput(
  component: RuntimeComponent,
  transport: TransportStrategy,
): void {
  const binding = component.definition.binding;
  if (binding.kind === "none") return;

  const registeredInputIsMounted = component.tryElement() !== undefined;
  if (!registeredInputIsMounted) return;

  const object = component.object();
  const runtimeValue = object.read(binding.valueMember);

  emitGatheredValue(
    {
      name: binding.bindingPath,
      path: binding.path,
    },
    runtimeValue.usingDeclaredShape(),
    RuntimeShape.from(runtimeValue.shape),
    transport,
  );
}
