using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using BetterAW;
using HarmonyLib;
using Microsoft.Win32;

namespace sharp_injector.Patches {
    internal class PredictionsWindowPatcher : IPatcher {
        private object _predictionsWindow = null;
        public PredictionsWindowPatcher(object predictionsWindow) {
            _predictionsWindow = predictionsWindow;
            PatchRegister.RegisterPatch(this);
        }
        public void Patch() {
            try {

                var UpdateMethodType = Type.GetType("AppWriter.Windows.PredictionsWindow,AppWriter");
                var mUpdatePostion_Transpiler = typeof(PredictionsWindowPatcher).GetMethod(nameof(UpdatePostion_Transpiler), BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static);

                // <UpdatePosition>b__44_0 could be found smater than manual.
                PatchRegister.HarmonyInstance.Patch(UpdateMethodType.GetMethod("<UpdatePosition>b__44_0", BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static), null, null, new HarmonyMethod(mUpdatePostion_Transpiler));

            } catch (Exception ex) {
                Terminal.Print(string.Format("{0}\n", ex.ToString()));
            }
        }

        static IEnumerable<CodeInstruction> UpdatePostion_Transpiler(IEnumerable<CodeInstruction> instructions) {
            var codes = new List<CodeInstruction>(instructions);
            for (var i = 0; i < codes.Count; i++) {
                if (codes[i].opcode == OpCodes.Callvirt) {
                    switch (((MethodInfo)codes[i].operand).Name) {
                        case "get_DpiX": {
                                // Since the next call should be static remove member reference.
                                codes[i - 1].opcode = OpCodes.Nop;
                                codes[i - 1].operand = null;
                                // Do static call instet of callvert for new function.
                                codes[i].opcode = OpCodes.Call;
                                codes[i].operand = typeof(Helpers.CarretPosition).GetMethod(nameof(Helpers.CarretPosition.getCurrentScale));
                            }
                            break;
                        case "GetCaretInfo":
                            // Remove the fetch of the AppWriter Service.
                            codes[i - 2].opcode = OpCodes.Nop;
                            codes[i - 2].operand = null;
                            codes[i - 1].opcode = OpCodes.Nop;
                            codes[i - 1].operand = null;
                            // Do static call to better position function.
                            codes[i].opcode = OpCodes.Call;
                            codes[i].operand = typeof(Helpers.CarretPosition).GetMethod(nameof(Helpers.CarretPosition.getCaretPosition));
                            break;

                        default:
                            break;
                    }
                }


            }

            return codes.AsEnumerable();
        }
    }
}
