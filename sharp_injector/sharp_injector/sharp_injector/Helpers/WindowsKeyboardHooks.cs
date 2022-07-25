using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using BetterAW;
using System.Threading;
using sharp_injector.Events;
using System.IO;

namespace sharp_injector.Helpers {
    public delegate bool KeyboardEventDelegate(SortedSet<Keys> combination);
    public class WindowsKeyboardEvent : IComparable<WindowsKeyboardEvent> {


        public bool IgnoreInput { get; set; } = false;

        public KeyboardEventDelegate KeyboardEvent { get; set; }
        public SortedSet<Keys> KeyboardShortcut { get; set; }
        public WindowsKeyboardEvent() { }
        public WindowsKeyboardEvent(SortedSet<Keys> keyboardShortcut, KeyboardEventDelegate keyboardEvent) {
            KeyboardShortcut = keyboardShortcut;
            KeyboardEvent = keyboardEvent;
        }


        public static bool operator <(WindowsKeyboardEvent lhs, WindowsKeyboardEvent rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (lhs.KeyboardShortcut is null) {
                return !(rhs.KeyboardShortcut is null);
            }
            var lhsA = lhs.KeyboardShortcut.ToArray();
            var rhsA = rhs.KeyboardShortcut.ToArray();
            for (int i = 0; i < lhsA.Length; i++) {
                if (lhsA[i] < rhsA[i]) {
                    return true;
                } else if (lhsA[i] > rhsA[i]) {
                    return false;
                }
            }
            return false;
        }
        public static bool operator >(WindowsKeyboardEvent lhs, WindowsKeyboardEvent rhs) {
            if (lhs is null) {
                return false;
            }
            if (lhs.KeyboardShortcut is null) {
                return false;
            }
            var lhsA = lhs.KeyboardShortcut.ToArray();
            var rhsA = rhs.KeyboardShortcut.ToArray();
            for (int i = 0; i < lhsA.Length; i++) {
                if (lhsA[i] > rhsA[i]) {
                    return true;
                } else if (lhsA[i] < rhsA[i]) {
                    return false;
                }
            }
            return false;
        }
        public static bool operator ==(WindowsKeyboardEvent lhs, WindowsKeyboardEvent rhs) {
            if (lhs is null) {
                return rhs is null;
            }
            if (rhs is null) {
                return lhs is null;
            }
            if (lhs.KeyboardShortcut is null) {
                return rhs.KeyboardShortcut is null;
            }
            return lhs.KeyboardShortcut.SetEquals(rhs.KeyboardShortcut);
        }

        public static bool operator !=(WindowsKeyboardEvent lhs, WindowsKeyboardEvent rhs) {
            if (lhs is null) {
                return !(rhs is null);
            }
            if (rhs is null) {
                return !(lhs is null);
            }
            if (lhs.KeyboardShortcut is null) {
                return !(rhs.KeyboardShortcut is null);
            }
            return !lhs.KeyboardShortcut.SetEquals(rhs.KeyboardShortcut);
        }

        public static bool operator ==(WindowsKeyboardEvent lhs, SortedSet<Keys> rhs) {
            if (lhs == null) {
                return false;
            }
            if (lhs.KeyboardShortcut == null) {
                return rhs == null;
            }
            return lhs.KeyboardShortcut.SetEquals(rhs);
        }
        public static bool operator !=(WindowsKeyboardEvent lhs, SortedSet<Keys> rhs) {
            if (lhs == null) {
                return true;
            }
            if (lhs.KeyboardShortcut == null) {
                return rhs != null;
            }
            return !lhs.KeyboardShortcut.SetEquals(rhs);
        }
        public static implicit operator WindowsKeyboardEvent(SortedSet<Keys> keys) {
            return new WindowsKeyboardEvent(keys, null);
        }

        public override bool Equals(object obj) {

            // If obj is WindowsKeyboardEvent then use normal equal
            if (obj is WindowsKeyboardEvent) {
                return (obj as WindowsKeyboardEvent) == this;
            }

            // If obj is WindowsKeyboardEvent then use SortedSet<Keys> equal
            if (obj is SortedSet<Keys>) {
                return (obj as SortedSet<Keys>) == this;
            }

            // Else only return true if both is null
            return (obj == null && this == null);
        }
        public override int GetHashCode() {
            return KeyboardShortcut.GetHashCode();
        }
        public int CompareTo(WindowsKeyboardEvent y) {
            if (this == y) {
                return 0;
            }
            if (this < y) {
                return -1;
            }
            return 1;
        }
    }
    public static class WindowsKeyboardHooks {
        public static event KeyUpHookEventHandler KeyUpHook;
        public static event KeyDownHookEventHandler KeyDownHook;
        public static event AllKeyUpHookEventHandler AllKeyUpHook;
        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const ushort KEY_DOWN_MASK = 0x8000;
        private static LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;
        private static Thread eventThread = new Thread(HandleActions);
        private static byte[] virtualKeys = new byte[256];
        private static SortedSet<Keys> pressedKeys = new SortedSet<Keys>();
        private static SortedSet<Keys> lastPressedKeys = new SortedSet<Keys>();
        private static SortedSet<WindowsKeyboardEvent> windowsKeyboardEvents = new SortedSet<WindowsKeyboardEvent>();
        private static void HandleActions(object data) {
            _hookID = SetHook(_proc);
            Application.Run();
            UnhookWindowsHookEx(_hookID);
        }

        public static bool DisableShortcuts { get; set; } = false;

