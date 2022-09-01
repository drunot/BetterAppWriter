using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sharp_injector.Events {
    public class KeyUpHookEventArgs : EventArgs {
        public KeyUpHookEventArgs(Keys upKey, SortedSet<Keys> keysPressed) {
            UpKey = upKey;
            KeysPressed = keysPressed;
        }
        public Keys UpKey { get; set; }
        public SortedSet<Keys> KeysPressed { get; set; }

        public bool Handled = false;
    }

    public delegate void KeyUpHookEventHandler(object sender, KeyUpHookEventArgs args);
}
