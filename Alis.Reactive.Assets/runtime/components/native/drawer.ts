// Native drawer markup is a layout singleton rendered by @Html.NativeDrawer().
// Side-effect module uses its well-known DOM IDs directly; DSL open/close
// calls still target the app-level component through native set reactions.
import { unloadPartialSlot } from "../../lifecycle/boot";

const CONTENT_ID = "alis-drawer-content";
const VISIBLE_CLASS = "alis-drawer--visible";
const KEEPS_OPEN_ON_ESCAPE_CLASS = "alis-drawer--keeps-open-on-escape";
const SIZE_CLASSES = ["alis-drawer--sm", "alis-drawer--md", "alis-drawer--lg"];

function close(container: HTMLElement): void {
  container.classList.remove(VISIBLE_CLASS, KEEPS_OPEN_ON_ESCAPE_CLASS);
}

function closeOnEscape(container: HTMLElement, e: KeyboardEvent): void {
  if (e.key !== "Escape") return;
  const isOpen = container.classList.contains(VISIBLE_CLASS);
  const keepsOpenOnEscape = container.classList.contains(KEEPS_OPEN_ON_ESCAPE_CLASS);
  if (isOpen && !keepsOpenOnEscape) close(container);
}

function resetOnceClosed(container: HTMLElement): void {
  if (container.classList.contains(VISIBLE_CLASS)) return;
  container.setAttribute("aria-hidden", "true");
  container.classList.remove(...SIZE_CLASSES);
  unloadPartialSlot(CONTENT_ID);
  const content = document.getElementById(CONTENT_ID);
  if (content) content.innerHTML = "";
  const title = document.getElementById("alis-drawer-title");
  if (title) title.textContent = "";
}

function init(): void {
  const container = document.getElementById("alis-drawer");
  if (!container) return;

  const closeBtn = document.getElementById("alis-drawer-close");
  if (closeBtn) {
    closeBtn.addEventListener("click", () => close(container));
  }

  document.addEventListener("keydown", (e: KeyboardEvent) => closeOnEscape(container, e));
  container.addEventListener("transitionend", () => resetOnceClosed(container));
}

init();
