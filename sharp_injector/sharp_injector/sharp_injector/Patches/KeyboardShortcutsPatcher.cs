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
using System.Windows.Forms;
using System.Windows.Input;
using System.Threading;
using HarmonyLib;
using sharp_injector.Events;

// This is a mess and should proberbly be designed better in the future...

namespace sharp_injector.Patches {
    public class BaseInternalShortcutInfo {
        public string Name;

    }
    public delegate bool KeyboardInternalShortcutPreamble(KeyboardInternalShortcutInfo self);
    public delegate void KeyboardInternalShortcutPostamble(KeyboardInternalShortcutInfo self);
    public class KeyboardInternalShortcutInfo : BaseInternalShortcutInfo, IComparable<KeyboardInternalShortcutInfo>, ICloneable {
        public KeyboardInternalShortcutInfo() { }
        public KeyboardInternalShortcutPreamble shotcutAddPreamble = null;
        public KeyboardInternalShortcutPostamble shotcutAddPostamble = null;
        public KeyboardInternalShortcutPreamble shotcutRemovePreamble = null;
        public KeyboardInternalShortcutPostamble shotcutRemovePostamble = null;
        public KeyboardInternalShortcutInfo(string name) {
            Name = name;
        }

        public object Clone() {
            return MemberwiseClone();
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
            if (rhs is null) {
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

    public class KeyboardShortcutsPatcher : IPatcher, IPredictionPatcher {
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

        // Holds registered keyboard shortcuts.
        static SortedSet<KeyboardInternalShortcutInfo> RegisteredKeyboardShortcuts = new SortedSet<KeyboardInternalShortcutInfo>();
        static List<KeyboardInternalShortcutInfo> AvailableKeyboardShortcuts = new List<KeyboardInternalShortcutInfo>();
        static List<KeyboardInternalShortcutInfo> SpecialKeyboardShortcuts = new List<KeyboardInternalShortcutInfo>();

        // Window references
        static object predictionWindow_;
        static object appWriterService_;
        static object toolbarWindow_;
        static object languageWindow_;
        static Type appWriterServiceType_;

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
                PatchRegister.RegisterPredictionPatch(this);

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Add a keyboardshortcut to be handled.
        public static void RegisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true) {
            // If any of the special post amble functions exist. Add to list so it can be found later
            if (!(keyboardInternalShortcutInfo.shotcutRemovePreamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutRemovePostamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutAddPreamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutAddPostamble is null)) {
                SpecialKeyboardShortcuts.Add(keyboardInternalShortcutInfo);
            }

            // If preamble is null or the preamble returns true. Add the shortcut normally.
            if (keyboardInternalShortcutInfo.shotcutAddPreamble is null || keyboardInternalShortcutInfo.shotcutAddPreamble(keyboardInternalShortcutInfo)) {
                // Check if other shortcuts uses same keybinding.
                // Could probably be done with RegisteredKeyboardShortcuts.Contains.
                foreach (var shortcut in RegisteredKeyboardShortcuts) {
                    if (shortcut.keyBinding.SetEquals(keyboardInternalShortcutInfo.keyBinding)) {
                        KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(shortcut.Name, false));
                        break;
                    }
                }

                // If this function already have a shortcut. Remove it.
                if (RegisteredKeyboardShortcuts.Where(x => x.Name == keyboardInternalShortcutInfo.Name).FirstOrDefault() != default(KeyboardInternalShortcutInfo)) {
                    var toRemove = RegisteredKeyboardShortcuts.Where(x => x.Name == keyboardInternalShortcutInfo.Name).FirstOrDefault();
                    if (toRemove != default(KeyboardInternalShortcutInfo)) {
                        KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(toRemove.Name, false));
                    }
                    RegisteredKeyboardShortcuts.Remove(keyboardInternalShortcutInfo);
                }

                // Registor the new shortcut.
                RegisteredKeyboardShortcuts.Add(keyboardInternalShortcutInfo);
                Helpers.WindowsKeyboardHooks.AddKeyboardEvent(new WindowsKeyboardEvent(keyboardInternalShortcutInfo.keyBinding, keyboardInternalShortcutInfo.KeyboardEvent));
            }
            if (!(keyboardInternalShortcutInfo.shotcutAddPostamble is null)) {
                keyboardInternalShortcutInfo.shotcutAddPostamble(keyboardInternalShortcutInfo);
            }
            KeyboardShortcuts.ShortcutAddInvokeExternal(null, new BetterAW.Events.ShortcutAddEventArgs(keyboardInternalShortcutInfo.Name, keyboardInternalShortcutInfo.keyBinding));

