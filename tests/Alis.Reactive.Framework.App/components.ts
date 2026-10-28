// Every component of the framework test app. Each constructs itself on alis:mount and releases itself
// on alis:unmount, as any app's widgets do; the meter is a custom element, the tally a plugin, and the
// confirmation dialog the app's own answer to a page's p.Confirm(..).
import "./Components/Slate/slate";
import "./Components/Bell/bell";
import "./Components/Meter/meter";
import "./Components/Tally/tally";
import "./Components/Entry/entry";
import "./Components/DayPicker/day-picker";
import "./Components/ConfirmDialog/confirm-dialog";
