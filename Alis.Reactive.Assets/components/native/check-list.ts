// NativeCheckList: the container rendered with data-alis-check-list holds the checked values as its
// value, kept in step with the hidden input that posts them as CSV.

import { onMount } from "../hooks";

interface CheckList extends HTMLElement {
  value: string[];
}

onMount("[data-alis-check-list]", element => {
  const list = element as CheckList;
  const posted = list.querySelector<HTMLInputElement>("input[type=hidden]")!;
  list.value = posted.value.split(",").filter(Boolean);

  // Registered before the plan subscribes, so the value is current when a change handler reads it.
  list.addEventListener("change", event => {
    if ((event.target as HTMLInputElement).type !== "checkbox") return;
    list.value = Array.from(list.querySelectorAll<HTMLInputElement>("input[type=checkbox]:checked"), box => box.value);
    posted.value = list.value.join(",");
  });
});
