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
