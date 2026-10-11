// Native loader markup is a layout singleton rendered by @Html.NativeLoader(). This module gives the
// element the members the C# NativeLoader declares: show(), hide(), and the target and timeout of the
// next show, kept in data-target and data-timeout. A class observer then positions the loader over
// its target, runs the timeout, and puts it back on the page when it hides.
export {};

const VISIBLE = "alis-loader--visible";

function show(loader: HTMLElement): void {
  loader.classList.add(VISIBLE);
  loader.removeAttribute("aria-hidden");
}

function hide(loader: HTMLElement): void {
  loader.classList.remove(VISIBLE);
  loader.setAttribute("aria-hidden", "true");
}

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
    hide(loader);
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
      pendingAutoHide = setTimeout(() => hide(loader), ms);
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

  Object.assign(loader, {
    show: () => show(loader),
    hide: () => hide(loader),
  });
  // No target covers the viewport; a timeout of 0 never hides on its own.
  Object.defineProperties(loader, {
    target: {
      get: () => loader.dataset.target ?? "",
      set: (elementId: string) => { loader.dataset.target = elementId; },
      configurable: true,
    },
    timeout: {
      get: () => Number(loader.dataset.timeout ?? 0),
      set: (ms: number) => { loader.dataset.timeout = `${ms}`; },
      configurable: true,
    },
  });

  const observer = new MutationObserver(() => {
    const visible = loader.classList.contains(VISIBLE);
    if (visible) {
      handleVisible(loader);
    } else {
      handleHidden(loader);
    }
  });

  observer.observe(loader, { attributes: true, attributeFilter: ["class"] });
}

init();
