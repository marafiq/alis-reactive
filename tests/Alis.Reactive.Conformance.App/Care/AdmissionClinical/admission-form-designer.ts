import { mountMarked } from "../../Components/component-view";

export interface FieldEdit {
    propertyName: string;
    propertyValue: unknown;
    fieldName: string;
}

export interface FormBuilder {
    onFieldPropertyChanged: (change: FieldEdit) => void;
    appendTo(element: HTMLElement): void;
    destroy(): void;
}

export interface Submission {
    data: Record<string, unknown>;
    isValid: boolean;
}

export interface FormRenderer {
    submit: (submission: Submission) => void;
    appendTo(element: HTMLElement): void;
    destroy(): void;
}

declare const ej: {
    readonly formbuilder: { readonly FormBuilder: new (options: unknown) => FormBuilder };
    readonly formrenderer: { readonly FormRenderer: new (options: unknown) => FormRenderer };
};

mountMarked("data-admission-form-builder", element => {
    const options = element.getAttribute("data-admission-form-builder");
    if (options === null) throw new Error("The admission form designer requires its options.");
    const builder = new ej.formbuilder.FormBuilder(JSON.parse(options));
    builder.appendTo(element);
    return () => { builder.destroy(); };
});

mountMarked("data-admission-form-renderer", element => {
    const options = element.getAttribute("data-admission-form-renderer");
    if (options === null) throw new Error("The admission preview requires its schema.");
    const renderer = new ej.formrenderer.FormRenderer(JSON.parse(options));
    renderer.appendTo(element);
    return () => { renderer.destroy(); };
});
