using BetterAW;
using HarmonyLib;
using sharp_injector.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Collections.ObjectModel;
using System.Reflection.Emit;
using System.Windows.Interop;
using System.Windows.Input;

namespace sharp_injector.Patches {
    internal class ToolBarWindowPatcher : IPatcher {
        static object toolbarWindowWindow_;
        public ToolBarWindowPatcher(object toolbarWindowWindow) {
            toolbarWindowWindow_ = toolbarWindowWindow;
            PatchRegister.RegisterPatch(this);
        }

        private static readonly Events.PrioritiesedEvent<Events.MouseHookEventArgs>.EventDelegate dragHandler = (s, e) => {
            (toolbarWindowWindow_ as Window).Dispatcher.Invoke(() => {
                WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(toolbarWindowWindow_ as Window);

                Helpers.CarretPosition.updateObstructingOldPos(toolWinIntHelper.EnsureHandle());
                Helpers.WindowsHIDHooks.MouseLButtonUpHook -= dragHandler;
            });
        };

        private static void OnMouseLeftButtonDown_prefix(MouseButtonEventArgs e) {
            Helpers.WindowsHIDHooks.MouseLButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(dragHandler, 1);
        }

        public void Patch() {
            try {
                Events.PrioritiesedEvent<Events.MouseHookEventArgs>.EventDelegate handler = (object sender, Events.MouseHookEventArgs eventArgs) => {
                    // Move tool bar window.
                    (toolbarWindowWindow_ as Window).Dispatcher.Invoke(() => {
                        WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(toolbarWindowWindow_ as Window);
                        Terminal.Print($"Window moved? {Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle())}\n");
                    });
                };
                Events.PrioritiesedEvent<Events.KeyDownHookEventArgs>.EventDelegate downHandler = (object sender, Events.KeyDownHookEventArgs eventArgs) => {
                    // Move tool bar window.
                    (toolbarWindowWindow_ as Window).Dispatcher.Invoke(() => {
                        WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(toolbarWindowWindow_ as Window);
                        Terminal.Print($"Window moved? {Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle())}\n");
                    });
                };

                Helpers.WindowsHIDHooks.MouseLButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseLButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseRButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseRButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseMButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseMButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.KeyDownHook += new Events.PrioritiesedEvent<Events.KeyDownHookEventArgs>.Event(downHandler, 2);
                Helpers.WindowsHIDHooks.KeyUpHook += (s, e) => {
                    // Move tool bar window.
                    (toolbarWindowWindow_ as Window).Dispatcher.Invoke(() => {
                        WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(toolbarWindowWindow_ as Window);
                        Terminal.Print($"Window moved? {Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle())}\n");
                    });
                };
                var mOnMouseLeftButtonDown_prefix = typeof(ToolBarWindowPatcher).GetMethod(nameof(OnMouseLeftButtonDown_prefix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var OnMouseLeftButtonDown = toolbarWindowWindow_.GetType().GetMethod("OnMouseLeftButtonDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                PatchRegister.HarmonyInstance.Patch(OnMouseLeftButtonDown, new HarmonyMethod(mOnMouseLeftButtonDown_prefix));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
    }
}
