import { ComponentEvents } from "../component-events";
import type { Change, ComponentListener } from "../component-events";
import { ComponentView } from "../component-view";
interface PeerElement extends HTMLElement {
    conformancePeer?: ConformancePeer;
}
export class ConformancePeer extends ComponentEvents {
    readonly element: PeerElement;
    private readonly view: ComponentView;
    private heldRatio = 0;
    private heldText = "";
    override addEventListener(type: "change", callback: ((value: Change) => void) | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: string, callback: ComponentListener | null, options?: boolean | AddEventListenerOptions): void;
    override addEventListener(type: string, callback: ComponentListener | null, options?: boolean | AddEventListenerOptions): void {
        super.addEventListener(type, callback, options);
    }
    constructor(element: PeerElement) {
        super();
        this.element = element;
        element.conformancePeer = this;
        element.setAttribute("role", "region");
        element.setAttribute("aria-label", "Peer");
        this.view = new ComponentView(element);
        this.ratio = 25;
        this.text = "";
        this.view.button("Change", () => { this.emit("change", { value: this.text, previousValue: this.text, isInteracted: true }); });
    }
    get ratio(): number { return this.heldRatio; }
    set ratio(value: number) {
        this.heldRatio = value;
        this.view.status("Ratio", "Ratio " + String(value));
    }
    get text(): string { return this.heldText; }
    set text(value: string) {
        this.heldText = value;
        this.view.status("Text", value);
    }
    destroy(): void {
        this.view.lifetime.abort();
        delete this.element.conformancePeer;
    }
}
