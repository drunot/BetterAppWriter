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
    public class BaseShortcutInfo
    {
        public string Name;
        public string ShortcutText;
        public object Icon;
    }
    public class KeyboardShortcutInfo : BaseShortcutInfo
        {
        public KeyboardShortcutInfo() { }
        public KeyboardShortcutInfo(string name)
        {
            Name = name;
            ShortcutText = String.Empty;
            ShortcutEvent = null;
            keyBinding = new HashSet<System.Windows.Forms.Keys>();
            Icon = null;
        }

        public ShortcutEvent ShortcutEvent;
        public HashSet<System.Windows.Forms.Keys> keyBinding;

        /* Make it much easier to compaire the things. */
        /* Do to how this should work in many plases both the hash creation and the object compairation is limited to only check the name */
        /* If keyboard shortcut should be compaired be explicit. */
        public static bool Equals(KeyboardShortcutInfo a, KeyboardShortcutInfo b)
        {
            if(a is null || b is null)
            {
                return a is null && b is null;
            }
            return a.Name == b.Name;
        }
        public bool Equals(KeyboardShortcutInfo a)
        {
            return Equals(this, a);
        }

        public static bool operator ==(KeyboardShortcutInfo a, KeyboardShortcutInfo b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(KeyboardShortcutInfo a, KeyboardShortcutInfo b)
        {
            return !Equals(a, b);
        }
        public static bool Equals(KeyboardShortcutInfo a, HashSet<System.Windows.Forms.Keys> b)
        {
            return a.keyBinding.IsSubsetOf(b) && a.keyBinding.Count == b.Count;
        }
        public bool Equals(HashSet<System.Windows.Forms.Keys> a)
        {
            return Equals(this, a);
        }

        public static bool operator ==(KeyboardShortcutInfo a, HashSet<System.Windows.Forms.Keys> b)
        {
            return Equals(a, b);
        }


        public static bool operator !=(KeyboardShortcutInfo a, HashSet<System.Windows.Forms.Keys> b)
        {
            return !Equals(a, b);
        }

        public static bool Equals(KeyboardShortcutInfo a, string b)
        {
            return a.Name == b;
        }
        public bool Equals(string a)
        {
            return Equals(this, a);
        }

        public static bool operator ==(KeyboardShortcutInfo a, string b)
        {
            return Equals(a, b);
        }

        public static bool operator !=(KeyboardShortcutInfo a, string b)
        {
            return !Equals(a, b);
        }
        public override bool Equals(object a)
        {
            return a != null && a.GetType() == typeof(KeyboardShortcutInfo) && Equals((KeyboardShortcutInfo)a);
        }
        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }
    }
    public class LanguageShortcutInfo : BaseShortcutInfo { }
    public partial class KeyboardShortcuts : Window
    {
        public delegate void RekordingKey(HashSet<System.Windows.Forms.Keys> newShortcut);
        public delegate void StartRekordingKeys(RekordingKey pressedKeys);
        public delegate KeyboardShortcutInfo RegisterShortcut(KeyboardShortcutInfo newShortcut);
        public static StartRekordingKeys RekordingKeysDelegate = (pressedKeys) => { };
        public static RegisterShortcut RegisterShortcutDelegate = (e) => e;
        public static List<UIElement> stackElements = null;
        public static Dictionary<string, List<BaseShortcutInfo>> Shortcuts { get; private set; } = new Dictionary<string, List<BaseShortcutInfo>>();
        // Add shortcut to the window.
        public static void AddShortcut(string name, BaseShortcutInfo Shortcut)
        {
            try 
            {
                if (Shortcuts.Keys.Contains(name))
                {
                    Shortcuts[name].Add(Shortcut);
                }
                else
                {
                    Shortcuts[name] = new List<BaseShortcutInfo> { Shortcut };
                }

            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        // Draw all the shortcuts to the window.
        public void LoadShortcuts() {
            try 
            {
                foreach (KeyValuePair<string, List<BaseShortcutInfo>> entry in Shortcuts)
                {
                    KeyboardShortcutLabel label = new KeyboardShortcutLabel();
                    label.Content = entry.Key;
                    this.ContentPanel.Children.Add(label);
                    KeyboardShortcut.AddEvent = (self) =>
                    {
                        RekordingKeysDelegate((newShortcut) => self.Dispatcher.Invoke(new Action(() =>
                        {
                            try 
                            { 
                                var shortcut = self.ShortcutInfo;
                                System.Windows.Forms.Keys[] keys = new System.Windows.Forms.Keys[newShortcut.Count];
                                newShortcut.CopyTo(keys, 0);
                                shortcut.keyBinding = new HashSet<System.Windows.Forms.Keys>(keys);
                                self.ShortcutInfo = shortcut;
                                RegisterShortcutDelegate(shortcut);
                            }
                            catch (Exception ex)
                            {
                                Terminal.Print(string.Format("{0}\n", ex.ToString()));
                            }
                        })));
                    };
                    KeyboardShortcut.RemoveEvent = (self) =>
                    {
                        self.Dispatcher.Invoke(new Action(() =>
                        {
                            try 
                            { 
                                var shortcut = self.ShortcutInfo;
                                shortcut.keyBinding = null;
                                self.ShortcutInfo = shortcut;
                                RegisterShortcutDelegate(shortcut);

                            }
                            catch (Exception ex)
                            {
                                Terminal.Print(string.Format("{0}\n", ex.ToString()));
                            }
                        }));
                    };
                    foreach (BaseShortcutInfo shortcutInfo in entry.Value)
                    {
                        if(shortcutInfo.GetType() == typeof(KeyboardShortcutInfo))
                        {
                            this.ContentPanel.Children.Add(new KeyboardShortcut((KeyboardShortcutInfo)shortcutInfo));
                        } else if(shortcutInfo.GetType() == typeof(LanguageShortcutInfo))
                        {
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
