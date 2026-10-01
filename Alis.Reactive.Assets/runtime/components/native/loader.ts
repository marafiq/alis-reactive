// Native loader markup is a layout singleton rendered by @Html.NativeLoader().
// Side-effect module uses its well-known DOM IDs directly; DSL show/hide
// calls still target the app-level component through native set reactions.
export {};

// The auto-hide of the Show in progress. A Hide cancels it and a later Show restarts it, so a
// timeout never outlives the Hide of the request that set it.
let pendingAutoHide: ReturnType<typeof setTimeout> | undefined;

// A targeted loader lives inside the element it covers. When that element leaves the page (a drawer
// closes, a partial is replaced), the loader goes back to the page hidden: it is an app-level object
// and must stay mounted for the next Show or Hide.
let targetRemovalWatch: MutationObserver | undefined;

function watchTargetRemoval(loader: HTMLElement): void {
  targetRemovalWatch?.disconnect();
  targetRemovalWatch = new MutationObserver(() => {
    if (loader.isConnected) return;
    document.body.appendChild(loader);
    loader.classList.remove("alis-loader--visible");
    loader.setAttribute("aria-hidden", "true");
  });
  targetRemovalWatch.observe(document.body, { childList: true, subtree: true });
}

function handleVisible(loader: HTMLElement): void {
  const targetId = loader.dataset.target;
  if (targetId) {
    const target = document.getElementById(targetId);
    if (target) {
      target.style.position = "relative";
      target.appendChild(loader);
      watchTargetRemoval(loader);
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
  targetRemovalWatch?.disconnect();
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
