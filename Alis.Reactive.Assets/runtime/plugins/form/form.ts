// alis.form (grammar.md 6.4), keyed by the validation container's element id. Plans contribute their
// fields under their owner; each field names what it checks by binding name and carries its own tests, so
// the form needs nothing but the evaluator and the page's table of inputs.
// F1 A field names a member, or every item of a collection, by binding name (Lines[*].Qty, alis.model M8).
//    It applies at each mounted input inside the container that its name names, whichever plan rendered
//    it, at that moment; an input outside the container is not checked. A missing container fails
//    validation when the form has tests. A test reads its field's value and every member it names by
//    binding name, at the indexes of the item it checks (M8).
// F3 Per input, tests whose `when` is false are skipped and the first failing check decides.
// F4 validate() checks the tests written for its container: it clears every inline message inside the
//    container, whichever form showed it (a section saved on its own), and the summary, then evaluates
//    every field and returns the AND.
// F5 A member field no input on the page binds posts no value: each active test checks a missing value,
//    and the first that fails puts its message in the summary. An item field with no item on the page
//    checks nothing: the server sees no missing item.
// F6 A message goes inline into {id}_error when that slot can render, else into the form's summary, the
//    element the form's data names ({formId}_validation_summary), which the server renders.
// F7 Live, for every mounted input whichever plan rendered it and whenever it mounted: the DOM `input`
//    event on it, or inside it, clears its message; DOM `blur` and alis:commit (the model re-raises each
//    input's own commit event as it) revalidate it against every container that holds it. The form hears
//    them on the document, so an input is live as soon as it is in the page's table and stops with it.
// F8 showErrors(problem) clears first, then places each server error beside the mounted input inside the
//    container with that exact binding name, whichever plan rendered it (alis.model M7), else in the
//    summary; a form only the server checks needs no fields. A body that is not a record is logged and
//    shows nothing.
// F9 A slot's load or unload removes exactly its own fields, then clears and hides the form's summary.

import type { FormField, FormPluginData, FormTest, PluginData } from "../../types/index";
import type { Contribution, Plugin } from "../../objects/plugins";
import type { PlanObjects } from "../../objects/plan-objects";
import { evaluate, rootScope } from "../../grammar/evaluate";
import { asText } from "../../grammar/shape";
import { isRecord } from "../../shared/record";
import { logger } from "../../diagnostics/trace";
import { COMMIT_EVENT } from "../../plans/mount-events";
import { containedInputs, isMountedInput, namedInputs, type NamedInput } from "../model";
import {
  addToSummary, canRenderInline, clearInline, clearSummary, hideSummaryDiv, removeSummaryEntry,
  showInline, showServerInline, showsServerMessage, showSummaryDiv, summaryHasEntries,
} from "./display";

const log = logger("validation");

interface Entry {
  readonly field: FormField;
  readonly objects: PlanObjects;
  /** The id of the form's summary, from the data the field came with. */
  readonly summary: string;
}

export interface FormObject {
  validate(): boolean;
  showErrors(problem: unknown): void;
}

const formsByContainer = new Map<string, Entry[]>();
let inputsListened = false;

function entriesOf(containerId: string): Entry[] {
  let entries = formsByContainer.get(containerId);
  if (!entries) {
    entries = [];
    formsByContainer.set(containerId, entries);
  }
  return entries;
}

// ── summary ────────────────────────────────────────────────────────────────

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

function evaluateWhen(entry: Entry, test: FormTest, indexes: readonly number[]): boolean {
  const when = evaluate(test.when, rootScope(entry.objects)) as (indexes: readonly number[]) => unknown;
  return !!when(indexes);
}

function checkPasses(entry: Entry, test: FormTest, value: unknown, indexes: readonly number[]): boolean {
  const check = evaluate(test.check, rootScope(entry.objects)) as (value: unknown, indexes: readonly number[]) => unknown;
  return !!check(value, indexes);
}

// A name with an index in it, Lines[*].Qty, stands for every item of a collection; any other names one member.
function namesItems(field: FormField): boolean {
  return field.name.includes("[*]");
}

function evaluateField(entry: Entry, container: HTMLElement, summary: HTMLElement | null): boolean {
  const inputs = namedInputs(entry.field.name);
  if (inputs.length === 0) return namesItems(entry.field) || unmountedField(entry, summary);

  let valid = true;
  for (const input of inputs) {
    if (!container.contains(input.element)) {
      log.trace("field.out-of-scope", { field: input.name });
      continue;
    }
    if (!evaluateInput(entry, input, summary)) valid = false;
  }
  return valid;
}

// The field's tests at one input it names: the first that fails puts its message beside the input, or in the
// summary when the input's message slot cannot render.
function evaluateInput(entry: Entry, input: NamedInput, summary: HTMLElement | null): boolean {
  const inlineHidden = !canRenderInline(input.id);
  const value = input.value;
  for (const test of entry.field.tests) {
    if (!evaluateWhen(entry, test, input.indexes)) continue;
    if (checkPasses(entry, test, value, input.indexes)) continue;

    log.trace("rule.failed", { field: input.name, rule: test.rule, value, message: test.message });
    if (inlineHidden) {
      addSummaryError(summary, input.name, test.message);
    } else {
      showInline(input.id, test.message);
      removeSummaryError(summary, input.name);
    }
    return false;
  }
  return true;
}

