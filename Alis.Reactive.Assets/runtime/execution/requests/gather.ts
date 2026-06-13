// Request input gathering uses the same ValueExpression resolver as execution.
// Authored assignments and IncludeAll registered inputs share the same request input writer.

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
  readonly planDocument: PlanDocument;
  readonly runtimePlan: RuntimePlan;
  readonly context: ExecContext;
}

type RequestInputWriter = ReturnType<typeof requestPayloadWriterFor>;

function emptyRequestInput(): ResolvedRequestInput {
  return { urlParams: [], routeParams: {}, headers: {}, body: {} };
}

export function resolveRequestInput(
  authoredInput: RequestInput,
  method: HttpMethod,
  planDocument: PlanDocument,
  context: ExecContext,
): ResolvedRequestInput {
  const execution = {
    method,
    planDocument,
    runtimePlan: RuntimePlan.from(planDocument),
    context,
  };
  switch (authoredInput.kind) {
    case "none":
      return emptyRequestInput();
    case "gather":
      return resolveGatherRequestInput(authoredInput, execution);
    default:
      return assertNever(authoredInput, "request input");
  }
}

function resolveGatherRequestInput(
  authoredInput: GatherRequestInput,
  execution: GatherExecution,
): ResolvedRequestInput {
  const resolvedInput = emptyRequestInput();
  const writer = requestPayloadWriterFor(resolvedInput, authoredInput.bodyFormat, execution.method);

  writeAuthoredAssignments(authoredInput.assignments, resolvedInput, writer, execution);
  writeRuntimeSelectedInputs(authoredInput.registeredInputs, execution, writer);

  return resolvedInput;
}

function writeAuthoredAssignments(
  assignments: RequestInputAssignment[],
  resolvedInput: ResolvedRequestInput,
  writer: RequestInputWriter,
  execution: GatherExecution,
): void {
  for (const assignment of assignments) {
    writeRequestInputAssignment(assignment, resolvedInput, writer, execution);
  }
}

function writeRequestInputAssignment(
  assignment: RequestInputAssignment,
  resolvedInput: ResolvedRequestInput,
  writer: RequestInputWriter,
  execution: GatherExecution,
): void {
  const rawValue = evaluateValue(assignment.source, execution.planDocument, execution.context);
  const shape = RuntimeShape.declaredBy(assignment.source);
  switch (assignment.target.kind) {
    case "payload":
      writeRequestPayloadValue(assignment.target, rawValue, shape, writer);
      return;
    case "header":
      if (isMissingRuntimeValue(rawValue)) return;
      writeRequestHeader(resolvedInput, assignment.target.name, rawValue, shape);
      return;
    case "route-param":
      writeRequestRouteParam(resolvedInput, assignment.target.name, rawValue, shape);
      return;
    default:
      return assertNever(assignment.target, "request input target");
  }
}

function writeRequestHeader(
  resolvedInput: ResolvedRequestInput,
  name: string,
  value: unknown,
  shape: RuntimeShape,
): void {
  resolvedInput.headers[name] = requestScalarWireValue("header", name, value, shape);
}

function writeRequestRouteParam(
  resolvedInput: ResolvedRequestInput,
  name: string,
  value: unknown,
  shape: RuntimeShape,
): void {
  const valueIsMissing = value === null || value === undefined;
  if (valueIsMissing) {
    throw new Error(`[alis] route param "${name}" evaluated to null; cannot build URL`);
  }

  resolvedInput.routeParams[name] = requestScalarWireValue("route param", name, value, shape);
}

function requestScalarWireValue(
  targetKind: "header" | "route param",
  name: string,
  value: unknown,
  shape: RuntimeShape,
): string {
  const wire = shape.formatForWire(value);
  const stringConversion = toString(wire);
  if (stringConversion.ok) return stringConversion.value;

  throw new Error(`[alis] ${targetKind} "${name}" cannot be serialized as a scalar: ${stringConversion.error}`);
}

function writeRuntimeSelectedInputs(
  registeredInputs: GatherRequestInput["registeredInputs"],
  execution: GatherExecution,
  writer: RequestInputWriter,
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
  writer: RequestInputWriter,
): void {
  const binding = component.definition.binding;
  if (binding.kind === "none") return;

  const registeredInputIsMounted = component.tryElement() !== null;
  if (!registeredInputIsMounted) return;

  const runtimeObject = component.object();
  const runtimeValue = runtimeObject.read(binding.valueMember);

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
