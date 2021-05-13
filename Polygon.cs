using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace JGRAN_Boss_Fight_Plugin
{
    public class Polygon
    {
        private float[] localVertices;
        private float[] worldVertices;
        private float x, y;
        private float originX, originY;
        private float rotation;
        private float scaleX = 1, scaleY = 1;
        private bool dirty = true;
        private Rectangle bounds;
        /// <summary>
        /// Constructs a new polygon with no vertices.
        /// </summary>
        public Polygon()
        {
            this.localVertices = new float[0];
        }

        /// <summary>
        /// Constructs a new polygon from a float array of parts of vertex points.
        /// </summary>
        /// <param name="vertices">an array where every even element represents the horizontal part of a point, and the following element
        ///           representing the vertical part</param>
        /// <exception cref="IllegalArgumentException">if less than 6 elements, representing 3 points, are provided</exception>
        public Polygon(float[] vertices)
        {
            if (vertices.Length < 6)
                throw new ArgumentException("polygons must contain at least 3 points.");
            this.localVertices = vertices;
        }

        /// <summary>
        /// Returns the polygon's local vertices without scaling or rotation and without being offset by the polygon position.
        /// </summary>
        public virtual float[] GetVertices()
        {
            return localVertices;
        }

        /// <summary>
        /// Calculates and returns the vertices of the polygon after scaling, rotation, and positional translations have been applied,
        /// as they are position within the world.
        /// </summary>
        /// <returns>vertices scaled, rotated, and offset by the polygon position.</returns>
        public virtual float[] GetTransformedVertices()
        {
            if (!dirty)
                return this.worldVertices;
            dirty = false;
            float[] localVertices = this.localVertices;
            if (this.worldVertices == null || this.worldVertices.Length != localVertices.Length)
                this.worldVertices = new float[localVertices.Length];
            float[] worldVertices = this.worldVertices;
            float positionX = x;
            float positionY = y;
            float originX = this.originX;
            float originY = this.originY;
            float scaleX = this.scaleX;
            float scaleY = this.scaleY;
            bool scale = scaleX != 1 || scaleY != 1;
            float rotation = this.rotation;
            float cos = MathUtils.CosDeg(rotation);
            float sin = MathUtils.SinDeg(rotation);
            for (int i = 0, n = localVertices.Length; i < n; i += 2)
            {
                float x = localVertices[i] - originX;
                float y = localVertices[i + 1] - originY;

                // scale if needed
                if (scale)
                {
                    x *= scaleX;
                    y *= scaleY;
                }


                // rotate if needed
                if (rotation != 0)
                {
                    float oldX = x;
                    x = cos * x - sin * y;
                    y = sin * oldX + cos * y;
                }

                worldVertices[i] = positionX + x + originX;
                worldVertices[i + 1] = positionY + y + originY;
            }

            return worldVertices;
        }

        /// <summary>
        /// Sets the origin point to which all of the polygon's local vertices are relative to.
        /// </summary>
        public virtual void SetOrigin(float originX, float originY)
        {
            this.originX = originX;
            this.originY = originY;
            dirty = true;
        }

        /// <summary>
        /// Sets the polygon's position within the world.
        /// </summary>
        public virtual void SetPosition(float x, float y)
        {
            this.x = x;
            this.y = y;
            dirty = true;
        }

        /// <summary>
        /// Sets the polygon's local vertices relative to the origin point, without any scaling, rotating or translations being applied.
        /// </summary>
        /// <param name="vertices">float array where every even element represents the x-coordinate of a vertex, and the proceeding element
        ///           representing the y-coordinate.</param>
        /// <exception cref="IllegalArgumentException">if less than 6 elements, representing 3 points, are provided</exception>
        public virtual void SetVertices(float[] vertices)
        {
            if (vertices.Length < 6)
                throw new ArgumentException("polygons must contain at least 3 points.");
            localVertices = vertices;
            dirty = true;
        }

        /// <summary>
        /// Set vertex position
        /// </summary>
        /// <param name="index">min=0, max=vertices.length/2-1</param>
        /// <exception cref="IllegalArgumentException">if vertex doesnt exist</exception>
        public virtual void SetVertex(int index, float x, float y)
        {
            if (index < 0 || index > localVertices.Length / 2 - 1)
                throw new ArgumentException("the vertex " + index + " doesn't exist");
            localVertices[2 * index] = x;
            localVertices[2 * index + 1] = y;
            dirty = true;
        }

        /// <summary>
        /// Translates the polygon's position by the specified horizontal and vertical amounts.
        /// </summary>
        public virtual void Translate(float x, float y)
        {
            this.x += x;
            this.y += y;
            dirty = true;
        }

        /// <summary>
        /// Sets the polygon to be rotated by the supplied degrees.
        /// </summary>
        public virtual void SetRotation(float degrees)
        {
            this.rotation = degrees;
            dirty = true;
        }

        /// <summary>
        /// Applies additional rotation to the polygon by the supplied degrees.
        /// </summary>
        public virtual void Rotate(float degrees)
        {
            rotation += degrees;
            dirty = true;
        }

        /// <summary>
        /// Sets the amount of scaling to be applied to the polygon.
        /// </summary>
        public virtual void SetScale(float scaleX, float scaleY)
        {
            this.scaleX = scaleX;
            this.scaleY = scaleY;
            dirty = true;
        }

        /// <summary>
        /// Applies additional scaling to the polygon by the supplied amount.
        /// </summary>
        public virtual void Scale(float amount)
        {
            this.scaleX += amount;
            this.scaleY += amount;
            dirty = true;
        }

        /// <summary>
        /// Sets the polygon's world vertices to be recalculated when calling {@link #getTransformedVertices() getTransformedVertices}.
        /// </summary>
        public virtual void Dirty()
        {
            dirty = true;
        }

        /// <summary>
        /// Returns the area contained within the polygon.
        /// </summary>
        public virtual float Area()
        {
            float[] vertices = GetTransformedVertices();
            return PolygonArea(vertices, 0, vertices.Length);
        }

        public static float PolygonArea(float[] polygon, int offset, int count)
        {
            float area = 0;
            int last = offset + count - 2;
            float x1 = polygon[last], y1 = polygon[last + 1];
            for (int i = offset; i <= last; i += 2)
            {
                float x2 = polygon[i], y2 = polygon[i + 1];
                area += x1 * y2 - x2 * y1;
                x1 = x2;
                y1 = y2;
            }

            return area * 0.5F;
        }

        /// <summary>
        /// Returns an axis-aligned bounding box of this polygon.
        /// 
        /// Note the returned Rectangle is cached in this polygon, and will be reused if this Polygon is changed.
        /// </summary>
        /// <returns>this polygon's bounding box {@link Rectangle}</returns>
        public virtual Rectangle GetBoundingRectangle()
        {
            float[] vertices = GetTransformedVertices();
            float minX = vertices[0];
            float minY = vertices[1];
            float maxX = vertices[0];
            float maxY = vertices[1];
            int numFloats = vertices.Length;
            for (int i = 2; i < numFloats; i += 2)
            {
                minX = minX > vertices[i] ? vertices[i] : minX;
                minY = minY > vertices[i + 1] ? vertices[i + 1] : minY;
                maxX = maxX < vertices[i] ? vertices[i] : maxX;
                maxY = maxY < vertices[i + 1] ? vertices[i + 1] : maxY;
            }

            if (bounds == null)
                bounds = new Rectangle();
            bounds.X = (int) minX;
            bounds.Y = (int) minY;
            bounds.Width = (int)(maxX - minX);
            bounds.Height = (int)(maxY - minY);
            return bounds;
        }

        /// <summary>
        /// Returns whether an x, y pair is contained within the polygon.
        /// </summary>
        public bool Contains(float x, float y)
        {
            float[] vertices = GetTransformedVertices();
            int numFloats = vertices.Length;
            int intersects = 0;
            for (int i = 0; i < numFloats; i += 2)
            {
                float x1 = vertices[i];
                float y1 = vertices[i + 1];
                float x2 = vertices[(i + 2) % numFloats];
                float y2 = vertices[(i + 3) % numFloats];
                if (((y1 <= y && y < y2) || (y2 <= y && y < y1)) && x < ((x2 - x1) / (y2 - y1) * (y - y1) + x1))
                    intersects++;
            }

            return (intersects & 1) == 1;
        }

        public bool Contains(Vector2 point)
        {
            return Contains(point.X, point.Y);
        }

        /// <summary>
        /// Returns the x-coordinate of the polygon's position within the world.
        /// </summary>
        public virtual float GetX()
        {
            return x;
        }

        /// <summary>
        /// Returns the y-coordinate of the polygon's position within the world.
        /// </summary>
        public virtual float GetY()
        {
            return y;
        }

        /// <summary>
        /// Returns the x-coordinate of the polygon's origin point.
        /// </summary>
        public virtual float GetOriginX()
        {
            return originX;
        }

        /// <summary>
        /// Returns the y-coordinate of the polygon's origin point.
        /// </summary>
        public virtual float GetOriginY()
        {
            return originY;
        }

        /// <summary>
        /// Returns the total rotation applied to the polygon.
        /// </summary>
        public virtual float GetRotation()
        {
            return rotation;
        }

        /// <summary>
        /// Returns the total horizontal scaling applied to the polygon.
        /// </summary>
        public virtual float GetScaleX()
        {
            return scaleX;
        }

        /// <summary>
        /// Returns the total vertical scaling applied to the polygon.
        /// </summary>
        public virtual float GetScaleY()
        {
            return scaleY;
        }
    }
}
