using BetterAW;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace sharp_injector.Helpers {
    public static class Translations {
        static Type translationType_ = null;
        static Translations() {
            translationType_ = Type.GetType("AppWriter.Resources.Translations,AppWriter");
        }

        public static string GetString(string name) {
            var prop = translationType_.GetProperty(name);
            if (prop == null) {
                return string.Empty;
            }
            return (string)prop.GetValue(null, null);
        }
    }
}
