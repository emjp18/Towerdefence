using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms.VisualStyles;
using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Media;
using SharpDX.MediaFoundation;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.Tab;

namespace Towerdefence
{
    
    
    public struct PositionConstraint
    {
        public Vector3 rb3;
        public Vector3 ra3;
        public float beta;
        public float alpha;
        public Vector3 n3;
        public float[,] J;
        public float penetrationdepth;
        public Vector2 n2;
        public Vector2 rb2;
        public Vector2 ra2;
        public float d;
        public float accum;
    }
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
       
        //SAT
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
       
        public void Impulse(GameObject a,GameObject b ,ref PositionConstraint constraint, float dt)
        {
            if (constraint.d >= 0)
                return;
           
            float bias = 0.5f / dt * constraint.penetrationdepth;

            float kNormal = a.GetInvMassMatrix()[0, 0] + b.GetInvMassMatrix()[0, 0];
            Vector3 rnA = Vector3.Cross(constraint.ra3, constraint.n3);
            Vector3 rnB = Vector3.Cross(constraint.rb3, constraint.n3);
            kNormal += Vector3.Dot(a.GetInvIntertiaMatrix()[0, 0] * rnA, rnA) + Vector3.Dot(b.GetInvIntertiaMatrix()[0, 0] * rnB, rnB);
            Vector3 v1 = new Vector3(a.GetVelocity().X, a.GetVelocity().Y, a.GetAngularVelocity());
            Vector3 v2 = new Vector3(b.GetVelocity().X, b.GetVelocity().Y, b.GetAngularVelocity());
            // Save inverse of J(M^-1)(J^T).
            float normalmass = 1.0f / kNormal;

            Vector2 dv = (b.GetVelocity()+constraint.rb2 - a.GetVelocity()-constraint.ra2);
               

            float jnV = Vector2.Dot(dv, constraint.n2);// dv.dot(normal);
            float vinitial = Vector3.Dot(v1 - v2, constraint.n3);
            float vfinal = jnV + bias;
            float restitution = vfinal / vinitial;

            float lambda = -jnV + 0.5f / dt + (-restitution * (jnV)) * normalmass;

            float oldAccumI = constraint.accum;
            constraint.accum += lambda;

            if (constraint.accum < 0)
            {
                constraint.accum = 0;
            }
            // Find real lambda
            float I = constraint.accum - oldAccumI;

            // Calculate linear impulse
            Vector2 nLinearI = constraint.n2 * I;

            // Calculate angular impulse

            Vector3 nAngularIA = rnA * I;
            Vector3 nAngularIB = rnB * I;

            // Apply linear impulse
            a.AddForce(-a.GetInvMassMatrix()[0, 0] * nLinearI);
            b.AddForce(b.GetInvMassMatrix()[0, 0] * nLinearI);

            

        }
        
