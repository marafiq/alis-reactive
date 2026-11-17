import { mountsTheConfirm } from "./NativeAppLevelObjects/NativeConfirm/native-confirm";
import { mountsTheDrawer } from "./NativeAppLevelObjects/NativeDrawer/native-drawer";
import { mountsCheckLists } from "./NativeComponents/NativeCheckList/native-check-list";
import { mountsHiddenFields } from "./NativeComponents/NativeHiddenField/native-hidden-field";
import { mountsRadioGroups } from "./NativeComponents/NativeRadioGroup/native-radio-group";

mountsHiddenFields();
mountsRadioGroups();
mountsCheckLists();
mountsTheDrawer();
mountsTheConfirm();
