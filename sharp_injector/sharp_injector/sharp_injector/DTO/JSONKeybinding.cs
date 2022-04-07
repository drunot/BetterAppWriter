using BetterAW;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sharp_injector.DTO
{
    public class JSONKeybinding
    {
        public string Name;
        public int[] KeyBindings;

        public static JSONKeybinding FromKeyboardShortcutInfo(KeyboardShortcutInfo keyboardShortcutInfo)
        {
            JSONKeybinding ret = new JSONKeybinding();
            ret.Name = keyboardShortcutInfo.Name;
            ret.KeyBindings = keyboardShortcutInfo.keyBinding.Select(x => ((int)x)).ToArray();
            return ret;
        }

        public static JSONKeybinding FromJObject(JObject JObj)
        {
            JSONKeybinding ret = new JSONKeybinding();
            ret.Name = (string)JObj["Name"];
            ret.KeyBindings = JObj["KeyBindings"].Select(x => ((int)x)).ToArray();
            return ret;
        }

        public KeyboardShortcutInfo ToKeyboardShortcutInfo()
        {
            KeyboardShortcutInfo info = new KeyboardShortcutInfo();
            info.Name = Name;
            info.keyBinding = new HashSet<System.Windows.Forms.Keys>(KeyBindings.Select(x => ((System.Windows.Forms.Keys)x)).ToArray());
            return info;
        }
    }
}
