export function announcedRoot(event: Event): HTMLElement {
  if (!(event instanceof CustomEvent)) throw new Error(event.type + " reached the Native components as no CustomEvent.");
  const detail: unknown = event.detail;
  if (typeof detail !== "object" || detail === null || !("root" in detail) || !(detail.root instanceof HTMLElement)) {
    throw new Error(event.type + " reached the Native components naming no root element.");
  }
  return detail.root;
}

export function mountsEach<TElement extends Element>(marker: string, kind: new () => TElement, mount: (element: TElement) => void): void {
  const mounted = new WeakSet<Element>();
  document.addEventListener("alis:mount", (event) => {
    for (const element of announcedRoot(event).querySelectorAll("[" + marker + "]")) {
      if (mounted.has(element)) continue;
      if (!(element instanceof kind)) throw new Error("The element '" + element.id + "' carries " + marker + " and is no " + kind.name + ".");
      mounted.add(element);
      mount(element);
    }
  });
}

export function mountsTheElement<TElement extends Element>(id: string, kind: new () => TElement, mount: (element: TElement) => void): void {
  const mounted = new WeakSet<Element>();
  document.addEventListener("alis:mount", (event) => {
    const element = document.getElementById(id);
    if (element === null || mounted.has(element) || !announcedRoot(event).contains(element)) return;
    if (!(element instanceof kind)) throw new Error("The element '" + id + "' is no " + kind.name + ".");
    mounted.add(element);
    mount(element);
  });
}
