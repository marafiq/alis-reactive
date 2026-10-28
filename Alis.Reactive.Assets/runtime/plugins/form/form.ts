// alis.form (grammar.md 6.4), keyed by the validation container's element id. Plans contribute their
// fields under their owner; each field carries its own value read and tests, so the form needs
// nothing but the evaluator.
// F1 A test applies only when its field's element is inside the container; a missing container fails
//    validation when the form has tests.
// F3 Per field, tests whose `when` is false are skipped and the first failing check decides.
// F4 validate() clears every inline message and the summary, evaluates every field, returns the AND.
// F5 A field with no element posts no value: each active test checks a missing value, and the first
//    that fails puts its message in the summary.
// F6 A message goes inline into {id}_error when that slot can render, else into the summary.
// F7 At creation, and whenever HTML mounts (a field a partial loads): DOM `input` clears a field's
//    message; DOM `blur` and alis:commit (the model re-raises each input's own commit event as it)
//    revalidate it against every container that holds it. Each element is wired once.
// F8 showErrors(problem) places server errors by exact binding name; a body that is not a record is
//    logged and shows nothing.
// F9 A slot's load or unload removes exactly its own fields, then clears and hides the summary.

import type { FormField, FormPluginData, FormTest } from "../../types/index";
import type { Contribution, Plugin } from "../../objects/plugins";
import type { Owner } from "../../objects/owner";
import type { PlanObjects } from "../../objects/plan-objects";
import { evaluate, rootScope } from "../../grammar/evaluate";
import { asText } from "../../grammar/shape";
import { isRecord } from "../../shared/record";
import { logger } from "../../diagnostics/trace";
import { COMMIT_EVENT, MOUNT_EVENT } from "../../plans/mount-events";
import {
  addToSummary, canRenderInline, clearInline, clearSummary, hideSummaryDiv, removeSummaryEntry,
  showInline, showServerInline, showsServerMessage, showSummaryDiv, summaryHasEntries,
} from "./display";

const log = logger("validation");

interface Entry {
  readonly field: FormField;
  readonly objects: PlanObjects;
  readonly owner: Owner;
}

export interface FormObject {
  validate(): boolean;
  showErrors(problem: unknown): void;
}

const formsByContainer = new Map<string, Entry[]>();
// The summary element of each validated container, from the form's data: a form with no browser
// check of its own still shows the server's answers there.
const summaries = new Map<string, string>();
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

