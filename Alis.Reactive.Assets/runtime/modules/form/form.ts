// alis.form (grammar.md 6.4), keyed by the validation container's element id. Plans contribute their
// fields under their owner; each field carries its own value read, committed event and tests, so the
// form needs nothing but the evaluator.
// F1 A test applies only when its field's element is inside the container; a missing container fails
//    validation when the form has tests.
// F3 Per field, tests whose `when` is false are skipped and the first failing check decides.
// F4 validate() clears every inline message and the summary, evaluates every field, returns the AND.
// F5 A field with no element is skipped when all its `when`s are false; otherwise the form is invalid
//    and the summary gets the message of the field's first test.
// F6 A message goes inline into {id}_error when that slot can render, else into the summary.
// F7 At creation: DOM `input` clears a field's message; DOM `blur` and its committed event revalidate it.
// F8 showErrors(problem) places server errors by exact binding name; without a record body it validates.
// F9 A slot's load or unload removes exactly its own fields, then clears and hides the summary.

import type { FormField } from "../../types/index";
import type { Contribution, Module } from "../../objects/modules";
import type { Owner } from "../../objects/owner";
import type { PlanWorld } from "../../objects/world";
import { Environment } from "../../grammar/environment";
import { evaluate } from "../../grammar/evaluate";
import { asText } from "../../grammar/shape";
import { scope } from "../../diagnostics/trace";
import {
  addToSummary, canRenderInline, clearInline, clearSummary, hideSummaryDiv, removeSummaryEntry,
  showInline, showServerErrorInline, showSummaryDiv, summaryHasEntries,
} from "./display";

const log = scope("validation");

interface Entry {
  readonly field: FormField;
  readonly world: PlanWorld;
  readonly summary: string;
}

export interface FormObject {
  validate(): boolean;
  showErrors(problem: unknown): void;
}

const formsByContainer = new Map<string, Entry[]>();
const liveWiredElements = new Set<string>();

function entriesOf(containerId: string): Entry[] {
  let entries = formsByContainer.get(containerId);
  if (!entries) {
    entries = [];
    formsByContainer.set(containerId, entries);
  }
  return entries;
}

// ── summary ────────────────────────────────────────────────────────────────

function summaryElement(entries: readonly Entry[]): HTMLElement | null {
  const first = entries[0];
  return first ? document.getElementById(first.summary) : null;
}

function addSummaryError(summary: HTMLElement | null, key: string, message: string): void {
  if (summary === null) return;
  removeSummaryEntry(summary, key);
  addToSummary(summary, key, message);
  showSummaryDiv(summary);
}

function removeSummaryError(summary: HTMLElement | null, key: string): void {
  if (summary === null) return;
  removeSummaryEntry(summary, key);
  if (!summaryHasEntries(summary)) hideSummaryDiv(summary);
}

function clearAndHide(summary: HTMLElement | null): void {
  if (summary === null) return;
  clearSummary(summary);
  hideSummaryDiv(summary);
}

// ── evaluation ─────────────────────────────────────────────────────────────

function evaluateWhen(entry: Entry, test: FormField["tests"][number]): boolean {
  return !!evaluate(test.when, { env: Environment.root(), world: entry.world });
}

function testIsInactive(entry: Entry, test: FormField["tests"][number]): boolean {
  try {
    return !evaluateWhen(entry, test);
  } catch {
    return true;
  }
}

function checkPasses(entry: Entry, test: FormField["tests"][number], value: unknown): boolean {
  const check = evaluate(test.check, { env: Environment.root(), world: entry.world }) as (value: unknown) => unknown;
  return !!check(value);
}

function evaluateField(entry: Entry, container: HTMLElement, summary: HTMLElement | null): boolean {
  const { field } = entry;
  const element = document.getElementById(field.id);
  if (element === null) return unmountedField(entry, summary);

  if (!container.contains(element)) {
    log.trace("field.out-of-scope", { field: field.id });
    return true;
  }

  const inlineHidden = !canRenderInline(field.id);
  const value = evaluate(field.value, { env: Environment.root(), world: entry.world });
  for (const test of field.tests) {
    if (!evaluateWhen(entry, test)) continue;
    if (checkPasses(entry, test, value)) continue;

    log.trace("rule.failed", { field: field.id, rule: test.rule, value, message: test.message });
    if (inlineHidden) {
      addSummaryError(summary, field.id, test.message);
    } else {
      showInline(field.id, test.message);
      removeSummaryError(summary, field.id);
    }
    return false;
  }
  return true;
}

