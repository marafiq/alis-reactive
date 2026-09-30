// Native loader markup is a layout singleton rendered by @Html.NativeLoader().
// Side-effect module uses its well-known DOM IDs directly; DSL show/hide
// calls still target the app-level component through native set reactions.
export {};

// The auto-hide of the Show in progress. A Hide cancels it and a later Show restarts it, so a
// timeout never outlives the Hide of the request that set it.
let pendingAutoHide: ReturnType<typeof setTimeout> | undefined;

function handleVisible(loader: HTMLElement): void {
  const targetId = loader.dataset.target;
  if (targetId) {
    const target = document.getElementById(targetId);
    if (target) {
      target.style.position = "relative";
      target.appendChild(loader);
    }
  }

  clearTimeout(pendingAutoHide);
  const timeout = loader.dataset.timeout;
  if (timeout) {
    const ms = parseInt(timeout, 10);
    if (ms > 0) {
      pendingAutoHide = setTimeout(() => {
        loader.classList.remove("alis-loader--visible");
        loader.setAttribute("aria-hidden", "true");
      }, ms);
    }
  }
}

function handleHidden(loader: HTMLElement): void {
  clearTimeout(pendingAutoHide);
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
