// gather.ts — Resolve request input projections using the shared ValueProducer concept.
// Every payload assignment is evaluated by evaluateValue(); runtime selected
// registered inputs use the same writer path after reading their component value member.

import type { PlanDocument, RequestInputProjection, RequestInputAssignment, HttpMethod, RequestInput } from "../types";
import type { ExecContext } from "../types";
import { assertNever } from "../core/assert-never";
import { evaluateValue } from "../core/evaluate";
import { RuntimePlan, type RuntimeComponent } from "../domain/runtime-plan";
import { RuntimeShape } from "../domain/runtime-shape";
import { RequestInputResolution, writeRequestPayloadValue, type ResolvedRequestInput, type RequestPayloadWriter } from "./request-payload-writer";
import { isMissingRuntimeValue } from "../domain/runtime-value";

export type { ResolvedRequestInput } from "./request-payload-writer";

interface RequestInputRuntime {
  readonly method: HttpMethod;
  readonly plan: PlanDocument;
  readonly runtimePlan: RuntimePlan;
  readonly ctx: ExecContext;
}

/**
 * Resolve request input into URL params, route params, headers, and body/FormData.
 */
export function resolveRequestInput(
  input: RequestInput,
  method: HttpMethod,
  plan: PlanDocument,
  ctx: ExecContext,
): ResolvedRequestInput {
  const runtime = { method, plan, runtimePlan: RuntimePlan.from(plan), ctx };
  return resolveRequestInputPlan(input, runtime);
}

function resolveRequestInputPlan(input: RequestInput, runtime: RequestInputRuntime): ResolvedRequestInput {
  switch (input.kind) {
    case "none":
      return RequestInputResolution.empty();
    case "gather":
      return resolveInputProjection(input, runtime);
    default:
      return assertNever(input, "request input");
  }
}

function resolveInputProjection(input: RequestInputProjection, runtime: RequestInputRuntime): ResolvedRequestInput {
  const resolved = RequestInputResolution.for(input.bodyFormat, runtime.method);

  for (const assignment of input.assignments) {
    writeRequestInputAssignment(assignment, resolved, runtime);
  }

  writeSelectedRegisteredInputs(input.sourceSelection, runtime, resolved.writer);

  return resolved.toResult();
}

function writeRequestInputAssignment(
  assignment: RequestInputAssignment,
  resolved: RequestInputResolution,
  runtime: RequestInputRuntime,
): void {
  const raw = evaluateValue(assignment.source, runtime.plan, runtime.ctx);
  const shape = RuntimeShape.declaredBy(assignment.source);
  switch (assignment.target.kind) {
    case "payload":
      writeRequestPayloadValue(assignment.target, raw, shape, resolved.writer);
      return;
    case "header":
      if (isMissingRuntimeValue(raw)) return;
      resolved.writeHeader(assignment.target.name, raw, shape);
      return;
    case "route-param":
      resolved.writeRouteParameter(assignment.target.name, raw, shape);
      return;
    default:
      return assertNever(assignment.target, "request input target");
  }
}

function writeSelectedRegisteredInputs(
  sourceSelection: RequestInputProjection["sourceSelection"],
  runtime: RequestInputRuntime,
  writer: RequestPayloadWriter,
): void {
  switch (sourceSelection.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of runtime.runtimePlan.components.entries()) {
        writeSelectedRegisteredInput(component, writer);
      }
      return;
    default:
      return assertNever(sourceSelection, "gather source selection");
  }
}

function writeSelectedRegisteredInput(
  component: RuntimeComponent,
  writer: RequestPayloadWriter,
): void {
  const binding = component.definition.binding;
  if (binding.kind === "none") return;

  const registeredInputIsMounted = component.tryElement() !== undefined;
  if (!registeredInputIsMounted) return;

  const object = component.object();
  const runtimeValue = object.read(binding.valueMember);

  writeRequestPayloadValue(
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
