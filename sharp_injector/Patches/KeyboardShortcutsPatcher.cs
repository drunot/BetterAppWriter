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
using System.Diagnostics.Tracing;
using BetterAW.Events;
using sharp_injector.Events;
using BetterAW.Helpers;
using System.Activities.Expressions;
using System.Activities.Statements;
using System.Windows.Documents;

// This is a mess and should proberbly be designed better in the future...

namespace sharp_injector.Patches {
    public class BaseInternalShortcutInfo {
        public string Name;
        public string ShortcutText;
        public object Icon;
    }
    public delegate bool KeyboardInternalShortcutPreamble(KeyboardInternalShortcutInfo self);
    public delegate void KeyboardInternalShortcutPostamble(KeyboardInternalShortcutInfo self);
    public class KeyboardInternalShortcutInfo : BaseInternalShortcutInfo, IComparable<KeyboardInternalShortcutInfo>, ICloneable {

        public delegate bool EventDelegate(KeyboardInternalShortcutInfo self);
        public delegate bool ConditionDelegate(KeyboardInternalShortcutInfo self, object sender, KeyDownHookEventArgs eventArgs);
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
        private EventDelegate _underLayingEvent;

        public ConditionDelegate _shortcutCondition = (self, sender, eventArgs) => eventArgs.KeysPressed.SetEquals(self.KeyBinding) && !eventArgs.Handled;
        public ConditionDelegate ShortcutCondition {
            get {
                return _shortcutCondition;
            }
            set {
                _shortcutCondition = value;
                KeyboardEvent = (sender, e) => {
                    if (ShortcutCondition(this, sender, e)) {
                        e.Handled = _underLayingEvent(this);
                    }
                };
            }
        }

        public EventDelegate UnderLayingEvent {
            set {
                _underLayingEvent = value;
                KeyboardEvent = (sender, e) => {
                    if (ShortcutCondition(this, sender, e)) {
                        e.Handled = _underLayingEvent(this);
                    }
                };
            }
            get {
                return _underLayingEvent;
            }
        }
        public PrioritiesedEvent<KeyDownHookEventArgs>.EventDelegate KeyboardEvent { get; private set; }
        private SortedSet<System.Windows.Forms.Keys> _keyBinding = new SortedSet<System.Windows.Forms.Keys>();
        public SortedSet<System.Windows.Forms.Keys> KeyBinding {
            get {
                return _keyBinding;
            }
            set {
                _keyBinding = value;
                KeyboardEvent = (sender, e) => {
                    if (ShortcutCondition(this, sender, e)) {
                        e.Handled = _underLayingEvent(this);
                    }
                };
            }
        }
        public static bool operator <(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (lhs.KeyBinding is null) {
                return !(rhs.KeyBinding is null);
            }
            var lhsA = lhs.KeyBinding.ToArray();
            var rhsA = rhs.KeyBinding.ToArray();
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
            if (lhs.KeyBinding is null) {
                return false;
            }
            var lhsA = lhs.KeyBinding.ToArray();
            var rhsA = rhs.KeyBinding.ToArray();
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
            if (lhs.KeyBinding is null) {
                return rhs.KeyBinding is null;
            }
            return lhs.KeyBinding.SetEquals(rhs.KeyBinding);
        }