function summaryElement(containerId: string): HTMLElement | null {
  const id = summaries.get(containerId);
  return id === undefined ? null : document.getElementById(id);
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

function evaluateWhen(entry: Entry, test: FormTest): boolean {
  return !!evaluate(test.when, rootScope(entry.objects));
}

function checkPasses(entry: Entry, test: FormTest, value: unknown): boolean {
  const check = evaluate(test.check, rootScope(entry.objects)) as (value: unknown) => unknown;
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
  const value = evaluate(field.value, rootScope(entry.objects));
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

  const summary = summaryElement(containerId);
  for (const entry of entries) clearInline(entry.field.id);
  clearAndHide(summary);

  let valid = true;
  for (const entry of entries) {
    if (!evaluateField(entry, container, summary)) valid = false;
  }
  log.debug("validated", { id: containerId, valid });
  return valid;
}

interface Holder {
  readonly container: HTMLElement;
  readonly summary: HTMLElement | null;
  readonly entries: readonly Entry[];
}

// Every validated container that holds the field's element, with the field's entries in it. A field can
// sit in more than one: a section saved on its own, inside the whole form.
function holdersOf(field: HTMLElement): Holder[] {
  const holders: Holder[] = [];
  for (const [containerId, entries] of formsByContainer) {
    const own = entries.filter(entry => entry.field.id === field.id);
    const container = own.length > 0 ? document.getElementById(containerId) : null;
    if (container === null || !container.contains(field)) continue;
    holders.push({ container, summary: summaryElement(containerId), entries: own });
  }
  return holders;
}

// Leaving a field rechecks it against every container that holds it; the first failing test shows.
function revalidate(field: HTMLElement): void {
  const holders = holdersOf(field);
  clearInline(field.id);
  for (const { summary } of holders) removeSummaryError(summary, field.id);
  for (const { container, summary, entries } of holders) {
    for (const entry of entries) {
      if (!evaluateField(entry, container, summary)) return;
    }
  }
}

function showErrors(containerId: string, problem: unknown): void {
  if (!isRecord(problem)) {
    log.error("server-errors.not-a-record", { id: containerId });
    return;
  }

  const entries = entriesOf(containerId);
  const summary = summaryElement(containerId);
  for (const entry of entries) clearInline(entry.field.id);
  clearAndHide(summary);

  const errors = problem["errors"];
  if (!isRecord(errors)) {
    log.warn("server-errors.wrong-shape");
    return;
  }

  // Each server message is a sentence (FluentValidation's and MVC's end with a period), so several read
  // one after another.
  for (const [name, messages] of Object.entries(errors)) {
    const message = Array.isArray(messages) ? messages.join(" ") : asText(messages) ?? "";
    const entry = entries.find(candidate => candidate.field.name === name);
    const element = entry ? document.getElementById(entry.field.id) : null;
    if (entry && element !== null && canRenderInline(entry.field.id)) {
      showServerInline(entry.field.id, message);
    } else {
      addSummaryError(summary, name, message);
    }
  }
  log.debug("server-errors.shown", { id: containerId });
}

// ── live wiring ────────────────────────────────────────────────────────────

function wireLive(entry: Entry): void {
  const { field, owner } = entry;
  const element = document.getElementById(field.id);
  if (element === null || liveWiredElements.has(element)) return;

  const clear = (): void => clearInline(field.id);
  const recheck = (): void => revalidate(element);
  // Leaving a field the server judged keeps the server's message: no input and no commit came since,
  // so the value it judged has not changed.
  const recheckOnLeave = (): void => {
    if (!showsServerMessage(field.id)) revalidate(element);
  };
  // Typing clears the message before the component hears the keystroke (capture runs first), so a
  // commit the component raises from that same input event (EJ2's MaskedTextBox on a paste) re-checks
  // the field after the clear and its message stays.
  element.addEventListener("input", clear, true);
  element.addEventListener("blur", recheckOnLeave);
  element.addEventListener(COMMIT_EVENT, recheck);
  liveWiredElements.add(element);
  owner.onDispose(() => {
    element.removeEventListener("input", clear, true);
    element.removeEventListener("blur", recheckOnLeave);
    element.removeEventListener(COMMIT_EVENT, recheck);
    liveWiredElements.delete(element);
  });
}

// Fields a partial renders appear after their form was contributed: wire them when they mount.
function wireMountedFields(): void {
  for (const entries of formsByContainer.values()) {
    for (const entry of entries) wireLive(entry);
  }
}

function listenForMounts(): void {
  if (mountListened) return;
  mountListened = true;
  document.addEventListener(MOUNT_EVENT, wireMountedFields);
}

export const formPlugin: Plugin = {
  object: (containerId): FormObject => ({
    validate: () => validate(containerId),
    showErrors: problem => showErrors(containerId, problem),
  }),
  // The plan gives alis.form no data but a form's: its fields and its summary.
  contribute({ key, data: contributed, owner, objects }: Contribution) {
    const data = contributed as FormPluginData;
    const entries = entriesOf(key);
    if (!summaries.has(key)) summaries.set(key, data.summary);
    const added = data.fields.map(field => ({ field, objects, owner }));
    entries.push(...added);
    if (owner.isSlot) clearAndHide(document.getElementById(data.summary));
    for (const entry of added) wireLive(entry);
    listenForMounts();

    owner.onDispose(() => {
      const remaining = entriesOf(key).filter(entry => !added.includes(entry));
      formsByContainer.set(key, remaining);
      if (remaining.length === 0 && summaries.get(key) === data.summary) summaries.delete(key);
      clearAndHide(document.getElementById(data.summary));
    });
  },
};