function unmountedField(entry: Entry, summary: HTMLElement | null): boolean {
  const { field } = entry;
  log.trace("validation-field.inactive", { field: field.id });
  const allInactive = field.tests.every(test => testIsInactive(entry, test));
  if (allInactive) return true;
  const first = field.tests[0];
  if (first) addSummaryError(summary, field.id, first.message);
  return false;
}

function validate(containerId: string): boolean {
  const entries = entriesOf(containerId);
  const container = document.getElementById(containerId);
  if (container === null) {
    if (entries.length === 0) return true;
    log.warn("form.missing", { id: containerId });
    return false;
  }

  const summary = summaryElement(entries);
  for (const entry of entries) clearInline(entry.field.id);
  clearAndHide(summary);

  let valid = true;
  for (const entry of entries) {
    if (!evaluateField(entry, container, summary)) valid = false;
  }
  log.debug("validated", { id: containerId, valid });
  return valid;
}

function revalidate(containerId: string, fieldId: string): void {
  const entries = entriesOf(containerId).filter(entry => entry.field.id === fieldId);
  if (entries.length === 0) return;
  const summary = summaryElement(entriesOf(containerId));
  clearInline(fieldId);
  removeSummaryError(summary, fieldId);

  const container = document.getElementById(containerId);
  if (container === null) return;
  for (const entry of entries) {
    if (!evaluateField(entry, container, summary)) return;
  }
}

function showErrors(containerId: string, problem: unknown): void {
  const isRecord = typeof problem === "object" && problem !== null && !Array.isArray(problem);
  if (!isRecord) {
    log.debug("show-validation.client", { id: containerId });
    validate(containerId);
    return;
  }

  const entries = entriesOf(containerId);
  const summary = summaryElement(entries);
  for (const entry of entries) clearInline(entry.field.id);
  clearAndHide(summary);

  const errors = (problem as Record<string, unknown>)["errors"];
  if (typeof errors !== "object" || errors === null || Array.isArray(errors)) {
    log.warn("server-errors.wrong-shape");
    return;
  }

  for (const [name, messages] of Object.entries(errors as Record<string, unknown>)) {
    const message = Array.isArray(messages) ? messages.join(", ") : asText(messages) ?? "";
    const entry = entries.find(candidate => candidate.field.name === name);
    const element = entry ? document.getElementById(entry.field.id) : null;
    if (entry && element && canRenderInline(entry.field.id)) {
      showServerErrorInline(entry.field.id, message, element);
    } else {
      addSummaryError(summary, name, message);
    }
  }
  log.debug("server-errors.shown", { id: containerId });
}

// ── live wiring ────────────────────────────────────────────────────────────

function wireLive(containerId: string, entry: Entry, owner: Owner): void {
  const { field } = entry;
  if (liveWiredElements.has(field.id)) return;
  const element = document.getElementById(field.id);
  if (element === null) return;

  const clear = (): void => clearInline(field.id);
  const recheck = (): void => revalidate(containerId, field.id);
  element.addEventListener("input", clear);
  element.addEventListener("blur", recheck);
  liveWiredElements.add(field.id);
  owner.onDispose(() => {
    element.removeEventListener("input", clear);
    element.removeEventListener("blur", recheck);
    liveWiredElements.delete(field.id);
  });

  try {
    entry.world.subscribe(field.id, field.committed, recheck, owner);
  } catch {
    // The widget is not constructed yet: the committed event is not wired, as today.
    log.trace("committed.deferred", { field: field.id });
  }
}

export const formModule: Module = {
  object: (containerId): FormObject => ({
    validate: () => validate(containerId),
    showErrors: problem => showErrors(containerId, problem),
  }),
  contribute({ key, data, owner, world }: Contribution) {
    if (data.kind !== "form") return;
    const entries = entriesOf(key);
    const added = data.fields.map(field => ({ field, world, summary: data.summary }));
    entries.push(...added);
    if (owner.isSlot) clearAndHide(document.getElementById(data.summary));
    for (const entry of added) wireLive(key, entry, owner);

    owner.onDispose(() => {
      const remaining = entriesOf(key).filter(entry => !added.includes(entry));
      formsByContainer.set(key, remaining);
      clearAndHide(document.getElementById(data.summary));
    });
  },
};

export function resetFormsForTests(): void {
  formsByContainer.clear();
  liveWiredElements.clear();
}