        public static bool operator !=(KeyboardInternalShortcutInfo lhs, KeyboardInternalShortcutInfo rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (rhs is null) {
                return !(lhs is null);
            }
            if (lhs.KeyBinding is null) {
                return !(rhs.KeyBinding is null);
            }
            return !lhs.KeyBinding.SetEquals(rhs.KeyBinding);
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
            return KeyBinding.GetHashCode();
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

        static KeyboardInternalShortcutInfo GetDataFromContextMenuItem(FieldInfo MenuItemInfo, object context) {
            var kisi = new KeyboardInternalShortcutInfo();
            // Get the values of the MenuItem to propagate KeyboardShortcutInfo
            kisi.Name = MenuItemInfo.Name;
            object MenuItem = MenuItemInfo.GetValue(context);
            // Ignore hidden elements.
            if (((UIElement)MenuItem).Visibility != Visibility.Visible) {
                return null;
            }
            kisi.ShortcutText = (string)((PropertyInfo)MenuItem.GetType().GetMember("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);
            kisi.Icon = ((PropertyInfo)MenuItem.GetType().GetMember("MenuItemIcon", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);

            // Try and get the eventhandler function from the context. Return null if not found.
            var PreviewMouseLeftButtonDownInfo = context.GetType().GetMethod(MenuItemInfo.Name + "_PreviewMouseLeftButtonUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (PreviewMouseLeftButtonDownInfo != null) {
                kisi.UnderLayingEvent = (self) => {
                    PreviewMouseLeftButtonDownInfo.Invoke(context, new object[] { null, null });
                    return true;
                };
            } else {
                Terminal.Print($"{MenuItemInfo.Name}: Was skipped.\n");
                return null;
            }
            return kisi;
        }

        // Holds registered keyboard shortcuts.
        static SortedSet<KeyboardInternalShortcutInfo> RegisteredKeyboardShortcuts = new SortedSet<KeyboardInternalShortcutInfo>();
        static SortedSet<KeyboardInternalShortcutInfo> SpecialKeyboardShortcuts = new SortedSet<KeyboardInternalShortcutInfo>();
        static List<KeyboardInternalShortcutInfo> AvailableKeyboardShortcuts = new List<KeyboardInternalShortcutInfo>();

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
        public static void RegisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true, int priority = 10) {
            // If any of the special post amble functions exist. Add to list so it can be found later
            if (!(keyboardInternalShortcutInfo.shotcutRemovePreamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutRemovePostamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutAddPreamble is null) ||
                !(keyboardInternalShortcutInfo.shotcutAddPostamble is null)) {
                SpecialKeyboardShortcuts.Add(keyboardInternalShortcutInfo);
            }
            Terminal.Print("Test!\n");
            // If preamble is null or the preamble returns true. Add the shortcut normally.
            if (keyboardInternalShortcutInfo.shotcutAddPreamble is null || keyboardInternalShortcutInfo.shotcutAddPreamble(keyboardInternalShortcutInfo)) {
                Terminal.Print("shotcutAddPreamble!\n");
                // Check if other shortcuts uses same keybinding.
                // Could probably be done with RegisteredKeyboardShortcuts.Contains.
                var shortcut = RegisteredKeyboardShortcuts.FirstOrDefault((s) => s.KeyBinding.SetEquals(keyboardInternalShortcutInfo.KeyBinding));
                if(shortcut != default(KeyboardInternalShortcutInfo)) {
                    Terminal.Print("Other removed!\n");
                    RemoveShortcut(shortcut.Name, false);
                }
                // If this function already have a shortcut. Remove it.
                shortcut = RegisteredKeyboardShortcuts.FirstOrDefault(x => x.Name == keyboardInternalShortcutInfo.Name);
                if (shortcut != default(KeyboardInternalShortcutInfo)) {
                    Terminal.Print("Old removed!\n");
                    RemoveShortcut(shortcut.Name, false);
                }
                Terminal.Print("Reregister!\n");
                // Registor the new shortcut.
                RegisteredKeyboardShortcuts.Add(keyboardInternalShortcutInfo);
                WindowsHIDHooks.KeyDownHook += new PrioritiesedEvent<KeyDownHookEventArgs>.Event(keyboardInternalShortcutInfo.KeyboardEvent, priority);
            }
            if (!(keyboardInternalShortcutInfo.shotcutAddPostamble is null)) {
                keyboardInternalShortcutInfo.shotcutAddPostamble(keyboardInternalShortcutInfo);
            }
            KeyboardShortcuts.AddKeybordShortcut(keyboardInternalShortcutInfo.Name, keyboardInternalShortcutInfo.KeyBinding);

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
            KeyboardShortcuts.RemoveKeybordShortcut(Name);
            // TODO: Do all the other stuff as whell
        }

        public static void UnregisterKeyboardShortcut(KeyboardInternalShortcutInfo keyboardInternalShortcutInfo, bool saveAfter = true) {
            UnregisterKeyboardShortcut(keyboardInternalShortcutInfo.Name, saveAfter);
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
                                    var internalInfo = GetDataFromContextMenuItem((FieldInfo)elem, writeMenu);
                                    if (!(internalInfo is null)) {
                                        AvailableKeyboardShortcuts.Add(internalInfo);
                                        foreach (var setting in settings) {
                                            Terminal.Print($"setting.Name: {setting["Name"]}\n");
                                            Terminal.Print($"infoNonNull.Name: {internalInfo.Name}\n");
                                            if ((string)setting["Name"] == internalInfo.Name && !JSONKeybinding.FromJObject(setting).Removed) {
                                                internalInfo.KeyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().KeyBinding;
                                                RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false);
                                            }
                                        }
                                        KeyboardShortcuts.AddShortcut(keyName, internalInfo.Name, internalInfo.ShortcutText, internalInfo.KeyBinding);
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
        public static bool ToggleLanguage() {
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
                KeyboardInternalShortcutInfo firstInternalInfo = new KeyboardInternalShortcutInfo();
                firstInternalInfo.ShortcutText = Translation.ShortcutToggleLanguages;
                firstInternalInfo.Name = "LangToggle";
                firstInternalInfo.UnderLayingEvent = (self) => ToggleLanguage();
                var settings = LoadSettngs(settingsPath);
                var langSettings = settings.Where(x => (string)x["Name"] == firstInternalInfo.Name).ToArray();
                AvailableKeyboardShortcuts.Add(firstInternalInfo);
                if (langSettings.Length > 0 && !JSONKeybinding.FromJObject(langSettings[0]).Removed) {
                    firstInternalInfo.KeyBinding = JSONKeybinding.FromJObject(langSettings[0]).ToKeyboardShortcutInfo().KeyBinding;
                    RegisterKeyboardShortcut(firstInternalInfo, false);
                }
                KeyboardShortcuts.AddShortcut(categoryName, firstInternalInfo.Name, firstInternalInfo.ShortcutText, firstInternalInfo.KeyBinding);
                var langauges = new SortedDictionary<string, string>(AppWriterServicePatcher.GetAvalibleLanguage().ToDictionary(x => Translations.GetString(x.Replace("-", "_")), x => x));
                foreach (var langauge in langauges) {
                    object icon = null;
                    if (icons.ContainsKey(langauge.Value)) {
                        icon = icons[langauge.Value];
                    }
                    bool selected = false;
                    foreach (var setting in settings) {
                        if ((string)setting["Name"] == langauge.Value) {
                            selected = (bool)setting["Selected"];
                            LanguageShortcut.EnabledLanguage.Add(langauge.Value);
                            break;
                        }
                    }
                    KeyboardShortcuts.AddBoolen(categoryName, langauge.Value, langauge.Key, selected, icon);
                }
                // Make Language shortcut changes save the json file:
                LanguageShortcut.ChnagedChekedEvent = (checkedChnaged) => SaveSettings(settingsPath);
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        private static void AddShortcut(string categoryName, KeyboardInternalShortcutInfo internalInfo, SortedSet<Keys> defaultKeyboardShortcut = null, int priority = 10) {

            // Load settings.
            var settings = LoadSettngs(settingsPath);

            bool settingFound = false;
            // If setting is found add it.
            foreach (var setting in settings) {
                if ((string)setting["Name"] == internalInfo.Name) {
                    settingFound = true;
                    if (JSONKeybinding.FromJObject(setting).Removed) {
                        if (!(internalInfo.shotcutRemovePostamble is null)) {
                            internalInfo.shotcutRemovePostamble(internalInfo);
                        }
                        break;
                    }
                    internalInfo.KeyBinding = JSONKeybinding.FromJObject(setting).ToKeyboardShortcutInfo().KeyBinding;
                    RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false, priority);
                    break;
                }
            }
            // Else use default.
            if (!settingFound) {
                internalInfo.KeyBinding = defaultKeyboardShortcut;
                RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)internalInfo.Clone(), false, priority);
            }
            KeyboardShortcuts.AddShortcut(categoryName, internalInfo.Name, internalInfo.ShortcutText, internalInfo.KeyBinding);
            // Add to avalible shortcuts:
            AvailableKeyboardShortcuts.Add(internalInfo);
        }

        private static void RemoveShortcut(string name, bool saveSettubgs = true) {
            var elem = SpecialKeyboardShortcuts.FirstOrDefault(x => x.Name == name);
            if(elem == default(KeyboardInternalShortcutInfo)) {
                elem = RegisteredKeyboardShortcuts.FirstOrDefault(x => x.Name == name);
            }
            
            if (elem != default(KeyboardInternalShortcutInfo)) {
                if (elem.shotcutRemovePreamble is null || elem.shotcutRemovePreamble(elem)) {
                    if (elem != default(KeyboardInternalShortcutInfo)) {
                        Helpers.WindowsHIDHooks.KeyDownHook -= elem.KeyboardEvent;
                        if (saveSettubgs) {
                            SaveSettings(settingsPath);
                        }
                    }
                }
                if (!(elem.shotcutRemovePostamble is null)) {
                    elem.shotcutRemovePostamble(elem);
                }
                RegisteredKeyboardShortcuts.Remove(elem);
                KeyboardShortcuts.RemoveKeybordShortcut(elem.Name);
            } 
        }


        private static void AddNumberedShortcuts() {
            string pwCategoryName = $"{Translation.PredictionWindow} {Translation.ShortcutShortcuts}";
            void internal_insert_shortcut(object sender, Events.KeyUpHookEventArgs e) {
                if (e.KeysPressed.Count() <= 1 && e.KeysPressed.SetEquals(new SortedSet<Keys>() { e.UpKey })) {
                    // Running this in a new thread somehow fixes a problem with using alt in the selection shortcut.
                    Thread t = new Thread(() => {
                        Helpers.PredictionWindowHelper.InsertSelectedPrediction((Window)predictionWindow_);
                        Helpers.WindowsHIDHooks.KeyUpHook -= internal_insert_shortcut;
                    }
                    );
                    t.Start();
                }
            }
            foreach (var number in Enumerable.Range(1, 10)) {
                var keyboardShortcut = new SortedSet<Keys>() { Keys.Menu, (Keys)((int)Keys.D0 + number % 10) };

                KeyboardInternalShortcutInfo info = new KeyboardInternalShortcutInfo($"InsertPredictionIndex{number}");
                info.ShortcutText = string.Format(Translation.ShortcutPredictionInsertNumber, number);
                info.UnderLayingEvent = (self) => {
                    try {
                        if (predictionWindow_ is null || ((Window)predictionWindow_).Visibility != Visibility.Visible) {
                            return false;
                        }
                            ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                                Helpers.PredictionWindowHelper.SelectPredictionIndex((Window)predictionWindow_, number - 1);
                            });
                        Helpers.WindowsHIDHooks.KeyUpHook += internal_insert_shortcut;
                    } catch (Exception ex) {
                        Terminal.Print($"{ex}\n");
                        return false;
                    }
                    return true;
                };
                info.shotcutAddPostamble = (self) => {
                    Helpers.PredictionWindowHelper.UpdateShortcutText((Window)predictionWindow_, number - 1, self.KeyBinding);
                };
                info.shotcutRemovePostamble = (self) => {
                    Helpers.PredictionWindowHelper.UpdateShortcutText((Window)predictionWindow_, number - 1, null);
                };

                AddShortcut(pwCategoryName, info, keyboardShortcut);
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

            // Add read shortcut.
            KeyboardInternalShortcutInfo readInfo = new KeyboardInternalShortcutInfo("Read");
            readInfo.ShortcutText = Helpers.Translations.GetString("Read");
            readInfo.UnderLayingEvent = (self) => {
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
            };
            AddShortcut(tbCategoryName, readInfo, new SortedSet<Keys>() { Keys.F8 });

            // Add next prediction shortcut.
            KeyboardInternalShortcutInfo nextPredictionInfo = new KeyboardInternalShortcutInfo("NextPrediction");
            nextPredictionInfo.ShortcutText = Translation.ShortcutPredictionNavigateDown;
            nextPredictionInfo.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    Helpers.PredictionWindowHelper.IncrementSelection((Window)predictionWindow_);
                    return true;
                }
                return false;
            };
            AddShortcut(pwCategoryName, nextPredictionInfo, new SortedSet<Keys>() { Keys.ControlKey, Keys.Down });

