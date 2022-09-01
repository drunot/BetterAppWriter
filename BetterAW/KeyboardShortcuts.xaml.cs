using BetterAW.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Xml.Linq;
using static System.Windows.Forms.VisualStyles.VisualStyleElement;
using Window = System.Windows.Window;

namespace BetterAW {

    public partial class KeyboardShortcuts : Window {
        private static List<KeyboardShortcuts> openWindows = new List<KeyboardShortcuts>();
        public static event ShortcutAddEventHandler addEvent;
        public static event ShortcutRemoveEventHandler removeEvent;
        private class BaseEntry {
            public string Name;
            public string Discription;
            public object Icon;

            public BaseEntry(string name, string discription, object icon = null) {
                Name = name;
                Discription = discription;
                Icon = icon;
            }
        }

        private class ShortcutEntry : BaseEntry {
            public SortedSet<System.Windows.Forms.Keys> Keys = new SortedSet<System.Windows.Forms.Keys>();

            public ShortcutEntry(string name, string discription, SortedSet<System.Windows.Forms.Keys> keys) : base(name, discription) {
                Keys = keys;
            }
        }

        private class BooleanEntry : BaseEntry {
            public bool Checked = false;

            public BooleanEntry(string name, string discription, bool isChecked, object icon = null) : base(name, discription, icon) {
                Checked = isChecked;
            }
        }

        private static Dictionary<string, List<BaseEntry>> Entries = new Dictionary<string, List<BaseEntry>>();

        public static void AddBoolen(string category, string name, string discription, bool isChecked = false, object icon = null) {
            try {
                if (!Entries.Keys.Contains(category)) {
                    Entries.Add(category, new List<BaseEntry>());

                }
                Entries[category].Add(new BooleanEntry(name, discription, isChecked, icon));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Add shortcut to the window.
        public static void AddShortcut(string category, string name, string discription, SortedSet<System.Windows.Forms.Keys> keys) {
            try {
                if (!Entries.Keys.Contains(category)) {
                    Entries.Add(category, new List<BaseEntry>());

                }
                Entries[category].Add(new ShortcutEntry(name, discription, keys));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public static void AddKeybordShortcut(string name, SortedSet<System.Windows.Forms.Keys> keys) {
            bool match = false;
            foreach (var entry in Entries) {
                foreach (var subentry in entry.Value) {
                    if (subentry.GetType() == typeof(BooleanEntry)) {
                        continue;
                    }
                    if (subentry.Name == name) {
                        (subentry as ShortcutEntry).Keys = keys;
                        match = true;
                        break;
                    }
                }
                if (match) {
                    break;
                }
            }
            foreach (var window in openWindows) {
                window.Dispatcher.Invoke(() => window.LoadShortcuts());
                
            }
        }
        public static void RemoveKeybordShortcut(string name) {
            AddKeybordShortcut(name, null);
        }

        // Draw all the shortcuts to the window.
        public void LoadShortcuts() {
            try {
                this.ContentPanel.Children.Clear();
                foreach (var entry in Entries) {
                    KeyboardShortcutLabel label = new KeyboardShortcutLabel();
                    label.Content = entry.Key;
                    this.ContentPanel.Children.Add(label);
                    foreach (var subentry in entry.Value) {
                        if (subentry.GetType() == typeof(ShortcutEntry)) {
                            this.ContentPanel.Children.Add(new KeyboardShortcut(subentry.Name, subentry.Discription, (subentry as ShortcutEntry).Keys));
                        } else if (subentry.GetType() == typeof(BooleanEntry)) {
                            this.ContentPanel.Children.Add(new LanguageShortcut(subentry.Name, subentry.Discription, (subentry as BooleanEntry).Checked, subentry.Icon));
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
            openWindows.Add(this);
            this.LoadShortcuts();
            this.IsVisibleChanged += (s, e) => {
                IsClosed = !this.IsVisible;
            };
            this.Closed += (s, e) => {
                IsClosed = true;
                openWindows.Remove(this);
            };
            KeyboardShortcut.AddEvent = (self) => {
                if (addEvent != null) {
                    var eventArgs = new ShortcutAddEventArgs(self.ShortcutName);
                    addEvent(self, eventArgs);
                }
            };
            KeyboardShortcut.RemoveEvent = (self) => {
                if (removeEvent != null) {
                    Terminal.Print("removeEvent != null\n");
                    var eventArgs = new ShortcutRemoveEventArgs(self.ShortcutName);
                    removeEvent(self, eventArgs);
                }
            };
        }
    }
}
