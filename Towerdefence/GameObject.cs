using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Towerdefence
{
    internal abstract class GameObject
    {
        int ID;
        Texture2D tex;
        Vector2 pos;
        Rectangle rect;
        float mass;
        float radius;
        float orientation = 0;
        float speed;
        float[,] massMatrix = new float[3, 3];
        float[,] invmassMatrix = new float[3, 3];
        float[,] inertiaMatrix = new float[3, 3];
        float[,] invinertiaMatrix = new float[3, 3];
        float torque;
        Vector2 force;
        float inertia;
        float angularVelocity;
        Vector2 velocity;
        Vector2 contactNormal;
        Vector2 contactPoint;
        Vector2 r;
        OBB obb;
        public GameObject(string tex, Rectangle rect,float mass, int id, float speed = 0 ) 
        {

            this.pos = rect.Location.ToVector2();
            this.rect = rect;
            this.mass = mass;
            //assumes all bounds are circles
            //Ix = Iy = π/4 * radius⁴
            //
            this.ID = id;
            radius = MathF.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height);
            radius /= 2;
            this.speed = speed;
            //inertia = MathF.PI / 4 * radius;
            ////Assumes all bounds are rectangles
            inertia = 1f / 12f * mass * ( (rect.Height * rect.Height) + (rect.Width * rect.Width));
            obb.Width = rect.Width; obb.Height = rect.Height;
            r = new Vector2(rect.Width / 2.0f, rect.Height / 2.0f);
            massMatrix[0, 0] = mass;
            massMatrix[0, 1] = 0;
            massMatrix[0, 2] = 0;
            massMatrix[1, 0] = 0;
            massMatrix[1, 1] = mass;
            massMatrix[1, 2] = 0;
            massMatrix[2, 0] = 0;
            massMatrix[2, 1] = 0;
            massMatrix[2, 2] = mass;

            inertiaMatrix[0, 0] = inertia;
            inertiaMatrix[0, 1] = 0;
            inertiaMatrix[0, 2] = 0;
            inertiaMatrix[1, 0] = 0;
            inertiaMatrix[1, 1] = inertia;
            inertiaMatrix[1, 2] = 0;
            inertiaMatrix[2, 0] = 0;
            inertiaMatrix[2, 1] = 0;
            inertiaMatrix[2, 2] = inertia;

            float determinant = MatrixMath.Determinant3x3(massMatrix);
            if(determinant != 0)
            {
                invmassMatrix = MatrixMath.Inverse3x3(massMatrix);
            }

            float determinantinertia = MatrixMath.Determinant3x3(inertiaMatrix);
            if (determinantinertia != 0)
            {
                invinertiaMatrix = MatrixMath.Inverse3x3(inertiaMatrix);
            }
            ResourceManager.SetGO(this);

            obb.center = pos;
            obb.UpDir = MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(orientation), -Vector2.UnitY);
            obb.LeftDir = MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(orientation), -Vector2.UnitX);
            obb.BottomRight = obb.center - obb.UpDir * (obb.Height * 0.5f) - obb.LeftDir * (obb.Width * 0.5f);
            obb.BottomLeft = obb.center - obb.UpDir * (obb.Height * 0.5f) + obb.LeftDir * (obb.Width * 0.5f);
            obb.TopLeft = obb.center + obb.UpDir * (obb.Height * 0.5f) + obb.LeftDir * (obb.Width * 0.5f);
            obb.TopRight = obb.center + obb.UpDir * (obb.Height * 0.5f) - obb.LeftDir * (obb.Width * 0.5f);

        }
        public float GetOrientation() { return orientation; }
        public Vector2 GetVelocity() { return velocity; }
        public float GetAngularVelocity() { return angularVelocity; }
        public float[,] GetInvIntertiaMatrix() { return invinertiaMatrix; }
        public float[,] GetInertiaMatrix() { return inertiaMatrix; }
        public float[,] GetInvMassMatrix() { return invmassMatrix; }
        public float[,] GetMassMatrix() { return massMatrix; }
        public int GetID() { return ID; }
        public Vector3 GetQ()
        {
            return new Vector3(pos.X, pos.Y, orientation);
        }
        public Vector2 GetContactNormal() { return contactNormal; }
        public Vector2 GetContactPoint() { return contactPoint; }
        public void SetContactInfo(Vector2 n, Vector2 contactp)
        {
            contactPoint = contactp;
            contactNormal = n;
        }
        public float Getmass() { return mass; }
        public float GetInertia() { return inertia; }
      public Rectangle GetRectangle() { return rect; }
        public float GetRadius() { return radius; }
        public Vector3 GetF() { return new Vector3(force.X, force.Y, torque); }
        
        public void AddForce(Vector2 force)
        {
            //must happen before the collision function.
            this.force += force;

            //torque += r.X * force.Y - r.Y * force.X;
        }
        public void AddTorque(float t)
        {
            torque+= t;
        }
        public Vector2 GetPos() { return pos; }
        public OBB getOBB() { return obb; }
        public virtual void Update(GameTime gametime)
        {
            float dt = (float)gametime.ElapsedGameTime.TotalSeconds;
            Vector2 linearAcceleration = force / mass;
            velocity += linearAcceleration * dt;
            pos += velocity * dt;
            float angularAcceleration = torque / inertia;
            angularVelocity += angularAcceleration * dt;
            orientation += angularVelocity * dt;
            force = Vector2.Zero;
            torque = 0;

            obb.center = pos;
            obb.UpDir = MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(orientation), -Vector2.UnitY);
            obb.LeftDir = MatrixMath.TransformVector2x2(MatrixMath.GetRotationMatrix2x2(orientation), -Vector2.UnitX);
            obb.BottomRight = obb.center - obb.UpDir * (obb.Height * 0.5f)- obb.LeftDir * (obb.Width * 0.5f);
            obb.BottomLeft = obb.center - obb.UpDir * (obb.Height * 0.5f) + obb.LeftDir * (obb.Width * 0.5f);
            obb.TopLeft = obb.center + obb.UpDir * (obb.Height * 0.5f) + obb.LeftDir * (obb.Width * 0.5f);
            obb.TopRight = obb.center + obb.UpDir * (obb.Height * 0.5f) - obb.LeftDir * (obb.Width * 0.5f);
        }
       
        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(tex, rect, new Rectangle(0, 0, rect.Width, rect.Height), Color.White,
                orientation, new Vector2(rect.Width * 0.5f, rect.Height * 0.5f), SpriteEffects.None, 0);
        }
        
    }
}
