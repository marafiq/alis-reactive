import type { Plugin } from "../objects/plugins";
import { logger } from "../diagnostics/trace";
import { isMissing } from "../grammar/shape";

const executeLog = logger("execute");
const triggerLog = logger("trigger");

const RAISE_ATTRIBUTE = "data-alis-raise";
const DETAIL_KEY = "alisDetail";

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

function wireMarkupRaise(): void {
  if (markupRaiseWired) return;
  markupRaiseWired = true;
  document.addEventListener("click", event => {
    const target = event.target;
    if (!(target instanceof Element)) return;
    const source = target.closest<HTMLElement>(`[${RAISE_ATTRIBUTE}]`);
    if (!source) return;
    raise(source.getAttribute(RAISE_ATTRIBUTE)!, markupDetail(source));
  }, true);
}

function markupDetail(source: HTMLElement): Record<string, string> {
  const detail: Record<string, string> = {};
  for (const [key, text] of Object.entries(source.dataset)) {
    if (!key.startsWith(DETAIL_KEY)) continue;
    const member = key.slice(DETAIL_KEY.length);
    detail[member.charAt(0).toLowerCase() + member.slice(1)] = text!;
  }
  return detail;
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
