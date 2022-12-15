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
        bool hasinverse = true;
        float torque;
        Vector2 force;
        float inertia;
        Vector2 velocity;
        Vector2 contactNormal;
        Vector2 contactPoint;
        public GameObject(string tex, Rectangle rect,float mass,int id, float speed = 0 ) 
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
            inertia = MathF.PI / 4 * radius;
            this.ID = id;
            massMatrix[0, 0] = mass;
            massMatrix[0, 1] = 0;
            massMatrix[0, 2] = 0;
            massMatrix[1, 0] = 0;
            massMatrix[1, 1] = mass;
            massMatrix[1, 2] = 0;
            massMatrix[2, 0] = 0;
            massMatrix[2, 1] = 0;
            massMatrix[2, 2] = inertia;

            float determinant = MatrixMath.Determinant3x3(massMatrix);
            if(determinant == 0)
            {
                hasinverse = false;
            }
            else
            {
                invmassMatrix= MatrixMath.Inverse3x3(massMatrix);






            }


        }
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
        public void AddTorque(Vector2 f, Vector2 contactpoint)
        {
            Vector2 r = contactpoint - rect.Center.ToVector2();
            float angle = MathF.Acos(Vector2.Dot(r,f) / r.Length() * f.Length());
            angle *= 180 / MathF.PI;
            torque+=r.Length() * f.Length() * MathF.Sin(angle);
        }
        public void AddForce(Vector2 force)
        {
            //must happen before the collision function.
            this.force += force;
        }
        public Vector2 GetPos() { return pos; }
        public virtual void Update(GameTime gametime)
        {
            velocity += force / mass * (float)gametime.ElapsedGameTime.TotalSeconds;
            pos += velocity * (float)gametime.ElapsedGameTime.TotalSeconds;
            force = Vector2.Zero;
        }
        public virtual void Draw(SpriteBatch sb)
        {
            sb.Draw(tex, rect, new Rectangle(0, 0, rect.Width, rect.Height), Color.White,
                orientation, new Vector2(rect.Width * 0.5f, rect.Height * 0.5f), SpriteEffects.None, 0);
        }
        
    }
}
