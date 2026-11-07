interface MountDetail {
  readonly root: HTMLElement;
}

export function onMount<THost extends HTMLElement>(selector: string, construct: (host: THost) => void): void {
  document.addEventListener("alis:mount", (event) => {
    const { root } = (event as CustomEvent<MountDetail>).detail;
    if (root.matches(selector)) construct(root as THost);
    for (const host of root.querySelectorAll<THost>(selector)) construct(host);
  });
}

interface ProvidedPlugin {
  readonly name: string;
  readonly instance: unknown;
}

interface ProvidedPlugins {
  push(...provided: ProvidedPlugin[]): number;
}

interface PluginWindow extends Window {
  __alisPlugins?: ProvidedPlugins;
}

export function providePlugin(name: string, instance: unknown): void {
  const host = window as PluginWindow;
  (host.__alisPlugins ??= []).push({ name, instance });
}
