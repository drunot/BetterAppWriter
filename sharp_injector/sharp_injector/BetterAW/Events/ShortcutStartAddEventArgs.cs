using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BetterAW.Events {
    public class ShortcutStartAddEventArgs : EventArgs {
        public ShortcutStartAddEventArgs(string shortcutName) { ShortcutName = shortcutName; }

        public string ShortcutName { get; set; }
    }
    public delegate void ShortcutStartAddEventHandler(object sender, ShortcutStartAddEventArgs e);
}
