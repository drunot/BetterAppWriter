using BetterAW;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using sharp_injector.Debug;
using System.Threading;
using System.Reflection.Emit;

namespace sharp_injector.Patches
{
    class PatchEntryPoint
    {
        static private object toolBarWindow = null;
        static private object predictionWindow = null;
        static private object menuContextWindow = null;
        static private object dictionaryWindow = null;
        public static void Patch()
        {
            // Use debugging for now.
            Harmony.DEBUG = true;
            // Registor harmony.
            PatchRegister.HarmonyInstance = new Harmony("com.antonvigensmolarz.appwriter.betteraw");

            // Get toolbar window type and and the function to inject into it.
            var mPrefix_Toolbar_Window_Loaded = typeof(PatchEntryPoint).GetMethod(nameof(Prefix_Toolbar_Window_Loaded), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var mPrefix_Prediction_Window_Loaded = typeof(PatchEntryPoint).GetMethod(nameof(Prefix_Prediction_Window_Loaded), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var toolBarWType = Type.GetType("AppWriter.Windows.ToolbarWindow,AppWriter");
            var languageContextMenuWType = Type.GetType("AppWriter.Windows.ToolbarWindow,AppWriter");
            var predictionWType = Type.GetType("AppWriter.Windows.PredictionsWindow,AppWriter");

            if (toolBarWType != null)
            {
                // If the toolbarType is found inject the function.
                var toolBarWindow_Loaded = toolBarWType.GetMethod("Window_Loaded", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                PatchRegister.HarmonyInstance.Patch(toolBarWindow_Loaded, new HarmonyMethod(mPrefix_Toolbar_Window_Loaded));

               
                if (toolBarWType != null)
                {
                    // If the toolbarType is found inject the function.
                    var predictionWindow_Loaded = predictionWType.GetMethod("Window_Loaded", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    PatchRegister.HarmonyInstance.Patch(predictionWindow_Loaded, new HarmonyMethod(mPrefix_Prediction_Window_Loaded));

                    Thread thread = new Thread(() =>
                    {
                        Terminal.Print("Assemblies loaded:\n");
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
                        {
                            Terminal.Print(string.Format("{0}\n", assembly.GetName().FullName));
                        }
                        Terminal.Print("\n");
                        while (menuContextWindow == null) ;
                        MenuContextMenuPatcher mw = new MenuContextMenuPatcher(menuContextWindow);
                        AppWriterServicePatcher asp = new AppWriterServicePatcher(toolBarWindow);
                        DictionaryPatch tw = new DictionaryPatch(dictionaryWindow);
                        KeyboardShortcutsPatcher ks = new KeyboardShortcutsPatcher(predictionWindow, toolBarWindow);
                        PatchRegister.DoPatching();
                    });
                    thread.Start();
                }
                else
                {
                    Terminal.Print("\nCould not find PredictionWindow type\n");
                }
            }
            else
            {
                Terminal.Print("\nCould not find ToolbarWindow type\n");
            }

        }
        public static void Prefix_Toolbar_Window_Loaded(ref object __instance)
        {
            try
            {
                // Get instances of the toolbar and the different windows.
                toolBarWindow = __instance;
                var toolBarWType = __instance.GetType();
                var _menuWindowType = toolBarWType.GetMember("_menuWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var _dictionaryWindowType = toolBarWType.GetMember("_dictionaryWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                dictionaryWindow = ((FieldInfo)_dictionaryWindowType[0]).GetValue(__instance);
                menuContextWindow = ((FieldInfo)_menuWindowType[0]).GetValue(__instance);
                if (menuContextWindow != null && toolBarWindow != null && dictionaryWindow != null)
                {
                    Terminal.Print("Toolbar Window contexts found\n");
                }

            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }

        }
        public static void Prefix_Prediction_Window_Loaded(ref object __instance)
        {
            try
            {
                // Get instances of the toolbar and the Prediction.
                predictionWindow = __instance;
                if (menuContextWindow != null && toolBarWindow != null && dictionaryWindow != null)
                {
                    Terminal.Print("Prediction Window contexts found\n");
                }

            }
            catch (Exception ex)
            {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }

        }



    }
}
