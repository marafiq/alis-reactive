// Validation Orchestrator — Fail-Closed
//
// Every declared validation field MUST be accounted for.
// No silent skips. No pass-by-default. Three possible outcomes per field:
//   1. Enriched + visible → validate, error inline
//   2. Enriched + hidden  → validate, error to summary
//   3. Unenriched         → first rule message to summary (cannot read value)
//
// Vendor-agnostic: delegates value reading to component.ts via resolveRoot.

import type { ValidationDescriptor, ValidationField } from "../types";
import { resolveRoot } from "../component";
import { scope } from "../trace";
import { walk } from "../walk";
import { ruleFails, type PeerReader } from "./rule-engine";
import { evalCondition, type ConditionReader } from "./condition";
import {
  showInline, clearAllInline,
  addToSummary, clearSummary, showSummaryDiv, hideSummaryDiv, findSummaryElement,
  showServerErrorInline, isHidden,
} from "./error-display";

const log = scope("validation");

// ── Public API ──────────────────────────────────────────────

export function validate(desc: ValidationDescriptor): boolean {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement();
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const container = document.getElementById(desc.formId);
  if (!container) {
    // Fail closed: declared validation with no form container → block request
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
    // ── Unenriched: cannot read value → first rule message to summary ──
    if (!f.fieldId || !f.vendor || !f.readExpr) {
      if (f.rules.length > 0 && summaryEl) {
        addToSummary(summaryEl, f.fieldName, f.rules[0].message);
        summaryHasErrors = true;
      }
      valid = false;
      continue;
    }

    const el = document.getElementById(f.fieldId);

    // ── Element not in DOM → first rule message to summary ──
    if (!el) {
      if (f.rules.length > 0 && summaryEl) {
        addToSummary(summaryEl, f.fieldName, f.rules[0].message);
        summaryHasErrors = true;
      }
      valid = false;
      continue;
    }

    // ── Element outside this form → skip (belongs to different form) ──
    if (!container.contains(el)) continue;

    const hidden = isHidden(el);

    // ── Read value (hidden or visible — value is still in DOM) ──
    const root = resolveRoot(el, f.vendor);
    const value = walk(root, f.readExpr);

    // ── Evaluate rules ──
    for (const rule of f.rules) {
      if (rule.when) {
        const condResult = evalCondition(rule.when, condReader);
        if (condResult === false) continue; // condition not met → skip rule
        // condResult === null (source unresolvable) → route to summary
        if (condResult === null) {
          if (summaryEl) {
            addToSummary(summaryEl, f.fieldName, rule.message);
            summaryHasErrors = true;
          }
          valid = false;
          break;
        }
      }

      if (ruleFails(rule, value, peerReader)) {
        if (hidden) {
          // Hidden field error → summary (can't show inline)
          if (summaryEl) {
            addToSummary(summaryEl, f.fieldName, rule.message);
            summaryHasErrors = true;
          }
        } else {
          // Visible field error → inline
          showInline(desc.formId, f, rule.message);
        }
        valid = false;
        break;
      }
    }
  }

  if (summaryHasErrors && summaryEl) {
    showSummaryDiv(summaryEl);
  }

  log.debug("validate", { formId: desc.formId, valid });
  return valid;
}

export function showServerErrors(desc: ValidationDescriptor, data: unknown): void {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement();
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }

  const errors = extractErrors(data);
  if (!errors) return;

  let summaryHasErrors = false;

  for (const [name, msgs] of Object.entries(errors)) {
    const msg = Array.isArray(msgs) ? msgs.join(", ") : String(msgs);

    // Try inline — if a span exists for the field (in any state), display there
    const spanExists = findErrorSpanExists(desc.formId, name);

    if (spanExists) {
      showServerErrorInline(desc.formId, name, msg, desc.fields);
    } else if (summaryEl) {
      // No span at all → route to summary
      addToSummary(summaryEl, name, msg);
      summaryHasErrors = true;
    }
  }

  if (summaryHasErrors && summaryEl) {
    showSummaryDiv(summaryEl);
  }

  log.debug("showServerErrors", { formId: desc.formId, fieldCount: Object.keys(errors).length });
}

export function clearAll(desc: ValidationDescriptor): void {
  clearAllInline(desc.formId, desc.fields);
  const summaryEl = findSummaryElement();
  if (summaryEl) {
    clearSummary(summaryEl);
    hideSummaryDiv(summaryEl);
  }
}

// ── DOM readers (bridge pure modules ↔ DOM) ─────────────

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
      return val == null ? "" : String(val);
    },
  };
}

function domPeerReader(byName: Map<string, ValidationField>): PeerReader {
  return {
    readPeer(fieldName: string): string | undefined {
      const other = byName.get(fieldName);
      if (!other?.fieldId || !other.vendor || !other.readExpr) return undefined;
      const otherEl = document.getElementById(other.fieldId);
      if (!otherEl) return undefined;
      const otherRoot = resolveRoot(otherEl, other.vendor);
      return String(walk(otherRoot, other.readExpr) ?? "");
    },
  };
}

// ── Helpers ─────────────────────────────────────────────

function buildByName(desc: ValidationDescriptor): Map<string, ValidationField> {
  const map = new Map<string, ValidationField>();
  for (const f of desc.fields) map.set(f.fieldName, f);
  return map;
}

function findErrorSpanExists(containerId: string, fieldName: string): boolean {
  const container = document.getElementById(containerId);
  if (!container) return false;
  return container.querySelector(`span[data-valmsg-for="${fieldName}"]`) !== null;
}

function extractErrors(data: unknown): Record<string, unknown> | null {
  if (!data || typeof data !== "object") return null;
  if ("errors" in data && typeof (data as Record<string, unknown>).errors === "object") {
    return (data as Record<string, Record<string, unknown>>).errors;
  }
  return data as Record<string, unknown>;
}
