using BetterAW;
using Newtonsoft.Json;
using sharp_injector.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Windows;
using sharp_injector.DTO;
using System.IO;

namespace sharp_injector.Patches {

    public class KeyboardShortcutsPatcher : IPatcher {
        private static readonly string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\BetterAppWriter\default.json";
        // Get the tooltip from the button (IDK why I made this to a function but o'well)
        private static string GetStringFromBtn(object button) {
            return (string)((PropertyInfo)button.GetType().GetMember("ToolTip", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(button, null);

        }

        static KeyboardShortcutInfo? GetDataFromContextMenuItem(FieldInfo MenuItemInfo, object context) {
            KeyboardShortcutInfo c = new KeyboardShortcutInfo();
            // Get the values of the MenuItem to propagate KeyboardShortcutInfo
            c.Name = MenuItemInfo.Name;
            object MenuItem = MenuItemInfo.GetValue(context);
            c.ShortcutText = (string)((PropertyInfo)MenuItem.GetType().GetMember("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);
            c.Icon = ((PropertyInfo)MenuItem.GetType().GetMember("MenuItemIcon", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(MenuItem, null);

            // Try and get the eventhandler function from the context. Return null if not found.
            var PreviewMouseLeftButtonDownInfo = context.GetType().GetMethod(MenuItemInfo.Name + "_PreviewMouseLeftButtonUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            if (PreviewMouseLeftButtonDownInfo != null)
            {
                c.ShortcutEvent = () => { PreviewMouseLeftButtonDownInfo.Invoke(context, new object[] { null, null }); };
            } else
            {
                Terminal.Print($"{MenuItemInfo.Name}: Was skipped.\n");
                return null;
            }
            return c;
        }

        // Holds current pressed keys.
        static HashSet<System.Windows.Forms.Keys> pressedKeys = new HashSet<System.Windows.Forms.Keys>();
        // Holds registered keyboard shortcuts.
        static HashSet<KeyboardShortcutInfo> RegisteredKeyboardShortcut = new HashSet<KeyboardShortcutInfo>();

        // Window references
        static object predictionWindow_;
        static object appWriterService_;
        static object toolbarWindow_;
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
                // Register this patch.
                PatchRegister.RegisterPatch(this);

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Add a keyboardshortcut to be handled.
        public static KeyboardShortcutInfo? RegisterKeyboardShortcut(KeyboardShortcutInfo keyboardShortcutInfo) {
            KeyboardShortcutInfo? ret = null;
            foreach (var shortcut in RegisteredKeyboardShortcut) {
                if (shortcut == keyboardShortcutInfo.keyBinding) {
                    ret = shortcut;
                }
            }
            if (RegisteredKeyboardShortcut.Contains(keyboardShortcutInfo)) {
                RegisteredKeyboardShortcut.Remove(keyboardShortcutInfo);
            }
            RegisteredKeyboardShortcut.Add(keyboardShortcutInfo);
            SaveSettings(settingsPath);
            return ret;
        }

        public static void SaveSettings(string path)
        {

            if (!File.Exists(Path.GetDirectoryName(path))) {
                Directory.CreateDirectory(Path.GetDirectoryName(path));
            }
            string toSave = JsonConvert.SerializeObject(RegisteredKeyboardShortcut.Select(x => JSONKeybinding.FromKeyboardShortcutInfo(x)));
            File.WriteAllText(path, toSave);
        }
        public static KeyboardShortcutInfo[] LoadSettngs(string path)
        {

            if (!File.Exists(path))
            {
                return null;
            }
            string data = File.ReadAllText(path);
            return JsonConvert.DeserializeObject<JSONKeybinding[]>(data).Select(x => x.ToKeyboardShortcutInfo()).ToArray();
        }

        /* Unregister a keyboard shortcut */
        public static bool UnregisterKeyboardShortcut(string Name) {
            return RegisteredKeyboardShortcut.Remove(new KeyboardShortcutInfo(Name));
        }

        public static bool UnregisterKeyboardShortcut(KeyboardShortcutInfo keyboardShortcutInfo)
        {
            return RegisteredKeyboardShortcut.Remove(keyboardShortcutInfo);
        }

        // Handle pressed keys.
        public static void DownHandler(object sender, System.Windows.Forms.KeyEventArgs e) {
            try
            {
                switch (e.KeyCode)
                {
                    case System.Windows.Forms.Keys.LControlKey:
                    case System.Windows.Forms.Keys.RControlKey:
                        pressedKeys.Add(System.Windows.Forms.Keys.ControlKey);
                        break;
                    case System.Windows.Forms.Keys.LMenu:
                    case System.Windows.Forms.Keys.RMenu:
                        pressedKeys.Add(System.Windows.Forms.Keys.Menu);
                        break;
                    case System.Windows.Forms.Keys.LShiftKey:
                    case System.Windows.Forms.Keys.RShiftKey:
                        pressedKeys.Add(System.Windows.Forms.Keys.ShiftKey);
                        break;
                    default:
                        pressedKeys.Add(e.KeyCode);
                        break;
                }
                foreach (var shortcut in RegisteredKeyboardShortcut)
                {
                    if (shortcut == pressedKeys)
                    {
                        Terminal.Print("Match\n");
                        shortcut.ShortcutEvent();
                    }
                }
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }

        }
        // Handle released keys.
        public static void UpHandler(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            onUpKey(pressedKeys);
            switch (e.KeyCode)
            {
                case System.Windows.Forms.Keys.LControlKey:
                case System.Windows.Forms.Keys.RControlKey:
                    pressedKeys.Remove(System.Windows.Forms.Keys.ControlKey);
                    break;
                case System.Windows.Forms.Keys.LMenu:
                case System.Windows.Forms.Keys.RMenu:
                    pressedKeys.Remove(System.Windows.Forms.Keys.Menu);
                    break;
                case System.Windows.Forms.Keys.LShiftKey:
                case System.Windows.Forms.Keys.RShiftKey:
                    pressedKeys.Remove(System.Windows.Forms.Keys.ShiftKey);
                    break;
                default:
                    pressedKeys.Remove(e.KeyCode);
                    break;
            }
            onUpKey = (var) => { };
        }
        private static KeyboardShortcuts.RekordingKey onUpKey = (var) => {};

        private static void FindShortcuts(string windowName, string buttonName)
        {
            try
            {
                // Get the writeWindow.
                object writeMenu = toolbarWindow_.GetType().GetField(windowName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                // Invoke a dispatcher for the window.
                ((Window)writeMenu).Dispatcher.Invoke(new Action(() => {
                    try
                    {
                        // Get the toolTip from the WriteSettingsBtn to use in the Title for the shortcuts.
                        object WriteSettingsBtn = toolbarWindow_.GetType().GetField(buttonName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(toolbarWindow_);
                        var keyName = $"{GetStringFromBtn(WriteSettingsBtn)} shortcuts";
                        var settings = LoadSettngs(settingsPath);
                        // Get all menuContextItems, and add them to the keyboard shortcuts if their event handler is found.
                        foreach (var elem in writeMenu.GetType().GetMembers(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static))
                        {
                            if (elem.MemberType == MemberTypes.Field)
                            {
                                if (((FieldInfo)elem).FieldType.Name == "ContextMenuItem")
                                {
                                    var info = GetDataFromContextMenuItem((FieldInfo)elem, writeMenu);
                                    if (info != null)
                                    {
                                        KeyboardShortcutInfo infoNonNull = ((KeyboardShortcutInfo)info);
                                        foreach (var setting in settings)
                                        {
                                            Terminal.Print($"setting.Name: {setting.Name}\n");
                                            Terminal.Print($"infoNonNull.Name: {infoNonNull.Name}\n");
                                            if (setting.Name == infoNonNull.Name)
                                            {
                                                infoNonNull.keyBinding = setting.keyBinding;
                                                RegisterKeyboardShortcut(infoNonNull);
                                            }
                                        }
                                        KeyboardShortcuts.AddShortcut(keyName, infoNonNull);
                                    }
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        // Do the patching
        public void Patch()
        {
            try
            {
                FindShortcuts("_writeWindow", "WriteSettingsBtn");
                FindShortcuts("_readWindow", "ReadSettingsBtn");
                KeyboardShortcuts.RekordingKeysDelegate = (onUpEvent) => { onUpKey = onUpEvent; };
                KeyboardShortcuts.RegisterShortcutDelegate = (getKeys) => {
                    try
                    {
                        if (getKeys.keyBinding == null)
                        {
                            UnregisterKeyboardShortcut(getKeys);
                            return null;
                        }
                        return RegisterKeyboardShortcut(getKeys);
                    }
                    catch (Exception ex)
                    {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                        return null;
                    }
                };
                // Get the events and their parameter type.
                var appWriterServiceKeyDownEventInfo = appWriterServiceType_.GetEvent("KeyDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var appWriterServiceKeyUpEventInfo = appWriterServiceType_.GetEvent("KeyUp", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                injectedKeyEventTypes = appWriterServiceKeyDownEventInfo.EventHandlerType.GetMethod("Invoke").GetParameters().Select(x => x.ParameterType).ToArray();
                // Create the dynamic versions of the event handlers.
                DynamicMethod handlerDown = ConvertToInjectedKeyboardEvent(typeof(KeyboardShortcutsPatcher).GetMethod(nameof(DownHandler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), "InjectedHandlerDown");
                DynamicMethod handlerUp = ConvertToInjectedKeyboardEvent(typeof(KeyboardShortcutsPatcher).GetMethod(nameof(UpHandler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), "InjectedHandlerUp");
                // Register the new events.
                appWriterServiceKeyDownEventInfo.AddEventHandler(appWriterService_, handlerDown.CreateDelegate(appWriterServiceKeyDownEventInfo.EventHandlerType));
                appWriterServiceKeyUpEventInfo.AddEventHandler(appWriterService_, handlerUp.CreateDelegate(appWriterServiceKeyUpEventInfo.EventHandlerType));

            }
            catch (Exception ex)
            {
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