            // Save settings if true.
            if (saveAfter) {
                SaveSettings(settingsPath);
            }
        }

        public static void SaveSettings(string path) {

            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }

            string toSave = JsonConvert.SerializeObject(((object[])RegisteredKeyboardShortcuts.Select(x => JSONKeybinding.FromKeyboardShortcutInfo(x)).ToArray())
                .Concat(((object[])SpecialKeyboardShortcuts.Where(x => !RegisteredKeyboardShortcuts.Where(y => y.Name == x.Name).Any()).Select(x => JSONKeybinding.FromKeyboardShortcutInfo(x)).ToArray())
                .Concat(LanguageShortcut.EnabledLanguage.Select(x => {
                    var k = new JSONLanguage();
                    k.Name = x;
                    k.Selected = true;
                    return k;
                }).ToArray())
                .Concat(AvailableKeyboardShortcuts.Where(x => !RegisteredKeyboardShortcuts.Where(y => y.Name == x.Name).Any() && !SpecialKeyboardShortcuts.Where(y => y.Name == x.Name).Any())
                .Select(x => JSONKeybinding.FromKeyboardShortcutInfo(x, true)))));
            File.WriteAllText(path, toSave);
        }
        public static JObject[] LoadSettngs(string path) {

            if (!File.Exists(path)) {
                return new JObject[] { };
            }
            string data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<JObject[]>(data);
        }

        /* Unregister a keyboard shortcut */
        public static void UnregisterKeyboardShortcut(string Name, bool saveAfter = true) {
            // Use unregister event for this.
            KeyboardShortcuts.ShortcutRemoveInvokeExternal(null, new BetterAW.Events.ShortcutRemoveEventArgs(Name, saveAfter));
        }

