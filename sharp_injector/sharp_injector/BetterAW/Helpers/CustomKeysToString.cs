using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterAW.Helpers {
    public static class CustomKeysToString {
        public static string CustomToString(this Keys key) {
            switch (key) {
                case Keys.Menu:
                case Keys.RMenu:
                case Keys.LMenu:
                    return Regex.Replace(key.ToString(), @"(?<=^[LR]?)Menu", "Alt");
                case Keys.ControlKey:
                case Keys.RControlKey:
                case Keys.LControlKey:
                    return Regex.Replace(key.ToString(), @"(?<=^[LR]?)ControlKey", "Ctrl");
                case Keys.ShiftKey:
                case Keys.RShiftKey:
                case Keys.LShiftKey:
                    return Regex.Replace(key.ToString(), @"(?<=^[LR]?)ShiftKey", "Shift");
                case Keys.D0:
                case Keys.D1:
                case Keys.D2:
                case Keys.D3:
                case Keys.D4:
                case Keys.D5:
                case Keys.D6:
                case Keys.D7:
                case Keys.D8:
                case Keys.D9:
                    return $"{(char)key}";
                default:
                    return key.ToString();
            }
        }

        public static string CustomToString(this SortedSet<Keys> keyboardShortcut) {
            if(keyboardShortcut is null) {
                return string.Empty;
            }
            return keyboardShortcut.Aggregate(string.Empty, (agregate, next) => {
                if (agregate == String.Empty) {
                    return next.CustomToString();
                }
                return $"{agregate}+{next.CustomToString()}";
            });
        }
    }
}
