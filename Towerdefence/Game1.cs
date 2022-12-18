using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Spline;

using System.IO;

namespace Towerdefence
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        Texture2D tex;

        public static int width = 1920;
        public static int height =1080;

        GameObject testobject = new Item("", new Rectangle(700, 500, 50, 50), 1, 30, 0);
        GameObject testobject2 = new Item("", new Rectangle(100, 500, 50, 50), 1, 30, 1);
       
        PhysicsManager physicsManager= new PhysicsManager();
        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();
        }

        protected override void LoadContent()
        {
            
            graphics.PreferredBackBufferHeight = height;
            graphics.PreferredBackBufferWidth = width;
            graphics.ApplyChanges();

            spriteBatch = new SpriteBatch(GraphicsDevice);

            tex = Content.Load<Texture2D>("ball");

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();
            if(Keyboard.GetState().IsKeyDown(Keys.Space))
            {
                testobject.AddForce(testobject2.GetPos() - testobject.GetPos());
                testobject2.AddForce(new Vector2(100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Left))
            {
                testobject.AddForce(new Vector2(-100, 0));
                testobject2.AddForce(new Vector2(-100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Right))
            {
                testobject.AddForce(new Vector2(100, 0));
                testobject2.AddForce(new Vector2(100, 0));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Up))
            {
                testobject.AddForce(new Vector2(0, -100));
                testobject2.AddForce(new Vector2(0, -100));
            }
            if (Keyboard.GetState().IsKeyDown(Keys.Down))
            {
                testobject.AddForce(new Vector2(0, 100));
                testobject2.AddForce(new Vector2(0, 100));
            }

            testobject.Update(gameTime);
            testobject2.Update(gameTime);

            physicsManager.ConstrainWindowBounds(testobject);
            physicsManager.ConstrainWindowBounds(testobject2);

            if (physicsManager.GJK(testobject, testobject2))
            {
                physicsManager.SequentialImpulse((float)gameTime.ElapsedGameTime.TotalSeconds, testobject, testobject2);
            }
            
            //
           
            base.Update(gameTime);
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.CornflowerBlue);

            spriteBatch.Begin();
            
            spriteBatch.Draw(tex, testobject.GetRectangle()
                ,null, Color.White, testobject.GetOrientation(), testobject.GetOrigin(), SpriteEffects.None, 0);

            spriteBatch.Draw(tex, testobject2.GetRectangle(), null, Color.White, testobject2.GetOrientation(), 
                testobject2.GetOrigin(), SpriteEffects.None, 0);
       
            spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}