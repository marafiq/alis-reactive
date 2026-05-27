// gather.ts — Gather values for HTTP requests using the shared ValueProducer concept.
// Every payload assignment is evaluated by evaluateValue(); runtime registered
// inputs use the same writer path after reading their component value member.

import type { Plan, GatherInput, RequestInputAssignment, HttpMethod, RequestInput } from "../types";
import type { ExecContext } from "../types";
import { assertNever } from "../core/assert-never";
import { evaluateValue } from "../core/evaluate";
import { RuntimePlan, type RuntimeComponent } from "../domain/runtime-plan";
import { RuntimeShape } from "../domain/runtime-shape";
import { GatheredRequestInput, writeGatheredValue, type GatherResult, type RequestPayloadWriter } from "./request-payload-writer";
import { isMissingRuntimeValue } from "../domain/runtime-value";

export type { GatherResult } from "./request-payload-writer";

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
      return GatheredRequestInput.empty();
    case "gather":
      return resolveGatherInput(input, runtime);
    default:
      return assertNever(input, "request input");
  }
}

function resolveGatherInput(input: GatherInput, runtime: GatherRuntime): GatherResult {
  const gathered = GatheredRequestInput.for(input.bodyFormat, runtime.method);

  for (const assignment of input.assignments) {
    writeRequestInputAssignment(assignment, gathered, runtime);
  }

  writeRuntimeSelectedInputs(input.sourceSelection, runtime, gathered.writer);

  return gathered.toResult();
}

function writeRequestInputAssignment(
  assignment: RequestInputAssignment,
  gathered: GatheredRequestInput,
  runtime: GatherRuntime,
): void {
  const raw = evaluateValue(assignment.source, runtime.plan, runtime.ctx);
  const shape = RuntimeShape.declaredBy(assignment.source);
  switch (assignment.target.kind) {
    case "payload":
      writeGatheredValue(assignment.target, raw, shape, gathered.writer);
      return;
    case "header":
      if (isMissingRuntimeValue(raw)) return;
      gathered.writeHeader(assignment.target.name, raw, shape);
      return;
    case "route-param":
      gathered.writeRouteParameter(assignment.target.name, raw, shape);
      return;
    default:
      return assertNever(assignment.target, "request input target");
  }
}

function writeRuntimeSelectedInputs(
  sourceSelection: GatherInput["sourceSelection"],
  runtime: GatherRuntime,
  writer: RequestPayloadWriter,
): void {
  switch (sourceSelection.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of runtime.runtimePlan.components.entries()) {
        writeRuntimeRegisteredInput(component, writer);
      }
      return;
    default:
      return assertNever(sourceSelection, "gather source selection");
  }
}

function writeRuntimeRegisteredInput(
  component: RuntimeComponent,
  writer: RequestPayloadWriter,
): void {
  const binding = component.definition.binding;
  if (binding.kind === "none") return;

  const registeredInputIsMounted = component.tryElement() !== undefined;
  if (!registeredInputIsMounted) return;

  const object = component.object();
  const runtimeValue = object.read(binding.valueMember);

  writeGatheredValue(
    {
      kind: "payload",
      name: binding.bindingPath,
      path: binding.path,
    },
    runtimeValue.usingDeclaredShape(),
    RuntimeShape.from(runtimeValue.shape),
    writer,
  );
}
