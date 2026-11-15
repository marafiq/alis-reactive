import { pageInputs } from "../InputsAndIds/index";
import { PageForms } from "./forms";

export const pageForms = new PageForms(input => pageInputs.typedText(input));
