export interface Change {
    readonly value: string | null;
    readonly previousValue: string | null;
    readonly isInteracted: boolean;
}
export type ComponentListener = EventListenerOrEventListenerObject | ((...args: never[]) => void);
export class ComponentEvents extends EventTarget {
    private readonly arguments = new WeakMap<Event, unknown[]>();
    private readonly listeners = new Map<ComponentListener, Map<string, EventListener>>();
    override addEventListener(type: string, callback: ComponentListener | null, options?: boolean | AddEventListenerOptions): void {
        if (callback === null)
            return;
        const capture = typeof options === "boolean" ? options : options?.capture === true;
        const key = type + ":" + String(capture);
        let listeners = this.listeners.get(callback);
        if (listeners === undefined) {
            listeners = new Map();
            this.listeners.set(callback, listeners);
        }
        let listener = listeners.get(key);
        if (listener === undefined) {
            listener = (event) => {
                const args = this.arguments.get(event);
                if (typeof callback === "function")
                    Reflect.apply(callback, this, args ?? [event]);
                else
                    callback.handleEvent(event);
            };
            listeners.set(key, listener);
        }
        super.addEventListener(type, listener, options);
    }
    override removeEventListener(type: string, callback: ComponentListener | null, options?: boolean | EventListenerOptions): void {
        if (callback === null)
            return;
        const capture = typeof options === "boolean" ? options : options?.capture === true;
        const listener = this.listeners.get(callback)?.get(type + ":" + String(capture));
        if (listener !== undefined)
            super.removeEventListener(type, listener, options);
    }
    protected emit(type: string, ...args: unknown[]): void {
        const event = new Event(type);
        this.arguments.set(event, args);
        this.dispatchEvent(event);
    }
}