        public bool Line(ref List<Vector2> simplex, ref Vector2 direction)
        {
            Vector2 a = simplex[0];
            Vector2 b = simplex[1];
            Vector2 ab = b - a;
            Vector2 ao = -a;
            if (SameDirection(ab, ao))
            {
                Vector3 ab3 = new Vector3(ab.X, ab.Y, 0);
                Vector3 ao3 = new Vector3(ao.X, ao.Y, 0);
                Vector3 cross = MatrixMath.Cross(ab3, ao3);
                Vector3 dir3 = MatrixMath.Cross(cross, ab3);
                direction.X = dir3.X;
                direction.Y = dir3.Y;
            }
            else
            {
                simplex.Clear();
                simplex.Add(a);
                direction = ao;

            }

            return false;
        }
        public bool SameDirection(
             Vector2 direction,
             Vector2 ao)
        {
            return Vector2.Dot(direction, ao) > 0;
        }
        public bool Triangle(ref List<Vector2> simplex, ref Vector2 direction)
        {
            Vector2 a = simplex[0];
            Vector2 b = simplex[1];
            Vector2 c = simplex[2];

            Vector2 ab = b - a;
            Vector2 ac = c - a;
            Vector2 ao = -a;
            Vector3 ao3 = new Vector3(ao.X, ao.Y, 0);
            Vector3 ab3 = new Vector3(ab.X, ab.Y, 0);
            Vector3 ac3 = new Vector3(ac.X, ac.Y, 0);
            Vector3 abc = Vector3.Cross(ab3, ac3);
            Vector3 cross = Vector3.Cross(abc, ac3);
            if (SameDirection(new Vector2(cross.X, cross.Y), ao))
            {
                if (SameDirection(ac, ao))
                {
                    simplex.Clear();
                    simplex.Add(a);
                    simplex.Add(c);

                    cross = Vector3.Cross(Vector3.Cross(ac3, ao3), ac3);
                    direction = new Vector2(cross.X, cross.Y);
                }
                else
                {
                    simplex.Clear();
                    simplex.Add(a);
                    simplex.Add(b);
                    return Line(ref simplex, ref direction);
                }
            }
            else
            {
                cross = Vector3.Cross(Vector3.Cross(ab3, abc), ao3);
                if (SameDirection(new Vector2(cross.X, cross.Y), ao))
                {
                    simplex.Clear();
                    simplex.Add(a);
                    simplex.Add(b);


                    return Line(ref simplex, ref direction);
                }
                else
                {
                    if (SameDirection(new Vector2(abc.X, abc.Y), ao))
                    {
                        direction = new Vector2(abc.X, abc.Y);
                    }
                    else
                    {
                        simplex.Clear();
                        simplex.Add(a);
                        simplex.Add(c);
                        simplex.Add(b);
                        direction = new Vector2(-abc.X, -abc.Y);
                    }
                }
            }
            return true;
        }
        public bool EvolveSimplex(OBB obbA, OBB obbB, ref List<Vector2> simplex, ref Vector2 direction)
        {

            switch (simplex.Count)
            {
                case 2:
                    {
                        return Line(ref simplex, ref direction);

                    }
                case 3:
                    {
                        return Triangle(ref simplex, ref direction);
                    }
            }
            return false;


        }
        public Vector2 Support(OBB obbA, OBB obbB, Vector2 direction)
        {
            float maxA = float.MinValue;
            float maxB = float.MinValue;
            Vector2[] pointsA = new Vector2[4];
            pointsA[0] = obbA.TopLeft;
            pointsA[1] = obbA.TopRight;
            pointsA[2] = obbA.BottomLeft;
            pointsA[3] = obbA.BottomRight;
            Vector2[] pointsB = new Vector2[4];
            pointsB[0] = obbB.TopLeft;
            pointsB[1] = obbB.TopRight;
            pointsB[2] = obbB.BottomLeft;
            pointsB[3] = obbB.BottomRight;
            Vector2 a = Vector2.Zero;
            Vector2 b = Vector2.Zero;
            for (int i = 0; i < 4; i++)
            {
                float dotA = Vector2.Dot(pointsA[i], direction);
                if (dotA > maxA)
                {
                    maxA = dotA;
                    a = pointsA[i];
                }
                float dotB = Vector2.Dot(pointsB[i], -direction);
                if (dotB > maxB)
                {
                    maxB = dotB;
                    b = pointsB[i];
                }
            }



            return a - b;

        }
        public PositionConstraint GetPositionConstraint(OBB obbA, OBB obbB, Vector2 mtv)
        {
            PositionConstraint pc = new PositionConstraint();
            Vector2 contactpointA = obbA.center - mtv;
            Vector2 contactpointB = -contactpointA;
            obbA.UpDir.Normalize();
            obbB.UpDir.Normalize();
            Vector2 ra = contactpointA - obbA.center;
            Vector2 rb = contactpointB - obbB.center;
            float alpha = MathHelper.ToDegrees(MathF.Acos(Vector2.Dot(obbA.UpDir, new Vector2(0, -1))));
            float beta = MathHelper.ToDegrees(MathF.Acos(Vector2.Dot(obbB.UpDir, new Vector2(0, -1))));
            Vector2 n = contactpointB - contactpointA;
            n.Normalize();
            //float c = Vector2.Dot((obbB.center + MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(beta), rb)
            //     - obbA.center - MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(alpha), ra)), n);

            //J = np.hstack((-n, np.cross(ra, n), n, -np.cross(rb, n)))
            Vector3 ra3 = new Vector3(ra.X, ra.Y, 0);
            Vector3 rb3 = new Vector3(rb.X, rb.Y, 0);
            Vector3 n3 = new Vector3(n.X, n.Y, 0);
            Vector3 ra3crn3 = Vector3.Cross(ra3, n3);
            Vector3 rb3crn3 = Vector3.Cross(rb3, n3);
            float[,] J = new float[12, 1]{ {n3.X },{n3.Y },{0 },{ra3crn3.X },{ ra3crn3.Y},{ra3crn3.Z },
                {-n3.X },{ -n3.Y},{0  },{-rb3crn3.X },{ -rb3crn3.Y},{-rb3crn3.Z } };

           
            pc.ra3 = ra3;
            pc.J = J;
            pc.beta = beta;
            pc.alpha = alpha;
            pc.n3 = n3;
            pc.rb3 = rb3;
            pc.penetrationdepth = mtv.Length();
            pc.n2 = n;
            pc.ra2 = ra;
            pc.rb2 = rb;
            pc.d = Vector2.Dot(((obbA.center + ra) - obbB.center + rb), n);
            pc.accum = 0;
            return pc;
        }
        public bool GJK(OBB obbA, OBB obbB)
        {
            List<Vector2> simplex = new List<Vector2>();

            Vector2 direction = new Vector2(1, 0);
            Vector2 startsupport = Support(obbA, obbB, direction);

            simplex.Add(startsupport);

            direction = -startsupport;
            bool colliding = false;
            while (true)
            {
                
                Vector2 support = Support(obbA, obbB, direction);
                if (Vector2.Dot(support, direction) <= 0)
                {
                    break;
                }
                List<Vector2> simplexCopy = new List<Vector2>();
                foreach (Vector2 point in simplex)
                {
                    simplexCopy.Add(point);
                }

                simplex.Clear();
                simplex.Add(support);
                foreach (Vector2 point in simplexCopy)
                {
                    simplex.Add(point);
                }

                if (EvolveSimplex(obbA, obbB, ref simplex, ref direction))
                {
                    colliding = true;
                    break;
                }

            }
            if (colliding)
            {
                EPA(obbA, obbB, simplex);
                
            }

            return colliding;


        }
        public Vector2 EPA(OBB obba, OBB obbb, List<Vector2> simplex)
        {
            int minIndex = 0;
            float minDistance = float.MaxValue;
            Vector2 minNormal = new Vector2();

            while (minDistance == float.MaxValue)
            {
                for (int i = 0; i < simplex.Count; i++)
                {
                    int j = (i + 1) % simplex.Count;

                    Vector2 vertexI = simplex[i];
                    Vector2 vertexJ = simplex[j];

                    Vector2 ij = vertexJ - vertexI;

                    Vector2 normal = new Vector2(ij.Y, -ij.X);
                    normal.Normalize();
                    float distance = Vector2.Dot(normal, vertexI);

                    if (distance < 0)
                    {
                        distance *= -1;
                        normal *= -1;
                    }

                    if (distance < minDistance)
                    {
                        minDistance = distance;
                        minNormal = normal;
                        minIndex = j;
                    }
                    Vector2 support = Support(obba, obbb, minNormal);
                    float sDistance = Vector2.Dot(minNormal, support);

                    if (MathF.Abs(sDistance - minDistance) > 0.001)
                    {
                        minDistance = float.MaxValue;

                        List<Vector2> simplexCopy = new List<Vector2>();
                        foreach (Vector2 point in simplex)
                        {
                            simplexCopy.Add(point);
                        }

                        simplex.Clear();

                        for (int k = 0; k < simplexCopy.Count; k++)
                        {
                            if (k == minIndex)
                                simplex.Add(support);

                            simplex.Add(simplexCopy[k]);
                        }




                    }

                }

            }
            return minNormal * (minDistance + 0.001f);
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