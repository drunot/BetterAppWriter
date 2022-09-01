using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharp_injector.Helpers {

    public class Setting<T> {
        public string Name;
        public T Value;
    }
    static internal class Settings {
        private static readonly string settingsPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData) + @"\BetterAppWriter\settings.json";
        private static List<Setting<dynamic>> settings = new List<Setting<dynamic>>();
        private static bool initialized = false;

        public static void Initialize() {
            if (initialized) return;
            initialized = true;
            if (!File.Exists(settingsPath)) {
                return;
            }
            string data = File.ReadAllText(settingsPath);
            settings = JsonConvert.DeserializeObject<Setting<dynamic>[]>(data).ToList();
        }

        public static T GetSetting<T>(string name, out bool found) {
            foreach (var setting in settings) {
                if (setting.Name == name) {
                    found = true;
                    return (T)setting.Value;
                }
            }
            found = false;
            return default(T);
        }

        public static void SetSetting<T>(string name, T value) {
            bool found = false;
            foreach (var setting in settings) {
                if (setting.Name == name) {
                    setting.Value = value;
                    found = true;
                    break;
                }
            }
            if (!found) {
                settings.Add(new Setting<dynamic> { Name = name, Value = value });
            }
            var toSave = JsonConvert.SerializeObject(settings);
            File.WriteAllText(settingsPath, toSave);
        }
    }
}
