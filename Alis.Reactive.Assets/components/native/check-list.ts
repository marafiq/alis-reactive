// NativeCheckList: the container rendered with data-alis-check-list is the component. Its value is the
// values of the boxes checked; writing it checks exactly the boxes whose values it holds. The hidden
// input that posts them as CSV follows both. A write raises no event, as a script's write to any
// native input raises none.

import { onMount } from "../hooks";

onMount("[data-alis-check-list]", list => {
  const posted = list.querySelector<HTMLInputElement>("input[type=hidden]")!;
  const boxes = Array.from(list.querySelectorAll<HTMLInputElement>("input[type=checkbox]"));
  const checkedValues = (): string[] => boxes.filter(box => box.checked).map(box => box.value);
  const post = (): void => { posted.value = checkedValues().join(","); };

  Object.defineProperty(list, "value", {
    get: checkedValues,
    // A response may carry null for a list with nothing in it, as a script may write null to an
    // input's value: both leave nothing checked.
    set: (values: readonly string[] | null) => {
      for (const box of boxes) box.checked = (values ?? []).includes(box.value);
      post();
    },
    configurable: true,
  });
  post();

  list.addEventListener("change", event => {
    if ((event.target as HTMLInputElement).type === "checkbox") post();
  });
});
