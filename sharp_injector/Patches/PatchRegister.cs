using BetterAW;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace sharp_injector.Patches {
    internal static class PatchRegister {
        static public Harmony HarmonyInstance { get; set; }


        public static List<IPatcher> Patches = new List<IPatcher>();
        public static List<IPredictionPatcher> PredictionPatches = new List<IPredictionPatcher>();

        public static void RegisterPatch(IPatcher patch) {
            Patches.Add(patch);
        }

        public static void RegisterPredictionPatch(IPredictionPatcher patch) {
            PredictionPatches.Add(patch);
        }

        public static void DoPatching() {
            foreach (var patch in Patches) {
                patch.Patch();
            }
        }

        public static void DoPredictionPatching(object predictionWindow) {
            foreach (var patch in PredictionPatches) {
                patch.PredictionPatch(predictionWindow);
            }

        }
    }
}
