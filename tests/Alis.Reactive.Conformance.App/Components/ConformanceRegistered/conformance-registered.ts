import { ComponentView, mountComponents } from "../component-view";
import { providePlugin } from "../plugin-queue";
interface RegisteredStart {
    reading: number;
    madeBy: string | null;
}
type Listener = (...args: unknown[]) => void;
const registry = new WeakMap<Element, ConformanceRegistered>();
export class ConformanceRegistered {
    private readonly start: RegisteredStart;
    private readonly view: ComponentView;
    private readonly listeners = new Map<string, Set<Listener>>();
    private heldThreshold = 0;
    constructor(element: HTMLElement, start: RegisteredStart) {
        this.start = start;
        this.view = new ComponentView(element);
        this.threshold = 0;
        this.view.button("Sound the alarm", () => { this.emit("alarmed"); });
    }
    get reading(): number { return this.start.reading; }
    get threshold(): number { return this.heldThreshold; }
    set threshold(value: number) {
        this.heldThreshold = value;
        this.view.status("Threshold", String(value));
    }
    raise(): void { this.view.status("Alarm", "Alarm raised"); }
    lower(): void { this.view.status("Alarm", "Alarm lowered"); }
    on(event: "alarmed", listener: Listener): void {
        let listeners = this.listeners.get(event);
        if (listeners === undefined) {
            listeners = new Set();
            this.listeners.set(event, listeners);
        }
        listeners.add(listener);
    }
    off(event: "alarmed", listener: Listener): void { this.listeners.get(event)?.delete(listener); }
    destroy(): void { this.view.lifetime.abort(); }
    private emit(event: string): void {
        for (const listener of [...(this.listeners.get(event) ?? [])])
            listener();
    }
}
providePlugin("conformance-registry", (element: unknown): ConformanceRegistered | undefined => element instanceof Element ? registry.get(element) : undefined);
mountComponents("data-conformance-registered", (element, value) => {
    if (typeof value !== "object" || value === null)
        throw new Error("The gauge's start must be an object.");
    const start = value as RegisteredStart;
    const lifetime = new AbortController();
    const make = (): void => {
        if (!registry.has(element))
            registry.set(element, new ConformanceRegistered(element, start));
    };
    if (start.madeBy === null)
        make();
    else {
        const button = document.getElementById(start.madeBy);
        if (button === null)
            throw new Error("No button makes the gauge: " + start.madeBy);
        button.addEventListener("click", make, { signal: lifetime.signal });
    }
    return () => {
        lifetime.abort();
        registry.get(element)?.destroy();
        registry.delete(element);
    };
});
