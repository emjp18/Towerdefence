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
        public GameObject(string tex, Rectangle rect,float mass, float speed = 0 ) 
        {
            this.pos = rect.Location.ToVector2();
            this.rect = rect;
            this.mass = mass;
            //assumes all bounds are circles
            //Ix = Iy = π/4 * radius⁴
            //
            radius = MathF.Sqrt(rect.Width * rect.Width + rect.Height * rect.Height);
            radius /= 2;
            this.speed = speed;
            //inertia = MathF.PI / 4 * radius;
            ////Assumes all bounds are rectangles
            inertia = 1f / 12f * mass * ( (rect.Height * rect.Height) + (rect.Width * rect.Width));

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

        }
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
        //public void AddTorque(Vector2 f, Vector2 contactpoint)
        //{
        //    Vector2 r = contactpoint - rect.Center.ToVector2();
        //    float angle = MathF.Acos(Vector2.Dot(r,f) / r.Length() * f.Length());
        //    angle *= 180 / MathF.PI;
        //    torque+=r.Length() * f.Length() * MathF.Sin(angle);
        //}
        public void AddForce(Vector2 force)
        {
            //must happen before the collision function.
            this.force += force;

            torque += r.X * force.Y - r.Y * force.X;
        }
        public Vector2 GetPos() { return pos; }
        public virtual void Update(GameTime gametime)
        {
            float dt = (float)gametime.ElapsedGameTime.TotalSeconds;
            velocity += force / mass * dt;
            pos += velocity * dt;
            float angularAcceleration = torque / inertia;
            angularVelocity += angularAcceleration * dt;
            orientation += angularVelocity * dt;
            force = Vector2.Zero;
            torque = 0;
        }
        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(tex, rect, new Rectangle(0, 0, rect.Width, rect.Height), Color.White,
                orientation, new Vector2(rect.Width * 0.5f, rect.Height * 0.5f), SpriteEffects.None, 0);
        }
        
    }
}
