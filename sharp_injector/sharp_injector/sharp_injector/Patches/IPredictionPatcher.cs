using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sharp_injector.Patches {
    internal interface IPredictionPatcher {
        void PredictionPatch(object predictionWindow);
    }
}
