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
using sharp_injector.Events;

namespace sharp_injector.Patches {
    internal class ToolBarWindowPatcher : IPatcher {
        static object _toolbarWindow;
        private object _writeWindow = null;
        static bool movementEnabled = true;

        public ToolBarWindowPatcher(object toolbarWindowWindow, object writeWindow) {
            _toolbarWindow = toolbarWindowWindow;
            _writeWindow = writeWindow;
            PatchRegister.RegisterPatch(this);
        }

        private static readonly Events.PrioritiesedEvent<Events.MouseHookEventArgs>.EventDelegate dragHandler = (s, e) => {
            if (movementEnabled) {
                (_toolbarWindow as Window).Dispatcher.Invoke(() => {
                    WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(_toolbarWindow as Window);

                    Helpers.CarretPosition.updateObstructingOldPos(toolWinIntHelper.EnsureHandle());
                    Helpers.WindowsHIDHooks.MouseLButtonUpHook -= dragHandler;
                });
            }
        };

        private static void OnMouseLeftButtonDown_prefix(MouseButtonEventArgs e) {

            if (movementEnabled) {
                Helpers.WindowsHIDHooks.MouseLButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(dragHandler, 1);
            }
        }

        public void Patch() {
            try {
                bool found;
                Helpers.Settings.Initialize();
                movementEnabled = Helpers.Settings.GetSetting<bool>("ToolbarAvoidCarret", out found);
                if (!found) {
                    movementEnabled = true;
                }
                ((Window)_writeWindow).Dispatcher.Invoke(new Action(() => {
                    try {
                        var sp = (StackPanel)Helpers.TreeSearcher.getObjectFromWindow((DependencyObject)_writeWindow, (obj) => obj.GetType() == typeof(StackPanel));
                        if (sp != null) {
                            var ToolbarSeparator = Helpers.ContextMenuItemHelper.CreateStringSeparator(BetterAW.Translation.Instance.Toolbar);
                            sp.Children.Add(ToolbarSeparator);
                            var ToolbarContextMenuItem = Helpers.ContextMenuItemHelper.CreateContextMenuItem(BetterAW.Translation.Instance.ToolbarAvoidTextCursor);
                            ToolbarContextMenuItem.PreviewMouseLeftButtonDown += (sender, e) => {
                                movementEnabled = !movementEnabled;
                                Helpers.Settings.SetSetting("ToolbarAvoidCarret", movementEnabled);
                                Helpers.ContextMenuItemHelper.SetTickContextMenuItem(ToolbarContextMenuItem, movementEnabled);
                            };
                            Helpers.ContextMenuItemHelper.SetTickContextMenuItem(ToolbarContextMenuItem, movementEnabled);
                            sp.Children.Add(ToolbarContextMenuItem);
                        }
                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));
                Events.PrioritiesedEvent<Events.MouseHookEventArgs>.EventDelegate handler = (object sender, Events.MouseHookEventArgs eventArgs) => {
                    if (movementEnabled) {
                        // Move tool bar window.
                        (_toolbarWindow as Window).Dispatcher.Invoke(() => {
                            WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(_toolbarWindow as Window);
                            Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle());
                        });
                    }
                };
                Events.PrioritiesedEvent<Events.KeyDownHookEventArgs>.EventDelegate downHandler = (object sender, Events.KeyDownHookEventArgs eventArgs) => {
                    if (movementEnabled) {
                        // Move tool bar window.
                        (_toolbarWindow as Window).Dispatcher.Invoke(() => {
                            WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(_toolbarWindow as Window);
                            Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle());
                        });
                    }
                };

                Helpers.WindowsHIDHooks.MouseLButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseLButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseRButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseRButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseMButtonDownHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.MouseMButtonUpHook += new Events.PrioritiesedEvent<Events.MouseHookEventArgs>.Event(handler, 2);
                Helpers.WindowsHIDHooks.KeyDownHook += new Events.PrioritiesedEvent<Events.KeyDownHookEventArgs>.Event(downHandler, 2);
                Helpers.WindowsHIDHooks.KeyUpHook += new PrioritiesedEvent<KeyUpHookEventArgs>.Event((s, e) => {
                    if (movementEnabled) {
                        // Move tool bar window.
                        (_toolbarWindow as Window).Dispatcher.Invoke(() => {
                            WindowInteropHelper toolWinIntHelper = new WindowInteropHelper(_toolbarWindow as Window);
                            Helpers.CarretPosition.moveWinIfObstructing(toolWinIntHelper.EnsureHandle());
                        });
                    }
                }, 1);
                var mOnMouseLeftButtonDown_prefix = typeof(ToolBarWindowPatcher).GetMethod(nameof(OnMouseLeftButtonDown_prefix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var OnMouseLeftButtonDown = _toolbarWindow.GetType().GetMethod("OnMouseLeftButtonDown", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                PatchRegister.HarmonyInstance.Patch(OnMouseLeftButtonDown, new HarmonyMethod(mOnMouseLeftButtonDown_prefix));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
    }
}
