// alis.form (grammar.md 6.4), keyed by the validation container's element id. Plans contribute their
// fields under their owner; each field carries its own value read, committed event and tests, so the
// form needs nothing but the evaluator.
// F1 A test applies only when its field's element is inside the container; a missing container fails
//    validation when the form has tests.
// F3 Per field, tests whose `when` is false are skipped and the first failing check decides.
// F4 validate() clears every inline message and the summary, evaluates every field, returns the AND.
// F5 A field with no element posts no value: each active test checks a missing value, and the first
//    that fails puts its message in the summary.
// F6 A message goes inline into {id}_error when that slot can render, else into the summary.
// F7 At creation, and whenever HTML mounts (a field a partial loads): DOM `input` clears a field's
//    message; DOM `blur` and alis:commit (the model re-raises each input's own commit event as it)
//    revalidate it. Each element is wired once.
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
import { COMMIT_EVENT, MOUNT_EVENT } from "../../plans/mount-events";
import {
  addToSummary, canRenderInline, clearInline, clearSummary, hideSummaryDiv, removeSummaryEntry,
  showInline, showServerErrorInline, showSummaryDiv, summaryHasEntries,
} from "./display";

const log = scope("validation");

interface Entry {
  readonly field: FormField;
  readonly world: PlanWorld;
  readonly summary: string;
  readonly owner: Owner;
}

export interface FormObject {
  validate(): boolean;
  showErrors(problem: unknown): void;
}

const formsByContainer = new Map<string, Entry[]>();
// The elements wired for live validation. A partial that reloads brings new elements under the same
// ids, so wiring is remembered per element, not per id.
let liveWiredElements = new WeakSet<HTMLElement>();
let mountListened = false;

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

// A field the page does not render posts no value, so its active tests run against nothing, as the
// server's run against a missing member: a required test fails, a length or a pattern passes. A
// failure has no message slot on the page and goes to the summary.
function unmountedField(entry: Entry, summary: HTMLElement | null): boolean {
  const { field } = entry;
  log.trace("validation-field.unmounted", { field: field.id });
  for (const test of field.tests) {
    if (!evaluateWhen(entry, test)) continue;
    if (checkPasses(entry, test, undefined)) continue;
    addSummaryError(summary, field.id, test.message);
    return false;
  }
  return true;
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

  // Each server message is a sentence (FluentValidation's and MVC's end with a period), so several read
  // one after another.
  for (const [name, messages] of Object.entries(errors as Record<string, unknown>)) {
    const message = Array.isArray(messages) ? messages.join(" ") : asText(messages) ?? "";
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

function wireLive(containerId: string, entry: Entry): void {
  const { field, owner } = entry;
  const element = document.getElementById(field.id);
  if (element === null || liveWiredElements.has(element)) return;

  const clear = (): void => clearInline(field.id);
  const recheck = (): void => revalidate(containerId, field.id);
  element.addEventListener("input", clear);
  element.addEventListener("blur", recheck);
  element.addEventListener(COMMIT_EVENT, recheck);
  liveWiredElements.add(element);
  owner.onDispose(() => {
    element.removeEventListener("input", clear);
    element.removeEventListener("blur", recheck);
    element.removeEventListener(COMMIT_EVENT, recheck);
    liveWiredElements.delete(element);
  });
}

// Fields a partial renders appear after their form was contributed: wire them when they mount.
function wireMountedFields(): void {
  for (const [containerId, entries] of formsByContainer) {
    for (const entry of entries) wireLive(containerId, entry);
  }
}

function listenForMounts(): void {
  if (mountListened) return;
  mountListened = true;
  document.addEventListener(MOUNT_EVENT, wireMountedFields);
}

export const formModule: Module = {
  object: (containerId): FormObject => ({
    validate: () => validate(containerId),
    showErrors: problem => showErrors(containerId, problem),
  }),
  contribute({ key, data, owner, world }: Contribution) {
    if (data.kind !== "form") return;
    const entries = entriesOf(key);
    const added = data.fields.map(field => ({ field, world, summary: data.summary, owner }));
    entries.push(...added);
    if (owner.isSlot) clearAndHide(document.getElementById(data.summary));
    for (const entry of added) wireLive(key, entry);
    listenForMounts();

    owner.onDispose(() => {
      const remaining = entriesOf(key).filter(entry => !added.includes(entry));
      formsByContainer.set(key, remaining);
      clearAndHide(document.getElementById(data.summary));
    });
  },
};

export function resetFormsForTests(): void {
  formsByContainer.clear();
  liveWiredElements = new WeakSet<HTMLElement>();
}
