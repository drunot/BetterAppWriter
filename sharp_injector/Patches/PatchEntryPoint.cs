﻿using BetterAW;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using sharp_injector.Debug;
using sharp_injector.Helpers;
using System.Threading;
using System.Reflection.Emit;
using System.IO;

namespace sharp_injector.Patches {
    class PatchEntryPoint {
        static private object toolBarWindow = null;
        static private object predictionWindow = null;
        static private object menuContextWindow = null;
        static private object menuWriteSettingsContextWindow = null;
        static private object dictionaryWindow = null;
        
        static private void LoadTranslation() {
            try {
                var langPath = $"Lib\\Translations\\{Translations.GetString("Language")}.lang";
                if (File.Exists(langPath)) {
                    BetterAW.Translation.ReadFromBinaryFile(langPath);
                }
            } catch (Exception ex) {
                Terminal.Print($"{ex}\n");
            }
        }

        public static void Patch() {
            // Use debugging for now.
            // Harmony.DEBUG = true;
            // Registor harmony.
            Terminal.Print(string.Format($"{BetterAW.Translation.Instance.BetterAppWriterVersion}\n", $"{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Major}.{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Minor}.{typeof(MenuContextMenuPatcher).Assembly.GetName().Version.Build}"));
            PatchRegister.HarmonyInstance = new Harmony("com.antonvigensmolarz.appwriter.betteraw");
            ClassPrinter.PrintMembers("AppWriter.AppWriterService,AppWriter.Core");

            // Get toolbar window type and and the function to inject into it.
            var mPrefix_Toolbar_Window_Loaded = typeof(PatchEntryPoint).GetMethod(nameof(Prefix_Toolbar_Window_Loaded), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var mPrefix_Prediction_Window_Loaded = typeof(PatchEntryPoint).GetMethod(nameof(Prefix_Prediction_Window_Loaded), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
            var toolBarWType = Type.GetType("AppWriter.Windows.ToolbarWindow,AppWriter");
            var languageContextMenuWType = Type.GetType("AppWriter.Windows.ToolbarWindow,AppWriter");
            var predictionWType = Type.GetType("AppWriter.Windows.PredictionsWindow,AppWriter");

            if (toolBarWType != null) {
                // If the toolbarType is found inject the function.
                var toolBarWindow_Loaded = toolBarWType.GetMethod("Window_Loaded", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                PatchRegister.HarmonyInstance.Patch(toolBarWindow_Loaded, new HarmonyMethod(mPrefix_Toolbar_Window_Loaded));


                if (predictionWType != null) {
                    // If the toolbarType is found inject the function.
                    var predictionWindow_Loaded = predictionWType.GetMethod("Window_Loaded", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                    PatchRegister.HarmonyInstance.Patch(predictionWindow_Loaded, new HarmonyMethod(mPrefix_Prediction_Window_Loaded));


                    Thread thread = new Thread(() => {
                        Terminal.Print("Assemblies loaded:\n");
                        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies()) {
                            Terminal.Print(string.Format("{0}\n", assembly.GetName().FullName));
                        }
                        Terminal.Print("\n");
                        while (menuContextWindow == null) ;
                        MenuContextMenuPatcher mw = new MenuContextMenuPatcher(menuContextWindow);
                        AppWriterServicePatcher asp = new AppWriterServicePatcher(toolBarWindow);
                        DictionaryPatch dp = new DictionaryPatch(dictionaryWindow);
                        KeyboardShortcutsPatcher ks = new KeyboardShortcutsPatcher(predictionWindow, toolBarWindow);
                        PredictionsWindowPatcher pw = new PredictionsWindowPatcher(predictionWindow, menuWriteSettingsContextWindow, toolBarWindow);
                        ToolBarWindowPatcher tbw = new ToolBarWindowPatcher(toolBarWindow, menuWriteSettingsContextWindow);
                        LoadTranslation();
                        PatchRegister.DoPatching();
                        while (predictionWindow is null) ;
                        PatchRegister.DoPredictionPatching(predictionWindow);
                        Terminal.Print($"Language: {Translations.GetString("Language")}\n");
                    });
                    thread.Start();
                } else {
                    Terminal.Print("\nCould not find PredictionWindow type\n");
                }
            } else {
                Terminal.Print("\nCould not find ToolbarWindow type\n");
            }

        }
        public static void Prefix_Toolbar_Window_Loaded(ref object __instance) {
            try {
                // Get instances of the toolbar and the different windows.
                toolBarWindow = __instance;
                var toolBarWType = __instance.GetType();
                var _menuWindowType = toolBarWType.GetMember("_menuWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var _writeWindowType = toolBarWType.GetMember("_writeWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                var _dictionaryWindowType = toolBarWType.GetMember("_dictionaryWindow", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);
                dictionaryWindow = ((FieldInfo)_dictionaryWindowType[0]).GetValue(__instance);
                menuContextWindow = ((FieldInfo)_menuWindowType[0]).GetValue(__instance);
                menuWriteSettingsContextWindow = ((FieldInfo)_writeWindowType[0]).GetValue(__instance);
                if (menuContextWindow != null && toolBarWindow != null && dictionaryWindow != null && menuWriteSettingsContextWindow != null) {
                    Terminal.Print("Window contexts found\n");
                }

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }

        }
        public static void Prefix_Prediction_Window_Loaded(ref object __instance) {
            try {
                // Get instances of the toolbar and the Prediction.
                predictionWindow = __instance;
                if (menuContextWindow != null && toolBarWindow != null && dictionaryWindow != null) {
                    Terminal.Print("Prediction Window contexts found\n");
                }

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }

        }



    }
}
