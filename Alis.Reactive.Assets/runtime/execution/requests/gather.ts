// Request input gathering uses the same ValueExpression resolver as execution.
// Every payload assignment is evaluated by evaluateValue; runtime selected
// registered inputs use the same writer path after reading their component value member.

import type { PlanDocument, GatherRequestInput, RequestInputAssignment, HttpMethod, RequestInput } from "../../types/index";
import type { ExecContext } from "../../types/index";
import { assertNever } from "../../shared/assert-never";
import { evaluateValue } from "../../values/evaluate";
import { toString } from "../../shared/shape-convert";
import { RuntimePlan, type RuntimeComponent } from "../../browser-objects/runtime-plan";
import { RuntimeShape } from "../../browser-objects/runtime-shape";
import {
  requestPayloadWriterFor,
  writeRequestPayloadValue,
  type ResolvedRequestInput,
} from "./request-payload-writer";
import { isMissingRuntimeValue } from "../../browser-objects/runtime-value";

export type { ResolvedRequestInput } from "./request-payload-writer";

interface GatherExecution {
  readonly method: HttpMethod;
  readonly plan: PlanDocument;
  readonly runtimePlan: RuntimePlan;
  readonly ctx: ExecContext;
}

type RequestPayloadWriter = ReturnType<typeof requestPayloadWriterFor>;

function emptyRequestInput(): ResolvedRequestInput {
  return { urlParams: [], routeParams: {}, headers: {}, body: {} };
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

  writeAuthoredAssignments(input.assignments, requestInput, writer, execution);
  writeRuntimeSelectedInputs(input.registeredInputs, execution, writer);

  return requestInput;
}

function writeAuthoredAssignments(
  assignments: RequestInputAssignment[],
  requestInput: ResolvedRequestInput,
  writer: RequestPayloadWriter,
  execution: GatherExecution,
): void {
  for (const assignment of assignments) {
    writeRequestInputAssignment(assignment, requestInput, writer, execution);
  }
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

function writeRequestHeader(
  requestInput: ResolvedRequestInput,
  name: string,
  value: unknown,
  shape: RuntimeShape,
): void {
  requestInput.headers[name] = requestScalarWireValue("header", name, value, shape);
}

function writeRequestRouteParam(
  requestInput: ResolvedRequestInput,
  name: string,
  value: unknown,
  shape: RuntimeShape,
): void {
  const valueIsMissing = value === null || value === undefined;
  if (valueIsMissing) {
    throw new Error(`[alis] route param "${name}" evaluated to null; cannot build URL`);
  }

  requestInput.routeParams[name] = requestScalarWireValue("route param", name, value, shape);
}

function requestScalarWireValue(
  targetKind: "header" | "route param",
  name: string,
  value: unknown,
  shape: RuntimeShape,
): string {
  const wire = shape.formatForWire(value);
  const result = toString(wire);
  if (result.ok) return result.value;

  throw new Error(`[alis] ${targetKind} "${name}" cannot be serialized as a scalar: ${result.error}`);
}

function writeRuntimeSelectedInputs(
  registeredInputs: GatherRequestInput["registeredInputs"],
  execution: GatherExecution,
  writer: RequestPayloadWriter,
): void {
  switch (registeredInputs.kind) {
    case "explicit":
      return;
    case "all-registered-inputs":
      for (const component of execution.runtimePlan.components.entries()) {
        writeMountedRegisteredInput(component, writer);
      }
      return;
    default:
      return assertNever(registeredInputs, "registered input selection");
  }
}

function writeMountedRegisteredInput(
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
