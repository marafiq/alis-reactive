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