// A member the page does not render posts no value, so its active tests run against nothing, as the
// server's run against a missing member: a required test fails, a length or a pattern passes. A
// failure has no message slot on the page and goes to the summary.
function unmountedField(entry: Entry, summary: HTMLElement | null): boolean {
  const { field } = entry;
  log.trace("validation-field.unmounted", { field: field.name });
  for (const test of field.tests) {
    if (!evaluateWhen(entry, test, [])) continue;
    if (checkPasses(entry, test, undefined, [])) continue;
    addSummaryError(summary, field.name, test.message);
    return false;
  }
  return true;
}

function validate(containerId: string, summaryId: string): boolean {
  const entries = entriesOf(containerId);
  const container = document.getElementById(containerId);
  if (container === null) {
    if (entries.length === 0) return true;
    log.warn("form.missing", { id: containerId });
    return false;
  }

  // A section of the form saved on its own may have shown a message the form's own tests do not make.
  const summary = document.getElementById(summaryId);
  clearInlineMessages(containedInputs(containerId).ids);
  clearAndHide(summary);

  let valid = true;
  for (const entry of entries) {
    if (!evaluateField(entry, container, summary)) valid = false;
  }
  log.debug("validated", { id: containerId, valid });
  return valid;
}

interface Holder {
  readonly summary: HTMLElement | null;
  readonly checks: readonly { readonly entry: Entry; readonly input: NamedInput }[];
}

// Every validated container that holds the input's element, with each of its fields that names the input. An
// input can sit in more than one: a section saved on its own, inside the whole form.
function holdersOf(element: HTMLElement): Holder[] {
  const holders: Holder[] = [];
  for (const [containerId, entries] of formsByContainer) {
    const container = entries.length > 0 ? document.getElementById(containerId) : null;
    if (container === null || !container.contains(element)) continue;
    const checks = entries.flatMap(entry =>
      namedInputs(entry.field.name).filter(input => input.id === element.id).map(input => ({ entry, input })));
    if (checks.length === 0) continue;
    holders.push({ summary: document.getElementById(checks[0]!.entry.summary), checks });
  }
  return holders;
}

// Leaving an input rechecks it against every container that holds it; the first failing test shows.
function revalidate(element: HTMLElement): void {
  const holders = holdersOf(element);
  clearInline(element.id);
  for (const { summary, checks } of holders) removeSummaryError(summary, checks[0]!.input.name);
  for (const { summary, checks } of holders) {
    for (const { entry, input } of checks) {
      if (!evaluateInput(entry, input, summary)) return;
    }
  }
}

function showErrors(containerId: string, summaryId: string, problem: unknown): void {
  if (!isRecord(problem)) {
    log.error("server-errors.not-a-record", { id: containerId });
    return;
  }

  // The mounted inputs inside the form, whichever plan rendered them; a form not on the page fails here (M1).
  const inputs = containedInputs(containerId).ids;
  const summary = document.getElementById(summaryId);
  clearInlineMessages(inputs);
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
    const id = inputs[name];
    if (id !== undefined && canRenderInline(id)) {
      showServerInline(id, message);
    } else {
      addSummaryError(summary, name, message);
    }
  }
  log.debug("server-errors.shown", { id: containerId });
}

// Every inline message the form may show: each mounted input's inside it (M7), whichever plan rendered it.
function clearInlineMessages(inputs: Record<string, string>): void {
  for (const id of Object.values(inputs)) clearInline(id);
}

// ── live ───────────────────────────────────────────────────────────────────

// The mounted input whose element is the target, or holds it: a component's own input inside its element.
function inputAround(target: EventTarget | null): HTMLElement | null {
  for (let node = target instanceof HTMLElement ? target : null; node !== null; node = node.parentElement) {
    if (isMountedInput(node)) return node;
  }
  return null;
}

function mountedInputAt(target: EventTarget | null): HTMLElement | null {
  return target instanceof HTMLElement && isMountedInput(target) ? target : null;
}

function listenToInputs(): void {
  if (inputsListened) return;
  inputsListened = true;
  // Typing clears the message before the component hears the keystroke (the document's capture runs first),
  // so a commit the component raises from that same input event (EJ2's MaskedTextBox on a paste) re-checks
  // the input after the clear and its message stays.
  document.addEventListener("input", event => {
    const input = inputAround(event.target);
    if (input !== null) clearInline(input.id);
  }, true);
  // Leaving an input the server judged keeps the server's message: no input and no commit came since, so the
  // value it judged has not changed. Blur does not bubble; the document hears it as it is captured.
  document.addEventListener("blur", event => {
    const input = mountedInputAt(event.target);
    if (input !== null && !showsServerMessage(input.id)) revalidate(input);
  }, true);
  document.addEventListener(COMMIT_EVENT, event => {
    const input = mountedInputAt(event.target);
    if (input !== null) revalidate(input);
  });
}

// A plan writes a form's data on every form it validates or shows the server's errors on, so the plan that
// asks for the form names its summary.
function summaryOf(data: PluginData): string {
  return (data as FormPluginData).summary;
}

export const formPlugin: Plugin = {
  object: (containerId, data): FormObject => ({
    validate: () => validate(containerId, summaryOf(data)),
    showErrors: problem => showErrors(containerId, summaryOf(data), problem),
  }),
  // The plan gives alis.form no data but a form's: its fields and its summary.
  contribute({ key, data: contributed, owner, objects }: Contribution) {
    const data = contributed as FormPluginData;
    const entries = entriesOf(key);
    const added = data.fields.map(field => ({ field, objects, summary: data.summary }));
    entries.push(...added);
    if (owner.isSlot) clearAndHide(document.getElementById(data.summary));
    listenToInputs();

    owner.onDispose(() => {
      formsByContainer.set(key, entriesOf(key).filter(entry => !added.includes(entry)));
      clearAndHide(document.getElementById(data.summary));
    });
  },
};
