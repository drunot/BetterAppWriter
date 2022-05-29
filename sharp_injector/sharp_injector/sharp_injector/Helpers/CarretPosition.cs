using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Automation;

namespace sharp_injector.Helpers {
    static class CarretPosition {
        [StructLayout(LayoutKind.Sequential)]
        public struct CarretDirmensions {
            public Int32 X;
            public Int32 Y;
            public Int32 Width;
            public Int32 Height;
        };

        [DllImport("WinAPIHooks.dll")]
        public static extern CarretDirmensions getCursorPos();

        public static object getCaretPosition() {
            var returnType = Type.GetType("AppWriter.Helpers.CaretInfo,AppWriter.Core");
            var tempPos = getCursorPos();
            object toReturn = Activator.CreateInstance(returnType, new object[] { (double)tempPos.Width, (double)tempPos.Height, (double)tempPos.X, (double)tempPos.Y }); return toReturn;
        }
        [DllImport("WinAPIHooks.dll")]
        public static extern UInt32 getCurrentScale();

    }
}
