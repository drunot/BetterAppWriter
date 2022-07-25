using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace BetterAW.Events {

    public class ShortcutRemoveEventArgs : EventArgs {
        public ShortcutRemoveEventArgs(string shortcutName, bool saveSetting = true) { ShortcutName = shortcutName; SaveSettings = saveSetting; }

        public string ShortcutName { get; set; }
        public bool SaveSettings { get; set; }
    }

    public delegate void ShortcutRemoveEventHandler(object sender, ShortcutRemoveEventArgs e);
}
