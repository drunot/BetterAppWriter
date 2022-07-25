using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace sharp_injector.Events {
    public class AllKeyUpHookEventArgs : EventArgs {
        public Keys LastUpKey { get; set; }

        public AllKeyUpHookEventArgs(Keys lastUpKey) {
            LastUpKey = lastUpKey;
        }
    }

    public delegate void AllKeyUpHookEventHandler(object sender, AllKeyUpHookEventArgs args);
}
