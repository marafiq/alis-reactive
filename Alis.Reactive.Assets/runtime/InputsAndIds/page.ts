import { PageInputs } from "./model";

const aHiddenInputItSends = "input[type=\"hidden\"][name]:not([name=\"\"]):not(:disabled)";

export const pageInputs = new PageInputs(id => {
  const container = document.getElementById(id);
  if (container === null) return null;
  return inputId => {
    const input = document.getElementById(inputId);
    return input !== null && container.contains(input);
  };
}, id => {
  const container = document.getElementById(id);
  if (container === null) return null;
  return [...container.querySelectorAll<HTMLInputElement>(aHiddenInputItSends)].map(input => ({ name: input.name, value: input.value }));
});
