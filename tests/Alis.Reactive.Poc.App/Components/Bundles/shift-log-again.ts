import { providePlugin } from "../mount";
import { ShiftLog } from "../ShiftPlugins/shift-plugins";

providePlugin("shift-log", new ShiftLog());
