import { afterEach, vi } from "vitest";
import { resetBootStateForTests } from "../lifecycle/boot";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";
import { resetLiveClearForTests } from "../validation";

afterEach(() => {
  resetNativeActionLinksForTests();
  resetBootStateForTests();
  resetLiveClearForTests();
  vi.restoreAllMocks();
  delete (globalThis as { alis?: unknown }).alis;
  delete (globalThis as { ej?: unknown }).ej;
  document.body.innerHTML = "";
});
