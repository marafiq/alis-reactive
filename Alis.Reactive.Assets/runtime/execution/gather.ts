// gather.ts — Resolve gathered request input using the shared ValueExpression concept.
// Every payload assignment is evaluated by evaluateValue(); runtime selected
// registered inputs use the same writer path after reading their component value member.

import type { PlanDocument, GatherRequestInput, RequestInputAssignment, HttpMethod, RequestInput } from "../types";
import type { ExecContext } from "../types";
import { assertNever } from "../core/assert-never";
import { evaluateValue } from "../core/evaluate";
import { RuntimePlan, type RuntimeComponent } from "../domain/runtime-plan";
import { RuntimeShape } from "../domain/runtime-shape";
import {
  emptyRequestInput,
  requestPayloadWriterFor,
  writeRequestHeader,
  writeRequestPayloadValue,
  writeRequestRouteParam,
  type ResolvedRequestInput,
  type RequestPayloadWriter,
} from "./request-payload-writer";
import { isMissingRuntimeValue } from "../domain/runtime-value";

export type { ResolvedRequestInput } from "./request-payload-writer";

interface GatherExecution {
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
  const execution = { method, plan, runtimePlan: RuntimePlan.from(plan), ctx };
  switch (input.kind) {
    case "none":
      return emptyRequestInput();
    case "gather":
      return resolveGatherRequestInput(input, execution);
    default:
      return assertNever(input, "request input");
  }
}

function resolveGatherRequestInput(input: GatherRequestInput, execution: GatherExecution): ResolvedRequestInput {
  const requestInput = emptyRequestInput();
  const writer = requestPayloadWriterFor(requestInput, input.bodyFormat, execution.method);

  for (const assignment of input.assignments) {
    writeRequestInputAssignment(assignment, requestInput, writer, execution);
  }

  writeRegisteredInputs(input.registeredInputs, execution, writer);

  return requestInput;
}

function writeRequestInputAssignment(
  assignment: RequestInputAssignment,
  requestInput: ResolvedRequestInput,
  writer: RequestPayloadWriter,
  execution: GatherExecution,
): void {
  const raw = evaluateValue(assignment.source, execution.plan, execution.ctx);
  const shape = RuntimeShape.declaredBy(assignment.source);
  switch (assignment.target.kind) {
    case "payload":
      writeRequestPayloadValue(assignment.target, raw, shape, writer);
      return;
    case "header":
      if (isMissingRuntimeValue(raw)) return;
      writeRequestHeader(requestInput, assignment.target.name, raw, shape);
      return;
    case "route-param":
      writeRequestRouteParam(requestInput, assignment.target.name, raw, shape);
      return;
    default:
      return assertNever(assignment.target, "request input target");
  }
}

function writeRegisteredInputs(
  registeredInputs: GatherRequestInput["registeredInputs"],
  execution: GatherExecution,
  writer: RequestPayloadWriter,
): void {
  switch (registeredInputs.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of execution.runtimePlan.components.entries()) {
        writeSelectedRegisteredInput(component, writer);
      }
      return;
    default:
      return assertNever(registeredInputs, "registered input selection");
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
