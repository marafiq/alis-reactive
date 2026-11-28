import { mountsEach } from "../mounted";

const marker = "data-fusion-uploader";

interface Uploader {
  getFilesData(): readonly unknown[];
  addEventListener(eventName: string, handler: () => void): void;
  trigger(eventName: string, eventProp: object): void;
}

type GetComponent = (element: HTMLElement, moduleName: string) => unknown;

function memberOf(holder: unknown, member: string): unknown {
  return typeof holder === "object" && holder !== null ? Reflect.get(holder, member) : undefined;
}

function uploaderOn(input: HTMLInputElement): Uploader {
  const getComponent = memberOf(memberOf(memberOf(window, "ej"), "base"), "getComponent");
  if (typeof getComponent !== "function") throw new Error("The uploader '" + input.id + "' finds no EJ2 getComponent: link ej2.min.js before alis-reactive-fusion.js.");
  const uploader = (getComponent as GetComponent)(input, "uploader");
  if (uploader === undefined) throw new Error("The element '" + input.id + "' carries " + marker + " and holds no EJ2 Uploader.");
  return uploader as Uploader;
}

function raisesFilesChanged(uploader: Uploader): () => void {
  return () => {
    queueMicrotask(() => {
      uploader.trigger("filesChanged", { filesData: [...uploader.getFilesData()] });
    });
  };
}

function mountsUploader(input: HTMLInputElement): void {
  const uploader = uploaderOn(input);
  const raises = raisesFilesChanged(uploader);
  uploader.addEventListener("selected", raises);
  uploader.addEventListener("removing", raises);
  uploader.addEventListener("clearing", raises);
}

export function mountsUploaders(): void {
  mountsEach(marker, HTMLInputElement, mountsUploader);
}
