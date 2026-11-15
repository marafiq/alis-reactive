import { ComponentView, mountMarked } from "../component-view";
interface CallbacksElement extends HTMLElement {
    conformanceCallbacks?: ConformanceCallbacks;
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
    private readonly lines: HTMLUListElement;
    private readonly names: HTMLUListElement;
    private throwing = false;
    constructor(element: CallbacksElement) {
        this.element = element;
        this.view = new ComponentView(element);
        this.own = () => {
            this.append("own");
            if (this.throwing)
                throw new Error("The callbacks' own function threw.");
        };
        this.onRaised = this.own;
        const log = document.createElement("div");
        log.setAttribute("role", "log");
        log.setAttribute("aria-label", "Log");
        this.lines = document.createElement("ul");
        log.append(this.lines);
        element.append(log);
        this.view.button("Raise room 7", () => { this.raiseRoom(); });
        this.view.button("Make it throw", () => { this.throwing = true; });
        this.view.button("Check", () => { this.view.status("Holds", this.onRaised === this.own ? "own" : "other"); });
        const box = this.view.input("Search", "search");
        box.addEventListener("input", () => { this.onSearch?.(box.value, (names) => { this.show(names); }); }, { signal: this.view.lifetime.signal });
        this.names = this.view.list("Names", []);
        element.conformanceCallbacks = this;
    }
    destroy(): void {
        this.view.lifetime.abort();
        delete this.element.conformanceCallbacks;
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
    private append(text: string): void {
        const line = this.lines.lastElementChild;
        if (line === null)
            throw new Error("The log holds no line to append to.");
        line.textContent = line.textContent === "" ? text : line.textContent + " " + text;
    }
    private show(names: readonly unknown[]): void {
        this.names.replaceChildren(...names.map((name) => {
            const item = document.createElement("li");
            item.textContent = String(name);
            return item;
        }));
    }
}
mountMarked("data-conformance-callbacks", (element) => {
    const callbacks = new ConformanceCallbacks(element);
    return () => { callbacks.destroy(); };
});
