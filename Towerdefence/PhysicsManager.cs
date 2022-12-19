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
        public float massNormal;
        public float massTangent;
        public float bias;
        public float restitution;
        public Vector2 velocityA;
        public Vector2 VelocityB;
        public float anglularVelA;
        public float angularVelB;
        public float massA;
        public float massB;
        public float inertiaA;
        public float inertiaB;
        public int IDA;
        public int IDB;
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
        List<PositionConstraint> constraints = new List<PositionConstraint>();
        public int numIterations = 10;
        public float friction =1f;
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
     public void ResetConstraints() { constraints.Clear(); }
        public void ApplyFriction(float dt)
        {
            for(int i=0; i<constraints.Count; i++)
            {
                PositionConstraint c = constraints[i];
                GameObject a = ResourceManager.GetGameObject(c.IDA);
                GameObject b = ResourceManager.GetGameObject(c.IDB);

                Vector2 rv = b.GetVelocity() - a.GetVelocity();
                // Solve for the tangent vector 
                Vector2 tangent = rv - Vector2.Dot(rv, c.n2) * c.n2;
                tangent.Normalize();
                float velAlongNormal = Vector2.Dot(rv, tangent);
                // Solve for magnitude to apply along the friction vector 
                float jt = -Vector2.Dot(rv, tangent);
                jt = jt / (1 / a.Getmass() + 1 / b.Getmass());
                float j = -(1 + 0.1f) * velAlongNormal;
                j /= 1 / a.Getmass() + 1 / b.Getmass();
                float mu = friction;
                Vector2 frictionImpulse;
                if (MathF.Abs(jt) < j * mu)
                    frictionImpulse = jt * tangent;
                else
                {

                    frictionImpulse = -j * tangent * friction;
                }
                a.BypassVelocity((1 / a.Getmass()) * frictionImpulse, 0, dt);
                b.BypassVelocity((-1 / b.Getmass()) * frictionImpulse, 0, dt);

            }





        }
    public void SequentialImpulse(float dt)
        {
            
            float k_slop = 0.004f;
            float k_biasFactor = 0.1f;
            for(int j=0; j<numIterations; j++)
            {
                for (int i = 0; i < constraints.Count; i++)
                {
                    PositionConstraint c = constraints[i];
                    if (c.penetrationdepth <= float.Epsilon)
                        continue;
                    GameObject a = ResourceManager.GetGameObject(c.IDA);
                    GameObject b = ResourceManager.GetGameObject(c.IDB);
                    Vector2 ra = c.ra2;
                    Vector2 rb = c.rb2;
                   
                    float invMassA = 1.0f / c.massA;
                    float invMassB = 1.0f / c.massB;
                    float massLinear = invMassA + invMassB;
                    float invIA = 1.0f / c.inertiaA;
                    float invIB = 1.0f / c.inertiaB;

                    c.massNormal = massLinear + MathF.Pow(MatrixMath.Cross(c.n2, ra), 2) * invIA + MathF.Pow(
                        MatrixMath.Cross(c.n2, rb), 2) * invIB;

                    c.massNormal = 1.0f / c.massNormal;

                    c.massTangent = massLinear + MathF.Pow(Vector2.Dot(c.n2, ra), 2) * invIA + MathF.Pow(
                        Vector2.Dot(c.n2, rb), 2) * invIB; // Cross(tangent, r)^2 = Dot(normal, r)^2 : in 2D

                    c.massTangent = 1.0f / c.massTangent;

                    Vector2 dv = c.VelocityB + MatrixMath.Cross(rb, c.angularVelB) - c.velocityA
                        - MatrixMath.Cross(ra, c.anglularVelA);

                    float vn = Vector2.Dot(dv, c.n2);
                    float maxrestitution = 0.3f;
                    c.restitution = vn < -1.0f ? vn * maxrestitution : 0.0f;

                    c.bias = MathF.Min(0.0f, c.penetrationdepth + k_slop) * k_biasFactor / dt;


                    float nLambda = (-vn + c.bias + c.restitution) * c.massNormal;

                    float oldAccumI = c.accum;
                    c.accum += nLambda;

                    if (c.accum < 0)
                    {
                        c.accum = 0;
                    }
                    // Find real lambda
                    float I = c.accum - oldAccumI;

                    // Calculate linear impulse
                    Vector2 nLinearI = c.n2 * I;

                    // Calculate angular impulse
                    float rnA = MatrixMath.Cross(c.ra2, c.n2);
                    float rnB = MatrixMath.Cross(c.rb2, c.n2);
                    float nAngularIA = rnA * I;
                    float nAngularIB = rnB * I;
                  
                    // Apply linear impulse
                    a.BypassVelocity(-invMassA * nLinearI, -invIA * nAngularIA,dt);
                    b.BypassVelocity(invMassB * nLinearI, invIB * nAngularIB,dt);
                    

                    constraints[i] = c;

                }

            }



            /*
             * 
                             * Equation: A = J * M⁻¹ * Jt

                * Jt = transposed Jacobian

                * Only in 2D the inverse effective mass is =

                                 J                                M⁻¹                       Jt
                [nx, ny, n X ra, -nx, -ny, -n X rb] |m1⁻¹  0      0     0     0       0| | nx    |
                                                    |0     m1⁻¹   0     0     0       0| | ny    |
                                                    |0     0      I1⁻¹  0     0       0| | n X ra|  
                                                    |0     0      0     m2⁻¹  0       0| |-nx    |  
                                                    |0     0      0     0     m2⁻¹    0| |-ny    |    
                                                    |0     0      0     0     0    I2⁻¹| |-n X rb|
             * 
             */











        }
        public void ConstrainWindowBounds( )
        {
            for (int i=0; i<ResourceManager.GetGameObjectsCount(); i++)
            {
                GameObject o = ResourceManager.GetGameObject(i);
                Vector2 vel = o.GetVelocity();
                if (o.getOBB().BottomLeft.Y > Game1.height
                    || o.getOBB().TopLeft.Y < 0
                    )
                {
                    vel.Y *= -1;
                    o.SetVelocity(vel);
                }

                if (o.getOBB().BottomRight.X > Game1.width
                    || o.getOBB().BottomLeft.X < 0)
                {
                    vel.X *= -1;
                    o.SetVelocity(vel);
                }
            }
            
        }
        public bool Line(ref List<Vector2> simplex, ref Vector2 direction)
        {
            Vector2 a = simplex[0];
            Vector2 b = simplex[1];
            Vector2 ab = b - a;
            Vector2 ao = -a;
            if (SameDirection(ab, ao))
            {
                //Vector3 ab3 = new Vector3(ab.X, ab.Y, 0);
                //Vector3 ao3 = new Vector3(ao.X, ao.Y, 0);
                direction = MatrixMath.TripleCross(ab, ao, ab);
                direction = direction;
                //Vector3 triplecross = Vector3.Cross(Vector3.Cross(ab3, ao3), ab3);
                //direction.X = triplecross.X;
                //direction.Y = triplecross.Y;
                //direction.X = -ab.Y;
                //direction.Y = ab.X;

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
            //Vector3 ao3 = new Vector3(ao.X, ao.Y, 0);
            Vector3 ab3 = new Vector3(ab.X, ab.Y, 0);
            Vector3 ac3 = new Vector3(ac.X, ac.Y, 0);
            Vector3 abc = Vector3.Cross(ab3, ac3);
            //Vector3 cross = Vector3.Cross(abc, ac3);
            //Vector2 cross2;
            if (SameDirection(MatrixMath.TripleCross(ab, ac, ac),ao))
            {
                if (SameDirection(ac, ao))
                {
                    simplex.Clear();
                    simplex.Add(a);
                    simplex.Add(c);

                    //cross = Vector3.Cross(Vector3.Cross(ac3, ao3), ac3);
                    //direction = new Vector2(cross.X, cross.Y);

                    direction = MatrixMath.TripleCross(ac, ao, ac);
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
                //cross = Vector3.Cross(Vector3.Cross(ab3, abc), ao3);
                //cross2 = MatrixMath.TripleCross(ab, abc, ao);
                if (SameDirection(MatrixMath.TripleCross(ab, ab, ac), ao))
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

            //switch (simplex.Count)
            //{
            //    case 2:
            //        {
            //            return Line(ref simplex, ref direction);

            //        }
            //    case 3:
            //        {
            //            return Triangle(ref simplex, ref direction);
            //        }
            //}
            //return false;


            Vector2 a = simplex[simplex.Count() - 1];
            // compute AO (same thing as -A)
            Vector2 ao = -a;
            if (simplex.Count() == 3)
            {
                // then its the triangle case
                // get b and c
                Vector2 b = simplex[1];
                Vector2 c = simplex[0];
                // compute the edges
                Vector2 ab = b - a;
                Vector2 ac = c - a;
                // compute the normals
                Vector2 abPerp = MatrixMath.TripleCross(ac, ab, ab);
                Vector2 acPerp = MatrixMath.TripleCross(ab, ac, ac);
                // is the origin in R4
                if (SameDirection(abPerp,ao)) {
                    // remove point c
                   
                    simplex.RemoveAt(2);
                    // set the new direction to abPerp
                    direction = abPerp;
                } else
                {
                    // is the origin in R3
                    if (SameDirection(acPerp, ao)) {
                        // remove point b
                        simplex.RemoveAt(1);
                        // set the new direction to acPerp
                        direction = acPerp;
                    } else
                    {
                        // otherwise we know its in R5 so we can return true
                        return true;
                    }
                }
            }
            else
            {
                // then its the line segment case
                Vector2 b = simplex[0];
                // compute AB
                Vector2 ab = b - a;
                // get the perp to AB in the direction of the origin
                Vector2 abPerp = MatrixMath.TripleCross(ab, ao, ab);
                // set the direction to abPerp
                direction = abPerp;
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
        public PositionConstraint CreatePositionConstraint(OBB obbA, OBB obbB, GameObject a, GameObject b,Vector2 mtv, Vector2 origin)
        {
            PositionConstraint pc = new PositionConstraint();
            Vector2 contactpointA = obbA.center + mtv;
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
            pc.velocityA = a.GetVelocity();
            pc.VelocityB = b.GetVelocity();
            pc.anglularVelA = a.GetAngularVelocity();
            pc.angularVelB = b.GetAngularVelocity();
            pc.massA = a.Getmass();
            pc.massB = b.Getmass();
            pc.inertiaA = a.GetInertia();
            pc.inertiaB = b.GetInertia();
            pc.IDA = a.GetID();
            pc.IDB = b.GetID(); 
            return pc;
        }
        public bool GJK(GameObject a, GameObject b)
        {
            List<Vector2> simplex = new List<Vector2>();

            Vector2 direction = Vector2.UnitX;
            Vector2 startsupport = Support(a.getOBB(), b.getOBB(), direction);

            simplex.Add(startsupport);

            //direction = -startsupport;
            direction = -direction;
            bool colliding = false;
            while (true)
            {
                
                Vector2 support = Support(a.getOBB(), b.getOBB(), direction);
                if (Vector2.Dot(support, direction) <= 0)
                {
                    break;
                }
                //List<Vector2> simplexCopy = new List<Vector2>();
                //foreach (Vector2 point in simplex)
                //{
                //    simplexCopy.Add(point);
                //}

                //simplex.Clear();
                //simplex.Add(support);
                //foreach (Vector2 point in simplexCopy)
                //{
                //    simplex.Add(point);
                //}
                simplex.Add(support);
                if (EvolveSimplex(a.getOBB(), b.getOBB(), ref simplex, ref direction))
                {
                    colliding = true;
                    break;
                }

            }
            if (colliding)
            {
                  Vector2 mtv = EPA(a.getOBB(), b.getOBB(), simplex);
                constraints.Add(CreatePositionConstraint(a.getOBB(), b.getOBB(),a,b, mtv, simplex[simplex.Count()-1]));
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