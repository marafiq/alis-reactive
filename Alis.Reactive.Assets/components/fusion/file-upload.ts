import { onMount } from "../hooks";

interface SyncfusionUploader {
  readonly filesData: readonly unknown[];
  addEventListener(eventName: string, handler: () => void): void;
  trigger(eventName: string, args: object): void;
}

interface SyncfusionHost extends HTMLElement {
  readonly ej2_instances?: readonly SyncfusionUploader[];
}

onMount("[data-alis-file-upload]", element => {
  const uploader = (element as SyncfusionHost).ej2_instances?.[0];
  if (!uploader) throw new Error(`[alis] #${element.id} is rendered as a file upload but its Syncfusion Uploader was not made`);

  const raiseFilesChanged = (): void => queueMicrotask(() =>
    uploader.trigger("filesChanged", { filesData: uploader.filesData.slice() }));
  uploader.addEventListener("selected", raiseFilesChanged);
  uploader.addEventListener("removing", raiseFilesChanged);
});
