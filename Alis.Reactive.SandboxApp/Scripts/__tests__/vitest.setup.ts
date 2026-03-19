import { afterEach, vi } from "vitest";
import { resetBootStateForTests } from "../lifecycle/boot";
import { resetNativeActionLinksForTests } from "../components/native/native-action-link";

afterEach(() => {
  resetNativeActionLinksForTests();
  resetBootStateForTests();
  vi.restoreAllMocks();
  document.body.innerHTML = "";
});
