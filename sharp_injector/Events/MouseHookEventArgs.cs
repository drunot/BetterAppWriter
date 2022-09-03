using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sharp_injector.Events {
    public class MouseHookEventArgs : EventArgs {
        public MouseHookEventArgs(MouseButtons button, bool mouseDown) {
            Button = button;
            MouseDown = mouseDown;
        }
        public MouseButtons Button;
        public bool MouseDown;

        public bool MouseUp {
            get {
                return !MouseDown;
            }
            set {
                MouseDown = !value;
            }
        }
    }
}
