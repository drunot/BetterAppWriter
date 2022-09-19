using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterAW {
    // Right now no translation is supported, but might as well make it easy to implement in the future.
    [Serializable]
    public class Translation {

        private Translation() {

        }

        static Translation() {
            Instance = new Translation();
        }
        public static Translation Instance {
            get;
            private set;
        }

        public string BetterAppWriterSettings { get; set; } = "Better AppWriter Settings";
        public string BetterAppWriterVersion { get; set; } = "Better AppWriter Version: {0}";
        public string KeyboardShortcutsTooltip { get; set; } = "Keyboard shortcuts";
        public string TerminalButtonTooltip { get; set; } = "Open/Close Terminal (For developers)";
        public string ShortcutToggleLanguages { get; set; } = "Toggle selected languages.";
        public string ShortcutToggleLanguagesShortcut { get; set; } = "Toggle Languages Shortcut";
        public string Toolbar { get; set; } = "Toolbar";
        public string ShortcutShortcuts { get; set; } = "shortcuts";
        public string ShortcutPredictionNavigateDown { get; set; } = "Navigate down";
        public string ShortcutPredictionNavigateUp { get; set; } = "Navigate up";
        public string ShortcutPredictionNavigateLeft { get; set; } = "Navigate left";
        public string ShortcutPredictionNavigateRight { get; set; } = "Navigate right";
        public string ShortcutPredictionHideWindow { get; set; } = "Hide window";
        public string ShortcutPredictionInsertSelected { get; set; } = "Insert selected";
        public string ShortcutPredictionInsertNumber { get; set; } = "Insert prediction number {0}";
        public string ShortcutPredictionCancelInsertion { get; set; } = "Cancel prediction insertion";
        public string PredictionWindow { get; set; } = "Prediction Window";
        public string PredictionWindowPreferBelowTextCursor { get; set; } = "Prefer below text cursor";
        public string PredictionWindowForceBelowTextCursor { get; set; } = "Force below text cursor";
        public string PredictionWindowPreferAboveTextCursor { get; set; } = "Prefer above text cursor";
        public string PredictionWindowForceAboveTextCursor { get; set; } = "Force above text cursor";
        public string ToolbarAvoidTextCursor { get; set; } = "Toolbar avoid text cursor";

        public void WriteToBinaryFile(string filePath) {
            using (Stream stream = File.Open(filePath, FileMode.Create)) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                binaryFormatter.Serialize(stream, this);
            }
        }

        public static void ReadFromBinaryFile(string filePath) {
            using (Stream stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)) {
                var binaryFormatter = new System.Runtime.Serialization.Formatters.Binary.BinaryFormatter();
                Instance = binaryFormatter.Deserialize(stream) as Translation;
            }
        }
    }
}
