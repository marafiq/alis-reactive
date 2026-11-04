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

function evaluateWhen(entry: Entry, test: FormTest, indexes: readonly number[]): boolean {
  const when = evaluate(test.when, rootScope(entry.objects)) as (indexes: readonly number[]) => unknown;
  return !!when(indexes);
}

function checkPasses(entry: Entry, test: FormTest, value: unknown, indexes: readonly number[]): boolean {
  const check = evaluate(test.check, rootScope(entry.objects)) as (value: unknown, indexes: readonly number[]) => unknown;
  return !!check(value, indexes);
}

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

function evaluateInput(entry: Entry, input: NamedInput, summary: HTMLElement | null): boolean {
  const inlineHidden = !canRenderInline(input.id);
  const value = input.value;
  for (const test of entry.field.tests) {
    if (!evaluateWhen(entry, test, input.indexes)) continue;
    if (checkPasses(entry, test, value, input.indexes)) continue;

    log.trace("test.failed", { field: input.name, value, message: test.message });
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

  const inputs = containedInputs(containerId).ids;
  const summary = document.getElementById(summaryId);
  clearInlineMessages(inputs);
  clearAndHide(summary);

  const errors = problem["errors"];
  if (!isRecord(errors)) {
    log.warn("server-errors.wrong-shape");
    return;
  }

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

function clearInlineMessages(inputs: Record<string, string>): void {
  for (const id of Object.values(inputs)) clearInline(id);
}

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
  document.addEventListener("input", event => {
    const input = inputAround(event.target);
    if (input !== null) clearInline(input.id);
  }, true);
  document.addEventListener("blur", event => {
    const input = mountedInputAt(event.target);
    if (input !== null && !showsServerMessage(input.id)) revalidate(input);
  }, true);
  document.addEventListener(COMMIT_EVENT, event => {
    const input = mountedInputAt(event.target);
    if (input !== null) revalidate(input);
  });
}

function summaryOf(data: PluginData): string {
  return (data as FormPluginData).summary;
}

export const formPlugin: Plugin = {
  object: (containerId, data): FormObject => ({
    validate: () => validate(containerId, summaryOf(data)),
    showErrors: problem => showErrors(containerId, summaryOf(data), problem),
  }),
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
