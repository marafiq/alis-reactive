// conditions.ts — V3 ConditionGraph evaluation.
// The SYNC subset (compare/all/any/not) lives in ./compare-engine, a DI leaf that
// receives the value-evaluator as a parameter (so it never imports values/evaluate). This
// module keeps the public entry points and owns the async lane: confirm is the only term
// that crosses to async, so full ConditionGraph evaluation may return a Promise.

import type {
  ConditionGraph,
  PlanDocument,
  ExecContext,
  ValidationCondition,
} from "../types/index";
import { scope } from "../diagnostics/trace";
import { assertNever } from "../shared/assert-never";
import { evaluateValue } from "../values/evaluate";
import { ExecutionContext } from "../browser-objects/execution-context";
import { evaluateSyncCondition, evaluateCompare } from "./compare-engine";

const log = scope("conditions");

interface AlisBrowserApi {
  readonly alis?: {
    readonly confirm?: (message: string) => Promise<boolean> | boolean;
  };
}

/** Sync condition evaluation for validation conditions (compare/all/any/not). */
export function evaluateCondition(condition: ValidationCondition, plan: PlanDocument, ctx?: ExecContext): boolean {
  return evaluateSyncCondition(condition, plan, ExecutionContext.from(ctx), evaluateValue);
}

/** Current-lane condition evaluation. Crosses to async only when a reached term requires it. */
export function evaluateConditionInCurrentLane(
  condition: ConditionGraph,
  plan: PlanDocument,
  ctx?: ExecContext,
): boolean | Promise<boolean> {
  return evaluateConditionInLane(condition, plan, ExecutionContext.from(ctx));
}

function evaluateConditionInLane(
  condition: ConditionGraph,
  plan: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, plan, context, evaluateValue);
    case "all":
      return evaluateAllInLane(condition.terms, plan, context, 0);
    case "any":
      return evaluateAnyInLane(condition.terms, plan, context, 0);
    case "not":
      return negateConditionInLane(condition.term, plan, context);
    case "confirm":
      return evaluateConfirmCondition(condition.message);
    default:
      return assertNever(condition, "condition kind");
  }
}

function evaluateAllInLane(
  terms: readonly ConditionGraph[],
  plan: PlanDocument,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, plan, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches =>
        matches ? evaluateAllInLane(terms, plan, context, index + 1) : false);
    }

    if (!termMatches) return false;
  }

  return true;
}

function evaluateAnyInLane(
  terms: readonly ConditionGraph[],
  plan: PlanDocument,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, plan, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches =>
        matches ? true : evaluateAnyInLane(terms, plan, context, index + 1));
    }

    if (termMatches) return true;
  }

  return false;
}

function negateConditionInLane(
  condition: ConditionGraph,
  plan: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  const termMatches = evaluateConditionInLane(condition, plan, context);
  if (termMatches instanceof Promise) return termMatches.then(matches => !matches);

  return !termMatches;
}

async function evaluateConfirmCondition(message: string): Promise<boolean> {
  const confirmFn = (window as AlisBrowserApi).alis?.confirm;
  if (!confirmFn) {
    log.error("confirm.dialog-missing");
    throw new Error("[alis] confirm condition requires @Html.FusionConfirmDialog() in layout");
  }

  const accepted = await confirmFn(message);
  log.debug("confirm.result", { accepted, message });
  return accepted;
}
