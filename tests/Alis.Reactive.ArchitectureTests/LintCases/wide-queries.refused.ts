export function found(root: ParentNode, field: Element): number {
  const first = root.querySelector("form");
  const all = root.querySelectorAll("input");
  const byClass = document.getElementsByClassName("field");
  const byTag = document.getElementsByTagName("input");
  const byName = document.getElementsByName("dose");
  const around = field.closest("form");
  const isAField = field.matches(".field");
  return [first, around].length + all.length + byClass.length + byTag.length + byName.length + Number(isAField);
}
