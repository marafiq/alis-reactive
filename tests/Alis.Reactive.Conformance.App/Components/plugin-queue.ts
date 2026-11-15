declare global {
    interface Window {
        __alisPlugins?: { readonly name: string; readonly instance: unknown }[];
    }
}
export function providePlugin(name: string, instance: unknown): void {
    (window.__alisPlugins ??= []).push({ name, instance });
}
