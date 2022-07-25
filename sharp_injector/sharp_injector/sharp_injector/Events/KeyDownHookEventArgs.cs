using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sharp_injector.Events {

    public class KeyDownHookEventArgs : EventArgs {
        public KeyDownHookEventArgs(Keys downKey, SortedSet<Keys> keysPressed) {
            UpKey = downKey;
            KeysPressed = keysPressed;
        }
        public Keys UpKey { get; set; }
        public SortedSet<Keys> KeysPressed { get; set; }

        public bool Handled = false;
    }

    public delegate void KeyDownHookEventHandler(object sender, KeyDownHookEventArgs args);

}
