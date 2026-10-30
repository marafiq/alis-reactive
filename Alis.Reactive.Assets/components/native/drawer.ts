// Native drawer markup is a layout singleton rendered by @Html.NativeDrawer(). When it mounts, this
// gives the element the members the C# NativeDrawer declares, so the DSL calls them like any
// component's: open(), close() and the size property. It also wires the close button, the Escape key
// and the reset after the close transition, through the drawer's well-known DOM IDs.

import { announceUnmount, onMount } from "../hooks";

const SIZES = ["sm", "md", "lg"] as const;
type DrawerSize = (typeof SIZES)[number];

const VISIBLE = "alis-drawer--visible";

function sizeClass(size: DrawerSize): string {
  return `alis-drawer--${size}`;
}

function open(container: HTMLElement): void {
  container.classList.add(VISIBLE);
  container.removeAttribute("aria-hidden");
}

function close(container: HTMLElement): void {
  container.classList.remove(VISIBLE);
}

// The applied size, or "" when none is: the panel then has the stylesheet's default width.
function appliedSize(container: HTMLElement): DrawerSize | "" {
  return SIZES.find(size => container.classList.contains(sizeClass(size))) ?? "";
}

function applySize(container: HTMLElement, size: DrawerSize): void {
  container.classList.remove(...SIZES.map(sizeClass));
  container.classList.add(sizeClass(size));
}

function declareMembers(container: HTMLElement): void {
  Object.assign(container, {
    open: () => open(container),
    close: () => close(container),
  });
  Object.defineProperty(container, "size", {
    get: () => appliedSize(container),
    set: (size: DrawerSize) => applySize(container, size),
    configurable: true,
  });
}

// The content a plan loaded into the drawer goes with the close: announced first, so what it brought
// (its components, rules and behavior) is released before the HTML is emptied.
function resetAfterClose(container: HTMLElement): void {
  container.setAttribute("aria-hidden", "true");
  container.classList.remove(...SIZES.map(sizeClass));
  const content = document.getElementById("alis-drawer-content");
  if (content) {
    announceUnmount(content);
    content.innerHTML = "";
  }
  const title = document.getElementById("alis-drawer-title");
  if (title) title.textContent = "";
}

onMount("#alis-drawer", container => {
  declareMembers(container);

  const closeBtn = document.getElementById("alis-drawer-close");
  if (closeBtn) {
    closeBtn.addEventListener("click", () => close(container));
  }

  document.addEventListener("keydown", (e: KeyboardEvent) => {
    if (e.key === "Escape" && container.classList.contains(VISIBLE)) {
      close(container);
    }
  });

  container.addEventListener("transitionend", () => {
    if (!container.classList.contains(VISIBLE)) {
      resetAfterClose(container);
    }
  });
});
