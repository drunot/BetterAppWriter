using BetterAW;
using Newtonsoft.Json.Linq;
using sharp_injector.Helpers;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sharp_injector.Debug {
    public static class FilePrinter {
        private static readonly string printPath = $"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\\Better_AppWriter.log";

        public static void Print(string msg) {
            using (FileStream aFile = new FileStream(printPath, FileMode.Append, FileAccess.Write))
            using (StreamWriter sw = new StreamWriter(aFile)) {
                sw.WriteLine(msg);
            }
        }
    }
}
