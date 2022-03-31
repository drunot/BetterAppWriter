using System;
using System.Diagnostics;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Windows.Threading;
using System.Threading;

namespace BetterAW
{
    public static class InterceptKeys
    {
        //static Dispatcher keyDispacher = null;
        //static Thread keyThread = null;
        static HashSet<Keys> pressedKeys = new HashSet<Keys>();
        static HashSet<Keys> Dummy = new HashSet<Keys>() { Keys.LMenu, Keys.A};

        private const int WH_KEYBOARD_LL = 13;
        private const int WM_KEYDOWN = 0x0100;
        private const int WM_KEYUP = 0x0101;
        private const int WM_HOTKEY = 0x0312;
        private static readonly LowLevelKeyboardProc _proc = HookCallback;
        private static IntPtr _hookID = IntPtr.Zero;

        public static void Hook()
        {
            try {
                //if (keyThread == null)
                //{
                //    ManualResetEvent dispatcherReadyEvent = new ManualResetEvent(false);
                //    new Thread(new ThreadStart(() =>
                //    {
                //        keyDispacher = Dispatcher.CurrentDispatcher;
                //        dispatcherReadyEvent.Set();
                //        Dispatcher.Run();
                //    })).Start();

                //    dispatcherReadyEvent.WaitOne();
                //}
                _hookID = SetHook(_proc);
                Terminal.Print("Hooked\n");
                
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public static void Unhook()
        {
            try { 
                UnhookWindowsHookEx(_hookID);
                Terminal.Print("Unhooked\n");
            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        private static IntPtr SetHook(LowLevelKeyboardProc proc)
        {
            using (Process curProcess = Process.GetCurrentProcess())
            using (ProcessModule curModule = curProcess.MainModule)
            {
                return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
                    GetModuleHandle(curModule.ModuleName), 0);
            }
        }

        private delegate IntPtr LowLevelKeyboardProc(
            int nCode, IntPtr wParam, IntPtr lParam);

        private static IntPtr HookCallback(
            int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                if (!pressedKeys.Contains((Keys)vkCode))
                {
                    Terminal.Print($"WM_KEYDOWN {(Keys)vkCode}\n");
                    pressedKeys.Add((Keys)vkCode);
                    if (Dummy.Count == pressedKeys.Count)
                    {
                        bool matched = true;
                        foreach (var key in pressedKeys)
                        {
                            if (!Dummy.Contains(key))
                            {
                                matched = false;
                                break;
                            }
                        }
                        if (matched)
                        {
                            Terminal.Print("Command matched!\n");
                            pressedKeys.Add((Keys)vkCode);
                            return (System.IntPtr)(-1);
                        }
                    }
                }
            }
            if (nCode >= 0 && wParam == (IntPtr)WM_KEYUP)
            {
                int vkCode = Marshal.ReadInt32(lParam);
                Terminal.Print($"WM_KEYUP {(Keys)vkCode}\n");
                pressedKeys.Remove((Keys)vkCode);
            }
            return CallNextHookEx(_hookID, nCode, wParam, lParam);
        }

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr SetWindowsHookEx(int idHook,
            LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool UnhookWindowsHookEx(IntPtr hhk);

        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
            IntPtr wParam, IntPtr lParam);

        [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
        private static extern IntPtr GetModuleHandle(string lpModuleName);
    }
}