        public static void UnregisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true) {
            UnregisterKeyboardShortcut(keyboardInternalShortcutInfo.Name, saveAfter);
        }


        public static void UpHandler(object sender, BetterAW.Events.ShortcutStartAddEventArgs eventArgs) {
            // Private function for regitoring the key on a upKey event.
            void private_KeyUpHook(object s, Events.KeyUpHookEventArgs e) {
                try {
                    // On key up registor keyboard shortcut.
                    KeyboardInternalShortcutInfo elem = (KeyboardInternalShortcutInfo)AvailableKeyboardShortcuts.Where(x => x.Name == eventArgs.ShortcutName).First().Clone();
                    elem.keyBinding = e.KeysPressed;
                    RegisterKeyboardShortcut(elem);

                } catch (Exception ex) {
                    Terminal.Print(string.Format("{0}\n", ex.ToString()));
                }

                // Unregistor event until next time.
                Helpers.WindowsKeyboardHooks.KeyUpHook -= private_KeyUpHook;
                Helpers.WindowsKeyboardHooks.DisableShortcuts = false;
            };
            // Registor private function.
            Helpers.WindowsKeyboardHooks.KeyUpHook += private_KeyUpHook;
            Helpers.WindowsKeyboardHooks.DisableShortcuts = true;
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
                        var keyName = $"{GetStringFromBtn(WriteSettingsBtn)} {BetterAW.Translation.ShortcutShortcuts}";
                        var settings = LoadSettngs(settingsPath);
                        // Get all menuContextItems, and add them to the keyboard shortcuts if their event handler is found.
                        foreach (var elem in writeMenu.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)) {
                            if (elem.MemberType == MemberTypes.Field) {
                                if (((FieldInfo)elem).FieldType.Name == "ContextMenuItem") {
                                    GetDataFromContextMenuItem((FieldInfo)elem, writeMenu, out var info, out var internalInfo);
                                    if (!(info is null)) {
                                        AvailableKeyboardShortcuts.Add(internalInfo);
                                        foreach (var setting in settings) {
                                            Terminal.Print($"setting.Name: {setting["Name"]}\n");
                                            Terminal.Print($"infoNonNull.Name: {info.Name}\n");
                                            if ((string)setting["Name"] == info.Name && !JSONKeybinding.FromJObject(setting).Removed) {
                                                internalInfo.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                                                info.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                                                RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false);
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
                var OnLanguageClicked = languageWindow_.GetType().GetMethod("OnLanguageClicked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                bool found = false;
                string curLang = AppWriterServicePatcher.GetCurrentLanguage();
                foreach (var lang in LanguageShortcut.EnabledLanguage) {
                    if (curLang == lang) {
                        found = true;
                    } else if (found) {
                        ((Window)languageWindow_).Dispatcher.Invoke(() => OnLanguageClicked.Invoke(languageWindow_, null));
                        method.Invoke(appWriterService_, new object[] { lang });
                        return false;
                    }
                }
                        ((Window)languageWindow_).Dispatcher.Invoke(() => OnLanguageClicked.Invoke(languageWindow_, null));
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
                var categoryName = BetterAW.Translation.ShortcutToggleLanguagesShortcut;
                KeyboardShortcutInfo firstInfo = new KeyboardShortcutInfo();
                KeyboardInternalShortcutInfo firstInternalInfo = new KeyboardInternalShortcutInfo();
                firstInfo.ShortcutText = Translation.ShortcutToggleLanguages;
                firstInfo.Name = "LangToggle";
                firstInternalInfo.Name = "LangToggle";
                firstInternalInfo.KeyboardEvent = ToggleLanguage;
                var settings = LoadSettngs(settingsPath);
                var langSettings = settings.Where(x => (string)x["Name"] == firstInfo.Name).ToArray();
                AvailableKeyboardShortcuts.Add(firstInternalInfo);
                if (langSettings.Length > 0 && !JSONKeybinding.FromJObject(langSettings[0]).Removed) {
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

        private static void AddShortcut(string Name,
            string ShortcutText,
            string categoryName,
            SortedSet<Keys> defaultKeyboardShortcut,
            KeyboardEventDelegate kbEvent,
            KeyboardInternalShortcutPreamble addPreamble = null,
            KeyboardInternalShortcutPostamble addPostamble = null,
            KeyboardInternalShortcutPreamble removePreamble = null,
            KeyboardInternalShortcutPostamble removePostamble = null) {

            // Create the info classes.
            KeyboardShortcutInfo info = new KeyboardShortcutInfo();
            KeyboardInternalShortcutInfo internalInfo = new KeyboardInternalShortcutInfo();
            info.Name = Name;
            internalInfo.Name = Name;
            info.ShortcutText = ShortcutText;
            internalInfo.shotcutAddPreamble = addPreamble;
            internalInfo.shotcutAddPostamble = addPostamble;
            internalInfo.shotcutRemovePreamble = removePreamble;
            internalInfo.shotcutRemovePostamble = removePostamble;
            internalInfo.KeyboardEvent = kbEvent;

            // Add to avalible shortcuts:
            AvailableKeyboardShortcuts.Add(internalInfo);

            // Load settings.
            var settings = LoadSettngs(settingsPath);

            bool settingFound = false;
            // If setting is found add it.
            foreach (var setting in settings) {
                if ((string)setting["Name"] == internalInfo.Name) {
                    settingFound = true;
                    if (JSONKeybinding.FromJObject(setting).Removed) {
                        if (!(removePostamble is null)) {
                            removePostamble(internalInfo);
                        }
                        break;
                    }
                    internalInfo.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                    info.keyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().keyBinding;
                    RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false);
                    break;
                }
            }
            // Else use default.
            if (!settingFound) {
                internalInfo.keyBinding = defaultKeyboardShortcut;
                info.keyBinding = defaultKeyboardShortcut;
                RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false);
            }
            KeyboardShortcuts.AddShortcut(categoryName, info);
        }


        private static void AddNumberedShortcuts() {
            string pwCategoryName = $"{Translation.PredictionWindow} {Translation.ShortcutShortcuts}";
            void internal_insert_shortcut(object sender, Events.KeyUpHookEventArgs e) {
                if (e.KeysPressed.Count <= 1 && e.KeysPressed.Contains(e.UpKey)) {
                    Thread t = new Thread(() => {
                        Helpers.PredictionWindowHelper.InsertSelectedPrediction((Window)predictionWindow_);
                        Helpers.WindowsKeyboardHooks.KeyUpHook -= internal_insert_shortcut;
                    }
                );
                    t.Start();
                }
                // Running this in a new thread somehow fixes a problem with using alt in the selection shortcut.

            }
            foreach (var number in Enumerable.Range(1, 10)) {
                var keyboardShortcut = new SortedSet<Keys>() { Keys.Menu, (Keys)((int)Keys.D0 + number % 10) };

                AddShortcut($"InsertPredictionIndex{number}",
                    string.Format(Translation.ShortcutPredictionInsertNumber, number),
                    pwCategoryName,
                    keyboardShortcut,
                    (shortcut) => {
                        try {
                            if (predictionWindow_ is null || ((Window)predictionWindow_).Visibility != Visibility.Visible) {
                                return false;
                            }
                            ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                                Helpers.PredictionWindowHelper.SelectPredictionIndex((Window)predictionWindow_, number - 1);
                            });
                            Helpers.WindowsKeyboardHooks.KeyUpHook += internal_insert_shortcut;
                            return true;


                        } catch (Exception ex) {
                            Terminal.Print($"{ex}\n");
                        }
                        return false;
                    },
                    null,
                    (self) => {
                        Helpers.PredictionWindowHelper.UpdateShortcutText((Window)predictionWindow_, number - 1, self.keyBinding);
                    },
                    null,
                    (self) => {
                        Helpers.PredictionWindowHelper.UpdateShortcutText((Window)predictionWindow_, number - 1, null);
                    });
            }
        }

        static PrioritiesedEvent<KeyDownHookEventArgs>.EventDelegate CancelInsertionEventHandler = null;

        private static void BuildInShortcuts() {
            var toolvarWindowType = toolbarWindow_.GetType();
            //MouseButtonEventArgs
            var ReadplayFunc = toolvarWindowType.GetMethod("PlayBtn_Click", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var ReadstopFUnc = toolvarWindowType.GetMethod("StopBtn_Click", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            string tbCategoryName = $"{Translation.Toolbar} {Translation.ShortcutShortcuts}";
            string pwCategoryName = $"{Translation.PredictionWindow} {Translation.ShortcutShortcuts}";

            var PlayBtn = (UIElement)toolvarWindowType.GetField("PlayBtn", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
            AddShortcut("Read",
                Helpers.Translations.GetString("Read"),
                tbCategoryName,
                new SortedSet<Keys>() { Keys.F8 },
                (shortcut) => {

                    try {
                        if (PlayBtn.Visibility == Visibility.Visible) {
                            ((Window)toolbarWindow_).Dispatcher.Invoke(new Action(() => ReadplayFunc.Invoke(toolbarWindow_, new object[] { null, new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds, MouseButton.Left) })));
                        } else {
                            ((Window)toolbarWindow_).Dispatcher.Invoke(new Action(() => ReadstopFUnc.Invoke(toolbarWindow_, new object[] { null, new MouseButtonEventArgs(Mouse.PrimaryDevice, (int)TimeSpan.FromTicks(DateTime.Now.Ticks).TotalSeconds, MouseButton.Left) })));
                        }
                        return true;
                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        return false;
                    }
                });

            AddShortcut("NextPrediction",
                Translation.ShortcutPredictionNavigateDown,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.ControlKey, Keys.Down },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        Helpers.PredictionWindowHelper.IncrementSelection((Window)predictionWindow_);
                        return true;
                    }
                    return false;
                });


            AddShortcut("PrevPrediction",
                Translation.ShortcutPredictionNavigateUp,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.ControlKey, Keys.Up },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        Helpers.PredictionWindowHelper.DecrementSelection((Window)predictionWindow_);
                        return true;
                    }
                    return false;
                });

            AddShortcut("NextPagePrediction",
                Translation.ShortcutPredictionNavigateRight,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.ControlKey, Keys.Right },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        Helpers.PredictionWindowHelper.TogglePredictioTypeOrIncrement((Window)predictionWindow_);
                        return true;
                    }
                    return false;

                });

            AddShortcut("PrevPagePrediction",
                Translation.ShortcutPredictionNavigateLeft,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.ControlKey, Keys.Left },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        Helpers.PredictionWindowHelper.TogglePredictioTypeOrDecrement((Window)predictionWindow_);
                        return true;
                    }
                    return false;

                });

            AddShortcut("HidePredWPrediction",
                Translation.ShortcutPredictionHideWindow,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.Escape },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                            ((Window)predictionWindow_).Visibility = Visibility.Collapsed;
                        });
                        return true;
                    }
                    return false;
                });

            AddShortcut("SelectCurrentPrediction",
                Translation.ShortcutPredictionInsertSelected,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.Enter },
                (shortcut) => {
                    if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                        return Helpers.PredictionWindowHelper.InsertSelectedPrediction((Window)predictionWindow_);
                    }
                    return false;
                });

            AddNumberedShortcuts();



            AddShortcut("CancelSelectedPrediction",
                Translation.ShortcutPredictionCancelInsertion,
                pwCategoryName,
                new SortedSet<Keys>() { Keys.Escape },
                (shortcut) => {
                    Terminal.Print("###############\nTHIS SHOULD NEVER BE HERE\n###############\n");
                    return false;
                }, (self) => {
                    if (!(CancelInsertionEventHandler is null)) {
                        Helpers.WindowsKeyboardHooks.KeyDownHook -= CancelInsertionEventHandler;
                    }
                    CancelInsertionEventHandler = (sender, e) => {
                        // Get NavigatingPredictions
                        if (predictionWindow_ is null) {
                            return;
                        }
                        var pwType = predictionWindow_.GetType();
                        var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow_);
                        var _serviceType = _service.GetType();

                        var NavigatingPredictions = (bool)_serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(_service);


                        if (NavigatingPredictions && self.keyBinding.IsSubsetOf(e.KeysPressed)) {
                            ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                                Helpers.PredictionWindowHelper.DeselectPrediction((Window)predictionWindow_);
                            });
                            e.Handled = true;
                        }
                    };
                    Helpers.WindowsKeyboardHooks.KeyDownHook += new PrioritiesedEvent<KeyDownHookEventArgs>.Event(CancelInsertionEventHandler, 1);
                    return false;
                },
                null, null,
                (self) => {
                    if (!(CancelInsertionEventHandler is null)) {
                        Helpers.WindowsKeyboardHooks.KeyDownHook -= CancelInsertionEventHandler;
                    }
                });
        }

        // Do the patching
        public void Patch() {
            try {
                // Use own shortcut system.
                Helpers.WindowsKeyboardHooks.ApplicationHook();
                BuildInShortcuts();
                FindShortcuts("_writeWindow", "WriteSettingsBtn");
                FindShortcuts("_readWindow", "ReadSettingsBtn");
                LanguageToggleShortcuts();
                KeyboardShortcuts.ShortcutStartAdd += UpHandler;
                KeyboardShortcuts.ShortcutRemove += (s, e) => {
                    KeyboardInternalShortcutInfo elem;
                    // Find special functions and run them if they exist. Else run function normally.
                    var eTemp = SpecialKeyboardShortcuts.Where(x => x.Name == e.ShortcutName);
                    if (eTemp.Any()) {
                        var temp = eTemp.First();
                        if (temp.shotcutRemovePreamble is null || temp.shotcutRemovePreamble(temp)) {
                            elem = RegisteredKeyboardShortcuts.Where(x => x.Name == e.ShortcutName).FirstOrDefault();
                            if (elem != default(KeyboardInternalShortcutInfo)) {
                                Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(elem.keyBinding);
                                Terminal.Print($"Remove: {RegisteredKeyboardShortcuts.Remove(elem)}\n");
                                if (e.SaveSettings) {
                                    SaveSettings(settingsPath);
                                }
                            }
                        }
                        if (!(temp.shotcutRemovePostamble is null)) {
                            temp.shotcutRemovePostamble(temp);
                        }
                        return;
                    }
                    // Remove the shortcut on this event.
                    elem = RegisteredKeyboardShortcuts.Where(x => x.Name == e.ShortcutName).FirstOrDefault();
                    if (elem != default(KeyboardInternalShortcutInfo)) {
                        Helpers.WindowsKeyboardHooks.RemoveKeyboardEvent(elem.keyBinding);
                        Terminal.Print($"Remove: {RegisteredKeyboardShortcuts.Remove(elem)}\n");
                        if (e.SaveSettings) {
                            SaveSettings(settingsPath);
                        }
                    }
                };
                Terminal.Print("AvailableKeyboardShortcuts\n");
                foreach (var shortcut in AvailableKeyboardShortcuts) {
                    Terminal.Print($"{shortcut.Name}\n");
                }
                Terminal.Print("RegisteredKeyboardShortcuts\n");
                foreach (var shortcut in RegisteredKeyboardShortcuts) {
                    Terminal.Print($"{shortcut.Name}\n");
                }
                //_keyboardHook
                //var _keyboardHook = appWriterServiceType_.GetField("_keyboardHook", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(appWriterService_);
                //var _keyboardHookType = _keyboardHook.GetType();
                //_keyboardHookType.GetMethod("Unhook", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(_keyboardHook, null);
                // Disable all build-in keyboard shortcuts.
                appWriterServiceType_.GetField("EventKeyDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(appWriterService_, null);
                appWriterServiceType_.GetField("EventKeyUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(appWriterService_, null);
                Helpers.PredictionWindowHelper.UpdateShortcutTextOnLoad();
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }


        public void PredictionPatch(object predictionWindow) {
            predictionWindow_ = predictionWindow;
        }
    }
}