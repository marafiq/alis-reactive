export interface MountEventDetail {
  readonly root: HTMLElement;
}

export const MOUNT_EVENT = "alis:mount";
export const UNMOUNT_EVENT = "alis:unmount";
export const COMMIT_EVENT = "alis:commit";

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
