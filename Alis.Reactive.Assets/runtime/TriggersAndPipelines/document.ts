import { queried, routed, type PageAddress } from "../ValuesAndSources/index";

type Listener = (...args: unknown[]) => void;

export interface MarkedAttribute {
  readonly name: string;
  readonly value: string;
}

const ready = "ready";
const raiseAttribute = "data-alis-raise";
const memberAttribute = "data-alis-detail-";
const nullAttribute = "data-alis-null";
const namesApart = " ";

export class DocumentPlugin {
  readonly #events: EventTarget;
  readonly #address: PageAddress;
  readonly #waitingForTheStart: Listener[] = [];

  constructor(events: EventTarget, address: PageAddress) {
    this.#events = events;
    this.#address = address;
  }

  addEventListener(event: string, listener: Listener): void {
    if (event === ready) this.#waitingForTheStart.push(listener);
    else this.#events.addEventListener(event, listener);
  }

  removeEventListener(event: string, listener: Listener): void {
    if (event !== ready) {
      this.#events.removeEventListener(event, listener);
      return;
    }
    const place = this.#waitingForTheStart.indexOf(listener);
    if (place >= 0) this.#waitingForTheStart.splice(place, 1);
  }

  dispatch(appEvent: string, detail: unknown): void {
    this.#events.dispatchEvent(new CustomEvent(appEvent, { detail }));
  }

  query(key: string): string | null {
    return queried(this.#address, key);
  }

  segment(template: readonly string[], place: number): string {
    return routed(this.#address, template, place);
  }

  started(): void {
    for (const listener of this.#waitingForTheStart.splice(0)) listener();
  }
}

export function markupDetail(attributes: Iterable<MarkedAttribute>): unknown {
  const detail: Record<string, unknown> = {};
  let carried = false;
  for (const { name, value } of attributes) {
    if (name.startsWith(memberAttribute)) {
      detail[memberNamed(name.slice(memberAttribute.length))] = value;
      carried = true;
    } else if (name === nullAttribute) {
      for (const member of value.split(namesApart)) if (member !== "") detail[member] = null;
      carried = true;
    }
  }
  return carried ? detail : null;
}

export function raisesOnClick(page: Document, plugin: DocumentPlugin): void {
  page.addEventListener("click", (click) => {
    const marked = innermostMarked(click.target);
    if (marked !== undefined) plugin.dispatch(marked.appEvent, markupDetail(Array.from(marked.element.attributes)));
  }, { capture: true });
}

function innermostMarked(target: EventTarget | null): { readonly element: Element; readonly appEvent: string } | undefined {
  for (let at = target instanceof Node ? elementAt(target) : null; at !== null; at = at.parentElement) {
    const appEvent = at.getAttribute(raiseAttribute);
    if (appEvent !== null) return { element: at, appEvent };
  }
  return undefined;
}

function elementAt(node: Node): Element | null {
  return node instanceof Element ? node : node.parentElement;
}

function memberNamed(attribute: string): string {
  let member = "";
  for (let at = 0; at < attribute.length; at++) {
    const next = attribute.charAt(at + 1);
    const upperNext = attribute.charAt(at) === "-" && next >= "a" && next <= "z";
    member += upperNext ? next.toUpperCase() : attribute.charAt(at);
    if (upperNext) at++;
  }
  return member;
}
