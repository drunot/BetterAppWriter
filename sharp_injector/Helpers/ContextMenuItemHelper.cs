using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Forms;
using BetterAW;

namespace sharp_injector.Helpers {
    public static class ContextMenuItemHelper {
        public static UIElement CreateStringSeparator(string separatorMsg) {
            var sparatorType = Type.GetType("AppWriter.Xaml.Elements.TextSeparator,AppWriter.Xaml.Elements");
            var toReturn = sparatorType.GetConstructors()[0].Invoke(null);
            sparatorType.GetProperty("TextSeparatorContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, separatorMsg);
            return (UIElement)toReturn;
        }

        public static UIElement CreateContextMenuItem(string menuEntry) {

            var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
            var toReturn = ContextMenuItemType.GetConstructors()[0].Invoke(null);
            ContextMenuItemType.GetProperty("MenuItemContent", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, menuEntry);
            return (UIElement)toReturn;
        }

        public static UIElement CreateContextMenuButton(string imageURI = "") {
            Terminal.Print($"Image{imageURI.Split('/').Last().Split('.')[0]}\n");
            var ContextMenuButtonType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuButton,AppWriter.Xaml.Elements");
            var toReturn = ContextMenuButtonType.GetConstructors()[0].Invoke(null);
            if (imageURI != "") {
                var resourceDictionary = new ResourceDictionary();
                resourceDictionary.Source =
                    new Uri(imageURI,
                            UriKind.RelativeOrAbsolute);
                Viewbox viewbox = resourceDictionary[$"Image{imageURI.Split('/').Last().Split('.')[0]}"] as Viewbox;
                viewbox.MaxHeight = 21;
                viewbox.MaxWidth = 25;
                viewbox.SnapsToDevicePixels = true;
                ContextMenuButtonType.GetProperty("Content", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, viewbox);
            }

            return (UIElement)toReturn;
        }

        public static UIElement CreateContextMenuButtonLabel(string labelString) {
            var ContextMenuButtonLabelType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuButtonLabel,AppWriter.Xaml.Elements");
            var toReturn = ContextMenuButtonLabelType.GetConstructors()[0].Invoke(null);
            ContextMenuButtonLabelType.GetProperty("Content", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(toReturn, labelString);
            return (UIElement)toReturn;
        }

        public static void ContextMenuButtonLabelSetLabel(UIElement label, string labelString) {
            var ContextMenuButtonLabelType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuButtonLabel,AppWriter.Xaml.Elements");
            ContextMenuButtonLabelType.GetProperty("Content", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(label, labelString);
        }

        public static void SetTickContextMenuItem(UIElement contextMenuItem, bool Tick) {
            var ContextMenuItemType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuItem,AppWriter.Xaml.Elements");
            ContextMenuItemType.GetProperty("IconState", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(contextMenuItem, Tick ? 2 : 0);

        }

        public static void SetIsCheckedContextMenuItem(UIElement contextMenuItem, bool isChecked) {
            var ContextMenuButtonType = Type.GetType("AppWriter.Xaml.Elements.ContextMenuButton,AppWriter.Xaml.Elements");
            ContextMenuButtonType.GetProperty("IsChecked", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static).SetValue(contextMenuItem, isChecked);

        }
    }
}
