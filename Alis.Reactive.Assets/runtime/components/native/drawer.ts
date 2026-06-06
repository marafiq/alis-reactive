// Native drawer markup is a layout singleton rendered by @Html.NativeDrawer().
// This side-effect module uses its well-known DOM IDs directly; DSL open/close
// calls still target the app-level component through native set reactions.
export {};

function close(container: HTMLElement): void {
  container.classList.remove("alis-drawer--visible");
}

function init(): void {
  const container = document.getElementById("alis-drawer");
  if (!container) return;

  const closeBtn = document.getElementById("alis-drawer-close");
  if (closeBtn) {
    closeBtn.addEventListener("click", () => close(container));
  }

  document.addEventListener("keydown", (e: KeyboardEvent) => {
    if (e.key === "Escape" && container.classList.contains("alis-drawer--visible")) {
      close(container);
    }
  });

  container.addEventListener("transitionend", () => {
    if (!container.classList.contains("alis-drawer--visible")) {
      container.setAttribute("aria-hidden", "true");
      container.classList.remove("alis-drawer--sm", "alis-drawer--md", "alis-drawer--lg");
      const content = document.getElementById("alis-drawer-content");
      if (content) content.innerHTML = "";
      const title = document.getElementById("alis-drawer-title");
      if (title) title.textContent = "";
    }
  });
}

init();
