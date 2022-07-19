using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using BetterAW;
using HarmonyLib;
using Microsoft.Win32;

namespace sharp_injector.Patches {
    internal class PredictionsWindowPatcher : IPatcher {
        private object _predictionsWindow = null;
        private object _writeWindow = null;
        public PredictionsWindowPatcher(object predictionsWindow, object writeWindow) {
            _predictionsWindow = predictionsWindow;
            _writeWindow = writeWindow;
            PatchRegister.RegisterPatch(this);
        }

        public DependencyObject getObjectFromWindow(DependencyObject window, Type objectType) {

            int childrenCount = VisualTreeHelper.GetChildrenCount(window);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(window, i);
                Terminal.Print($"childType: {child.GetType().FullName}\n");
                if (child.GetType() == objectType) {
                    return child;
                }
            }
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(window, i);
                // This recursion could fill up the call stack... But who cares.
                return getObjectFromWindow(child, objectType);
            }
            return null;
        }

        UIElement CreateStringSeparator(string separatorMsg) {
            var sparatorType = Type.GetType("AppWriter.Xaml.Elements.TextSeparator,AppWriter.Xaml.Elements");
            var toReturn = sparatorType.GetConstructors()[0].Invoke(null);
            sparatorType.GetProperty("TextSeparatorContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, separatorMsg);
            return (UIElement)toReturn;
        }

        UIElement CreateContextMenuItem(string menuEntry) {

            var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
            var toReturn = ContextMenuItemType.GetConstructors()[0].Invoke(null);
            ContextMenuItemType.GetProperty("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, menuEntry);
            return (UIElement)toReturn;
        }

        void SetTickContextMenuItem(UIElement contextMenuItem, bool Tick) {
            var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
            if (ContextMenuItemType.GetProperty("MenuItemIconTicked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static) == null) {
                Terminal.Print("MenuItemIconTicked was null :(\n");
                return;
            }
            ContextMenuItemType.GetProperty("IconState", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(contextMenuItem, Tick ? 2 : 0);

        }

        private enum predictionWindowPosition {
            prefer_bellow,
            force_bellow,
            prefer_above,
            force_above
        }

        static predictionWindowPosition _prediction_position_setting = predictionWindowPosition.prefer_bellow;

        public void Patch() {
            try {
                //AppWriterXamlElements:TextSeparator
                //Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
                //Debug.ClassPrinter.PrintMembers("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
                var UpdateMethodType = Type.GetType("AppWriter.Windows.PredictionsWindow,AppWriter");
                var mUpdatePosition_Prefix = typeof(PredictionsWindowPatcher).GetMethod(nameof(UpdatePosition_Prefix), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                // <UpdatePosition>b__44_0 could be found smater than manual.
                PatchRegister.HarmonyInstance.Patch(UpdateMethodType.GetMethod("<UpdatePosition>b__44_0", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), new HarmonyMethod(mUpdatePosition_Prefix), null, null);
                StackPanel sp = null;
                Helpers.Settings.Initialize();
                _prediction_position_setting = Helpers.Settings.GetSetting<predictionWindowPosition>("PredictionWindowPosition", out bool found);
                if (!found) {
                    _prediction_position_setting = predictionWindowPosition.prefer_bellow;
                    Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                }

                ((Window)_writeWindow).Dispatcher.Invoke(new Action(() => {
                    try {
                        sp = (StackPanel)getObjectFromWindow((DependencyObject)_writeWindow, typeof(StackPanel));
                        if (sp != null) {
                            ((Window)_writeWindow).Dispatcher.Invoke(new Action(() => {
                                var sparatorType = Type.GetType("AppWriter.Xaml.Elements.TextSeparator,AppWriter.Xaml.Elements");
                                for (int i = 0; i < sp.Children.Count; i++) {
                                    if (sp.Children[i].GetType() != sparatorType) {
                                        continue;
                                    }
                                    if (Helpers.Translations.GetString("Read_while_writing") == (string)sparatorType.GetProperty("TextSeparatorContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(sp.Children[i])) {
                                        sp.Children.Insert(i, CreateStringSeparator(Translation.SeparatorPredictionWindow));
                                        UIElement pBelowCursor = CreateContextMenuItem(Translation.PredictionWindowPreferBelowCursor);
                                        UIElement fBelowCursor = CreateContextMenuItem(Translation.PredictionWindowForceBelowCursor);
                                        UIElement pAboveCursor = CreateContextMenuItem(Translation.PredictionWindowPreferAboveCursor);
                                        UIElement fAboveCursor = CreateContextMenuItem(Translation.PredictionWindowForceAboveCursor);
                                        ((System.Windows.Controls.Control)pBelowCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                            SetTickContextMenuItem(pBelowCursor, true);
                                            SetTickContextMenuItem(fBelowCursor, false);
                                            SetTickContextMenuItem(pAboveCursor, false);
                                            SetTickContextMenuItem(fAboveCursor, false);
                                            _prediction_position_setting = predictionWindowPosition.prefer_bellow;
                                            Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                        };
                                        ((System.Windows.Controls.Control)fBelowCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                            SetTickContextMenuItem(pBelowCursor, false);
                                            SetTickContextMenuItem(fBelowCursor, true);
                                            SetTickContextMenuItem(pAboveCursor, false);
                                            SetTickContextMenuItem(fAboveCursor, false);
                                            _prediction_position_setting = predictionWindowPosition.force_bellow;
                                            Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                        };
                                        ((System.Windows.Controls.Control)pAboveCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                            SetTickContextMenuItem(pBelowCursor, false);
                                            SetTickContextMenuItem(fBelowCursor, false);
                                            SetTickContextMenuItem(pAboveCursor, true);
                                            SetTickContextMenuItem(fAboveCursor, false);
                                            _prediction_position_setting = predictionWindowPosition.prefer_above;
                                            Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                        };
                                        ((System.Windows.Controls.Control)fAboveCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                            SetTickContextMenuItem(pBelowCursor, false);
                                            SetTickContextMenuItem(fBelowCursor, false);
                                            SetTickContextMenuItem(pAboveCursor, false);
                                            SetTickContextMenuItem(fAboveCursor, true);
                                            _prediction_position_setting = predictionWindowPosition.force_above;
                                            Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                        };
                                        switch (_prediction_position_setting) {
                                            case predictionWindowPosition.prefer_bellow:
                                                SetTickContextMenuItem(pBelowCursor, true);
                                                break;
                                            case predictionWindowPosition.force_bellow:
                                                SetTickContextMenuItem(fBelowCursor, true);
                                                break;
                                            case predictionWindowPosition.prefer_above:
                                                SetTickContextMenuItem(pAboveCursor, true);
                                                break;
                                            case predictionWindowPosition.force_above:
                                                SetTickContextMenuItem(fAboveCursor, true);
                                                break;
                                        }
                                        sp.Children.Insert(i + 1, pBelowCursor);
                                        sp.Children.Insert(i + 2, fBelowCursor);
                                        sp.Children.Insert(i + 3, pAboveCursor);
                                        sp.Children.Insert(i + 4, fAboveCursor);
                                        break;
                                    }


                                }

                            }));
                        } else {
                            Terminal.Print("sp was null :(\n");
                        }
                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));


            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        static Screen lastScreen = Screen.PrimaryScreen;

        static bool UpdatePosition_Prefix(in object __instance) {
            try {
                // If prediction window is not visible do not do anything.
                if (((Window)__instance).Visibility != Visibility.Visible) {
                    return false;  // Overwrite normal functionality
                }
                var predictionWindowType = __instance.GetType();
                var Configuration = Type.GetType("AppWriter.ConfigurationManager,AppWriter").GetProperty("Configuration", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(null);
                var ConfigurationType = Configuration.GetType();

                // Update layout for now, might be able to be removed.
                ((Window)__instance).UpdateLayout();

                // Get the caretInfo
                var caretInfo = Helpers.CarretPosition.getCursorPos();
                // If caretInfo was not found Width and Height will be zero.
                bool wasCaretFound = caretInfo.Width != 0.0 || caretInfo.Height != 0.0;

                // Set defualt screen if none is found.
                Screen screen = lastScreen;
                var devMode = Helpers.CarretPosition.GetDevMode(screen);
                double scale = (double)screen.Bounds.Width / (double)devMode.dmPelsWidth;

                // Find correct screen.
                foreach (Screen allScreen in Screen.AllScreens) {
                    devMode = Helpers.CarretPosition.GetDevMode(allScreen);
                    if (caretInfo.X >= (double)devMode.dmPositionX && caretInfo.X <= (double)(devMode.dmPositionX + devMode.dmPelsWidth) && caretInfo.Y >= (double)devMode.dmPositionY && caretInfo.Y <= (double)(devMode.dmPositionY + devMode.dmPelsHeight)) {
                        screen = allScreen;
                        scale = (double)screen.Bounds.Width / (double)devMode.dmPelsWidth;
                        break;
                    }
                }

                // Get ShowPredictionsAtCaret from ConfigurationManager.
                bool ShowPredictionsAtCaret = (bool)ConfigurationType.GetProperty("ShowPredictionsAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Configuration);

                // Set _showAtCaret to ShowPredictionsAtCaret & wasCaretFound.
                predictionWindowType.GetField("_showAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(__instance, ShowPredictionsAtCaret & wasCaretFound);

                // caretInfo was not found hide the posibility to pin the prediction window.
                var pin_btn = (UIElement)predictionWindowType.GetField("Pin", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance);
                pin_btn.Visibility = wasCaretFound ? Visibility.Visible : Visibility.Collapsed;

                // Variables for later.
                System.Windows.Point point;
                double x1;
                double y1;

                if (!(bool)predictionWindowType.GetField("_showAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)) {
                    point = (System.Windows.Point)ConfigurationType.GetProperty("PredictionsLocation", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Configuration);
                    x1 = point.X;
                    y1 = point.Y;
                    ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                    ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                } else {
                    // If X is bellow zero use right hand of screen to find position else use left
                    if (caretInfo.X < 0) {
                        var ScreenPosRight = devMode.dmPositionX + devMode.dmPelsWidth;
                        var relativeCaretPos = (caretInfo.X - ScreenPosRight) * scale;
                        x1 = relativeCaretPos + screen.Bounds.Right;
                    } else {
                        var relativeCaretPos = (caretInfo.X - devMode.dmPositionX) * scale;
                        x1 = relativeCaretPos + screen.Bounds.Left;
                    }
                    // If Y is bellow zero use bottom of screen to find position else use top
                    if (caretInfo.Y < 0) {
                        var ScreenPosBottom = devMode.dmPositionY + devMode.dmPelsHeight;
                        var relativeCaretPos = (caretInfo.Y - ScreenPosBottom) * scale;
                        y1 = relativeCaretPos + screen.Bounds.Bottom;
                    } else {
                        var relativeCaretPos = (caretInfo.Y - devMode.dmPositionY) * scale;
                        y1 = relativeCaretPos + screen.Bounds.Top;
                    }
                    int arrow_height;
                    // Check how the height is off due to arrow visability.
                    if (((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility == Visibility.Collapsed &&
                    ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility == Visibility.Collapsed) {
                        arrow_height = 20;
                    } else if (((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility == Visibility.Visible &&
                    ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility == Visibility.Visible) {
                        arrow_height = -20;
                    } else {
                        arrow_height = 0;
                    }

                    // Check if it should be placed bellow, and then place it bellow.
                    if (_prediction_position_setting == predictionWindowPosition.force_bellow
                        || (_prediction_position_setting == predictionWindowPosition.prefer_bellow && !((y1 + ((Window)__instance).Height + caretInfo.Height + arrow_height) > (screen.Bounds.Y + screen.Bounds.Height)))
                        || (_prediction_position_setting == predictionWindowPosition.prefer_above && (y1 - (((Window)__instance).Height + arrow_height) < screen.Bounds.Y))) {
                        // Assume that predictionWindowType should be placed bellow the cursor for now.
                        ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Visible;
                        ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                        y1 += caretInfo.Height * scale;

                    }
                    x1 += caretInfo.Width * scale;
                    x1 -= ((Window)__instance).Width / 2.0;

                    // Check if it should be placed above, and then place it above.
                    if (_prediction_position_setting == predictionWindowPosition.force_above
                        || (_prediction_position_setting == predictionWindowPosition.prefer_bellow && (y1 + ((Window)__instance).Height + caretInfo.Height + arrow_height) > (screen.Bounds.Y + screen.Bounds.Height))
                        || (_prediction_position_setting == predictionWindowPosition.prefer_above && !(y1 - (((Window)__instance).Height + arrow_height) < screen.Bounds.Y))) {
                        ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                        ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Visible;
                        y1 -= (((Window)__instance).Height + arrow_height);
                    }
                    double leftMargin;
                    if ((x1) < screen.Bounds.Left) {

                        // If window is out of bounds on the left side set margin and redraw arrows.
                        leftMargin = (((Window)__instance).Width / 2.0) - (screen.Bounds.Left - x1);
                        leftMargin = leftMargin < 0 ? 0 : leftMargin;
                        var arrow_offset = (leftMargin / (((Window)__instance).Width / 2.0)) * -20.0;
                        string arrow_offset_str = (arrow_offset).ToString().Replace(",", ".");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,0 L 20,20 L {arrow_offset_str},20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,20 L {arrow_offset_str},0 L 20,0 Z");
                        x1 = screen.Bounds.Left;
                    } else if ((x1 + ((Window)__instance).Width) > screen.Bounds.Right) {

                        // If window is out of bounds on the right side set margin and redraw arrows.
                        leftMargin = (((Window)__instance).Width * 1.5) - (screen.Bounds.Right - x1);
                        leftMargin = leftMargin > ((Window)__instance).Width ? ((Window)__instance).Width : leftMargin;
                        var arrow_offset = (1.0 - (leftMargin - (((Window)__instance).Width / 2.0)) / (((Window)__instance).Width / 2.0)) * 20.0;
                        string arrow_offset_str = (arrow_offset).ToString().Replace(",", ".");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,0 L {arrow_offset_str},20 L -20,20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,20 L -20,0 L {arrow_offset_str},0 Z");
                        x1 = screen.Bounds.Right - ((Window)__instance).Width;
                    } else {

                        // If window is in the inside window bounds set margin and arrows to default.
                        leftMargin = ((Window)__instance).Width / 2.0;
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse("M 0,0 L 20,20 L -20,20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse("M 0,20 L -20,0 L 20,0 Z");
                    }

                    // Set margin to calculated value.
                    ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Margin = new Thickness(leftMargin, 0.0, 0.0, 0.0);
                    ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Margin = new Thickness(leftMargin, 0.0, 0.0, 0.0);
                }

                // Remeber last screen. In the case that it cannot find the screen next
                // time it is very likely that the last screen is the correct one.
                lastScreen = screen;

                // Set window position
                ((Window)__instance).Left = x1;
                ((Window)__instance).Top = y1;

                // Do some limitedContextPopup stuff that isn't completly understood.
                var limitedContextPopup = (Window)predictionWindowType.GetField("_limitedContextPopup", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance);
                if (!limitedContextPopup.IsVisible) {

                    return false; // Overwrite normal functionality
                }
                predictionWindowType.GetMethod("UpdatedLimitedContextPopupPosition", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).Invoke(__instance, null);
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
            return false; // Overwrite normal functionality
        }
    }
}
