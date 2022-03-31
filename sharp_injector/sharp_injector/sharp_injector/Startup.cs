using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using sharp_injector.Debug;
using sharp_injector.AppWrappers;
using System.Threading;
using BetterAW;
using System.Windows;
using sharp_injector.Helpers;
using HarmonyLib;
using System.Runtime.CompilerServices;
using System.Reflection.Emit;
using System.Windows.Input;
using sharp_injector.Patches;

namespace sharp_injector
{
    public class Startup
    {
        [STAThread]
        static int EntryPoint(string xyzstring)
        {
            // Open terminal
            NewWindowHandler();
            while (!Terminal.IsReady)
            {

            }
            PatchEntryPoint.Patch();
            return 0;
        }

        private static void NewWindowHandler()
        {
            // Open window in other thread.
            Thread newWindowThread = new Thread(new ThreadStart(ThreadStartingPoint));
            newWindowThread.SetApartmentState(ApartmentState.STA);
            newWindowThread.IsBackground = true;
            newWindowThread.Start();
        }

        private static void ThreadStartingPoint()
        {
            // Init terminal window.
            Terminal.Initialize();
            System.Windows.Threading.Dispatcher.Run();
        }

    }
}
