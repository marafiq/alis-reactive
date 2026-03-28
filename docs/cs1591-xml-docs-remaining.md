# CS1591 XML Documentation — Remaining Warnings

758 public members across 3 library projects need XML documentation.
Generated after enabling `<GenerateDocumentationFile>true</GenerateDocumentationFile>`.

## Summary

| Project | Warnings | Files |
|---------|----------|-------|
| Alis.Reactive (Core) | 570 | 47 |
| Alis.Reactive.Fusion | 128 | 18 |
| Alis.Reactive.Native | 60 | 16 |
| **Total** | **758** | **81** |

## Module Breakdown (by priority)

### Module 1: DesignSystem (276 warnings, 19 files)

Layout builders and design tokens. Largest single module.

- `Tokens/Layout.cs` — 40
- `Tokens/Colors.cs` — 36
- `Layout/CardOptions.cs` — 28
- `Tokens/Typography.cs` — 26
- `Tokens/TokenMap.cs` — 22
- `Tokens/Spacing.cs` — 20
- `Layout/TextBuilder.cs` — 16
- `Layout/HStackBuilder.cs` — 16
- `Layout/GridBuilder.cs` — 14
- `Tokens/Breakpoints.cs` — 12
- `Layout/VStackBuilder.cs` — 12
- `Layout/HeadingBuilder.cs` — 12
- `Layout/CardCss.cs` — 12
- `Layout/CardBuilder.cs` — 12
- `Layout/DividerCss.cs` — 8
- `Layout/ContainerBuilder.cs` — 8
- `Layout/CardHeaderBuilder.cs` — 8
- `Layout/CardFooterBuilder.cs` — 8
- `Layout/CardBodyBuilder.cs` — 8
- + 6 more small files (4 warnings each)

**Decision needed:** Should DesignSystem tokens/builders be public? If only used internally, make internal.

### Module 2: Descriptors (174 warnings, 17 files)

JSON plan descriptor types. Public for serialization and `[JsonDerivedType]` polymorphism.

- `Mutations/Mutation.cs` — 20
- `Requests/RequestDescriptor.cs` — 18
- `Mutations/MethodArg.cs` — 16
- `Commands/MutateElementCommand.cs` — 16
- `Requests/ComponentGather.cs` — 12
- `Triggers/ComponentEventTrigger.cs` — 10
- `Reactions/ParallelHttpReaction.cs` — 10
- `Commands/MutateEventCommand.cs` — 10
- `Commands/DispatchCommand.cs` — 10
- `Requests/StatusHandler.cs` — 8
- `Requests/StaticGather.cs` — 8
- `Commands/ValidationErrorsCommand.cs` — 8
- `Commands/IntoCommand.cs` — 6
- `Commands/Command.cs` — 6
- `Entry.cs` — 4
- + 6 more small files (2 warnings each)

### Module 3: Fusion AppLevel (60 warnings, 5 files)

FusionToast and FusionConfirm extensions.

- `FusionToast/FusionToastExtensions.cs` — 26
- `FusionToast/ToastPosition.cs` — 12
- `FusionToast/ToastType.cs` — 8
- `FusionToast/FusionToast.cs` — 4
- `FusionConfirm/FusionConfirmExtensions.cs` — 10

### Module 4: TestWidget (46 warnings, 11 files)

Test components used for architecture regression tests. Low priority.

- `TestWidgetSyncFusion/TestWidgetSyncFusionExtensions.cs` — 20
- + 10 more small files

### Module 5: Standalone Components (46 warnings, 14 files)

NativeButton, NativeHiddenField, NativeActionLink, FusionTab, FusionAccordion.

- `NativeHiddenField/NativeHiddenFieldExtensions.cs` — 10
- `NativeActionLink/NativeActionLinkBuilder.cs` — 8
- `NativeButton/NativeButtonExtensions.cs` — 6
- + 11 more small files (2-4 warnings each)

### Module 6: Misc (14 warnings, 5 files)

- `Serialization/WriteOnlyPolymorphicConverter.cs` — 6
- `Validation/ValidationDescriptor.cs` — 4
- `Validation/ValidationField.cs` — 2
- `Validation/ValidationCondition.cs` — 2
- `TypedEventDescriptor.cs` — 4
- `IComponent.cs` — 2
- `Validation/IValidationExtractor.cs` — 2

## Approach

For each module:
1. Check if the type **must** be public (cross-assembly use, serialization, `new()` constraint)
2. If no → make `internal` (eliminates the warning)
3. If yes → add XML docs following the `dotnet-xml-docs` skill
4. Rebuild and verify 0 CS1591 for that module before moving on

## Regenerate this list

```bash
dotnet clean --nologo -v q && dotnet build --nologo 2>&1 | grep "CS1591"
```
