import type { Expr, FormField, FormPluginData, PluginData } from "../../types/index";
import type { Contribution, Plugin } from "../../objects/plugins";
import type { PlanObjects } from "../../objects/plan-objects";
import { evaluate, rootScope } from "../../grammar/evaluate";
import { asText } from "../../grammar/shape";
import { isRecord } from "../../shared/record";
import { logger } from "../../diagnostics/trace";
import { COMMIT_EVENT } from "../../plans/mount-events";
import { boundNameOf, containedInputs, isMountedInput, nameAt, namedInputs, type NamedInput } from "../model";
import {
  addToSummary, canRenderInline, clearInline, clearSummary, hideSummaryDiv, removeSummaryEntry,
  showInline, showServerInline, showsServerMessage, showSummaryDiv, summaryHasEntries,
} from "./display";

const log = logger("validation");

interface Entry {
  readonly field: FormField;
  readonly live: Expr;
  readonly objects: PlanObjects;
}

interface Form {
  readonly summary: string;
  entries: Entry[];
}

export interface FormObject {
  validate(sent: () => unknown): boolean;
  showErrors(problem: unknown): void;
}

const forms = new Map<string, Form>();
let inputsListened = false;

function formOf(containerId: string, summary: string): Form {
  let form = forms.get(containerId);
  if (!form) {
    form = { summary, entries: [] };
    forms.set(containerId, form);
  }
  return form;
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

type Indexes = readonly number[];

function failedMessage(entry: Entry, indexes: Indexes, sent: unknown): string | undefined {
  const scope = rootScope(entry.objects);
  const value = (evaluate(entry.field.value, scope) as (indexes: Indexes, sent: unknown) => unknown)(indexes, sent);
  for (const test of entry.field.tests) {
    const when = evaluate(test.when, scope) as (indexes: Indexes, sent: unknown) => unknown;
    if (!when(indexes, sent)) continue;
    const check = evaluate(test.check, scope) as (value: unknown, indexes: Indexes, sent: unknown) => unknown;
    if (check(value, indexes, sent)) continue;

    log.trace("test.failed", { field: nameAt(entry.field.name, indexes), value, message: test.message });
    return test.message;
  }
  return undefined;
}

function evaluateField(entry: Entry, container: HTMLElement, summary: HTMLElement | null, sent: unknown): boolean {
  const held = namedInputs(entry.field.name).filter(input => container.contains(input.element));
  let valid = true;
  for (const indexes of itemsOf(entry, sent)) {
    const message = failedMessage(entry, indexes, sent);
    if (message === undefined) continue;

    valid = false;
    const inputs = held.filter(input => sameIndexes(input.indexes, indexes));
    if (inputs.length === 0) addSummaryError(summary, nameAt(entry.field.name, indexes), message);
    for (const input of inputs) show(input, summary, message);
  }
  return valid;
}

function itemsOf(entry: Entry, sent: unknown): readonly Indexes[] {
  return (evaluate(entry.field.items, rootScope(entry.objects)) as (sent: unknown) => readonly Indexes[])(sent);
}

function sameIndexes(held: Indexes, indexes: Indexes): boolean {
  return held.length === indexes.length && held.every((index, position) => index === indexes[position]);
}

function show(input: NamedInput, summary: HTMLElement | null, message: string): void {
  if (!canRenderInline(input.id)) {
    addSummaryError(summary, input.name, message);
    return;
  }
  showInline(input.id, message);
  removeSummaryError(summary, input.name);
}

function validate(containerId: string, summaryId: string, sentBody: () => unknown): boolean {
  const { entries } = formOf(containerId, summaryId);
  const container = document.getElementById(containerId);
  if (container === null) {
    if (entries.length === 0) return true;
    log.warn("form.missing", { id: containerId });
    return false;
  }

  const sent = sentBody();
  const summary = document.getElementById(summaryId);
  clearInlineMessages(containedInputs(containerId).ids);
  clearAndHide(summary);

  let valid = true;
  for (const entry of entries) {
    if (!evaluateField(entry, container, summary, sent)) valid = false;
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
  for (const [containerId, form] of forms) {
    const container = document.getElementById(containerId);
    if (container === null || !container.contains(element)) continue;
    const checks = form.entries.flatMap(entry =>
      namedInputs(entry.field.name).filter(input => input.id === element.id).map(input => ({ entry, input })));
    holders.push({ summary: document.getElementById(form.summary), checks });
  }
  return holders;
}

function revalidate(element: HTMLElement): void {
  const holders = holdersOf(element);
  const name = boundNameOf(element.id);
  clearInline(element.id);
  for (const { summary } of holders) removeSummaryError(summary, name);
  for (const { summary, checks } of holders) {
    for (const { entry, input } of checks) {
      const message = failedMessage(entry, input.indexes, evaluate(entry.live, rootScope(entry.objects)));
      if (message === undefined) continue;
      show(input, summary, message);
      return;
    }
  }
}

function showErrors(containerId: string, summaryId: string, problem: unknown): void {
  const inputs = containedInputs(containerId).ids;
  const summary = document.getElementById(summaryId);
  clearInlineMessages(inputs);
  clearAndHide(summary);

  const errors = isRecord(problem) ? problem["errors"] : undefined;
  if (!isRecord(errors)) {
    log.warn("server-errors.none", { id: containerId });
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
    validate: sent => validate(containerId, summaryOf(data), sent),
    showErrors: problem => showErrors(containerId, summaryOf(data), problem),
  }),
  contribute({ key, data: contributed, owner, objects }: Contribution) {
    const data = contributed as FormPluginData;
    const form = formOf(key, data.summary);
    const added = data.fields.map(field => ({ field, live: data.live, objects }));
    form.entries.push(...added);
    if (owner.isSlot) clearAndHide(document.getElementById(data.summary));
    listenToInputs();

    owner.onDispose(() => {
      form.entries = form.entries.filter(entry => !added.includes(entry));
      clearAndHide(document.getElementById(data.summary));
    });
  },
};
