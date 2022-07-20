using BetterAW;
using Newtonsoft.Json.Linq;
using sharp_injector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sharp_injector.DTO {
    public class JSONKeybinding {
        public string Name;
        public int[] KeyBindings;

        public static JSONKeybinding FromKeyboardShortcutInfo(Patches.KeyboardInternalShortcutInfo keyboardShortcutInfo) {
            JSONKeybinding ret = new JSONKeybinding();
            ret.Name = keyboardShortcutInfo.Name;
            ret.KeyBindings = keyboardShortcutInfo.keyBinding.Select(x => ((int)x)).ToArray();
            return ret;
        }

        public static JSONKeybinding FromJObject(JObject JObj) {
            JSONKeybinding ret = new JSONKeybinding();
            ret.Name = (string)JObj["Name"];
            ret.KeyBindings = JObj["KeyBindings"].Select(x => ((int)x)).ToArray();
            return ret;
        }

        public Patches.KeyboardInternalShortcutInfo ToKeyboardShortcutInfo() {
            Patches.KeyboardInternalShortcutInfo info = new Patches.KeyboardInternalShortcutInfo();
            info.Name = Name;
            info.keyBinding = new SortedSet<System.Windows.Forms.Keys>(KeyBindings.Select(x => ((System.Windows.Forms.Keys)x)).ToArray());
            return info;
        }
    }
}
