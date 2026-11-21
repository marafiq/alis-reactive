import { ComponentView, mountMarked } from "../component-view";
import { providePlugin } from "../plugin-queue";
export class ConformanceNotice {
    private readonly view: ComponentView;
    private readonly notices: string[] = [];
    constructor(element: HTMLElement) {
        this.view = new ComponentView(element);
        this.draw();
    }
    show(text: string | null | undefined, kind: string): void {
        this.notices.push(kind + ": " + (text === undefined ? "(undefined)" : text ?? "(null)"));
        this.draw();
    }
    private draw(): void {
        this.view.status("Notices", "Notices: " + (this.notices.length === 0 ? "none" : this.notices.join("; ")));
    }
}
mountMarked("data-conformance-notice", (element) => {
    providePlugin("notice", new ConformanceNotice(element));
    return () => undefined;
});
