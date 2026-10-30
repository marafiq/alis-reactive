// FusionFileUpload: Syncfusion's Uploader raises selected before it adds the chosen files to filesData
// and removing before it takes a file out, and raises nothing once it has (uploader.js
// renderSelectedFiles, remove). The uploader rendered with data-alis-file-upload raises filesChanged,
// carrying the files it then holds, after each of those events: EJ2 adds or removes the files in the
// same turn, right after the event's handlers return, so a microtask later filesData is the user's
// choice. A refused selection or removal leaves the files as they were, and filesChanged carries them
// unchanged. Clearing (a form reset, clearAll) raises nothing, as a script's write to any native input
// raises none.

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