        public static void AddKeyboardEvent(WindowsKeyboardEvent keyboardEvent) {

            // Reset the event if it exists.
            windowsKeyboardEvents.Remove(keyboardEvent);
            windowsKeyboardEvents.Add(keyboardEvent);
        }
        public static void RemoveKeyboardEvent(WindowsKeyboardEvent keyboardEvent) {

            // Reset the event if it exists.
            windowsKeyboardEvents.Remove(keyboardEvent);
        }

        public static bool RemoveKeyboardEvent(SortedSet<Keys> keyboardEvent) {

            // Reset the event if it exists.
            return windowsKeyboardEvents.Remove(keyboardEvent);
        }

        public static void ApplicationHook() {
            Terminal.Print($"Ran?\n");
            eventThread.Start();
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc, uint dwThreadID = 0) {
            //using (Process curProcess = Process.GetCurrentProcess())
            //using (ProcessModule curModule = curProcess.MainModule) {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    LoadLibrary("User32"), dwThreadID);
            //}
        }

        //private delegate IntPtr LowLevelKeyboardProc(
        //    int nCode, IntPtr wParam, IntPtr lParam);

        public delegate int LowLevelKeyboardProc(
          int code,
          int wParam,
          ref KeyboardHookStruct lParam);

        public struct KeyboardHookStruct {
            public int vkCode;
            public int scanCode;
            public int flags;
            public int time;
            public int dwExtraInfo;
        }


        private static int HookCallback( int nCode, int wParam, ref KeyboardHookStruct lParam) {
            if (nCode >= 0) {
                switch ((int)wParam) {
                    case WM_SYSKEYDOWN:
                    case WM_KEYDOWN: {
                            int vkCode = lParam.vkCode;
                            // Ignore Right and left identifiers.
                            switch ((Keys)vkCode) {
                                case Keys.LControlKey:
                                case Keys.RControlKey:
                                    vkCode = (int)Keys.ControlKey;
                                    break;
                                case Keys.LMenu:
                                case Keys.RMenu:
                                    vkCode = (int)Keys.Menu;
                                    break;
                                case Keys.LShiftKey:
                                case Keys.RShiftKey:
                                    vkCode = (int)Keys.ShiftKey;
                                    break;
                            }
                            // Cleanup keyboard presses. If they where lifted since last time but the event was canceled before getting here
                            // the key will now be registored as lifted.
                            pressedKeys = new SortedSet<Keys>(pressedKeys.Where(x => (GetAsyncKeyState((int)x) & KEY_DOWN_MASK) != 0));
                            // Since this hook fires before GetAsyncKeyState gets the event manually add this key.
                            pressedKeys.Add((Keys)vkCode);
                            if (!(KeyDownHook is null)) {

                                var e = new KeyDownHookEventArgs((Keys)vkCode, pressedKeys);
                                KeyDownHook(null, e);
                                if (e.Handled) {
                                    return 1;
                                }
                            }

                            // Check if the keyboard press was different from the last time.
                            if (!pressedKeys.SetEquals(lastPressedKeys)) {
                                // If shurtcuts is not disabled.
                                if (!DisableShortcuts) {
                                    // If an event is found run the handler.
                                    // (TryGetValue(pressedKeys, out var keyboardEvent) for some reason crashes the app.)
                                    var result = windowsKeyboardEvents.Where(x => x.KeyboardShortcut.SetEquals(pressedKeys));
                                    if (result.Any()) {
                                        if (result.First().KeyboardEvent(pressedKeys)) {
                                            lastPressedKeys = pressedKeys;
                                            return 1;
                                        }
                                    }
                                }
                                
                            }
                            lastPressedKeys = pressedKeys;
                        }
                        break;
                    case WM_SYSKEYUP:
                    case WM_KEYUP: {
                            int vkCode = lParam.vkCode;
                            // Ignore Right and left identifiers.
                            switch ((Keys)vkCode) {
                                case Keys.LControlKey:
                                case Keys.RControlKey:
                                    vkCode = (int)Keys.ControlKey;
                                    break;
                                case Keys.LMenu:
                                case Keys.RMenu:
                                    vkCode = (int)Keys.Menu;
                                    break;
                                case Keys.LShiftKey:
                                case Keys.RShiftKey:
                                    vkCode = (int)Keys.ShiftKey;
                                    break;
                            }

                            // Update PressedKeys.
                            pressedKeys = new SortedSet<Keys>(pressedKeys.Where(x => (GetAsyncKeyState((int)x) & KEY_DOWN_MASK) != 0));
                            pressedKeys.Remove((Keys)vkCode);

                            // If key up event is not null, invoke it.
                            if(KeyUpHook != null) {
                                KeyUpHook(null, new KeyUpHookEventArgs((Keys)vkCode, lastPressedKeys));
                            }
                            if(pressedKeys.Count <= 0) {
                                if(!(AllKeyUpHook is null)) {
                                    AllKeyUpHook(null, new AllKeyUpHookEventArgs((Keys)vkCode));
                                }
                            }
                            lastPressedKeys = pressedKeys;
                        }
                        break;
                    default:
                        break;
                    
                }

            }
            return CallNextHookEx(_hookID, nCode, wParam, ref lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hookType, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll")]
        static extern int CallNextHookEx(IntPtr hhk, int nCode, int wParam, ref KeyboardHookStruct lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);

        [DllImport("user32.dll")]
        public static extern bool GetKeyboardState(byte[] lpKeyState);

        [DllImport("user32.dll")]
        public static extern ushort GetAsyncKeyState(int lpKeyState);


        [DllImport("kernel32.dll")]
        private static extern IntPtr LoadLibrary(string lpFileName);
    }

    
}
