// loader.ts — self-initializing side-effect module
// Handles target positioning, timeout auto-hide, and cleanup.
// Plan-driven show/hide uses existing native mutations (AddClass/RemoveClass).
export {}; // Module marker — prevents TS global-scope collisions

function handleVisible(loader: HTMLElement): void {
  const targetId = loader.dataset.target;
  if (targetId) {
    const target = document.getElementById(targetId);
    if (target) {
      target.style.position = "relative";
      target.appendChild(loader);
    }
  }

  const timeout = loader.dataset.timeout;
  if (timeout) {
    const ms = parseInt(timeout, 10);
    if (ms > 0) {
      setTimeout(() => {
        loader.classList.remove("alis-loader--visible");
        loader.setAttribute("aria-hidden", "true");
      }, ms);
    }
  }
}

function handleHidden(loader: HTMLElement): void {
  if (loader.parentElement !== document.body) {
    document.body.appendChild(loader);
  }
  delete loader.dataset.target;
  delete loader.dataset.timeout;
  const msg = document.getElementById("alis-loader-message");
  if (msg) msg.textContent = "";
}

function init(): void {
  const loader = document.getElementById("alis-loader");
  if (!loader) return;

  const observer = new MutationObserver(() => {
    const visible = loader.classList.contains("alis-loader--visible");
    if (visible) {
      handleVisible(loader);
    } else {
      handleHidden(loader);
    }
  });

  observer.observe(loader, { attributes: true, attributeFilter: ["class"] });
}

init();
