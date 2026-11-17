import { mountsTheConfirm } from "./NativeAppLevelObjects/NativeConfirm/native-confirm";
import { mountsTheDrawer } from "./NativeAppLevelObjects/NativeDrawer/native-drawer";
import { mountsLoaders } from "./NativeAppLevelObjects/NativeLoader/native-loader";
import { mountsReconnectOffers } from "./NativeAppLevelObjects/NativeReconnectOffer/native-reconnect-offer";
import { providesRowActions } from "./NativeAppLevelObjects/NativeRowActions/native-row-actions";
import { mountsToasts } from "./NativeAppLevelObjects/NativeToast/native-toast";
import { mountsCheckLists } from "./NativeComponents/NativeCheckList/native-check-list";
import { mountsHiddenFields } from "./NativeComponents/NativeHiddenField/native-hidden-field";
import { mountsRadioGroups } from "./NativeComponents/NativeRadioGroup/native-radio-group";

mountsHiddenFields();
mountsRadioGroups();
mountsCheckLists();
mountsTheDrawer();
mountsTheConfirm();
providesRowActions();
mountsLoaders();
mountsToasts();
mountsReconnectOffers();
