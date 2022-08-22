// Most code taken from https://github.com/dotnet/corert/blob/master/src/System.Private.CoreLib/shared/System/HashCode.cs
// Which is a core exclusive liberary.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace sharp_injector.Helpers {
    public static class HashCombinator {
        private const uint Prime1 = 2654435761U;
        private const uint Prime2 = 2246822519U;
        private const uint Prime3 = 3266489917U;
        private const uint Prime4 = 668265263U;
        private const uint Prime5 = 374761393U;
        private static readonly uint s_seed = GenerateGlobalSeed();
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint QueueRound(uint hash, uint queuedValue) {
            int size = sizeof(uint) * 8;
            uint number = hash + queuedValue * Prime3;

            return (number << 17 | number >> (size - 17)) * Prime4;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixEmptyState() {
            return s_seed + Prime5;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static uint MixFinal(uint hash) {
            hash ^= hash >> 15;
            hash *= Prime2;
            hash ^= hash >> 13;
            hash *= Prime3;
            hash ^= hash >> 16;
            return hash;
        }

        private static uint GenerateGlobalSeed() {
            Random r = new Random();
            return (uint)r.Next();
        }

        public static int Combine(params int[] values) {

            uint hash = MixEmptyState();
            hash += (uint)values.Count() * 4u;

            foreach (var value in values) {
                hash = QueueRound(hash, (uint)value);
            }

            hash = MixFinal(hash);
            return (int)hash;
        }
    }
}
