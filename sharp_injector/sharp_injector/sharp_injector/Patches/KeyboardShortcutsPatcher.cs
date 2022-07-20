using BetterAW;
using Newtonsoft.Json;
using sharp_injector.Debug;
using sharp_injector.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using sharp_injector.DTO;
using System.IO;
using System.Windows.Controls;
using Newtonsoft.Json.Linq;
using sharp_injector.Helpers;
using System.Windows.Forms;

namespace sharp_injector.Patches {
    public class BaseInternalShortcutInfo {
        public string Name;

    }
    public class KeyboardInternalShortcutInfo : BaseInternalShortcutInfo, IComparable<KeyboardInternalShortcutInfo> {
        public KeyboardInternalShortcutInfo() { }
        public KeyboardInternalShortcutInfo(string name) {
            Name = name;
        }
        public Helpers.KeyboardEventDelegate KeyboardEvent;
        public SortedSet<System.Windows.Forms.Keys> keyBinding = new SortedSet<System.Windows.Forms.Keys>();
        public static bool operator <(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
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
        public static bool operator >(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
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
        public static bool operator ==(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
            if (lhs is null) {
                return rhs is null;
            }
            if (lhs.keyBinding is null) {
                return rhs.keyBinding is null;
            }
            return lhs.keyBinding.SetEquals(rhs.keyBinding);
        }

        public static bool operator !=(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if(rhs is null) {
                return !(lhs is null);
            }
            if (lhs.keyBinding is null) {
                return !(rhs.keyBinding is null);
            }
            return !lhs.keyBinding.SetEquals(rhs.keyBinding);
        }

        public override bool Equals(object obj) {

            // If obj is KeyboardInternalShortcutInfo then use normal equal
            if (obj is KeyboardInternalShortcutInfo) {
                return (obj as KeyboardInternalShortcutInfo) == this;
            }

            // Else only return true if both is null
            return (obj == null && this == null);
        }
        public override int GetHashCode() {
            return keyBinding.GetHashCode();
        }
        public int CompareTo(KeyboardInternalShortcutInfo y) {
            if (this == y) {
                return 0;
            }
            if (this < y) {
                return -1;
            }
            return 1;
        }
    }

    public class KeyboardShortcutsPatcher : IPatcher {
        private static readonly string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\BetterAppWriter\keyboard.json";
        // Get the tooltip from the button (IDK why I made this to a function but o'well)
        private static string GetStringFromBtn(object button) {
            return (string)((PropertyInfo)button.GetType().GetMember("ToolTip", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(button, null);

        }

        static void GetDataFromContextMenuItem(FieldInfo MenuItemInfo, object context, out KeyboardShortcutInfo ksi, out KeyboardInternalShortcutInfo kisi) {
            ksi = new KeyboardShortcutInfo();
            kisi = new KeyboardInternalShortcutInfo();
            // Get the values of the MenuItem to propagate KeyboardShortcutInfo
            ksi.Name = MenuItemInfo.Name;
            kisi.Name = MenuItemInfo.Name;
            object MenuItem = MenuItemInfo.GetValue(context);
            // Ignore hidden elements.
            if (((UIElement)MenuItem).Visibility != Visibility.Visible) {
                ksi = null;
                kisi = null;
                return;
            }
            ksi.ShortcutText = (string)((PropertyInfo)MenuItem.GetType().GetMember("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);
            ksi.Icon = ((PropertyInfo)MenuItem.GetType().GetMember("MenuItemIcon", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);

            // Try and get the eventhandler function from the context. Return null if not found.
            var PreviewMouseLeftButtonDownInfo = context.GetType().GetMethod(MenuItemInfo.Name + "_PreviewMouseLeftButtonUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (PreviewMouseLeftButtonDownInfo != null) {
                kisi.KeyboardEvent = (SortedSet<Keys> k) => { PreviewMouseLeftButtonDownInfo.Invoke(context, new object[] { null, null }); return false; };
            } else {
                Terminal.Print($"{MenuItemInfo.Name}: Was skipped.\n");
                ksi = null;
                kisi = null;
                return;
            }
            return;
        }

        // Holds current pressed keys.
        static HashSet<System.Windows.Forms.Keys> pressedKeys = new HashSet<System.Windows.Forms.Keys>();
        // Holds registered keyboard shortcuts.
        static SortedSet<KeyboardInternalShortcutInfo> RegisteredKeyboardShortcut = new SortedSet<KeyboardInternalShortcutInfo>();
        static SortedSet<KeyboardInternalShortcutInfo> AvailableKeyboardShortcut = new SortedSet<KeyboardInternalShortcutInfo>();

        // Window references
        static object predictionWindow_;
        static object appWriterService_;
        static object toolbarWindow_;
        static object languageWindow_;
        static Type appWriterServiceType_;
        // Holds the types for the dynamic DynamicMethod alocator.
        static Type[] injectedKeyEventTypes;
        // Normal Keyboard Event (that can be converted into a Injected keyboard event)
        public delegate void NormalKeyboardEvent(object sender, System.Windows.Forms.KeyEventArgs e);
        public KeyboardShortcutsPatcher(object predictionWindow, object toolbarWindow) {
            try {
                // Add references on creation.
                predictionWindow_ = predictionWindow;
                toolbarWindow_ = toolbarWindow;
                appWriterServiceType_ = Type.GetType("AppWriter.AppWriterService,AppWriter.Core");
                appWriterService_ = toolbarWindow_.GetType().GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                languageWindow_ = toolbarWindow_.GetType().GetField("_languageWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                // Register this patch.
                PatchRegister.RegisterPatch(this);

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Add a keyboardshortcut to be handled.
        public static void RegisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true) {
            Terminal.Print("Alive 1\n");
            KeyboardInternalShortcutInfo toRemove = null;
            foreach (var shortcut in RegisteredKeyboardShortcut) {
                if (shortcut.keyBinding.SetEquals(keyboardInternalShortcutInfo.keyBinding)) {
                    KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(keyboardInternalShortcutInfo.Name));
                    toRemove = shortcut;
                    Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(keyboardInternalShortcutInfo.keyBinding);
                    break;
                }
            }
            if (toRemove != null) {
                RegisteredKeyboardShortcut.Remove(toRemove);
            }
            Terminal.Print("Alive 2\n");
            if (RegisteredKeyboardShortcut.Contains(keyboardInternalShortcutInfo)) {
                Terminal.Print("Alive 2.5\n");
                if (RegisteredKeyboardShortcut.TryGetValue(keyboardInternalShortcutInfo, out var shortcut)) {
                    Terminal.Print("Alive 2.6\n");
                    Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(shortcut.keyBinding);
                }
                RegisteredKeyboardShortcut.Remove(keyboardInternalShortcutInfo);
            }
            Terminal.Print("Alive 3\n");
            RegisteredKeyboardShortcut.Add(keyboardInternalShortcutInfo);
            Helpers.WindowsKeyboardHooks.AddKeyboardEvent(new WindowsKeyboardEvent(keyboardInternalShortcutInfo.keyBinding, keyboardInternalShortcutInfo.KeyboardEvent));
            KeyboardShortcuts.ShortcutAddInvokeExternal(null, new BetterAW.Events.ShortcutAddEventArgs(keyboardInternalShortcutInfo.Name, keyboardInternalShortcutInfo.keyBinding));
            if (saveAfter) {
                SaveSettings(settingsPath);
            }
            Terminal.Print("Alive 4\n");
        }

        public static void SaveSettings(string path) {

            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            string toSave = JsonConvert.SerializeObject(((object[])RegisteredKeyboardShortcut.Select(x => JSONKeybinding.FromKeyboardShortcutInfo(x)).ToArray()).Concat(LanguageShortcut.EnabledLanguage.Select(x => {
                var k = new JSONLanguage();
                k.Name = x;
                k.Selected = true;
                return k;
            }).ToArray()));
            File.WriteAllText(path, toSave);
        }
        public static JObject[] LoadSettngs(string path) {

            if (!File.Exists(path)) {
                return null;
            }
            string data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<JObject[]>(data);
        }

        /* Unregister a keyboard shortcut */
        public static bool UnregisterKeyboardShortcut(string Name, bool saveAfter = true) {
            var elem = RegisteredKeyboardShortcut.Where(x => x.Name == Name).First();
            Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(elem.keyBinding);
            KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(Name));
            var ret = RegisteredKeyboardShortcut.Remove(elem);
            if (saveAfter) {
                SaveSettings(settingsPath);
            }
            return ret;
        }

        public static bool UnregisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true) {
            var elem = RegisteredKeyboardShortcut.Where(x => x.Name == keyboardInternalShortcutInfo.Name).First();
            Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(elem.keyBinding);
            KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(keyboardInternalShortcutInfo.Name));
            var ret = RegisteredKeyboardShortcut.Remove(elem);
            if (saveAfter) {
                SaveSettings(settingsPath);
            }
            return ret;
        }

        // Handle pressed keys.
        //public static void DownHandler(object sender, System.Windows.Forms.KeyEventArgs e) {
        //    try {
        //        switch (e.KeyCode) {
        //            case System.Windows.Forms.Keys.LControlKey:
        //            case System.Windows.Forms.Keys.RControlKey:
        //                pressedKeys.Add(System.Windows.Forms.Keys.ControlKey);
        //                break;
        //            case System.Windows.Forms.Keys.LMenu:
        //            case System.Windows.Forms.Keys.RMenu:
        //                pressedKeys.Add(System.Windows.Forms.Keys.Menu);
        //                break;
        //            case System.Windows.Forms.Keys.LShiftKey:
        //            case System.Windows.Forms.Keys.RShiftKey:
        //                pressedKeys.Add(System.Windows.Forms.Keys.ShiftKey);
        //                break;
        //            default:
        //                pressedKeys.Add(e.KeyCode);
        //                break;
        //        }
        //        //Terminal.Print("Down: ");
        //        //foreach(var key in pressedKeys)
        //        //{
        //        //    Terminal.Print($"{key} ");
        //        //}
        //        //Terminal.Print("\n");
        //        foreach (var shortcut in RegisteredKeyboardShortcut) {
        //            if (shortcut == pressedKeys) {
        //                Terminal.Print("Match\n");
        //                shortcut.ShortcutEvent();
        //            }
        //        }
        //    } catch (Exception ex) {
        //        Terminal.Print(string.Format("{0}\n", ex.ToString()));
        //    }

        //}
        // Handle released keys.
        public static void UpHandler(object sender, BetterAW.Events.ShortcutStartAddEventArgs eventArgs) {
            void private_KeyUpHook(object s, Events.KeyUpHookEventArgs e) {
                try {
                    var elem = AvailableKeyboardShortcut.Where(x => x.Name == eventArgs.ShortcutName).First();
                    elem.keyBinding = e.KeysPressed;
                    RegisterKeyboardShortcut(elem);

                } catch (Exception ex) {
                    Terminal.Print(string.Format("{0}\n", ex.ToString()));
                }
                Helpers.WindowsKeyboardHooks.KeyUpHook -= private_KeyUpHook;
            };
            Helpers.WindowsKeyboardHooks.KeyUpHook += private_KeyUpHook;
        }

        private static void FindShortcuts(string windowName, string buttonName) {
            try {
                // Get the writeWindow.
                object writeMenu = toolbarWindow_.GetType().GetField(windowName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                // Invoke a dispatcher for the window.
                ((Window)writeMenu).Dispatcher.Invoke(new Action(() => {
                    try {
                        // Get the toolTip from the WriteSettingsBtn to use in the Title for the shortcuts.
                        object WriteSettingsBtn = toolbarWindow_.GetType().GetField(buttonName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                        var keyName = $"{GetStringFromBtn(WriteSettingsBtn)} shortcuts";
                        var settings = LoadSettngs(settingsPath);
                        // Get all menuContextItems, and add them to the keyboard shortcuts if their event handler is found.
                        foreach (var elem in writeMenu.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                            if (elem.MemberType == MemberTypes.Field) {
                                if (((FieldInfo)elem).FieldType.Name == "ContextMenuItem") {
                                    GetDataFromContextMenuItem((FieldInfo)elem, writeMenu, out var info, out var internalInfo);
                                    if (!(info is null)) {
                                        AvailableKeyboardShortcut.Add(internalInfo);
                                        foreach (var setting in settings) {
                                            Terminal.Print($"setting.Name: {setting["Name"]}\n");
                                            Terminal.Print($"infoNonNull.Name: {info.Name}\n");
                                            if ((string)setting["Name"] == info.Name) {
                                                internalInfo.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                                                info.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                                                RegisterKeyboardShortcut(internalInfo, false);
                                            }
                                        }
                                        KeyboardShortcuts.AddShortcut(keyName, info);
                                    }
                                }
                            }
                        }
                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public static bool ToggleLanguage(SortedSet<Keys> k) {
            if (LanguageShortcut.EnabledLanguage.Count <= 0) {
                return false;
            } else {
                var method = appWriterService_.GetType().GetMethod("ChangeProfile", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                bool found = false;
                string curLang = AppWriterServicePatcher.GetCurrentLanguage();
                foreach (var lang in LanguageShortcut.EnabledLanguage) {
                    if (curLang == lang) {
                        found = true;
                    } else if (found) {
                        method.Invoke(appWriterService_, new object[] { lang });
                        return false;
                    }
                }
                method.Invoke(appWriterService_, new object[] { LanguageShortcut.EnabledLanguage.First() });
            }
            return false;
        }
        private static void LanguageToggleShortcuts() {
            try {
                StackPanel LanguageList = (StackPanel)languageWindow_.GetType().GetField("LanguageList", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(languageWindow_);
                Dictionary<string, object> icons = new Dictionary<string, object>();
                LanguageList.Dispatcher.Invoke(new Action(() => {
                    foreach (var contextMenu in LanguageList.Children) {
                        //MenuItemIcon
                        object icon = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements").GetProperty("MenuItemIcon", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(contextMenu, null);
                        string lang = (string)((FrameworkElement)contextMenu).Tag;
                        Terminal.Print($"{lang}\n");
                        icons.Add(lang, icon);
                    }
                }));
                var categoryName = "Toggle langauges shortcut";
                KeyboardShortcutInfo firstInfo = new KeyboardShortcutInfo();
                KeyboardInternalShortcutInfo firstInternalInfo = new KeyboardInternalShortcutInfo();
                firstInfo.ShortcutText = Translation.ShortcutToggleLanguages;
                firstInfo.Name = "LangToggle";
                firstInternalInfo.Name = "LangToggle";
                firstInternalInfo.KeyboardEvent = ToggleLanguage;
                var settings = LoadSettngs(settingsPath);
                var langSettings = settings.Where(x => (string)x["Name"] == firstInfo.Name).ToArray();
                if (langSettings.Length > 0) {
                    firstInternalInfo.keyBinding = JSONKeybinding.FromJObject(langSettings[0]).ToKeyboardShortcutInfo().keyBinding;
                    firstInfo.keyBinding = JSONKeybinding.FromJObject(langSettings[0]).ToKeyboardShortcutInfo().keyBinding;
                    RegisterKeyboardShortcut(firstInternalInfo, false);
                }
                KeyboardShortcuts.AddShortcut(categoryName, firstInfo);
                var langauges = new SortedDictionary<string, string>(AppWriterServicePatcher.GetAvalibleLanguage().ToDictionary(x => Translations.GetString(x.Replace("-", "_")), x => x));
                foreach (var langauge in langauges) {
                    LanguageShortcutInfo lang = new LanguageShortcutInfo();
                    lang.ShortcutText = langauge.Key;
                    lang.Name = langauge.Value;
                    if (icons.ContainsKey(lang.Name)) {
                        lang.Icon = icons[lang.Name];
                    }
                    foreach (var setting in settings) {
                        if ((string)setting["Name"] == lang.Name) {
                            lang.Selected = (bool)setting["Selected"];
                            LanguageShortcut.EnabledLanguage.Add(lang.Name);
                            break;
                        }
                    }
                    KeyboardShortcuts.AddShortcut(categoryName, lang);
                }
                // Make Language shortcut changes save the json file:
                LanguageShortcut.ChnagedChekedEvent = (checkedChnaged) => SaveSettings(settingsPath);
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Do the patching
        public void Patch() {
            try {
                Helpers.WindowsKeyboardHooks.ApplicationHook();
                FindShortcuts("_writeWindow", "WriteSettingsBtn");
                FindShortcuts("_readWindow", "ReadSettingsBtn");
                LanguageToggleShortcuts();
                KeyboardShortcuts.ShortcutStartAdd += UpHandler;
                // Get the events and their parameter type.
                //var appWriterServiceKeyDownEventInfo = appWriterServiceType_.GetEvent("KeyDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                //var appWriterServiceKeyUpEventInfo = appWriterServiceType_.GetEvent("KeyUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                //injectedKeyEventTypes = appWriterServiceKeyDownEventInfo.EventHandlerType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray();
                // Create the dynamic versions of the event handlers.
                //DynamicMethod handlerDown = ConvertToInjectedKeyboardEvent(typeof(KeyboardShortcutsPatcher).GetMethod(nameof(DownHandler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), "InjectedHandlerDown");
                //DynamicMethod handlerUp = ConvertToInjectedKeyboardEvent(typeof(KeyboardShortcutsPatcher).GetMethod(nameof(UpHandler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), "InjectedHandlerUp");
                // Disable all build in keyboard shortcuts.
                appWriterServiceType_.GetField("EventKeyDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(appWriterService_, null);
                appWriterServiceType_.GetField("EventKeyUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(appWriterService_, null);
                // Register the new events.
                //appWriterServiceKeyDownEventInfo.AddEventHandler(appWriterService_, handlerDown.CreateDelegate(appWriterServiceKeyDownEventInfo.EventHandlerType));
                //appWriterServiceKeyUpEventInfo.AddEventHandler(appWriterService_, handlerUp.CreateDelegate(appWriterServiceKeyUpEventInfo.EventHandlerType));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        //They use their own KeyEventArgs, luckily it can be casted to a normal KeyEventArgs. This function calls the normal version.
        public static DynamicMethod ConvertToInjectedKeyboardEvent(MethodInfo normalKeyboardEventInfo, string eventName) {
            var method = new DynamicMethod(eventName, null, injectedKeyEventTypes);
            var generator = method.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, normalKeyboardEventInfo);
            generator.Emit(OpCodes.Ret);
            return method;
        }
    }
}