import { ComponentView, mountMarked } from "../component-view";
import { providePlugin } from "../plugin-queue";
const page = "the page";
export class ConformanceBusy {
    private readonly view: ComponentView;
    private readonly shown = new Set<string>();
    private readonly calls: string[] = [];
    constructor(element: HTMLElement) {
        this.view = new ComponentView(element);
        this.draw();
    }
    show(target?: string): void {
        this.calls.push("show " + (target ?? page));
        this.shown.add(target ?? page);
        this.draw();
    }
    hide(target?: string): void {
        this.calls.push("hide " + (target ?? page));
        this.shown.delete(target ?? page);
        this.draw();
    }
    private draw(): void {
        const names = [...this.shown].sort();
        this.view.status("Busy", "Busy: " + (names.length === 0 ? "none" : names.join(", ")));
        this.view.status("Busy calls", "Busy calls: " + (this.calls.length === 0 ? "none" : this.calls.join("; ")));
    }
}
mountMarked("data-conformance-busy", (element) => {
    providePlugin("busy", new ConformanceBusy(element));
    return () => undefined;
});
