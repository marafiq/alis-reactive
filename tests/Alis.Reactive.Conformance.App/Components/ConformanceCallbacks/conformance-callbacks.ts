import { ComponentView, mountComponents } from "../component-view";
interface CallbacksElement extends HTMLElement {
    conformanceCallbacks?: ConformanceCallbacks;
}
interface CallbacksStart {
    own: string;
    ownNamesGeneration: boolean;
    raise: string;
    freeze: string;
}
interface RaisedRoom {
    readonly room: string;
}
type Raised = (raised: RaisedRoom) => void;
type Search = (query: string, show: (names: readonly unknown[]) => void) => void;
export class ConformanceCallbacks {
    onRaised: Raised | null;
    onSearch: Search | null = null;
    private readonly element: CallbacksElement;
    private readonly view: ComponentView;
    private readonly own: Raised;
    private readonly script: Raised;
    private readonly lines: HTMLUListElement;
    private readonly names: HTMLUListElement;
    private readonly throwing = { now: false };
    constructor(element: CallbacksElement, start: CallbacksStart, generation: number) {
        this.element = element;
        this.view = new ComponentView(element);
        const word = start.ownNamesGeneration ? start.own + " " + String(generation) : start.own;
        this.own = () => {
            this.append(word);
            if (this.throwing.now)
                throw new Error("The callbacks' own function threw.");
        };
        this.script = () => { this.append("script"); };
        this.onRaised = this.own;
        const log = document.createElement("div");
        log.setAttribute("role", "log");
        log.setAttribute("aria-label", "Log");
        this.lines = document.createElement("ul");
        log.append(this.lines);
        element.append(log);
        this.view.button(start.raise, () => { this.raiseRoom(); });
        this.view.button("Make it throw", () => { this.throwing.now = true; });
        this.view.button("Check", () => { this.view.status("Holds", this.holds()); });
        this.view.button("Replace the callback", () => { this.onRaised = this.script; });
        this.view.button(start.freeze, () => { Object.freeze(this); });
        const box = this.view.input("Search", "search");
        box.addEventListener("input", () => { this.onSearch?.(box.value, (names) => { this.show(names); }); }, { signal: this.view.lifetime.signal });
        this.names = this.view.list("Names", []);
        element.conformanceCallbacks = this;
    }
    append(text: string): void {
        const line = this.lines.lastElementChild;
        if (line === null)
            throw new Error("The log holds no line to append to.");
        line.textContent = line.textContent === "" ? text : line.textContent + " " + text;
    }
    destroy(): void {
        this.view.lifetime.abort();
        delete this.element.conformanceCallbacks;
        Object.freeze(this);
    }
    private holds(): string {
        if (this.onRaised === this.own)
            return "own";
        return this.onRaised === this.script ? "script" : "other";
    }
    private raiseRoom(): void {
        this.lines.append(document.createElement("li"));
        try {
            this.onRaised?.({ room: "7" });
        }
        catch (error) {
            this.view.status("Error", error instanceof Error ? error.message : String(error));
        }
    }
    private show(names: readonly unknown[]): void {
        this.names.replaceChildren(...names.map((name) => {
            const item = document.createElement("li");
            item.textContent = String(name);
            return item;
        }));
    }
}
mountComponents("data-conformance-callbacks", (element, value, generation) => {
    if (typeof value !== "object" || value === null)
        throw new Error("The callbacks' start must be an object.");
    const callbacks = new ConformanceCallbacks(element, value as CallbacksStart, generation);
    return () => { callbacks.destroy(); };
});
