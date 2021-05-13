using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JGRAN_Boss_Fight_Plugin
{
    public sealed class Intersector
    {
        private Intersector()
        {
        }

        /// <summary>
        /// Check whether convex polygons overlap (clockwise or counter-clockwise wound doesn't matter). If they do, optionally obtain
        /// a Minimum Translation Vector indicating the minimum magnitude vector required to push the polygon p1 out of collision with
        /// polygon p2.
        /// </summary>
        /// <param name="p1">The first polygon.</param>
        /// <param name="p2">The second polygon.</param>
        /// <param name="mtv">A Minimum Translation Vector to fill in the case of a collision, or null (optional).</param>
        /// <returns>Whether polygons overlap.</returns>
        public static bool OverlapConvexPolygons(Polygon p1, Polygon p2, MinimumTranslationVector mtv)
        {
            return OverlapConvexPolygons(p1.GetTransformedVertices(), p2.GetTransformedVertices(), mtv);
        }

        /// <remarks>@see#overlapConvexPolygons(float[], int, int, float[], int, int, MinimumTranslationVector)</remarks>
        public static bool OverlapConvexPolygons(float[] verts1, float[] verts2, MinimumTranslationVector mtv)
        {
            return OverlapConvexPolygons(verts1, 0, verts1.Length, verts2, 0, verts2.Length, mtv);
        }

        /// <summary>
        /// Check whether polygons defined by the given vertex arrays overlap (clockwise or counter-clockwise wound doesn't matter). If
        /// they do, optionally obtain a Minimum Translation Vector indicating the minimum magnitude vector required to push the polygon
        /// defined by verts1 out of the collision with the polygon defined by verts2.
        /// </summary>
        /// <param name="verts1">Vertices of the first polygon.</param>
        /// <param name="offset1">the offset of the verts1 array</param>
        /// <param name="count1">the amount that is added to the offset1</param>
        /// <param name="verts2">Vertices of the second polygon.</param>
        /// <param name="offset2">the offset of the verts2 array</param>
        /// <param name="count2">the amount that is added to the offset2</param>
        /// <param name="mtv">A Minimum Translation Vector to fill in the case of a collision, or null (optional).</param>
        /// <returns>Whether polygons overlap.</returns>
        public static bool OverlapConvexPolygons(float[] verts1, int offset1, int count1, float[] verts2, int offset2, int count2, MinimumTranslationVector mtv)
        {
            bool overlaps;
            if (mtv != null)
            {
                mtv.depth = float.MaxValue;
                mtv.normal.X = 0;
                mtv.normal.Y = 0;
            }

            overlaps = OverlapsOnAxisOfShape(verts2, offset2, count2, verts1, offset1, count1, mtv, true);
            if (overlaps)
            {
                overlaps = OverlapsOnAxisOfShape(verts1, offset1, count1, verts2, offset2, count2, mtv, false);
            }

            if (!overlaps)
            {
                if (mtv != null)
                {
                    mtv.depth = 0;
                    mtv.normal.X = 0;
                    mtv.normal.Y = 0;
                }

                return false;
            }

            return true;
        }

        private static bool OverlapsOnAxisOfShape(float[] verts1, int offset1, int count1, float[] verts2, int offset2, int count2, MinimumTranslationVector mtv, bool shapesShifted)
        {
            int endA = offset1 + count1;
            int endB = offset2 + count2;

            // get axis of polygon A
            for (int i = offset1; i < endA; i += 2)
            {
                float x1 = verts1[i];
                float y1 = verts1[i + 1];
                float x2 = verts1[(i + 2) % count1];
                float y2 = verts1[(i + 3) % count1];

                // Get the Axis for the 2 vertices
                float axisX = y1 - y2;
                float axisY = -(x1 - x2);
                float len = (float)Math.Sqrt(axisX * axisX + axisY * axisY);

                // We got a normalized Vector
                axisX /= len;
                axisY /= len;
                float minA = float.MaxValue;
                float maxA = -float.MaxValue;

                // project shape a on axis
                for (int v = offset1; v < endA; v += 2)
                {
                    float p = verts1[v] * axisX + verts1[v + 1] * axisY;
                    minA = Math.Min(minA, p);
                    maxA = Math.Max(maxA, p);
                }

                float minB = float.MaxValue;
                float maxB = -float.MaxValue;

                // project shape b on axis
                for (int v = offset2; v < endB; v += 2)
                {
                    float p = verts2[v] * axisX + verts2[v + 1] * axisY;
                    minB = Math.Min(minB, p);
                    maxB = Math.Max(maxB, p);
                }


                // There is a gap
                if (maxA < minB || maxB < minA)
                {
                    return false;
                }
                else
                {
                    if (mtv != null)
                    {
                        float o = Math.Min(maxA, maxB) - Math.Max(minA, minB);
                        bool aContainsB = minA < minB && maxA > maxB;
                        bool bContainsA = minB < minA && maxB > maxA;

                        // if it contains one or another
                        float mins = 0;
                        float maxs = 0;
                        if (aContainsB || bContainsA)
                        {
                            mins = Math.Abs(minA - minB);
                            maxs = Math.Abs(maxA - maxB);
                            o += Math.Min(mins, maxs);
                        }

                        if (mtv.depth > o)
                        {
                            mtv.depth = o;
                            bool condition;
                            if (shapesShifted)
                            {
                                condition = minA < minB;
                                axisX = condition ? axisX : -axisX;
                                axisY = condition ? axisY : -axisY;
                            }
                            else
                            {
                                condition = minA > minB;
                                axisX = condition ? axisX : -axisX;
                                axisY = condition ? axisY : -axisY;
                            }

                            if (aContainsB || bContainsA)
                            {
                                condition = mins > maxs;
                                axisX = condition ? axisX : -axisX;
                                axisY = condition ? axisY : -axisY;
                            }

                            mtv.normal.X = axisX;
                            mtv.normal.Y = axisY;
                        }
                    }
                }
            }

            return true;
        }

        /// <summary>
        /// Minimum translation required to separate two polygons.
        /// </summary>
        public class MinimumTranslationVector
        {
            /// <summary>
            /// Unit length vector that indicates the direction for the separation
            /// </summary>
            public Vector2 normal = new Vector2();
            /// <summary>
            /// Distance of the translation required for the separation
            /// </summary>
            public float depth = 0;
        }
    }
}
