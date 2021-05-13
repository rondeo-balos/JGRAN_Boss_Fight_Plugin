using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JGRAN_Boss_Fight_Plugin
{
        public sealed class MathUtils
        {
            private MathUtils()
            {
            }

            public static readonly float nanoToSec = 1 / 1E+09F;
            // ---
            public static readonly float FLOAT_ROUNDING_ERROR = 1E-06F; // 32 bits
            public static readonly float PI = (float)Math.PI;
            public static readonly float PI2 = PI * 2;
            public static readonly float HALF_PI = PI / 2;
            public static readonly float E = (float)Math.E;
            private static readonly int SIN_BITS = 14; // 16KB. Adjust for accuracy.
            private static readonly int SIN_MASK = ~(-1 << SIN_BITS);
            private static readonly int SIN_COUNT = SIN_MASK + 1;
            private static readonly float radFull = PI2;
            private static readonly float degFull = 360;
            private static readonly float radToIndex = SIN_COUNT / radFull;
            private static readonly float degToIndex = SIN_COUNT / degFull;
            /// <summary>
            /// multiply by this to convert from radians to degrees
            /// </summary>
            public static readonly float radiansToDegrees = 180F / PI;
            public static readonly float radDeg = radiansToDegrees;
            /// <summary>
            /// multiply by this to convert from degrees to radians
            /// </summary>
            public static readonly float degreesToRadians = PI / 180;
            public static readonly float degRad = degreesToRadians;
            private class Sin
            {
                public static readonly float[] table = new float[SIN_COUNT];
                static Sin()
                {
                    for (int i = 0; i < SIN_COUNT; i++)
                        table[i] = (float)Math.Sin((i + 0.5F) / SIN_COUNT * radFull);

                    // The four right angles get extra-precise values, because they are
                    // the most likely to need to be correct.
                    table[0] = 0F;
                    table[(int)(90 * degToIndex) & SIN_MASK] = 1F;
                    table[(int)(180 * degToIndex) & SIN_MASK] = 0F;
                    table[(int)(270 * degToIndex) & SIN_MASK] = -1F;
                }
            }

            /// <summary>
            /// Returns the sine in degrees from a lookup table. For optimal precision, use degrees between -360 and 360 (both
            /// inclusive).
            /// </summary>
            public static float SinDeg(float degrees)
            {
                return Sin.table[(int)(degrees * degToIndex) & SIN_MASK];
            }

            /// <summary>
            /// Returns the cosine in degrees from a lookup table. For optimal precision, use degrees between -360 and 360 (both
            /// inclusive).
            /// </summary>
            public static float CosDeg(float degrees)
            {
                return Sin.table[(int)((degrees + 90) * degToIndex) & SIN_MASK];
            }
        }
}
