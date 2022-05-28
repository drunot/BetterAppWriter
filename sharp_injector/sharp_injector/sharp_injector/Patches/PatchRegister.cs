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

        public static void RegisterPatch(IPatcher patch) {
            Patches.Add(patch);
        }

        public static void DoPatching() {
            foreach (var patch in Patches) {
                patch.Patch();
            }
        }
    }
}
