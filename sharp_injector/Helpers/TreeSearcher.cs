using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;

namespace sharp_injector.Helpers {
    internal static class TreeSearcher {


        public delegate bool DependencyObjectFinder (DependencyObject obj);
        public static DependencyObject getObjectFromWindow(DependencyObject window, DependencyObjectFinder objFinder) {

            int childrenCount = VisualTreeHelper.GetChildrenCount(window);
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(window, i);
                if (objFinder(child)) {
                    return child;
                }
            }
            for (int i = 0; i < childrenCount; i++) {
                var child = VisualTreeHelper.GetChild(window, i);
                // This recursion could fill up the call stack... But who cares.
                return getObjectFromWindow(child, objFinder);
            }
            return null;
        }
    }
}
