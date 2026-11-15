import { ComponentView, mountComponents } from "../component-view";
declare global {
    interface HTMLElementEventMap {
        opened: CustomEvent<{ room: string } | null>;
    }
}
interface ElementStart {
    room: string;
}
export class ConformanceElement extends HTMLElement {
    private view: ComponentView | null = null;
    build(start: ElementStart): void {
        const root = this.shadowRoot ?? this.attachShadow({ mode: "open" });
        const view = new ComponentView(root);
        view.input("Key", "text");
        view.button("Open with room", () => { this.dispatchEvent(new CustomEvent("opened", { detail: { room: start.room } })); });
        view.button("Open with no room", () => { this.dispatchEvent(new CustomEvent("opened")); });
        this.view = view;
    }
    destroy(): void {
        this.view?.lifetime.abort();
        this.shadowRoot?.replaceChildren();
        this.view = null;
    }
}
customElements.define("conformance-element", ConformanceElement);
mountComponents("data-conformance-element", (element, value) => {
    if (!(element instanceof ConformanceElement))
        throw new Error("The element marked data-conformance-element is no conformance-element.");
    if (typeof value !== "object" || value === null)
        throw new Error("The element's start must be an object.");
    element.build(value as ElementStart);
    return () => { element.destroy(); };
});
