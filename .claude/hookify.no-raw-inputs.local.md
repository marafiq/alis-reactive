---
name: no-raw-inputs
enabled: true
event: file
action: block
conditions:
  - field: file_path
    operator: regex_match
    pattern: \.cshtml$
  - field: new_text
    operator: regex_match
    pattern: <input[\s>]|<select[\s>]|<textarea[\s>]|Html\.TextBoxFor|Html\.DropDownListFor|Html\.CheckBoxFor|Html\.TextAreaFor|Html\.HiddenFor|\.DropDownListFor\(|\.AutoCompleteFor\(|\.NumericTextBoxFor\(|\.DatePickerFor\(|\.SwitchFor\(|\.TimePickerFor\(|\.MaskedTextBoxFor\(|\.RichTextEditorFor\(|\.UploaderFor\(|\.MultiSelectFor\(|\.DateTimePickerFor\(|\.DateRangePickerFor\(|\.ColorPickerFor\(
---

**BLOCKED: Raw input element or direct SF builder detected in a .cshtml view.**

All input components must go through the Alis.Reactive DSL:
- `Html.InputField(plan, m => m.Prop).NativeTextBox(build: b => ...)` — not `<input>`
- `Html.InputField(plan, m => m.Prop).FusionDropDownList(build: b => ...)` — not `.DropDownListFor()`
- `Html.InputField(plan, m => m.Prop).NativeCheckBox(build: b => ...)` — not `<input type="checkbox">`

Raw elements bypass stable id generation, object registration, binding gathering, and validation.
Direct SF builders bypass vendor abstraction and reactive wiring.
