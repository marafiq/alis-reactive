import type { Plan, RequestPlan, RequestValidation, Workflow } from "../types";
import { setLevel } from "../core/trace";
import { scope } from "../core/trace";
import { wireWorkflow } from "../execution/trigger";
import { wireLiveValidation, unwireFields } from "../validation/live-clear";
import { clearSummary, findSummaryElement, hideSummaryDiv } from "../validation/error-display";
import {
  applyMergedPlan,
  getBootedPlan as getTrackedBootedPlan,
  registerBootedPlan,
  resetMergePlanState,
} from "./merge-plan";

const log = scope("boot");
const BOOTED_ATTR = "alisBooted";

let bootAbort = new AbortController();

export function boot(plan: Plan): void {
  log.info("booting", { workflows: plan.workflows.length });

  wireRequestValidations(plan);
  wireWorkflows(plan.workflows, plan, bootAbort.signal);

  registerBootedPlan(plan);
  document.documentElement.dataset[BOOTED_ATTR] = "true";
  log.info("booted");
}

function wireWorkflows(workflows: Workflow[], plan: Plan, signal?: AbortSignal): void {
  const deferred: Workflow[] = [];

  for (const workflow of workflows) {
    if (workflow.when.kind === "dom-ready") {
      deferred.push(workflow);
    } else {
      wireWorkflow(workflow.when, workflow.run, { plan }, signal);
    }
  }

  for (const workflow of deferred) {
    wireWorkflow(workflow.when, workflow.run, { plan }, signal);
  }
}

export function mergePlan(incoming: Plan): void {
  const merged = applyMergedPlan(incoming, { wireWorkflows, unwireFields });
  wireRequestValidations(merged);
  clearSummaryForPlan(merged.planId);

  log.info("merge", {
    planId: merged.planId,
    newObjects: Object.keys(incoming.objects).length,
    newWorkflows: incoming.workflows.length,
  });
}

export function getBootedPlan(planId: string): Plan | undefined {
  return getTrackedBootedPlan(planId);
}

export function resetBootStateForTests(): void {
  bootAbort.abort();
  bootAbort = new AbortController();
  resetMergePlanState();
  delete document.documentElement.dataset[BOOTED_ATTR];
}

export const trace = { setLevel };

function wireRequestValidations(plan: Plan): void {
  for (const workflow of plan.workflows) {
    walkActionRequests(workflow.run, validation => wireLiveValidation(plan, validation));
  }
}

function walkActionRequests(action: import("../types").PlanAction, visitor: (validation: RequestValidation) => void): void {
  switch (action.kind) {
    case "sequence":
      for (const step of action.steps) walkActionRequests(step, visitor);
      return;

    case "branch":
      for (const branch of action.cases) walkActionRequests(branch.run, visitor);
      return;

    case "parallel":
      for (const step of action.steps) walkActionRequests(step, visitor);
      if (action.onSettled) walkActionRequests(action.onSettled, visitor);
      return;

    case "request":
      walkRequest(action.request, visitor);
      return;

    default:
      return;
  }
}

function walkRequest(request: RequestPlan, visitor: (validation: RequestValidation) => void): void {
  if (request.validation) visitor(request.validation);
  for (const action of request.before ?? []) walkActionRequests(action, visitor);
  for (const handler of request.onSuccess ?? []) walkActionRequests(handler.run, visitor);
  for (const handler of request.onError ?? []) walkActionRequests(handler.run, visitor);
  for (const action of request.onSettled ?? []) walkActionRequests(action, visitor);
  if (request.next) walkRequest(request.next, visitor);
}

function clearSummaryForPlan(planId: string): void {
  const el = findSummaryElement(planId);
  if (el) {
    clearSummary(el);
    hideSummaryDiv(el);
  }
}
