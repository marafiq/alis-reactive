import { PageInputs } from "./model";

export const pageInputs = new PageInputs(id => {
  const container = document.getElementById(id);
  if (container === null) return null;
  return inputId => {
    const input = document.getElementById(inputId);
    return input !== null && container.contains(input);
  };
});
