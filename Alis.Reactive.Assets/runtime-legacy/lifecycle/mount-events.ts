// The app constructs and destroys its own widgets; the framework only finds them. These two
// document events tell the app when to do it:
//   alis:mount   — HTML is in the document and about to be wired; construct widgets inside detail.root.
//   alis:unmount — wired HTML is about to be removed; destroy widgets inside detail.root.
// Both fire synchronously, so a widget constructed in an alis:mount listener exists when the
// runtime locates it.

export interface MountEventDetail {
  readonly root: HTMLElement;
}

export const MOUNT_EVENT = "alis:mount";
export const UNMOUNT_EVENT = "alis:unmount";

export function announceMount(root: HTMLElement): void {
  announce(MOUNT_EVENT, root);
}

export function announceUnmount(root: HTMLElement): void {
  announce(UNMOUNT_EVENT, root);
}

function announce(eventName: string, root: HTMLElement): void {
  const detail: MountEventDetail = { root };
  root.ownerDocument.dispatchEvent(new CustomEvent<MountEventDetail>(eventName, { detail }));
}
