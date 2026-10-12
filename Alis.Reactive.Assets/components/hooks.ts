// The hooks the framework's own components construct through, the same ones an app's components use
// (docs/design/bring-your-own-component.md). The runtime raises alis:mount when HTML is in the
// document and about to be wired (the page at boot, a partial when it loads) and alis:unmount before
// wired HTML is removed; both carry the HTML's root. It takes plugins from window.__alisPlugins when
// it boots. Nothing here imports the runtime.

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

/** Constructs each element of mounted HTML that matches the selector, the root included. */
export function onMount(selector: string, construct: (element: HTMLElement) => void): void {
  document.addEventListener("alis:mount", event => forEachWithin(event, selector, construct));
}

/** Destroys each element of HTML about to be removed that matches the selector, the root included. */
export function onUnmount(selector: string, destroy: (element: HTMLElement) => void): void {
  document.addEventListener("alis:unmount", event => forEachWithin(event, selector, destroy));
}

/** Gives the plan an object it locates by name, through the queue an app's plugins use. */
export function providePlugin(name: string, instance: object): void {
  const host = window as PluginQueueWindow;
  (host.__alisPlugins ??= []).push({ name, instance });
}

function forEachWithin(event: Event, selector: string, act: (element: HTMLElement) => void): void {
  const { root } = (event as CustomEvent<MountEventDetail>).detail;
  if (root.matches(selector)) act(root);
  root.querySelectorAll<HTMLElement>(selector).forEach(act);
}
