using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Forms;
using System.Windows.Interop;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Resources;
using BetterAW;
using HarmonyLib;
using Microsoft.Win32;

namespace sharp_injector.Patches {
    internal class PredictionsWindowPatcher : IPatcher {
        private object _predictionsWindow = null;
        private object _writeWindow = null;
        private static object _toolBarWindow = null;
        public PredictionsWindowPatcher(object predictionsWindow, object writeWindow, object toolBarWindow) {
            _predictionsWindow = predictionsWindow;
            _writeWindow = writeWindow;
            _toolBarWindow = toolBarWindow;
            PatchRegister.RegisterPatch(this);
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
                // Get PredictionsWindow type and functions to be overwritten.
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
                        sp = (StackPanel)Helpers.TreeSearcher.getObjectFromWindow((DependencyObject)_writeWindow, (obj) => obj.GetType() == typeof(StackPanel));
                        if (sp != null) {
                            ((Window)_writeWindow).Dispatcher.Invoke(new Action(() => {
                                try {
                                    var sparatorType = Type.GetType("AppWriter.Xaml.Elements.TextSeparator,AppWriter.Xaml.Elements");
                                    for (int i = 0; i < sp.Children.Count; i++) {
                                        if (sp.Children[i].GetType() != sparatorType) {
                                            continue;
                                        }
                                        if (Helpers.Translations.GetString("Read_while_writing") == (string)sparatorType.GetProperty("TextSeparatorContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(sp.Children[i])) {
                                            sp.Children.Insert(i, Helpers.ContextMenuItemHelper.CreateStringSeparator(Translation.Instance.PredictionWindow));
                                            UniformGrid ug = new UniformGrid();
                                            ug.Margin = new Thickness(20, 0, 20, 5);
                                            ug.Columns = 4;
                                            UIElement pBelowCursor = Helpers.ContextMenuItemHelper.CreateContextMenuButton("/BetterAW;component/Images/PWBellow.xaml");
                                            UIElement fBelowCursor = Helpers.ContextMenuItemHelper.CreateContextMenuButton("/BetterAW;component/Images/PWFBellow.xaml");
                                            UIElement pAboveCursor = Helpers.ContextMenuItemHelper.CreateContextMenuButton("/BetterAW;component/Images/PWAbove.xaml");
                                            UIElement fAboveCursor = Helpers.ContextMenuItemHelper.CreateContextMenuButton("/BetterAW;component/Images/PWFAbove.xaml");
                                            UIElement label = Helpers.ContextMenuItemHelper.CreateContextMenuButtonLabel("");
                                            ((System.Windows.Controls.Control)pBelowCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pBelowCursor, true);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pAboveCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fAboveCursor, false);
                                                _prediction_position_setting = predictionWindowPosition.prefer_bellow;
                                                Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                                Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowPreferBelowTextCursor);
                                            };
                                            ((System.Windows.Controls.Control)fBelowCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fBelowCursor, true);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pAboveCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fAboveCursor, false);
                                                _prediction_position_setting = predictionWindowPosition.force_bellow;
                                                Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                                Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowForceBelowTextCursor);
                                            };
                                            ((System.Windows.Controls.Control)pAboveCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pAboveCursor, true);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fAboveCursor, false);
                                                _prediction_position_setting = predictionWindowPosition.prefer_above;
                                                Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                                Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowPreferAboveTextCursor);
                                            };
                                            ((System.Windows.Controls.Control)fAboveCursor).PreviewMouseLeftButtonUp += (s, e) => {
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fBelowCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pAboveCursor, false);
                                                Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fAboveCursor, true);
                                                _prediction_position_setting = predictionWindowPosition.force_above;
                                                Helpers.Settings.SetSetting<predictionWindowPosition>("PredictionWindowPosition", _prediction_position_setting);
                                                Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowForceAboveTextCursor);
                                            };
                                            switch (_prediction_position_setting) {
                                                case predictionWindowPosition.prefer_bellow:
                                                    Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pBelowCursor, true);
                                                    Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowPreferBelowTextCursor);
                                                    break;
                                                case predictionWindowPosition.force_bellow:
                                                    Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fBelowCursor, true);
                                                    Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowForceBelowTextCursor);
                                                    break;
                                                case predictionWindowPosition.prefer_above:
                                                    Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(pAboveCursor, true);
                                                    Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowPreferAboveTextCursor);
                                                    break;
                                                case predictionWindowPosition.force_above:
                                                    Helpers.ContextMenuItemHelper.SetIsCheckedContextMenuItem(fAboveCursor, true);
                                                    Helpers.ContextMenuItemHelper.ContextMenuButtonLabelSetLabel(label, Translation.Instance.PredictionWindowForceAboveTextCursor);
                                                    break;
                                            }
                                            ug.Children.Insert(0, pBelowCursor);
                                            ug.Children.Insert(1, fBelowCursor);
                                            ug.Children.Insert(2, pAboveCursor);
                                            ug.Children.Insert(3, fAboveCursor);
                                            sp.Children.Insert(i + 1, ug);
                                            sp.Children.Insert(i + 2, label);
                                            break;
                                        }


                                    }

                                } catch (Exception ex) {
                                    Terminal.Print(string.Format("{0}\n", ex.ToString()));
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

                // Reset selected prediction since new predictions will be avalible on updated position.
                predictionWindowType.GetField("_selectedPrediction", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(__instance, null);
                //_service.NavigatingPredictions
                var _service = predictionWindowType.GetField("_service", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance);
                var _serviceType = _service.GetType();
                _serviceType.GetField("NavigatingPredictions", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(_service, false);
                // Update layout for now, might be able to be removed.
                ((Window)__instance).UpdateLayout();

                // Get the caretInfo
                var caretInfo = Helpers.CarretPosition.getCursorPos();
                // If caretInfo was not found Width and Height will be zero.
                bool wasCaretFound = caretInfo.Width != 0.0 || caretInfo.Height != 0.0;

                // Set defualt screen if none is found.
                Screen screen = lastScreen;
                var devMode = Helpers.CarretPosition.GetDevMode(screen);

                // Find correct screen.
                foreach (Screen allScreen in Screen.AllScreens) {
                    devMode = Helpers.CarretPosition.GetDevMode(allScreen);
                    if (caretInfo.X >= (double)devMode.dmPositionX && caretInfo.X <= (double)(devMode.dmPositionX + devMode.dmPelsWidth) && caretInfo.Y >= (double)devMode.dmPositionY && caretInfo.Y <= (double)(devMode.dmPositionY + devMode.dmPelsHeight)) {
                        screen = allScreen;
                        break;
                    }
                }
                var scale = 96.0 / Helpers.CarretPosition.getCurrentScale();
                var bounds = new Rectangle(devMode.dmPositionX, devMode.dmPositionY, devMode.dmPelsWidth, devMode.dmPelsHeight);

                var wih = new WindowInteropHelper((Window)__instance);
                // Get ShowPredictionsAtCaret from ConfigurationManager.
                bool ShowPredictionsAtCaret = (bool)ConfigurationType.GetProperty("ShowPredictionsAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Configuration);

                // Set _showAtCaret to ShowPredictionsAtCaret & wasCaretFound.
                predictionWindowType.GetField("_showAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(__instance, ShowPredictionsAtCaret & wasCaretFound);

                // caretInfo was not found hide the posibility to pin the prediction window.
                var pin_btn = (UIElement)predictionWindowType.GetField("Pin", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance);
                pin_btn.Visibility = wasCaretFound ? Visibility.Visible : Visibility.Collapsed;


                if (!(bool)predictionWindowType.GetField("_showAtCaret", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)) {
                    // Variables for later.
                    System.Windows.Point point;
                    double x1;
                    double y1;
                    point = (System.Windows.Point)ConfigurationType.GetProperty("PredictionsLocation", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(Configuration);
                    x1 = point.X;
                    y1 = point.Y;
                    ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                    ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                    // Set window poistion
                    ((Window)__instance).Left = x1;
                    ((Window)__instance).Top = y1;
                } else {
                    // If scrren x is less then zero then screen will be to the left of main dispaly and Right bound should be used.
                    int x1 = caretInfo.X;
                    // If scrren y is less then zero then screen will be to the top of main dispaly and Bottom bound should be used.
                    int y1 = caretInfo.Y;
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
                        || (_prediction_position_setting == predictionWindowPosition.prefer_bellow && !((y1 + (((Window)__instance).Height + arrow_height) / scale + caretInfo.Height) > (bounds.Y + bounds.Height)))
                        || (_prediction_position_setting == predictionWindowPosition.prefer_above && (y1 - ((((Window)__instance).Height + arrow_height) / scale) < bounds.Y))) {

                        ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Visible;
                        ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                        y1 += caretInfo.Height;

                    }
                    x1 += caretInfo.Width;
                    x1 -= (int)Math.Round(((Window)__instance).Width / scale / 2.0);

                    // Check if it should be placed above, and then place it above.
                    if (_prediction_position_setting == predictionWindowPosition.force_above
                        || (_prediction_position_setting == predictionWindowPosition.prefer_bellow && (y1 + (((Window)__instance).Height + arrow_height) / scale + caretInfo.Height) > (bounds.Y + bounds.Height))
                        || (_prediction_position_setting == predictionWindowPosition.prefer_above && !(y1 - (((Window)__instance).Height + arrow_height) / scale < bounds.Y))) {
                        ((UIElement)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Collapsed;
                        ((UIElement)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Visibility = Visibility.Visible;
                        y1 -= (int)Math.Round((((Window)__instance).Height + arrow_height) / scale);
                    }
                    double leftMargin;
                    if ((x1) < bounds.Left) {

                        // If window is out of bounds on the left side set margin and redraw arrows.
                        leftMargin = (((Window)__instance).Width / 2.0) - (bounds.Left - x1) * scale;
                        leftMargin = leftMargin < 0 ? 0 : leftMargin;
                        var arrow_offset = (leftMargin / (((Window)__instance).Width / 2.0)) * -20.0;
                        string arrow_offset_str = (arrow_offset).ToString().Replace(",", ".");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,0 L 20,20 L {arrow_offset_str},20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,20 L {arrow_offset_str},0 L 20,0 Z");
                        x1 = bounds.Left;
                    } else if ((x1 + ((Window)__instance).Width / scale) > bounds.Right) {

                        // If window is out of bounds on the right side set margin and redraw arrows.
                        leftMargin = (((Window)__instance).Width * 1.5) - (bounds.Right - x1) * scale;
                        leftMargin = leftMargin > ((Window)__instance).Width ? ((Window)__instance).Width : leftMargin;
                        var arrow_offset = (1.0 - (leftMargin - (((Window)__instance).Width / 2.0)) / (((Window)__instance).Width / 2.0)) * 20.0;
                        string arrow_offset_str = (arrow_offset).ToString().Replace(",", ".");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,0 L {arrow_offset_str},20 L -20,20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse($"M 0,20 L -20,0 L {arrow_offset_str},0 Z");
                        x1 = (int)Math.Round(bounds.Right - ((Window)__instance).Width / scale);
                    } else {

                        // If window is in the inside window bounds set margin and arrows to default.
                        leftMargin = ((Window)__instance).Width / 2.0;
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse("M 0,0 L 20,20 L -20,20 Z");
                        ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Data = Geometry.Parse("M 0,20 L -20,0 L 20,0 Z");
                    }

                    // Set margin to calculated value.
                    ((System.Windows.Shapes.Path)predictionWindowType.GetField("TopPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Margin = new Thickness(leftMargin, 0.0, 0.0, 0.0);
                    ((System.Windows.Shapes.Path)predictionWindowType.GetField("BottomPointer", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).GetValue(__instance)).Margin = new Thickness(leftMargin, 0.0, 0.0, 0.0);


                    // Set window position
                    Helpers.CarretPosition.moveWinScaled(wih.EnsureHandle(), x1, y1, (int)(((Window)__instance).Width * scale), (int)(((Window)__instance).Height * scale));
                }

                // Remeber last screen. In the case that it cannot find the screen next
                // time it is very likely that the last screen is the correct one.
                lastScreen = screen;



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
