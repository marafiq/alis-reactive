import { ComponentView, mountMarked } from "../component-view";
interface StoreElement extends HTMLElement {
    conformanceStore?: ConformanceStore;
}
interface Subscription {
    readonly event: string;
    readonly listener: (...args: unknown[]) => void;
}
export class ConformanceStore {
    private readonly element: StoreElement;
    private readonly view: ComponentView;
    private readonly subscriptions: Subscription[] = [];
    constructor(element: StoreElement) {
        this.element = element;
        this.view = new ComponentView(element);
        this.count();
        this.view.button("Post an update", () => { this.post("posted", { message: "Bed 4 needs water" }); });
        element.conformanceStore = this;
    }
    subscribe(event: string, listener: (...args: unknown[]) => void): () => void {
        const subscription: Subscription = { event, listener };
        this.subscriptions.push(subscription);
        this.count();
        return () => {
            const place = this.subscriptions.indexOf(subscription);
            if (place === -1)
                return;
            this.subscriptions.splice(place, 1);
            this.count();
        };
    }
    destroy(): void {
        this.view.lifetime.abort();
        delete this.element.conformanceStore;
    }
    private post(event: string, posted: { readonly message: string }): void {
        for (const subscription of this.subscriptions.filter((held) => held.event === event))
            subscription.listener(posted);
    }
    private count(): void { this.view.status("Subscribers", "subscribers: " + String(this.subscriptions.length)); }
}
mountMarked("data-conformance-store", (element) => {
    const store = new ConformanceStore(element);
    return () => { store.destroy(); };
});
