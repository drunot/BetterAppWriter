using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sharp_injector.Events {

    public class KeyDownHookEventArgs : EventArgs {
        public KeyDownHookEventArgs(Keys downKey, SortedSet<Keys> keysPressed, bool shortcutDisabled = false) {
            DownKey = downKey;
            KeysPressed = keysPressed;
            ShortcutDisabled = shortcutDisabled;
        }
        public Keys DownKey { get; set; }
        public SortedSet<Keys> KeysPressed { get; set; }

        public bool Handled = false;

        public bool ShortcutDisabled = false;
    }

    public delegate void KeyDownHookEventHandler(object sender, KeyDownHookEventArgs args);

}
