// alis.document (grammar.md 6.1).
// D1 `ready` is an event of the owner being started: a ready handler runs when its own owner starts,
//    after every handler of that owner has subscribed, and never again.
// D2 `raise(name, detail)` is synchronous: a non-bubbling CustomEvent carrying the detail the plan
//    gives it. A listener's payload is the detail, or the event itself when the detail is missing.
// D3 `query` is the query string read the way MVC binds [FromQuery]: keys match case-insensitively
//    and a scalar member reads the first value; `queryAll` gives an array member every value. An
//    absent key reads as missing.
// D4 A click on an element carrying data-alis-raise raises that event with the element's detail: the
//    record data-alis-detail holds as JSON; else a record of the one member data-alis-detail-member
//    names, holding the text of data-alis-detail-text, which the listener's typed read converts; else an
//    empty record. An element whose data-alis-raise is empty names no event, so the click goes on to the
//    element around it.

import type { Plugin } from "../objects/plugins";
import { logger } from "../diagnostics/trace";
import { isMissing } from "../grammar/shape";

const executeLog = logger("execute");
const triggerLog = logger("trigger");

const RAISE_ATTRIBUTE = "data-alis-raise";
const RAISE_DETAIL_ATTRIBUTE = "data-alis-detail";
const RAISE_MEMBER_ATTRIBUTE = "data-alis-detail-member";
const RAISE_TEXT_ATTRIBUTE = "data-alis-detail-text";
const NAMED_RAISE = `[${RAISE_ATTRIBUTE}]:not([${RAISE_ATTRIBUTE}=""])`;

export interface DocumentObject {
  raise(name: string, detail: unknown): void;
  readonly query: Record<string, string | undefined>;
  readonly queryAll: Record<string, string[]>;
}

export function raise(name: string, detail: unknown): void {
  executeLog.debug("dispatch", { event: name, detail });
  document.dispatchEvent(new CustomEvent(name, { detail }));
}

function queryRecord<T>(pick: (values: string[]) => T | undefined): Record<string, T> {
  const params = new URLSearchParams(window.location.search);
  // A plan reads the record only by a member's binding name (UrlRead), never by a symbol.
  return new Proxy({} as Record<string, T>, {
    get(_target, key: string) {
      const values = valuesFor(params, key);
      return values.length === 0 ? undefined : pick(values);
    },
  });
}

function valuesFor(params: URLSearchParams, key: string): string[] {
  const wanted = key.toLowerCase();
  const values: string[] = [];
  params.forEach((value, name) => {
    if (name.toLowerCase() === wanted) values.push(value);
  });
  return values;
}

const documentObject: DocumentObject = {
  raise,
  get query() {
    return queryRecord(values => values[0]);
  },
  get queryAll() {
    return queryRecord(values => values);
  },
};

let markupRaiseWired = false;

// Captured at the document, so a vendor that stops a click inside its own template cannot swallow it.
function wireMarkupRaise(): void {
  if (markupRaiseWired) return;
  markupRaiseWired = true;
  document.addEventListener("click", event => {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const source = target.closest(NAMED_RAISE);
    if (!source) return;
    // The selector matched the attribute with a value.
    raise(source.getAttribute(RAISE_ATTRIBUTE)!, markupDetail(source));
  }, true);
}

// Markup the server renders serializes its payload; a vendor template knows a row's value only as the
// text it writes, so it names one member and carries that text for the plan's read to convert.
function markupDetail(source: Element): unknown {
  const json = source.getAttribute(RAISE_DETAIL_ATTRIBUTE);
  if (json) return JSON.parse(json);
  const member = source.getAttribute(RAISE_MEMBER_ATTRIBUTE);
  if (member) return { [member]: source.getAttribute(RAISE_TEXT_ATTRIBUTE) };
  return {};
}

export const documentPlugin: Plugin = {
  object: () => {
    wireMarkupRaise();
    return documentObject;
  },
  subscribe(_key, event, listener, owner) {
    wireMarkupRaise();
    if (event === "ready") {
      owner.whenStarted(() => listener({}));
      return;
    }

    triggerLog.debug("document-event.listening", { event });
    const domListener = (domEvent: Event): void => {
      const detail = (domEvent as CustomEvent).detail;
      listener(isMissing(detail) ? domEvent : detail);
    };
    document.addEventListener(event, domListener);
    owner.onDispose(() => document.removeEventListener(event, domListener));
  },
};
