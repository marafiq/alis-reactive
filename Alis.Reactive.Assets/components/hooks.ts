interface MountEventDetail {
  readonly root: HTMLElement;
}

interface PendingPlugin {
  readonly name: string;
  readonly instance: object;
}

interface PluginQueueWindow extends Window {
  __alisPlugins?: PendingPlugin[];
}

export function onMount(selector: string, construct: (element: HTMLElement) => void): void {
  document.addEventListener("alis:mount", event => forEachWithin(event, selector, construct));
}

export function onUnmount(selector: string, destroy: (element: HTMLElement) => void): void {
  document.addEventListener("alis:unmount", event => forEachWithin(event, selector, destroy));
}

export function announceUnmount(root: HTMLElement): void {
  const detail: MountEventDetail = { root };
  root.ownerDocument.dispatchEvent(new CustomEvent<MountEventDetail>("alis:unmount", { detail }));
}

export function whenParsed(act: () => void): void {
  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", act, { once: true });
  else act();
}

export function providePlugin(name: string, instance: object): void {
  const host = window as PluginQueueWindow;
  (host.__alisPlugins ??= []).push({ name, instance });
}

function forEachWithin(event: Event, selector: string, act: (element: HTMLElement) => void): void {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  if (root.matches(selector)) act(root);
  root.querySelectorAll<HTMLElement>(selector).forEach(act);
}
