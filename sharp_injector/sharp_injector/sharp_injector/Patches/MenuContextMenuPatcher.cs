using BetterAW;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;

namespace sharp_injector.Patches {
    internal class MenuContextMenuPatcher : IPatcher {
        static Settings settings;
        object menuContextMenuWindow_;
        public MenuContextMenuPatcher(object menuContextMenuWindow) {
            PatchRegister.RegisterPatch(this);
            menuContextMenuWindow_ = menuContextMenuWindow;
        }
        public void AddContentMenuItem(string title, string image, string location) {
            try {

                var t = menuContextMenuWindow_.GetType();
                var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
                ((Window)menuContextMenuWindow_).Dispatcher.Invoke(new Action(() => {

                    try {
                        StackPanel ItemsPanel = (StackPanel)((FieldInfo)t.GetMember("Items", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(menuContextMenuWindow_);
                        StackPanel stackPanel = (StackPanel)((FieldInfo)t.GetMember(location, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(menuContextMenuWindow_);
                        var ContextMenuItemInstance = ContextMenuItemType.GetConstructors()[0].Invoke(null);
                        var MenuItemContent = ContextMenuItemType.GetMember("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                        ((PropertyInfo)MenuItemContent[0]).SetValue(ContextMenuItemInstance, title, null);
                        var MenuItemIcon = ContextMenuItemType.GetMember("MenuItemIcon", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                        Image image1 = new Image();
                        Viewbox viewbox = new Viewbox();
                        StreamResourceInfo sri = Application.GetResourceStream(new Uri(image, UriKind.Relative));
                        if (sri != null) {
                            using (Stream s = sri.Stream) {
                                viewbox = XamlReader.Load(s) as Viewbox;
                            }
                        }
                        viewbox.MaxHeight = 19;
                        viewbox.MaxWidth = 19;
                        ((Control)ContextMenuItemInstance).PreviewMouseLeftButtonUp += (sender, e) => {
                            try {
                                if (settings == null || settings.IsClosed) {
                                    settings = new Settings();
                                    settings.Show();
                                } else {
                                    settings.Focus();
                                }
                            } catch (Exception ex) {
                                Terminal.Print(string.Format("{0}\n", ex.ToString()));
                            }
                        };
                        ((PropertyInfo)MenuItemIcon[0]).SetValue(ContextMenuItemInstance, viewbox, null);
                        stackPanel.Children.Add(ContextMenuItemInstance as Control);
                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        public void AddBAWVersion() {
            try {

                var t = menuContextMenuWindow_.GetType();
                var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
                ((Window)menuContextMenuWindow_).Dispatcher.Invoke(new Action(() => {

                    try {
                        StackPanel ItemsPanel = (StackPanel)((FieldInfo)t.GetMember("Items", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(menuContextMenuWindow_);
                        Label bawVersion = new Label();
                        bawVersion.Content = string.Format(Translation.BetterAppWriterVersion, $"{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Major}.{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Minor}.{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Build}");
                        bawVersion.Margin = new Thickness(6, 0, 0, 0);
                        bawVersion.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#999999"));
                        bawVersion.HorizontalAlignment = HorizontalAlignment.Left;
                        bawVersion.FontSize = 11;
                        Grid grid = new Grid();
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.ColumnDefinitions.Add(new ColumnDefinition());
                        grid.Children.Add(bawVersion);
                        Label appVersion = null;
                        foreach (var child in ItemsPanel.Children) {
                            if (child.GetType() == typeof(Label)) {
                                appVersion = child as Label;
                                break;
                            }
                        }
                        if (appVersion != null) {
                            ItemsPanel.Children.Remove(appVersion);
                            Grid.SetColumn(appVersion, 1);
                            grid.Children.Add(appVersion);
                            ItemsPanel.Children.Add(grid);
                        }

                    } catch (Exception ex) {
                        Terminal.Print(string.Format("{0}\n", ex.ToString()));
                    }
                }));
            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }
        public void Patch() {
            AddContentMenuItem(Translation.BetterAppWriterSettings, "/BetterAW;component/Images/Settings.xaml", "PredictionItems");
            AddBAWVersion();
            // Show Wizkids own admin tools.
            //StackPanel AdminTools = (StackPanel)((FieldInfo)menuContextMenuWindow_.GetType().GetMember("AdminTools", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static)[0]).GetValue(menuContextMenuWindow_);
            //AdminTools.Visibility = Visibility.Visible;
            Terminal.Print("MenuContextMenuPatcher sucsess\n");
        }
    }
}
