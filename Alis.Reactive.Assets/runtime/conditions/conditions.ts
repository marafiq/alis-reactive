// ConditionGraph stays in the current lane until a reached confirm term crosses async.
// The sync subset delegates to compare-engine so evaluateValue remains the only resolver.

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

interface WindowWithConfirm {
  readonly alis?: {
    readonly confirm?: (message: string) => Promise<boolean> | boolean;
  };
}

// Validation conditions cannot contain confirm, so evaluation is always sync.
export function evaluateCondition(
  condition: ValidationCondition,
  planDocument: PlanDocument,
  context?: ExecContext,
): boolean {
  return evaluateSyncCondition(
    condition,
    planDocument,
    ExecutionContext.from(context),
    evaluateValue,
  );
}

// Branch execution depends on sync results staying sync until confirm is reached.
export function evaluateConditionInCurrentLane(
  condition: ConditionGraph,
  planDocument: PlanDocument,
  context?: ExecContext,
): boolean | Promise<boolean> {
  return evaluateConditionInLane(condition, planDocument, ExecutionContext.from(context));
}

function evaluateConditionInLane(
  condition: ConditionGraph,
  planDocument: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  switch (condition.kind) {
    case "compare":
      return evaluateCompare(condition, planDocument, context, evaluateValue);
    case "all":
      return evaluateAllInLane(condition.terms, planDocument, context, 0);
    case "any":
      return evaluateAnyInLane(condition.terms, planDocument, context, 0);
    case "not":
      return negateConditionInLane(condition.term, planDocument, context);
    case "confirm":
      return evaluateConfirmCondition(condition.message);
    default:
      return assertNever(condition, "condition kind");
  }
}

function evaluateAllInLane(
  terms: readonly ConditionGraph[],
  planDocument: PlanDocument,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, planDocument, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches => matches
        ? evaluateAllInLane(terms, planDocument, context, index + 1)
        : false);
    }

    if (!termMatches) return false;
  }

  return true;
}

function evaluateAnyInLane(
  terms: readonly ConditionGraph[],
  planDocument: PlanDocument,
  context: ExecutionContext,
  startIndex: number,
): boolean | Promise<boolean> {
  for (let index = startIndex; index < terms.length; index++) {
    const termMatches = evaluateConditionInLane(terms[index]!, planDocument, context);
    if (termMatches instanceof Promise) {
      return termMatches.then(matches => matches
        ? true
        : evaluateAnyInLane(terms, planDocument, context, index + 1));
    }

    if (termMatches) return true;
  }

  return false;
}

function negateConditionInLane(
  condition: ConditionGraph,
  planDocument: PlanDocument,
  context: ExecutionContext,
): boolean | Promise<boolean> {
  const termMatches = evaluateConditionInLane(condition, planDocument, context);
  if (termMatches instanceof Promise) return termMatches.then(matches => !matches);

  return !termMatches;
}

async function evaluateConfirmCondition(message: string): Promise<boolean> {
  const confirmFn = (window as WindowWithConfirm).alis?.confirm;
  if (!confirmFn) {
    log.error("confirm.dialog-missing");
    throw new Error("[alis] confirm condition requires @Html.FusionConfirmDialog() in layout");
  }

  const accepted = await confirmFn(message);
  log.debug("confirm.result", { accepted, message });
  return accepted;
}
