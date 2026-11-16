import { restarts } from "./component-host";
interface Mounted {
    readonly destroy: () => void;
    readonly generation: number;
}
export class ComponentView {
    readonly element: HTMLElement | ShadowRoot;
    readonly lifetime = new AbortController();
    private readonly statuses = new Map<string, HTMLElement>();
    constructor(element: HTMLElement | ShadowRoot) { this.element = element; }
    status(name: string, text: string): HTMLElement {
        let line = this.statuses.get(name);
        if (line === undefined) {
            line = document.createElement("div");
            line.setAttribute("role", "status");
            line.setAttribute("aria-label", name);
            this.statuses.set(name, line);
            this.element.append(line);
        }
        line.textContent = text;
        return line;
    }
    list(name: string, lines: string[], parent: ParentNode = this.element): HTMLUListElement {
        const list = document.createElement("ul");
        list.setAttribute("aria-label", name);
        for (const text of lines) {
            const item = document.createElement("li");
            item.textContent = text;
            list.append(item);
        }
        parent.append(list);
        return list;
    }
    button(name: string, act: () => void): void {
        const button = document.createElement("button");
        button.type = "button";
        button.textContent = name;
        button.addEventListener("click", act, { signal: this.lifetime.signal });
        this.element.append(button);
    }
    input(name: string, type: string): HTMLInputElement {
        const input = document.createElement("input");
        input.type = type;
        input.setAttribute("aria-label", name);
        this.element.append(input);
        return input;
    }
}
export function mountComponents(marker: string, build: (element: HTMLElement, start: unknown, generation: number) => () => void): void {
    mountMarked(marker, (element, generation) => build(element, startOf(element, marker), generation));
}
function startOf(element: HTMLElement, marker: string): unknown {
    const text = element.getAttribute(marker);
    if (text === null)
        throw new Error("The component start is missing: " + marker);
    return JSON.parse(text);
}
export function mountMarked(marker: string, build: (element: HTMLElement, generation: number) => () => void): void {
    const mounted = new WeakMap<HTMLElement, Mounted>();
    function rootOf(event: Event): HTMLElement {
        if (!(event instanceof CustomEvent))
            throw new Error("A component lifecycle event must be a CustomEvent.");
        const detail: unknown = event.detail;
        if (typeof detail !== "object" || detail === null || !("root" in detail) || !(detail.root instanceof HTMLElement)) {
            throw new Error("A component lifecycle event must name its root.");
        }
        return detail.root;
    }
    document.addEventListener("alis:mount", (event) => {
        for (const element of rootOf(event).querySelectorAll<HTMLElement>("[" + marker + "]")) {
            if (mounted.has(element))
                continue;
            mounted.set(element, { destroy: build(element, 1), generation: 1 });
        }
    });
    document.addEventListener("alis:unmount", (event) => {
        for (const element of rootOf(event).querySelectorAll<HTMLElement>("[" + marker + "]")) {
            mounted.get(element)?.destroy();
            mounted.delete(element);
        }
    });
    restarts((element) => {
        const record = mounted.get(element);
        if (record === undefined)
            return false;
        mounted.delete(element);
        record.destroy();
        element.replaceChildren();
        const generation = record.generation + 1;
        mounted.set(element, { destroy: build(element, generation), generation });
        return true;
    });
}
