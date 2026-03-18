// checklist.ts — self-initializing side-effect module
// Syncs checked checkbox values into a hidden input as a comma-separated string,
// then dispatches a synthetic change event on the hidden input so the plan's
// ComponentEventTrigger fires with the aggregate value.

function init(): void {
  const containers = document.querySelectorAll<HTMLElement>("[data-alis-checklist]");

  for (const container of containers) {
    const elementId = container.getAttribute("data-alis-checklist")!;
    const hidden = document.getElementById(elementId) as HTMLInputElement | null;
    if (!hidden) continue;

    container.addEventListener("change", () => {
      const values: string[] = [];
      let i = 0;
      while (true) {
        const cb = document.getElementById(`${elementId}_c${i}`) as HTMLInputElement | null;
        if (!cb) break;
        if (cb.checked) values.push(cb.value);
        i++;
      }
      hidden.value = values.join(",");
      hidden.dispatchEvent(new Event("change", { bubbles: true }));
    });
  }
}

init();
