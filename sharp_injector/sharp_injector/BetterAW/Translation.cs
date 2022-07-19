using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterAW {
    // Right now no translation is supported, but might as well make it easy to implement in the future.
    [Serializable]
    public static class Translation {
        public static string BetterAppWriterSettings = "Better AppWriter Settings";
        public static string BetterAppWriterVersion = "Better AppWriter Version: {0}";
        public static string KeyboardShortcutsTooltip = "Keyboard shortcuts";
        public static string TerminalButtonTooltip = "Open/Close Terminal (For developers)";
        public static string ShortcutToggleLanguages = "Toggle selected languages.";
        public static string SeparatorPredictionWindow = "Prediction Window";
        public static string PredictionWindowPreferBelowCursor = "Prefer below cursor";
        public static string PredictionWindowForceBelowCursor = "Force below cursor";
        public static string PredictionWindowPreferAboveCursor = "Prefer above cursor";
        public static string PredictionWindowForceAboveCursor = "Force above cursor";
    }
}
