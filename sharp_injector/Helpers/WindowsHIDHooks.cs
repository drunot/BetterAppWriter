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
using BetterAW.Helpers;

namespace sharp_injector.Helpers {

    public static class WindowsHIDHooks {
        public static event KeyUpHookEventHandler KeyUpHook;
        public static PrioritiesedEvent<KeyDownHookEventArgs> KeyDownHook = new PrioritiesedEvent<KeyDownHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseLButtonDownHook = new PrioritiesedEvent<MouseHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseLButtonUpHook = new PrioritiesedEvent<MouseHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseRButtonDownHook = new PrioritiesedEvent<MouseHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseRButtonUpHook = new PrioritiesedEvent<MouseHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseMButtonDownHook = new PrioritiesedEvent<MouseHookEventArgs>();
        public static PrioritiesedEvent<MouseHookEventArgs> MouseMButtonUpHook = new PrioritiesedEvent<MouseHookEventArgs>();
        private const int WH_KEYBOARD_LL = 13;
        private const int WH_MOUSE_LL = 14;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_SYSKEYDOWN = 0x0104;
        private const int WM_SYSKEYUP = 0x0105;
        private const ushort KEY_DOWN_MASK = 0x8000;
        private const int WM_LBUTTONDOWN = 0x0201;
        private const int WM_LBUTTONUP = 0x0202;
        private const int WM_RBUTTONDOWN = 0x0204;
        private const int WM_RBUTTONUP = 0x0205;
        private const int WM_MBUTTONDOWN = 0x0207;
        private const int WM_MBUTTONUP = 0x0208;
        private static LowLevelHIDProc _procKeyboard = HookCallbackKeyboard;
        private static LowLevelHIDProc _procMouse = HookCallbackMouse;
        private static IntPtr _hookIDKeyboard;
        private static IntPtr _hookIDMouse;
        private static Thread eventThread = new Thread(HandleActions);
        private static byte[] virtualKeys = new byte[256];
        private static SortedSet<Keys> pressedKeys = new SortedSet<Keys>();
        private static SortedSet<Keys> lastPressedKeys = new SortedSet<Keys>();
        private static void HandleActions(object data) {
            _hookIDKeyboard = SetHook(_procKeyboard, WH_KEYBOARD_LL);
            _hookIDMouse = SetHook(_procMouse, WH_MOUSE_LL);
            Application.Run();
            UnhookWindowsHookEx(_hookIDKeyboard);
            UnhookWindowsHookEx(_hookIDMouse);
        }

        public static bool DisableShortcuts { get; set; } = false;

        public static void ApplicationHook() {
            Terminal.Print($"Ran?\n");
            eventThread.Start();
        }

        private static IntPtr SetHook(LowLevelHIDProc proc, int hookType, uint dwThreadID = 0) {
            //using (Process curProcess = Process.GetCurrentProcess())
            //using (ProcessModule curModule = curProcess.MainModule) {
            return SetWindowsHookEx(hookType, proc,
                LoadLibrary("User32"), dwThreadID);
            //}
        }

        //private delegate IntPtr LowLevelHIDProc(
        //    int nCode, IntPtr wParam, IntPtr lParam);

        public delegate int LowLevelHIDProc(
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


        private static int HookCallbackKeyboard(int nCode, int wParam, ref KeyboardHookStruct lParam) {
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

                            // Run key down event.
                            var e = new KeyDownHookEventArgs((Keys)vkCode, pressedKeys);
                            KeyDownHook.Invoke(null, e);
                            if (e.Handled) {
                                return 1;
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
                            if (KeyUpHook != null) {
                                KeyUpHook(null, new KeyUpHookEventArgs((Keys)vkCode, lastPressedKeys));
                            }
                            lastPressedKeys = pressedKeys;
                        }
                        break;
                    default:
                        break;

                }

            }
            return CallNextHookEx(_hookIDKeyboard, nCode, wParam, ref lParam);
        }

        private static int HookCallbackMouse(int nCode, int wParam, ref KeyboardHookStruct lParam) {
            if (nCode >= 0) {
                switch ((int)wParam) {
                    case WM_LBUTTONDOWN: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Left, true);
                            MouseLButtonDownHook.Invoke(null, eventArgs);
                        }
                        break;
                    case WM_LBUTTONUP: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Left, false);
                            MouseLButtonUpHook.Invoke(null, eventArgs);
                        }
                        break;
                    case WM_RBUTTONDOWN: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Right, true);
                            MouseRButtonDownHook.Invoke(null, eventArgs);

                        }
                        break;
                    case WM_RBUTTONUP: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Right, false);
                            MouseRButtonUpHook.Invoke(null, eventArgs);

                        }
                        break;
                    case WM_MBUTTONDOWN: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Middle, true);
                            MouseMButtonDownHook.Invoke(null, eventArgs);

                        }
                        break;
                    case WM_MBUTTONUP: {
                            var eventArgs = new MouseHookEventArgs(MouseButtons.Middle, false);
                            MouseMButtonUpHook.Invoke(null, eventArgs);

                        }
                        break;
                    default:
                        break;

                }

            }
            return CallNextHookEx(_hookIDMouse, nCode, wParam, ref lParam);
        }

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr SetWindowsHookEx(int hookType, LowLevelHIDProc lpfn, IntPtr hMod, uint dwThreadId);

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
