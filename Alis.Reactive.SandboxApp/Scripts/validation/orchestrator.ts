// Validation Orchestrator — Fail-Closed
//
// Every declared validation field MUST be accounted for.
// No silent skips. No pass-by-default. Four possible outcomes per field:
//   1. Enriched + visible → validate, error inline
//   2. Enriched + hidden  → validate, error to summary
//   3. Unenriched + unconditional rules → first rule message to summary (block)
//   4. Unenriched + all conditions false → skip (field not needed yet)
//
// Vendor-agnostic: delegates value reading to component.ts via resolveRoot.

import type { ValidationDescriptor, ValidationField } from "../types";
import { resolveRoot } from "../resolution/component";
import { scope } from "../core/trace";
import { walk } from "../core/walk";
import { ruleFails, type PeerReader } from "./rule-engine";
import { evalCondition, type ConditionReader } from "./condition";
import { toString } from "../core/coerce";
import {
  showInline, clearInline, clearAllInline,
  addToSummary, removeSummaryEntry, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline,
} from "./error-display";

const log = scope("validation");

// ── Public API ──────────────────────────────────────────────

export function validate(desc: ValidationDescriptor): boolean {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement(desc.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const container = document.getElementById(desc.formId);
  if (!container) {
    if (desc.fields.length > 0) {
      log.warn("validate: form container missing, blocking", { formId: desc.formId });
      return false;
    }
    return true;
  }

  const byName = buildByName(desc);
  const condReader = domConditionReader(byName);
  const peerReader = domPeerReader(byName);
  let valid = true;
  let summaryHasErrors = false;

  for (const f of desc.fields) {
    if (!evaluateField(f, desc.formId, container, condReader, peerReader, summaryEl)) {
      valid = false;
      summaryHasErrors = summaryHasErrors || hasSummaryEntry(summaryEl, f.fieldName);
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);

  log.debug("validate", { formId: desc.formId, valid });
  return valid;
}

/** Re-validate a single field on blur/change (live-validate). */
export function revalidateField(desc: ValidationDescriptor, field: ValidationField): void {
  if (!field.fieldId || !field.vendor || !field.readExpr) return;

  clearInline(desc.formId, field);

  const container = document.getElementById(desc.formId);
  if (!container) return;

  const el = document.getElementById(field.fieldId);
  if (!el || !container.contains(el)) return;

  const byName = buildByName(desc);
  const condReader = domConditionReader(byName);
  const peerReader = domPeerReader(byName);
  const summaryEl = findSummaryElement(desc.planId);

  evaluateField(field, desc.formId, container, condReader, peerReader, summaryEl);
}

// ── Per-field evaluation (shared by validate + revalidateField) ──

/** Handles fields that cannot be resolved (unenriched or missing element). */
function handleUnresolvableField(
  f: ValidationField, condReader: ConditionReader, summaryEl: HTMLElement | null
): boolean {
  if (allRulesConditionallySkipped(f, condReader)) return true;
  if (f.rules.length > 0 && summaryEl) {
    addToSummary(summaryEl, f.fieldName, f.rules[0].message);
  }
  return false;
}

/**
 * Checks a rule's When condition. Returns:
 *   "skip"  — condition is false, skip this rule
 *   "block" — condition is unresolvable (null), block with summary
 *   "eval"  — condition passed or no condition, evaluate the rule
 */
function checkRuleCondition(
  rule: ValidationRule, condReader: ConditionReader
): "skip" | "block" | "eval" {
  if (!rule.when) return "eval";
  const result = evalCondition(rule.when, condReader);
  if (result === false) return "skip";
  if (result === null) return "block";
  return "eval";
}

/** Reports a validation failure — routes to inline or summary based on visibility. */
function reportFailure(
  f: ValidationField, message: string, hidden: boolean,
  formId: string, summaryEl: HTMLElement | null
): void {
  if (hidden) {
    if (summaryEl) addToSummary(summaryEl, f.fieldName, message);
  } else {
    showInline(formId, f, message);
    if (summaryEl) removeSummaryEntry(summaryEl, f.fieldName);
  }
}

/** Evaluates all rules for a resolved field. Returns true if all pass. */
function evaluateRules(
  f: ValidationField, value: unknown, hidden: boolean,
  formId: string, condReader: ConditionReader, peerReader: PeerReader,
  summaryEl: HTMLElement | null
): boolean {
  for (const rule of f.rules) {
    const condStatus = checkRuleCondition(rule, condReader);
    if (condStatus === "skip") continue;
    if (condStatus === "block") {
      if (summaryEl) addToSummary(summaryEl, f.fieldName, rule.message);
      return false;
    }

    if (ruleFails(rule, value, peerReader)) {
      reportFailure(f, rule.message, hidden, formId, summaryEl);
      return false;
    }
  }
  return true;
}

function evaluateField(
  f: ValidationField, formId: string, container: HTMLElement,
  condReader: ConditionReader, peerReader: PeerReader,
  summaryEl: HTMLElement | null
): boolean {
  if (!f.fieldId || !f.vendor || !f.readExpr) {
    return handleUnresolvableField(f, condReader, summaryEl);
  }

  const el = document.getElementById(f.fieldId);
  if (!el) {
    return handleUnresolvableField(f, condReader, summaryEl);
  }

  if (!container.contains(el)) {
    log.trace("field outside form, skipping", { fieldName: f.fieldName, formId });
    return true;
  }

  const errorSpan = document.getElementById(f.fieldId + "_error");
  const hidden = errorSpan?.parentElement ? isHidden(errorSpan.parentElement) : true;
  const root = resolveRoot(el, f.vendor);
  const value = walk(root, f.readExpr);

  return evaluateRules(f, value, hidden, formId, condReader, peerReader, summaryEl);
}

function hasSummaryEntry(summaryEl: HTMLElement | null, fieldName: string): boolean {
  if (!summaryEl) return false;
  return summaryEl.querySelector(`[data-valmsg-summary-for="${fieldName}"]`) !== null;
}

export function showServerErrors(desc: ValidationDescriptor, data: unknown): void {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement(desc.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const errors = extractErrors(data);
  if (!errors) return;

  let summaryHasErrors = false;

  for (const [name, msgs] of Object.entries(errors)) {
    const msg = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);

    const spanExists = findErrorSpanExists(name, desc.fields);
    if (spanExists) {
      showServerErrorInline(desc.formId, name, msg, desc.fields);
    } else if (summaryEl) {
      addToSummary(summaryEl, name, msg);
      summaryHasErrors = true;
    }
  }

  if (summaryHasErrors && summaryEl) showSummaryDiv(summaryEl);

  log.debug("showServerErrors", { formId: desc.formId, fieldCount: Object.keys(errors).length });
}

export function clearAll(desc: ValidationDescriptor): void {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement(desc.planId);
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

// ── DOM readers (bridge pure modules ↔ DOM) ─────────────

/**
 * Returns true if every rule on this field has a condition AND that condition evaluates to false.
 * Used for unenriched fields: if all rules are conditionally suppressed (e.g., AddressType != "Custom Address"),
 * the field doesn't need a component yet and shouldn't block.
 * If ANY rule is unconditional or has a true/null condition, returns false (must block).
 */
function allRulesConditionallySkipped(f: ValidationField, condReader: ConditionReader): boolean {
  if (f.rules.length === 0) return true;
  for (const rule of f.rules) {
    if (!rule.when) return false; // unconditional rule → must block
    const result = evalCondition(rule.when, condReader);
    if (result !== false) return false; // condition met or unresolvable → must block
  }
  return true; // all conditions false → skip
}

function domConditionReader(byName: Map<string, ValidationField>): ConditionReader {
  return {
    readConditionSource(fieldName: string): string | undefined {
      const srcField = byName.get(fieldName);
      if (!srcField?.fieldId || !srcField.vendor || !srcField.readExpr)
        return undefined;
      const el = document.getElementById(srcField.fieldId);
      if (!el) return undefined;
      const root = resolveRoot(el, srcField.vendor);
      const val = walk(root, srcField.readExpr);
      // Normalize: null/undefined/false → "" (empty = no value expressed)
      return (val == null || val === false) ? "" : toString(val);
    },
  };
}

function domPeerReader(byName: Map<string, ValidationField>): PeerReader {
  return {
    readPeer(fieldName: string): unknown {
      const other = byName.get(fieldName);
      if (!other?.fieldId || !other.vendor || !other.readExpr) return undefined;
      const otherEl = document.getElementById(other.fieldId);
      if (!otherEl) return undefined;
      const otherRoot = resolveRoot(otherEl, other.vendor);
      return walk(otherRoot, other.readExpr);
    },
  };
}

// ── Helpers ─────────────────────────────────────────────

function buildByName(desc: ValidationDescriptor): Map<string, ValidationField> {
  const map = new Map<string, ValidationField>();
  for (const f of desc.fields) map.set(f.fieldName, f);
  return map;
}

function findErrorSpanExists(fieldName: string, fields: ValidationField[]): boolean {
  const field = fields.find(f => f.fieldName === fieldName);
  if (!field?.fieldId) return false;
  return document.getElementById(field.fieldId + "_error") !== null;
}

/** Visibility check — owned by orchestrator (routing decision), not error-display. */
function isHidden(el: HTMLElement): boolean {
  let node: HTMLElement | null = el;
  while (node) {
    if (node.hasAttribute("hidden") || node.style?.display === "none") return true;
    node = node.parentElement;
  }
  return false;
}

/**
 * Extracts field errors from server response.
 * Accepts only ProblemDetails shape: { errors: Record<string, string[]> }.
 * Rejects arbitrary objects — validation lane should not reinterpret random payloads.
 */
function extractErrors(data: unknown): Record<string, unknown> | null {
  if (!data || typeof data !== "object") return null;
  const obj = data as Record<string, unknown>;
  if ("errors" in obj && typeof obj.errors === "object" && obj.errors !== null) {
    return obj.errors as Record<string, unknown>;
  }
  log.warn("showServerErrors: response is not ProblemDetails shape, ignoring", {});
  return null;
}
