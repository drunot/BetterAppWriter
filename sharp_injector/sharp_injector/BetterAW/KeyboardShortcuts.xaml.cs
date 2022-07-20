using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace BetterAW {

    public delegate void ShortcutEvent();
    public class BaseShortcutInfo {
        public string Name;
        public string ShortcutText;
        public object Icon;
    }
    public class KeyboardShortcutInfo : BaseShortcutInfo, IComparable<KeyboardShortcutInfo> {
        public KeyboardShortcutInfo() { }
        public KeyboardShortcutInfo(string name) {
            Name = name;
            ShortcutText = String.Empty;
            Icon = null;
        }

        public SortedSet<System.Windows.Forms.Keys> keyBinding = new SortedSet<System.Windows.Forms.Keys>();
        public static bool operator <(KeyboardShortcutInfo lhs, KeyboardShortcutInfo rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (lhs.keyBinding is null) {
                return !(rhs.keyBinding is null);
            }
            var lhsA = lhs.keyBinding.ToArray();
            var rhsA = rhs.keyBinding.ToArray();
            for (int i = 0; i < lhsA.Length; i++) {
                if (lhsA[i] < rhsA[i]) {
                    return true;
                } else if (lhsA[i] > rhsA[i]) {
                    return false;
                }
            }
            return false;
        }
        public static bool operator >(KeyboardShortcutInfo lhs, KeyboardShortcutInfo rhs) {
            if (lhs is null) {
                return false;
            }
            if (rhs is null) {
                return lhs is null;
            }
            if (lhs.keyBinding is null) {
                return false;
            }
            var lhsA = lhs.keyBinding.ToArray();
            var rhsA = rhs.keyBinding.ToArray();
            for (int i = 0; i < lhsA.Length; i++) {
                if (lhsA[i] > rhsA[i]) {
                    return true;
                } else if (lhsA[i] < rhsA[i]) {
                    return false;
                }
            }
            return false;
        }
        public static bool operator ==(KeyboardShortcutInfo lhs, KeyboardShortcutInfo rhs) {
            if (lhs is null) {
                return rhs is null;
            }
            if (rhs is null) {
                return !(lhs is null);
            }
            if (lhs.keyBinding is null) {
                return rhs.keyBinding is null;
            }
            return lhs.keyBinding.SetEquals(rhs.keyBinding);
        }

        public static bool operator !=(KeyboardShortcutInfo lhs, KeyboardShortcutInfo rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (lhs.keyBinding is null) {
                return !(rhs.keyBinding is null);
            }
            return !lhs.keyBinding.SetEquals(rhs.keyBinding);
        }

        public override bool Equals(object obj) {

            // If obj is KeyboardInternalShortcutInfo then use normal equal
            if (obj is KeyboardShortcutInfo) {
                return (obj as KeyboardShortcutInfo) == this;
            }

            // Else only return true if both is null
            return (obj == null && this == null);
        }
        public override int GetHashCode() {
            return keyBinding.GetHashCode();
        }
        public int CompareTo(KeyboardShortcutInfo y) {
            if (this == y) {
                return 0;
            }
            if (this < y) {
                return -1;
            }
            return 1;
        }
    }
    public class LanguageShortcutInfo : BaseShortcutInfo {
        public bool Selected;
    }
    public partial class KeyboardShortcuts : Window {
        public static event Events.ShortcutAddEventHandler ShortcutAdd;
        public static event Events.ShortcutStartAddEventHandler ShortcutStartAdd;
        public static event Events.ShortcutRemoveEventHandler ShortcutRemove;

        public static void ShortcutRemoveInvokeExternal(object sender, Events.ShortcutRemoveEventArgs e) {
            if (ShortcutRemove != null) {
                ShortcutRemove(sender, e);
            }
        }
        public static void ShortcutAddInvokeExternal(object sender, Events.ShortcutAddEventArgs e) {
            if (ShortcutAdd != null) {
                ShortcutAdd(sender, e);
            }
        }

        public delegate BaseShortcutInfo RegisterShortcut(BaseShortcutInfo newShortcut);
        public static List<UIElement> stackElements = null;
        public static Dictionary<string, List<BaseShortcutInfo>> Shortcuts { get; private set; } = new Dictionary<string, List<BaseShortcutInfo>>();
        // Add shortcut to the window.
        public static void AddShortcut(string name, BaseShortcutInfo Shortcut) {
            try {
                if (Shortcuts.Keys.Contains(name)) {
                    Shortcuts[name].Add(Shortcut);
                } else {
                    Shortcuts[name] = new List<BaseShortcutInfo> { Shortcut };
                }

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        // Draw all the shortcuts to the window.
        public void LoadShortcuts() {
            try {
                ShortcutRemove += (s, e) => {
                    Dispatcher.Invoke(new Action(() => {
                        try {
                            foreach (var child in this.ContentPanel.Children) {
                                if (child.GetType() != typeof(KeyboardShortcut)) {
                                    continue;
                                }
                                if ((child as KeyboardShortcut).ShortcutInfo.Name == e.ShortcutName) {
                                    var shortcut = (child as KeyboardShortcut).ShortcutInfo;
                                    shortcut.keyBinding = null;
                                    (child as KeyboardShortcut).ShortcutInfo = shortcut;

                                    break;
                                }
                            }
                        } catch (Exception ex) {
                            Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        }
                    }));
                };
                ShortcutAdd += (s, e) => {
                    Dispatcher.Invoke(new Action(() => {
                        try {
                            foreach (var child in this.ContentPanel.Children) {
                                if (child.GetType() != typeof(KeyboardShortcut)) {
                                    continue;
                                }
                                if ((child as KeyboardShortcut).ShortcutInfo.Name == e.ShortcutName) {

                                    var shortcut = (child as KeyboardShortcut).ShortcutInfo;
                                    shortcut.keyBinding = e.Shortcut;
                                    (child as KeyboardShortcut).ShortcutInfo = shortcut;

                                    break;
                                }
                            }
                        } catch (Exception ex) {
                            Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        }
                    }));
                };
                foreach (KeyValuePair<string, List<BaseShortcutInfo>> entry in Shortcuts) {
                    KeyboardShortcutLabel label = new KeyboardShortcutLabel();
                    label.Content = entry.Key;
                    this.ContentPanel.Children.Add(label);
                    KeyboardShortcut.AddEvent = (self) => {
                        // Unregister all other events for ShortcutAdd
                        if (ShortcutStartAdd != null) {
                            ShortcutStartAdd(self, new Events.ShortcutStartAddEventArgs(self.ShortcutInfo.Name));
                        }
                    };
                    KeyboardShortcut.RemoveEvent = (self) => {
                        try {
                            if (ShortcutRemove != null) {
                                ShortcutRemove(self, new Events.ShortcutRemoveEventArgs(self.ShortcutInfo.Name));
                            }
                        } catch (Exception ex) {
                            Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        }
                    };
                    foreach (BaseShortcutInfo shortcutInfo in entry.Value) {
                        if (shortcutInfo.GetType() == typeof(KeyboardShortcutInfo)) {
                            this.ContentPanel.Children.Add(new KeyboardShortcut((KeyboardShortcutInfo)shortcutInfo));
                        } else if (shortcutInfo.GetType() == typeof(LanguageShortcutInfo)) {
                            this.ContentPanel.Children.Add(new LanguageShortcut((LanguageShortcutInfo)shortcutInfo));
                        }
                    }
                }
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public bool IsClosed = true;
        public KeyboardShortcuts() {
            InitializeComponent();
            this.LoadShortcuts();
            this.IsVisibleChanged += (s, e) => { IsClosed = !this.IsVisible; };
            this.Closed += (s, e) => { IsClosed = true; };
        }
    }
}