            // Add previus prediction shortcut.
            KeyboardInternalShortcutInfo prevPredictionInfo = new KeyboardInternalShortcutInfo("PrevPrediction");
            prevPredictionInfo.ShortcutText = Translation.ShortcutPredictionNavigateUp;
            prevPredictionInfo.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    Helpers.PredictionWindowHelper.DecrementSelection((Window)predictionWindow_);
                    return true;
                }
                return false;
            };
            AddShortcut(pwCategoryName, prevPredictionInfo, new SortedSet<Keys>() { Keys.ControlKey, Keys.Up });

            // Add next page prediction shortcut.
            KeyboardInternalShortcutInfo nextPagePredictionInfo = new KeyboardInternalShortcutInfo("NextPagePrediction");
            nextPagePredictionInfo.ShortcutText = Translation.ShortcutPredictionNavigateRight;
            nextPagePredictionInfo.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    Helpers.PredictionWindowHelper.TogglePredictioTypeOrIncrement((Window)predictionWindow_);
                    return true;
                }
                return false;
            };
            AddShortcut(pwCategoryName, nextPagePredictionInfo, new SortedSet<Keys>() { Keys.ControlKey, Keys.Right });

            // Add previus page prediction shortcut.
            KeyboardInternalShortcutInfo prevPagePredictionInfo = new KeyboardInternalShortcutInfo("PrevPagePrediction");
            prevPagePredictionInfo.ShortcutText = Translation.ShortcutPredictionNavigateLeft;
            prevPagePredictionInfo.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    Helpers.PredictionWindowHelper.TogglePredictioTypeOrDecrement((Window)predictionWindow_);
                    return true;
                }
                return false;

            };
            AddShortcut(pwCategoryName, prevPagePredictionInfo, new SortedSet<Keys>() { Keys.ControlKey, Keys.Left });

            // Add hide prediction window shortcut.
            KeyboardInternalShortcutInfo hidePredWPrediction = new KeyboardInternalShortcutInfo("HidePredWPrediction");
            hidePredWPrediction.ShortcutText = Translation.ShortcutPredictionHideWindow;
            hidePredWPrediction.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                        ((Window)predictionWindow_).Visibility = Visibility.Collapsed;
                    });
                    return true;
                }
                return false;
            };
            AddShortcut(pwCategoryName, hidePredWPrediction, new SortedSet<Keys>() { Keys.Escape });

            // Add select current prediction shortcut.
            KeyboardInternalShortcutInfo selectCurrentPredictionInfo = new KeyboardInternalShortcutInfo("SelectCurrentPrediction");
            selectCurrentPredictionInfo.ShortcutText = Translation.ShortcutPredictionInsertSelected;
            selectCurrentPredictionInfo.UnderLayingEvent = (self) => {
                if (!(predictionWindow_ is null) && ((Window)predictionWindow_).Visibility == Visibility.Visible) {
                    return Helpers.PredictionWindowHelper.InsertSelectedPrediction((Window)predictionWindow_);
                }
                return false;
            };
            AddShortcut(pwCategoryName, selectCurrentPredictionInfo, new SortedSet<Keys>() { Keys.Enter });

            AddNumberedShortcuts();

            // Add cancel selected prediction shortcut.
            KeyboardInternalShortcutInfo cancelSelectedPredictionInfo = new KeyboardInternalShortcutInfo("CancelSelectedPrediction");
            cancelSelectedPredictionInfo.ShortcutText = Translation.ShortcutPredictionCancelInsertion;
            cancelSelectedPredictionInfo.UnderLayingEvent = (self) => {
                // Get NavigatingPredictions
                if (predictionWindow_ is null) {
                    return false;
                }
                var pwType = predictionWindow_.GetType();
                var _service = pwType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(predictionWindow_);
                var _serviceType = _service.GetType();

                var NavigatingPredictions = (bool)_serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(_service);


                if (NavigatingPredictions) {
                    ((Window)predictionWindow_).Dispatcher.Invoke(() => {
                        Helpers.PredictionWindowHelper.DeselectPrediction((Window)predictionWindow_);
                    });
                    return true;
                }
                return false;
            };
            cancelSelectedPredictionInfo.ShortcutCondition = (self, sender, eventArgs) => self.KeyBinding.Contains(eventArgs.DownKey) && !eventArgs.Handled;
            cancelSelectedPredictionInfo.shotcutAddPreamble = (self) => {
                if(cancelSelectedPredictionInfo.KeyBinding != null) {
                    // If this function already have a shortcut. Remove it.
                    var shortcut = RegisteredKeyboardShortcuts.FirstOrDefault(x => x.Name == self.Name);
                    if (shortcut != default(KeyboardInternalShortcutInfo)) {
                        Terminal.Print("Old removed!\n");
                        RemoveShortcut(shortcut.Name, false);
                    }
                    Terminal.Print("Reregister!\n");
                    // Registor the new shortcut.
                    RegisteredKeyboardShortcuts.Add(self);
                    WindowsHIDHooks.KeyDownHook += new PrioritiesedEvent<KeyDownHookEventArgs>.Event(self.KeyboardEvent, 1);

                }
                return false;
            };
            
            //cancelSelectedPredictionInfo.shotcutRemovePostamble = (self) => {
            //    if (!(CancelInsertionEventHandler is null)) {
            //        Helpers.WindowsKeyboardHooks.KeyDownHook -= cancelSelectedPredictionInfo.KeyboardEvent;
            //    }
            //};
            AddShortcut(pwCategoryName, cancelSelectedPredictionInfo, new SortedSet<Keys>() { Keys.Escape });
        }

        private void AddShortcutHandler(object sender, ShortcutAddEventArgs eventArgs) {
            void private_KeyUpHook(object s, Events.KeyUpHookEventArgs e) {
                try {
                    Terminal.Print("Add Happened?\n");
                    var sc = e.KeysPressed;
                    sc.Add(e.UpKey);
                    // On key up registor keyboard shortcut.
                    KeyboardInternalShortcutInfo elem = (KeyboardInternalShortcutInfo)AvailableKeyboardShortcuts.First(x => x.Name == eventArgs.ShortcutName).Clone();
                    elem.KeyBinding = e.KeysPressed;
                    RegisterKeyboardShortcut((KeyboardInternalShortcutInfo)elem.Clone());

                } catch (Exception ex) {
                    Terminal.Print(string.Format("{0}\n", ex.ToString()));
                }

                // Unregistor event until next time.
                Helpers.WindowsHIDHooks.KeyUpHook -= private_KeyUpHook;
                Helpers.WindowsHIDHooks.DisableShortcuts = false;
            };

            WindowsHIDHooks.KeyUpHook += private_KeyUpHook;
            WindowsHIDHooks.DisableShortcuts = true;
        }

        private void RemoveShortcutHandler(object sender, ShortcutRemoveEventArgs eventArgs) {
            Terminal.Print("Remove Happened?\n");
            RemoveShortcut(eventArgs.ShortcutName, true);
        }

        // Do the patching
        public void Patch() {


            try {
                // Use own shortcut system.
                Helpers.WindowsHIDHooks.ApplicationHook();
                BuildInShortcuts();
                FindShortcuts("_writeWindow", "WriteSettingsBtn");
                FindShortcuts("_readWindow", "ReadSettingsBtn");
                LanguageToggleShortcuts();
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
                KeyboardShortcuts.addEvent += AddShortcutHandler;
                KeyboardShortcuts.removeEvent += RemoveShortcutHandler;
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }


        public void PredictionPatch(object predictionWindow) {
            predictionWindow_ = predictionWindow;
        }
    }
}