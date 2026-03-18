// drawer.ts — self-initializing side-effect module
// Wires close button, Escape key, and transition cleanup.
// Plan-driven open/close uses existing native mutations (AddClass/RemoveClass).

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
