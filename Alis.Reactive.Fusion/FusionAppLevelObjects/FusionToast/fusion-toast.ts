import type { BeforeSanitizeHtmlArgs, Toast } from "@syncfusion/ej2-notifications";
import { announcedRoot, mountsEach } from "../../FusionComponents/mounted";

function memberOf(holder: unknown, member: string): unknown {
  return typeof holder === "object" && holder !== null ? Reflect.get(holder, member) : undefined;
}

function toastClass(host: HTMLElement): typeof Toast {
  const defined = memberOf(memberOf(memberOf(window, "ej"), "notifications"), "Toast");
  if (typeof defined !== "function") throw new Error("The toast '" + host.id + "' finds no EJ2 Toast: link ej2.min.js before alis-reactive-fusion.js.");
  return defined as typeof Toast;
}

function textMarkup(text: string): string {
  const escaped = text
    .replaceAll("&", "&amp;")
    .replaceAll("<", "&lt;")
    .replaceAll(">", "&gt;")
    .replaceAll('"', "&quot;")
    .replaceAll("'", "&#39;");
  return "<span>" + escaped + "</span>";
}

function sanitizeAsText(args: BeforeSanitizeHtmlArgs): void {
  args.cancel = true;
  args.helper = textMarkup;
}

function mountToast(host: HTMLDivElement): void {
  const originalParent = host.parentElement;
  const ToastType = toastClass(host);
  const toast = new ToastType({
    target: document.body,
    position: { X: "Right", Y: "Bottom" },
    newestOnTop: true,
    showCloseButton: true,
    beforeSanitizeHtml: sanitizeAsText,
  });
  toast.appendTo(host);

  const lifetime = new AbortController();
  document.addEventListener("alis:unmount", (unmount) => {
    const leaving = announcedRoot(unmount);
    if (!leaving.contains(host) && (originalParent === null || !leaving.contains(originalParent))) return;
    lifetime.abort();
    toast.destroy();
  }, { signal: lifetime.signal });
}

export function mountsFusionToast(): void {
  mountsEach("data-fusion-toast", HTMLDivElement, mountToast);
}
