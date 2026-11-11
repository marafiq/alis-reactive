export function name(element: HTMLElement | null): string {
  if (element === null) {
    return "";
  }

  return element.id;
}
