// alis.document (grammar.md 6.1).
// D1 `ready` is an event of the owner being started: a ready handler runs when its own owner starts,
//    after every handler of that owner has subscribed, and never again.
// D2 `raise(name, detail)` is synchronous: a non-bubbling CustomEvent carrying the detail the plan
//    gives it. A listener's payload is the detail, or the event itself when the detail is missing.
// D3 `query` is the query string read the way MVC binds [FromQuery]: keys match case-insensitively
//    and a scalar member reads the first value; `queryAll` gives an array member every value. An
//    absent key reads as missing.
// D4 A click on an element carrying data-alis-raise raises that event with the element's detail.

import type { Module } from "../objects/modules";
import { logger } from "../diagnostics/trace";
import { isMissing } from "../grammar/shape";

const executeLog = logger("execute");
const triggerLog = logger("trigger");

const RAISE_ATTRIBUTE = "data-alis-raise";
const RAISE_DETAIL_ATTRIBUTE = "data-alis-detail";

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
  return new Proxy({} as Record<string, T>, {
    get(_target, key) {
      if (typeof key !== "string") return undefined;
      const values = valuesFor(params, key);
      return values.length === 0 ? undefined : pick(values);
    },
    has(_target, key) {
      return typeof key === "string" && valuesFor(params, key).length > 0;
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
    const source = target.closest(`[${RAISE_ATTRIBUTE}]`);
    if (!source) return;
    const name = source.getAttribute(RAISE_ATTRIBUTE);
    if (!name) return;
    const detailText = source.getAttribute(RAISE_DETAIL_ATTRIBUTE);
    raise(name, detailText ? JSON.parse(detailText) : {});
  }, true);
}

export const documentModule: Module = {
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
