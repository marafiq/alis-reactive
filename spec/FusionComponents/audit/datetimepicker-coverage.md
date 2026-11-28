# First-visit appointment intervals

On the First visit page, the admission desk chooses a brief check-in or an assessment. The time list offers 09:00, 09:15 and 09:30 for check-ins. Switching a check-in to an assessment leaves 09:00 and 09:30. M21.X2020 observes the complete list of offered times after each choice. M21.X1010 also covers the booked date and time and the empty booking on this page.

The pinned calendars package is `@syncfusion/ej2-calendars` 33.1.44. The inventory at `9c77fd7d8` identifies `step` on the root DateTimePicker. Its public declaration at `src/datetimepicker/datetimepicker.d.ts:71` is `step: number`, with a default of 30 minutes. EJ2 uses that value when building the time list at `src/datetimepicker/datetimepicker.js:595`. The view writes the interval through `FusionDateTimePicker<DateTime?>.Step.Set(...)`. The net10.0 and net48 public manifests expose `Property<double>`. Chromium exercises the write on net10. [Pinned inventory](slice-rows.tsv)

The time-list format and morning opening hours are ordinary EJ2 render options. The vendor also declares `show(type?: string, e?: MouseEvent | KeyboardEvent | KeyboardEventArgs): void` at `datetimepicker.d.ts:552`. The test uses the picker's keyboard shortcut to open the list. The optional type and event arguments still need a typed caller and browser evidence of their own.

With the check-in interval changed to 30 minutes, the 09:15 option disappeared. With the assessment interval changed to 15 minutes, the 09:15 option appeared on the assessment list. After restoring both writes, the M21.F17 net10 selection passed both cases, 9m42s from assignment.
