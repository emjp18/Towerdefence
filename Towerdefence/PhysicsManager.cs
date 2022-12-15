using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Towerdefence
{
    public enum WINDING { Clockwise, CounterClockwise};

    public struct OBB
    {
        public Vector2 TopLeft, BottomLeft, BottomRight, TopRight;
        public Vector2 center;
        public float Width, Height;
        public Vector2 UpDir, LeftDir;
    }
    public struct EDGE
    {
        public float closestdistance;
        public int closestindex;
        public Vector2 closestnormal;
    }

    internal class PhysicsManager
    {
        float b = float.Epsilon;
        public bool CircleCircleCollision(ref GameObject a, ref GameObject b)
        {
            float x = a.GetRectangle().Center.X - b.GetRectangle().Center.X;
            float y = a.GetRectangle().Center.Y - b.GetRectangle().Center.Y;
            float centerDistanceSq = x * x + y * y; // squared distance
            float radius = a.GetRadius() + b.GetRadius();   
            float radiusSq = radius * radius;

            return centerDistanceSq <= radiusSq;
            
        }
        public OBB MinkowskiDifference(OBB obbA, OBB obbB)
        {
            OBB obbC = new OBB();
            obbC.BottomLeft = obbA.BottomLeft - obbB.BottomLeft;
            obbC.BottomRight = obbA.BottomRight - obbB.BottomRight;
            obbC.TopLeft = obbA.TopLeft - obbB.TopLeft;
            obbC.TopRight = obbA.TopRight - obbB.TopRight;
            obbC.Width = (obbC.TopRight - obbC.TopLeft).Length();
            obbC.Height = (obbC.BottomLeft - obbC.TopLeft).Length();
            obbC.center = obbC.TopLeft + new Vector2(obbC.Width, obbC.Height) * 0.5f;
            return obbC;
        }
        public void EvolveSimplex(OBB obbA, OBB obbB,ref Vector2[] simplex, Vector2 direction, ref bool foundOrigin)
        {
            

            Vector2 a = simplex[2];
            Vector2 b = simplex[1];
            Vector2 c = simplex[0];

            Vector2 a0 = a * -1; // v2 to the origin
            Vector2 ab = b - a; // v2 to v1
            Vector2 ac = c - a; // v2 to v0

            Vector2 abPerp = TripleCrossProduct(ac, ab, ab);
            Vector2 acPerp = TripleCrossProduct(ab, ac, ac);

            
            if (Vector2.Dot(abPerp, a0) > 0)
            {

                direction = abPerp;
                simplex[0] = Support(obbA, obbA, direction);
                Vector2 test = Support(obbA, obbA, direction);
                if (Vector2.Dot(direction, test) >= 0)
                {
                    EvolveSimplex(obbA, obbB,ref simplex, direction, ref foundOrigin);
                }
                else
                {
                    return;
                }
            }
            else if (Vector2.Dot(acPerp, a0) > 0)
            {
                direction = abPerp;
                simplex[1] = Support(obbA, obbA, direction);
                Vector2 test = Support(obbA, obbA, direction);
                if (Vector2.Dot(direction, test) >= 0)
                {
                    EvolveSimplex(obbA, obbB,ref simplex, direction,ref foundOrigin);
                }
                else
                {
                    return;
                }
            }
            else
            {

                foundOrigin = true;
                return;


            }
            
        }
        public EDGE FindClosestEdge(WINDING winding, Dictionary<int, Vector2> vertices)
        {
            float closestDistance = float.MaxValue;
            Vector2 closestNormal = new Vector2();
            int closestIndex = 0;
            Vector2 edge = new Vector2();


            for (int i = 0; i < vertices.Count; i++)
            {
                int j = i + 1;
                if (j >= vertices.Count) j = 0;

                edge = vertices[i] - vertices[j];
                Vector2 norm = Vector2.Zero;
                switch (winding)
                {
                    case WINDING.Clockwise:
                        norm = new Vector2(edge.Y, -edge.X);
                        break;
                    case WINDING.CounterClockwise:
                        norm = new Vector2(-edge.Y, edge.X);
                        break;
                }
                norm.Normalize();

                float dist = Vector2.Dot(norm, vertices[i]);

                if (dist < closestDistance)
                {
                    closestDistance = dist;
                    closestNormal = norm;
                    closestIndex = j;

                }

            }
            EDGE e = new EDGE();
            e.closestdistance= closestDistance;
            e.closestnormal = closestNormal;
            e.closestindex = closestIndex;
            return e;
        }
        public Vector2 Support(OBB obbA, OBB obbB, Vector2 direction)
        {
            return obbA.center + (direction * (obbA.TopLeft - obbA.BottomRight).Length() * 0.5f) -
                obbB.center + (-direction * (obbB.TopLeft - obbB.BottomRight).Length() * 0.5f);
        }
        public Vector2 EPA(OBB obba, OBB obbb, Vector2[] simplex)
        {
            float e0 = (simplex[1].X - simplex[0].X) * (simplex[1].Y + simplex[0].Y);
            float e1 = (simplex[2].X - simplex[1].X) * (simplex[2].Y + simplex[1].Y);
            float e2 = (simplex[0].X - simplex[2].X) * (simplex[0].Y + simplex[2].Y);
            WINDING winding;
            if (e0 + e1 + e2 >= 0) winding = WINDING.Clockwise;
            else winding = WINDING.CounterClockwise;
            Dictionary<int, Vector2> vertices = new Dictionary<int, Vector2>();
            vertices.Add(0,simplex[0]);
            vertices.Add(1,simplex[1]);
            vertices.Add(2,simplex[2]);
            int maximumdepth = 32;
            Vector2 intersection = Vector2.Zero;
            for (int i=0; i<maximumdepth;i++)
            {
                EDGE edge = FindClosestEdge(winding, vertices);
                Vector2 support = Support(obba, obbb, edge.closestnormal);
                float distance = Vector2.Dot(support, edge.closestnormal);
                intersection = edge.closestnormal * distance;
                if (Math.Abs(distance - edge.closestdistance) <= float.Epsilon)
                {
                    return intersection;
                }
                else
                {
                    vertices.Remove(edge.closestindex);
                    vertices.Add(edge.closestindex, support);
                }
            }

            return intersection;

        }
        public bool GJK(OBB obbA, OBB obbB, ref Vector2 intersectionpoint)
        {
          
            Vector2[] simplex = new Vector2[3];
            Vector2 direction = obbA.center - obbB.center;
            direction.Normalize();
            simplex[0] = Support(obbA, obbA, direction);

            direction *= -1;

            simplex[1] = Support(obbA, obbA, direction);

            Vector2 cb = direction * -1 - direction;
            Vector2 c0 = simplex[1];
            simplex[2] = TripleCrossProduct(cb, c0, cb);
            bool foundOrigin = false;
            EvolveSimplex(obbA, obbB, ref simplex, direction, ref foundOrigin);
            if(foundOrigin)
                intersectionpoint = EPA(obbA, obbB, simplex);

            return foundOrigin;
        }
        //A cheating way of getting a perpedincular line in 2d.
        public Vector2 TripleCrossProduct(Vector2 a , Vector2 b ,Vector2 c)
        {
            Vector3 a3 = new Vector3(a.X, a.Y, 0);
            Vector3 b3 = new Vector3(b.X, b.Y, 0);
            Vector3 c3 = new Vector3(c.X, c.Y, 0);
            Vector3 result = Vector3.Cross(Vector3.Cross(a3, b3), c3);
            Vector2 result2 = new Vector2();
            result2.X = result.X;
            result2.Y = result.Y;
            return result2;
        }
        public bool OBBOBBCollision(OBB obbA, OBB obbB)
        {

            float[] max = new float[4] { float.MinValue, float.MinValue, float.MinValue, float.MinValue };
            float[] max2 = new float[4] { float.MinValue, float.MinValue, float.MinValue, float.MinValue };
            float[] min = new float[4] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };
            float[] min2 = new float[4] { float.MaxValue, float.MaxValue, float.MaxValue, float.MaxValue };

            Vector2[] axis = new Vector2[2] { obbA.UpDir, obbA.LeftDir };
            Vector2[] axis2 = new Vector2[2] { obbB.UpDir, obbB.LeftDir };
            Vector2[] corners = new Vector2[4] { obbA.TopLeft, obbA.BottomLeft, obbA.TopRight, obbA.BottomRight };
            Vector2[] corners2 = new Vector2[4] { obbB.TopLeft, obbB.BottomLeft, obbB.TopRight, obbB.BottomRight };

            for (int i = 0; i < 2; i++)
            {
                axis[i].Normalize();
                axis2[i].Normalize();
            }
            for (int i = 0; i < 4; i++)
            {
                float d = Vector2.Dot(corners[i], axis[0]);
                if (d > max[0])
                    max[0] = d;
                if (d < min[0])
                    min[0] = d;

                float d2 = Vector2.Dot(corners2[i], axis[0]);
                if (d2 > max2[0])
                    max2[0] = d2;
                if (d2 < min2[0])
                    min2[0] = d2;


                d = Vector2.Dot(corners[i], axis[1]);
                if (d > max[1])
                    max[1] = d;
                if (d < min[1])
                    min[1] = d;

                d2 = Vector2.Dot(corners2[i], axis[1]);
                if (d2 > max2[1])
                    max2[1] = d2;
                if (d2 < min2[1])
                    min2[1] = d2;

                d = Vector2.Dot(corners[i], axis2[0]);
                if (d > max[2])
                    max[2] = d;
                if (d < min[2])
                    min[2] = d;

                d2 = Vector2.Dot(corners2[i], axis2[0]);
                if (d2 > max2[2])
                    max2[2] = d2;
                if (d2 < min2[2])
                    min2[2] = d2;

                d = Vector2.Dot(corners[i], axis2[1]);
                if (d > max[3])
                    max[3] = d;
                if (d < min[3])
                    min[3] = d;

                d2 = Vector2.Dot(corners2[i], axis2[1]);
                if (d2 > max2[3])
                    max2[3] = d2;
                if (d2 < min2[3])
                    min2[3] = d2;
            }
            if ((min[0] > max2[0]) || min2[0] > max[0])
                return false;
            if ((min[1] > max2[1]) || min2[1] > max[1])
                return false;
            if ((min[2] > max2[2]) || min2[2] > max[2])
                return false;
            if ((min[3] > max2[3]) || min2[3] > max[3])
                return false;

            return true;
        }
        public float NonPenetrationConstraint(Vector2 contactPoint, OBB obba, OBB obbb)
        {

        }
        public void SequentialImpulse(GameObject[] objects)
        {
            for(int i=0; i<objects.Count();i++)
            {
               
            }
        }
    }
}
/*
 * Sequential impulses do not use a monolith system, constraints are solved individually.
 * 1. apply forces to velocities
 * 2. apply impulses to velocities until max iterations or margin of error is small
 * 3. apply velocities to update positions
 * 
*/